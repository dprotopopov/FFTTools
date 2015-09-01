using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;
using FFTWSharp;

namespace FFTTools
{
    /// <summary>
    ///     Blur bitmap with the Fastest Fourier Transform
    /// </summary>
    public class BlurBuilder : IDisposable
    {
        private readonly Size _blinderSize; // blinder size
        private readonly int _filterStep;
        private readonly KeepOption _keepOption;
        private readonly Mode _mode;

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param>Bitmap blur blinder size</param>
        /// <param name="filterStep"></param>
        /// <param name="keepOption"></param>
        public BlurBuilder(int filterStep, KeepOption keepOption = KeepOption.AverageAndDelta)
        {
            _mode = Mode.FilterStep;
            _filterStep = filterStep;
            _keepOption = keepOption;
        }

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="blinderSize">Bitmap blur blinder size</param>
        /// <param name="keepOption"></param>
        public BlurBuilder(Size blinderSize, KeepOption keepOption = KeepOption.AverageAndDelta)
        {
            _mode = Mode.BlinderSize;
            _blinderSize = blinderSize;
            _keepOption = keepOption;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///     Blur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Blured bitmap</returns>
        private double[,,] Blur(double[,,] imageData)
        {
            int length = imageData.Length;
            int n0 = imageData.GetLength(0);
            int n1 = imageData.GetLength(1);
            int n2 = imageData.GetLength(2);

            var input = new fftw_complexarray(length);
            var output = new fftw_complexarray(length);
            fftw_plan forward = fftw_plan.dft_3d(n0, n1, n2, input, output,
                fftw_direction.Forward,
                fftw_flags.Estimate);
            fftw_plan backward = fftw_plan.dft_3d(n0, n1, n2, input, output,
                fftw_direction.Backward,
                fftw_flags.Estimate);

            var doubles = new double[length];
            Buffer.BlockCopy(imageData, 0, doubles, 0, length*sizeof (double));
            double average = doubles.Average();
            double delta = Math.Sqrt(doubles.Average(x => x*x) - average*average);
            switch (_keepOption)
            {
                case KeepOption.AverageAndDelta:
                    break;
                case KeepOption.Sum:
                    average = doubles.Sum();
                    break;
                case KeepOption.Square:
                    average = Math.Sqrt(doubles.Sum(x => x*x));
                    break;
                case KeepOption.AverageSquare:
                    average = Math.Sqrt(doubles.Average(x => x*x));
                    break;
                default:
                    throw new NotImplementedException();
            }

            input.SetData(doubles.Select(x => new Complex(x, 0)).ToArray());
            forward.Execute();
            Complex[] complex = output.GetData_Complex();

            var data = new Complex[n0, n1, n2];
            var buffer = new double[length*2];

            GCHandle complexHandle = GCHandle.Alloc(complex, GCHandleType.Pinned);
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr complexPtr = complexHandle.AddrOfPinnedObject();
            IntPtr dataPtr = dataHandle.AddrOfPinnedObject();

            Marshal.Copy(complexPtr, buffer, 0, buffer.Length);
            Marshal.Copy(buffer, 0, dataPtr, buffer.Length);
            switch (_mode)
            {
                case Mode.BlinderSize:
                    Blind(data, _blinderSize);
                    break;
                case Mode.FilterStep:
                    int filterStep = _filterStep;
                    var blinderSize = new Size(MulDiv(n1, filterStep, filterStep + 1),
                        MulDiv(n0, filterStep, filterStep + 1));
                    Blind(data, blinderSize);
                    break;
                default:
                    throw new NotImplementedException();
            }
            Marshal.Copy(dataPtr, buffer, 0, buffer.Length);
            Marshal.Copy(buffer, 0, complexPtr, buffer.Length);

            complexHandle.Free();
            dataHandle.Free();

            input.SetData(complex);
            backward.Execute();
            doubles = output.GetData_Complex().Select(x => x.Magnitude).ToArray();

            double average2 = doubles.Average();
            double delta2 = Math.Sqrt(doubles.Average(x => x*x) - average2*average2);
            switch (_keepOption)
            {
                case KeepOption.AverageAndDelta:
                    break;
                case KeepOption.Sum:
                    average2 = doubles.Sum();
                    break;
                case KeepOption.Square:
                    average2 = Math.Sqrt(doubles.Sum(x => x*x));
                    break;
                case KeepOption.AverageSquare:
                    average2 = Math.Sqrt(doubles.Average(x => x*x));
                    break;
                default:
                    throw new NotImplementedException();
            }
            // a*average2 + b == average
            // a*delta2 == delta
            double a = (_keepOption == KeepOption.AverageAndDelta) ? (delta/delta2) : (average/average2);
            double b = (_keepOption == KeepOption.AverageAndDelta) ? (average - a*average2) : 0;
            Debug.Assert(Math.Abs(a*average2 + b - average) < 0.1);
            doubles = doubles.Select(x => Math.Round(a*x + b)).ToArray();

            Buffer.BlockCopy(doubles, 0, imageData, 0, length*sizeof (double));
            return imageData;
        }

        /// <summary>
        ///     Blur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Blured bitmap</returns>
        public Image<Bgr, Byte> Blur(Image<Bgr, Byte> bitmap)
        {
            using (Image<Bgr, double> image = bitmap.Convert<Bgr, double>())
            {
                image.Data = Blur(image.Data);
                return image.Convert<Bgr, Byte>();
            }
        }

        /// <summary>
        ///     Blur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Blured bitmap</returns>
        public Image<Gray, Byte> Blur(Image<Gray, Byte> bitmap)
        {
            using (Image<Gray, double> image = bitmap.Convert<Gray, double>())
            {
                image.Data = Blur(image.Data);
                return image.Convert<Gray, Byte>();
            }
        }

        /// <summary>
        ///     Clear internal region of array
        /// </summary>
        /// <param name="data">Array of values</param>
        /// <param name="size">Internal blind region size</param>
        private static void Blind(Complex[,,] data, Size size)
        {
            int n0 = data.GetLength(0);
            int n1 = data.GetLength(1);
            int n2 = data.GetLength(2);
            int s0 = Math.Max(0, (n0 - size.Height)/2);
            int s1 = Math.Max(0, (n1 - size.Width)/2);
            int e0 = Math.Min((n0 + size.Height)/2, n0);
            int e1 = Math.Min((n1 + size.Width)/2, n1);
            for (int i = s0; i < e0; i++)
            {
                Array.Clear(data, i*n1*n2, n1*n2);
            }
            for (int i = 0; i < s0; i++)
            {
                Array.Clear(data, i*n1*n2 + s1*n2, (e1 - s1)*n2);
            }
            for (int i = e0; i < n0; i++)
            {
                Array.Clear(data, i*n1*n2 + s1*n2, (e1 - s1)*n2);
            }
        }

        /// <summary>
        ///     Blur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Blured bitmap</returns>
        public Bitmap Blur(Bitmap bitmap)
        {
            using (var image = new Image<Bgr, double>(bitmap))
            {
                image.Data = Blur(image.Data);
                return image.Bitmap;
            }
        }

        /// <summary>
        ///     ”множает Numerator на Number и делит pезультат на Denominator, окpугл€€ получаемое значение до длижайшего целого.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="numerator"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        private static int MulDiv(int number, int numerator, int denominator)
        {
            return (int) (((long) number*numerator)/denominator);
        }

        private enum Mode
        {
            BlinderSize,
            FilterStep
        };
    }
}