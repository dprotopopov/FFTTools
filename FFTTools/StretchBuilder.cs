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
    ///     Resize bitmap with the Fastest Fourier Transform
    /// </summary>
    public class StretchBuilder : IDisposable
    {
        private readonly int _filterStep;
        private readonly KeepOption _keepOption;
        private readonly Mode _mode;
        private readonly Size _newSize;

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="filterStep"></param>
        /// <param name="keepOption"></param>
        public StretchBuilder(int filterStep, KeepOption keepOption = KeepOption.AverageAndDelta)
        {
            _mode = Mode.FilterStep;
            _filterStep = filterStep;
            _keepOption = keepOption;
        }

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="newSize">Bitmap new size</param>
        /// <param name="keepOption"></param>
        public StretchBuilder(Size newSize, KeepOption keepOption = KeepOption.AverageAndDelta)
        {
            _mode = Mode.NewSize;
            _newSize = newSize;
            _keepOption = keepOption;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///     Copy arrays
        /// </summary>
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        private static void Copy(Complex[,,] input, Complex[,,] output)
        {
            int n0 = input.GetLength(0);
            int n1 = input.GetLength(1);
            int n2 = input.GetLength(2);
            int m0 = output.GetLength(0);
            int m1 = output.GetLength(1);
            int m2 = output.GetLength(2);
            int ex0 = Math.Min(n0, m0)/2;
            int ex1 = Math.Min(n1, m1)/2;
            int ex2 = Math.Min(n2, m2);
            Debug.Assert(n2 == m2);
            for (int k = 0; k < ex2; k++)
            {
                for (int i = 0; i <= ex0; i++)
                {
                    for (int j = 0; j <= ex1; j++)
                    {
                        int ni = n0 - i - 1;
                        int nj = n1 - j - 1;
                        int mi = m0 - i - 1;
                        int mj = m1 - j - 1;
                        output[i, j, k] = input[i, j, k];
                        output[mi, j, k] = input[ni, j, k];
                        output[i, mj, k] = input[i, nj, k];
                        output[mi, mj, k] = input[ni, nj, k];
                    }
                }
            }
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public Image<Bgr, Byte> Stretch(Image<Bgr, Byte> bitmap)
        {
            using (Image<Bgr, double> image = bitmap.Convert<Bgr, double>())
            {
                int length = image.Data.Length;
                int n0 = image.Data.GetLength(0);
                int n1 = image.Data.GetLength(1);
                int n2 = image.Data.GetLength(2);
                var doubles = new double[length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, length*sizeof (double));
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

                var input = new fftw_complexarray(doubles.Select(x => new Complex(x, 0)).ToArray());
                var output = new fftw_complexarray(length);
                fftw_plan.dft_3d(n0, n1, n2, input, output,
                    fftw_direction.Forward,
                    fftw_flags.Estimate).Execute();
                Complex[] complex = output.GetData_Complex();

                Size newSize = _newSize;
                switch (_mode)
                {
                    case Mode.NewSize:
                        break;
                    case Mode.FilterStep:
                        int filterStep = _filterStep;
                        Size size = image.Size;
                        newSize = new Size(MulDiv(size.Width, filterStep + filterStep + 1, filterStep + filterStep),
                            MulDiv(size.Height, filterStep + filterStep + 1, filterStep + filterStep));
                        break;
                    default:
                        throw new NotImplementedException();
                }
                using (var image2 = new Image<Bgr, double>(newSize))
                {
                    int length2 = image2.Data.Length;
                    int m0 = image2.Data.GetLength(0);
                    int m1 = image2.Data.GetLength(1);
                    int m2 = image2.Data.GetLength(2);
                    var complex2 = new Complex[length2];

                    var data = new Complex[n0, n1, n2];
                    var data2 = new Complex[m0, m1, m2];

                    var buffer = new double[length*2];
                    GCHandle complexHandle = GCHandle.Alloc(complex, GCHandleType.Pinned);
                    GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    IntPtr complexPtr = complexHandle.AddrOfPinnedObject();
                    IntPtr dataPtr = dataHandle.AddrOfPinnedObject();

                    Marshal.Copy(complexPtr, buffer, 0, buffer.Length);
                    Marshal.Copy(buffer, 0, dataPtr, buffer.Length);

                    complexHandle.Free();
                    dataHandle.Free();

                    Copy(data, data2);

                    buffer = new double[length2*2];
                    complexHandle = GCHandle.Alloc(complex2, GCHandleType.Pinned);
                    dataHandle = GCHandle.Alloc(data2, GCHandleType.Pinned);
                    complexPtr = complexHandle.AddrOfPinnedObject();
                    dataPtr = dataHandle.AddrOfPinnedObject();

                    Marshal.Copy(dataPtr, buffer, 0, buffer.Length);
                    Marshal.Copy(buffer, 0, complexPtr, buffer.Length);

                    complexHandle.Free();
                    dataHandle.Free();

                    var input2 = new fftw_complexarray(complex2);
                    var output2 = new fftw_complexarray(length2);
                    fftw_plan.dft_3d(m0, m1, m2, input2, output2,
                        fftw_direction.Backward,
                        fftw_flags.Estimate).Execute();
                    doubles = output2.GetData_Complex().Select(x => x.Magnitude).ToArray();

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

                    Buffer.BlockCopy(doubles, 0, image2.Data, 0, length2*sizeof (double));
                    return image2.Convert<Bgr, Byte>();
                }
            }
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public Image<Gray, Byte> Stretch(Image<Gray, Byte> bitmap)
        {
            using (Image<Gray, double> image = bitmap.Convert<Gray, double>())
            {
                int length = image.Data.Length;
                int n0 = image.Data.GetLength(0);
                int n1 = image.Data.GetLength(1);
                int n2 = image.Data.GetLength(2);
                var doubles = new double[length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, length*sizeof (double));
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

                var input = new fftw_complexarray(doubles.Select(x => new Complex(x, 0)).ToArray());
                var output = new fftw_complexarray(length);
                fftw_plan.dft_3d(n0, n1, n2, input, output,
                    fftw_direction.Forward,
                    fftw_flags.Estimate).Execute();
                Complex[] complex = output.GetData_Complex();

                Size newSize = _newSize;
                switch (_mode)
                {
                    case Mode.NewSize:
                        break;
                    case Mode.FilterStep:
                        int filterStep = _filterStep;
                        Size size = image.Size;
                        newSize = new Size(MulDiv(size.Width, filterStep + filterStep + 1, filterStep + filterStep),
                            MulDiv(size.Height, filterStep + filterStep + 1, filterStep + filterStep));
                        break;
                    default:
                        throw new NotImplementedException();
                }
                using (var image2 = new Image<Bgr, double>(newSize))
                {
                    int length2 = image2.Data.Length;
                    int m0 = image2.Data.GetLength(0);
                    int m1 = image2.Data.GetLength(1);
                    int m2 = image2.Data.GetLength(2);
                    var complex2 = new Complex[length2];

                    var data = new Complex[n0, n1, n2];
                    var data2 = new Complex[m0, m1, m2];

                    var buffer = new double[length*2];
                    GCHandle complexHandle = GCHandle.Alloc(complex, GCHandleType.Pinned);
                    GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    IntPtr complexPtr = complexHandle.AddrOfPinnedObject();
                    IntPtr dataPtr = dataHandle.AddrOfPinnedObject();

                    Marshal.Copy(complexPtr, buffer, 0, buffer.Length);
                    Marshal.Copy(buffer, 0, dataPtr, buffer.Length);

                    complexHandle.Free();
                    dataHandle.Free();

                    Copy(data, data2);

                    buffer = new double[length2*2];
                    complexHandle = GCHandle.Alloc(complex2, GCHandleType.Pinned);
                    dataHandle = GCHandle.Alloc(data2, GCHandleType.Pinned);
                    complexPtr = complexHandle.AddrOfPinnedObject();
                    dataPtr = dataHandle.AddrOfPinnedObject();

                    Marshal.Copy(dataPtr, buffer, 0, buffer.Length);
                    Marshal.Copy(buffer, 0, complexPtr, buffer.Length);

                    complexHandle.Free();
                    dataHandle.Free();

                    var input2 = new fftw_complexarray(complex2);
                    var output2 = new fftw_complexarray(length2);
                    fftw_plan.dft_3d(m0, m1, m2, input2, output2,
                        fftw_direction.Backward,
                        fftw_flags.Estimate).Execute();
                    doubles = output2.GetData_Complex().Select(x => x.Magnitude).ToArray();

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

                    Buffer.BlockCopy(doubles, 0, image2.Data, 0, length2*sizeof (double));
                    return image2.Convert<Gray, Byte>();
                }
            }
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public Bitmap Stretch(Bitmap bitmap)
        {
            using (var image = new Image<Bgr, double>(bitmap))
            {
                int length = image.Data.Length;
                int n0 = image.Data.GetLength(0);
                int n1 = image.Data.GetLength(1);
                int n2 = image.Data.GetLength(2);
                var doubles = new double[length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, length*sizeof (double));
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

                var input = new fftw_complexarray(doubles.Select(x => new Complex(x, 0)).ToArray());
                var output = new fftw_complexarray(length);
                fftw_plan.dft_3d(n0, n1, n2, input, output,
                    fftw_direction.Forward,
                    fftw_flags.Estimate).Execute();
                Complex[] complex = output.GetData_Complex();

                Size newSize = _newSize;
                switch (_mode)
                {
                    case Mode.NewSize:
                        break;
                    case Mode.FilterStep:
                        int filterStep = _filterStep;
                        Size size = image.Size;
                        newSize = new Size(MulDiv(size.Width, filterStep + filterStep + 1, filterStep + filterStep),
                            MulDiv(size.Height, filterStep + filterStep + 1, filterStep + filterStep));
                        break;
                    default:
                        throw new NotImplementedException();
                }
                using (var image2 = new Image<Bgr, double>(newSize))
                {
                    int length2 = image2.Data.Length;
                    int m0 = image2.Data.GetLength(0);
                    int m1 = image2.Data.GetLength(1);
                    int m2 = image2.Data.GetLength(2);
                    var complex2 = new Complex[length2];

                    var data = new Complex[n0, n1, n2];
                    var data2 = new Complex[m0, m1, m2];

                    var buffer = new double[length*2];
                    GCHandle complexHandle = GCHandle.Alloc(complex, GCHandleType.Pinned);
                    GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    IntPtr complexPtr = complexHandle.AddrOfPinnedObject();
                    IntPtr dataPtr = dataHandle.AddrOfPinnedObject();

                    Marshal.Copy(complexPtr, buffer, 0, buffer.Length);
                    Marshal.Copy(buffer, 0, dataPtr, buffer.Length);

                    complexHandle.Free();
                    dataHandle.Free();

                    Copy(data, data2);

                    buffer = new double[length2*2];
                    complexHandle = GCHandle.Alloc(complex2, GCHandleType.Pinned);
                    dataHandle = GCHandle.Alloc(data2, GCHandleType.Pinned);
                    complexPtr = complexHandle.AddrOfPinnedObject();
                    dataPtr = dataHandle.AddrOfPinnedObject();

                    Marshal.Copy(dataPtr, buffer, 0, buffer.Length);
                    Marshal.Copy(buffer, 0, complexPtr, buffer.Length);

                    complexHandle.Free();
                    dataHandle.Free();

                    var input2 = new fftw_complexarray(complex2);
                    var output2 = new fftw_complexarray(length2);
                    fftw_plan.dft_3d(m0, m1, m2, input2, output2,
                        fftw_direction.Backward,
                        fftw_flags.Estimate).Execute();
                    doubles = output2.GetData_Complex().Select(x => x.Magnitude).ToArray();

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

                    Buffer.BlockCopy(doubles, 0, image2.Data, 0, length2*sizeof (double));
                    return image2.Bitmap;
                }
            }
        }

        /// <summary>
        ///     Умножает Numerator на Number и делит pезультат на Denominator, окpугляя получаемое значение до длижайшего целого.
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
            NewSize,
            FilterStep
        };
    }
}