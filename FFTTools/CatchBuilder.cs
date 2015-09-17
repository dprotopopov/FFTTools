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
    ///     Catch pattern bitmap with the Fastest Fourier Transform
    /// </summary>
    public class CatchBuilder : BuilderBase, IBuilder
    {
        private readonly bool _fastMode;
        private readonly Image<Gray, double> _patternImage;

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="patternBitmap">Pattern bitmap</param>
        /// <param name="fastMode">Do not calculate power. Do not divide to power.</param>
        public CatchBuilder(Bitmap patternBitmap, bool fastMode = false)
        {
            _fastMode = fastMode;
            _patternImage =
                new Image<Gray, Byte>(patternBitmap)
                    .Convert<Gray, double>();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        public Bitmap ToBitmap(Bitmap source)
        {
            Matrix<double> matrix = Catch(source);
            int n0 = matrix.Data.GetLength(0); // Image height
            int n1 = matrix.Data.GetLength(1); // Image width
            int length = matrix.Data.Length;
            var doubles = new double[length];
            Buffer.BlockCopy(matrix.Data, 0, doubles, 0, length*sizeof (double));
            double max = doubles.Max();
            doubles = doubles.Select(x => Math.Round(255.0*x/max)).ToArray();
            using (var image = new Image<Gray, double>(n1, n0))
            {
                Buffer.BlockCopy(doubles, 0, image.Data, 0, length*sizeof (double));
                return image.Convert<Bgr, Byte>().ToBitmap();
            }
        }

        /// <summary>
        ///     Catch pattern bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Array of values</returns>
        public Matrix<double> Catch(Image<Bgr, Byte> bitmap)
        {
            using (Image<Gray, Byte> image = bitmap.Convert<Gray, Byte>())
                return Catch(image);
        }

        /// <summary>
        ///     Catch pattern bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Matrix of values</returns>
        public Matrix<double> Catch(Image<Gray, Byte> bitmap)
        {
            using (Image<Gray, double> image = bitmap.Convert<Gray, double>())
                return Catch(image);
        }

        /// <summary>
        ///     Catch pattern bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Matrix of values</returns>
        private Matrix<double> Catch(Image<Gray, double> image)
        {
            const double f = 1.0;
            int length = image.Data.Length; // Image length = height*width*colors
            int n0 = image.Data.GetLength(0); // Image height
            int n1 = image.Data.GetLength(1); // Image width
            int n2 = image.Data.GetLength(2); // Image colors

            Debug.Assert(n2 == 1);

            var matrix = new Matrix<double>(n0, n1);

            double[,,] patternData = _patternImage.Data;
            double[,,] imageData = image.Data;
            double[,] data = matrix.Data;

            Array.Clear(data, 0, data.Length);
            var doubles = new double[length];

            // Calculate Divisor
            Copy(patternData, data);
            Buffer.BlockCopy(data, 0, doubles, 0, length*sizeof (double));
            Complex[] first = doubles.Select(x => new Complex(x, 0)).ToArray();
            Buffer.BlockCopy(imageData, 0, doubles, 0, length*sizeof (double));
            Complex[] second = doubles.Select(x => new Complex(x, 0)).ToArray();

            Fourier(n0, n1, n2, first, FourierDirection.Forward);
            Fourier(n0, n1, n2, second, FourierDirection.Forward);

            first = first.Select(Complex.Conjugate).Zip(second,
                (x, y) => x*y).ToArray();

            Fourier(n0, n1, n2, first, FourierDirection.Backward);
            double[] doubles1 = first.Select(x => x.Magnitude).ToArray();

            if (_fastMode)
            {
                // Fast Result
                Buffer.BlockCopy(doubles1, 0, data, 0, length*sizeof (double));
                return matrix;
            }

            // Calculate Divider (aka Power)
            CopyAndReplace(_patternImage.Data, data);
            Buffer.BlockCopy(data, 0, doubles, 0, length*sizeof (double));
            first = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, n2, first, FourierDirection.Forward);

            first = first.Select(Complex.Conjugate).Zip(second,
                (x, y) => x*y).Select(Complex.Conjugate).Zip(second,
                    (x, y) => x*y).ToArray();

            Fourier(n0, n1, n2, first, FourierDirection.Backward);
            double[] doubles2 = first.Select(x => x.Magnitude).ToArray();

            // Result
            Buffer.BlockCopy(doubles1.Zip(doubles2, (x, y) => (f + x*x)/(f + y)).ToArray(), 0, data, 0,
                length*sizeof (double));
            return matrix;
        }

        /// <summary>
        ///     Copy 3D array to 2D array (sizes can be different)
        ///     Reduce last dimension
        /// </summary>
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        private static void Copy(double[,,] input, double[,] output)
        {
            int n0 = output.GetLength(0);
            int n1 = output.GetLength(1);
            int m0 = Math.Min(n0, input.GetLength(0));
            int m1 = Math.Min(n1, input.GetLength(1));
            int m2 = input.GetLength(2);

            var buffer = new double[m2];
            for (int i = 0; i < m0; i++)
                for (int j = 0; j < m1; j++)
                {
                    Buffer.BlockCopy(input, (i*m1 + j)*m2*sizeof (double), buffer, 0, m2*sizeof (double));
                    output[i, j] = buffer.Sum();
                }
        }

        /// <summary>
        ///     Copy 3D array to 2D array (sizes can be different)
        ///     Replace items copied by value
        ///     Reduce last dimension
        /// </summary>
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        /// <param name="value">Value to replace copied data</param>
        private static void CopyAndReplace(double[,,] input, double[,] output, double value = 1.0)
        {
            int n0 = output.GetLength(0);
            int n1 = output.GetLength(1);
            int m0 = Math.Min(n0, input.GetLength(0));
            int m1 = Math.Min(n1, input.GetLength(1));
            int m2 = input.GetLength(2);

            for (int i = 0; i < m0; i++)
                for (int j = 0; j < m1; j++)
                    output[i, j] = value;
        }

        /// <summary>
        ///     Find a maximum element in the matrix
        /// </summary>
        /// <param name="matrix">Matrix of values</param>
        /// <param name="x">Index of maximum element</param>
        /// <param name="y">Index of maximum element</param>
        /// <param name="value">Value of maximum element</param>
        public void Max(Matrix<double> matrix, out int x, out int y, out double value)
        {
            double[,] data = matrix.Data;
            int n0 = data.GetLength(0); // Image height
            int n1 = data.GetLength(1); // Image width
            value = data[0, 0];
            x = y = 0;
            for (int i = 0; i < n0; i++)
            {
                for (int j = 0; j < n1; j++)
                {
                    double t = data[i, j];
                    if (t < value) continue;
                    value = t;
                    x = j;
                    y = i;
                }
            }
        }

        /// <summary>
        ///     Catch pattern bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Array of values</returns>
        public Matrix<double> Catch(Bitmap bitmap)
        {
            using (var image = new Image<Gray, Byte>(bitmap))
                return Catch(image);
        }
    }
}