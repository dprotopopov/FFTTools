using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Emgu.CV;
using Emgu.CV.Structure;
using FFTWSharp;

namespace FFTTools
{
    /// <summary>
    ///     Catch pattern bitmap with the Fastest Fourier Transform
    /// </summary>
    public class CatchBuilder : IDisposable
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
            int length = image.Data.Length;
            int n0 = image.Data.GetLength(0);
            int n1 = image.Data.GetLength(1);
            int n2 = image.Data.GetLength(2);

            Debug.Assert(n2 == 1);

            // Allocate FFTW structures
            var input = new fftw_complexarray(length);
            var output = new fftw_complexarray(length);

            fftw_plan forward = fftw_plan.dft_3d(n0, n1, n2, input, output,
                fftw_direction.Forward,
                fftw_flags.Estimate);
            fftw_plan backward = fftw_plan.dft_3d(n0, n1, n2, input, output,
                fftw_direction.Backward,
                fftw_flags.Estimate);

            var matrix = new Matrix<double>(n0, n1);

            double[,,] patternData = _patternImage.Data;
            double[,,] imageData = image.Data;
            double[,] data = matrix.Data;

            var doubles = new double[length];

            // Calculate Divisor
            Copy(patternData, data);
            Buffer.BlockCopy(data, 0, doubles, 0, length*sizeof (double));
            input.SetData(doubles.Select(x => new Complex(x, 0)).ToArray());
            forward.Execute();
            Complex[] complex = output.GetData_Complex();

            Buffer.BlockCopy(imageData, 0, doubles, 0, length*sizeof (double));
            input.SetData(doubles.Select(x => new Complex(x, 0)).ToArray());
            forward.Execute();

            input.SetData(output.GetData_Complex().Zip(complex, (x, y) => x*Complex.Conjugate(y)).ToArray());
            backward.Execute();
            IEnumerable<double> doubles1 = output.GetData_Complex().Select(x => x.Magnitude);

            if (_fastMode)
            {
                // Fast Result
                Buffer.BlockCopy(doubles1.ToArray(), 0, data, 0, length*sizeof (double));
                return matrix;
            }

            // Calculate Divider (aka Power)
            input.SetData(doubles.Select(x => new Complex(x*x, 0)).ToArray());
            forward.Execute();
            complex = output.GetData_Complex();

            CopyAndReplace(_patternImage.Data, data);
            Buffer.BlockCopy(data, 0, doubles, 0, length*sizeof (double));
            input.SetData(doubles.Select(x => new Complex(x, 0)).ToArray());
            forward.Execute();

            input.SetData(complex.Zip(output.GetData_Complex(), (x, y) => x*Complex.Conjugate(y)).ToArray());
            backward.Execute();
            IEnumerable<double> doubles2 = output.GetData_Complex().Select(x => x.Magnitude);

            // Result
            Buffer.BlockCopy(doubles1.Zip(doubles2, (x, y) => (f + x*x)/(f + y)).ToArray(), 0, data, 0,
                length*sizeof (double));
            return matrix;
        }

        /// <summary>
        ///     Copy 3D array to 2D array (sizes can be different)
        ///     Flip copied data
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

            for (int i = 0; i < m0; i++)
                for (int j = 0; j < m1; j++)
                    output[i, j] = input[i, j, 0];

            for (int k = 1; k < m2; k++)
                for (int i = 0; i < m0; i++)
                    for (int j = 0; j < m1; j++)
                        output[i, j] += input[i, j, k];
        }

        /// <summary>
        ///     Copy 3D array to 2D array (sizes can be different)
        ///     Replace items copied by value
        ///     Flip copied data
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
            int n0 = data.GetLength(0);
            int n1 = data.GetLength(1);
            value = data[0, 0];
            x = y = 0;
            for (int i = 0; i < n0; i++)
            {
                for (int j = 0; j < n1; j++)
                {
                    if (data[i, j] < value) continue;
                    value = data[i, j];
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