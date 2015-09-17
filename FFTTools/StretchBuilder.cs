using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Emgu.CV;
using Emgu.CV.Structure;

namespace FFTTools
{
    /// <summary>
    ///     Resize bitmap with the Fastest Fourier Transform
    /// </summary>
    public class StretchBuilder : BuilderBase, IDisposable
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
        ///     Copy arrays
        /// </summary>
        /// <param name="n0">Source array size</param>
        /// <param name="n1">Source array size</param>
        /// <param name="m0">Destination array size</param>
        /// <param name="m1">Destination array size</param>
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        /// <param name="nm2">Source/Destination array size</param>
        private static void Copy(int n0, int n1, int m0, int m1, Complex[] input, Complex[] output, int nm2)
        {
            int ex0 = Math.Min(n0, m0)/2;
            int ex1 = Math.Min(n1, m1)/2;
            for (int i = 0; i <= ex0; i++)
            {
                for (int j = 0; j <= ex1; j++)
                {
                    int ni = n0 - i - 1;
                    int nj = n1 - j - 1;
                    int mi = m0 - i - 1;
                    int mj = m1 - j - 1;
                    Array.Copy(input, (i*n1 + j)*nm2, output, (i*m1 + j)*nm2, nm2);
                    Array.Copy(input, (ni*n1 + j)*nm2, output, (mi*m1 + j)*nm2, nm2);
                    Array.Copy(input, (i*n1 + nj)*nm2, output, (i*m1 + mj)*nm2, nm2);
                    Array.Copy(input, (ni*n1 + nj)*nm2, output, (mi*m1 + mj)*nm2, nm2);
                }
            }
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public double[,,] Stretch(double[,,] imageData)
        {
            int length = imageData.Length;
            int n0 = imageData.GetLength(0);
            int n1 = imageData.GetLength(1);
            int n2 = imageData.GetLength(2);

            var doubles = new double[length];
            Buffer.BlockCopy(imageData, 0, doubles, 0, length*sizeof (double));

            double average;
            double delta;
            AverageAndDelta(out average, out delta, doubles, _keepOption);

            Size newSize = _newSize;
            switch (_mode)
            {
                case Mode.NewSize:
                    break;
                case Mode.FilterStep:
                    int filterStep = _filterStep;
                    newSize = new Size(MulDiv(n1, filterStep + filterStep + 1, filterStep + filterStep),
                        MulDiv(n0, filterStep + filterStep + 1, filterStep + filterStep));
                    break;
                default:
                    throw new NotImplementedException();
            }

            var imageData2 = new double[newSize.Height, newSize.Width, n2];
            int length2 = imageData2.Length;
            int m0 = imageData2.GetLength(0);
            int m1 = imageData2.GetLength(1);
            int m2 = imageData2.GetLength(2);

            Complex[] complex = doubles.Select(x => new Complex(x, 0)).ToArray();
            var complex2 = new Complex[length2];
            Fourier(n0, n1, n2, complex, FourierDirection.Forward);
            Copy(n0, n1, m0, m1, complex, complex2, n2);
            Fourier(m0, m1, m2, complex2, FourierDirection.Backward);
            doubles = complex2.Select(x => x.Magnitude).ToArray();

            double average2;
            double delta2;
            AverageAndDelta(out average2, out delta2, doubles, _keepOption);

            // a*average2 + b == average
            // a*delta2 == delta
            double a = (_keepOption == KeepOption.AverageAndDelta) ? (delta/delta2) : (average/average2);
            double b = (_keepOption == KeepOption.AverageAndDelta) ? (average - a*average2) : 0;
            Debug.Assert(Math.Abs(a*average2 + b - average) < 0.1);
            doubles = doubles.Select(x => Math.Round(a*x + b)).ToArray();

            Buffer.BlockCopy(doubles, 0, imageData2, 0, length2*sizeof (double));
            return imageData2;
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public Image<Bgr, Byte> Stretch(Image<Bgr, Byte> bitmap)
        {
            using (Image<Bgr, double> image = bitmap.Convert<Bgr, double>())
            using (var image2 = new Image<Bgr, double>(Stretch(image.Data)))
                return image2.Convert<Bgr, Byte>();
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public Image<Gray, Byte> Stretch(Image<Gray, Byte> bitmap)
        {
            using (Image<Gray, double> image = bitmap.Convert<Gray, double>())
            using (var image2 = new Image<Gray, double>(Stretch(image.Data)))
                return image2.Convert<Gray, Byte>();
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public Bitmap Stretch(Bitmap bitmap)
        {
            using (var image = new Image<Bgr, double>(bitmap))
            using (var image2 = new Image<Bgr, double>(Stretch(image.Data)))
                return image2.Bitmap;
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