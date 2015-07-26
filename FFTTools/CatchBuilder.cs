using System;
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
        private readonly Image<Gray, double> _patternImage;

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="patternBitmap">Pattern bitmap</param>
        public CatchBuilder(Bitmap patternBitmap)
        {
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
        public double[,] Catch(Bitmap bitmap)
        {
            using (var image = new Image<Gray, Byte>(bitmap))
                return Catch(image);
        }

        /// <summary>
        ///     Catch pattern bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Array of values</returns>
        public double[,] Catch(Image<Bgr, Byte> bitmap)
        {
            using (Image<Gray, Byte> image = bitmap.Convert<Gray, Byte>())
                return Catch(image);
        }

        /// <summary>
        ///     Catch pattern bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Array of values</returns>
        public double[,] Catch(Image<Gray, Byte> bitmap)
        {
            using (Image<Gray, double> image = bitmap.Convert<Gray, double>())
                return Catch(image);
        }

        /// <summary>
        ///     Catch pattern bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Array of values</returns>
        private double[,] Catch(Image<Gray, double> image)
        {
            int length = image.Data.Length;
            int byteLength = Buffer.ByteLength(image.Data);
            int n0 = image.Data.GetLength(0);
            int n1 = image.Data.GetLength(1);
            int n2 = image.Data.GetLength(2);

            Debug.Assert(n2 == 1);

            var input = new fftw_complexarray(length);
            var output = new fftw_complexarray(length);
            var doubles = new double[length];

            fftw_plan fftwPlan = fftw_plan.dft_3d(n0, n1, n2,
                input,
                output,
                fftw_direction.Forward,
                fftw_flags.Estimate);

            Buffer.BlockCopy(image.Data, 0, doubles, 0, byteLength);
            input.SetData(doubles.Select(x => new Complex(x, 0)).ToArray());
            fftwPlan.Execute();
            Complex[] complex1 = output.GetData_Complex();

            var data = new double[n0, n1];

            Copy(_patternImage.Data, ref data);
            Buffer.BlockCopy(data, 0, doubles, 0, byteLength);
            input.SetData(doubles.Select(x => new Complex(x, 0)).ToArray());
            fftwPlan.Execute();
            Complex[] complex2 = output.GetData_Complex();

            input.SetData(complex1.Zip(complex2, (x, y) => x*y).ToArray());
            fftw_plan.dft_3d(n0, n1, n2,
                input,
                output,
                fftw_direction.Backward,
                fftw_flags.Estimate)
                .Execute();

            doubles = output.GetData_Complex().Select(x => x.Magnitude).ToArray();
            Buffer.BlockCopy(doubles, 0, data, 0, byteLength);
            return data;
        }

        /// <summary>
        ///     Copy 3D array to 2D array
        ///     Reduce last dimension
        ///     Flip
        /// </summary>
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        private static void Copy(double[,,] input, ref double[,] output)
        {
            int n0 = input.GetLength(0);
            int n1 = input.GetLength(1);
            int n2 = input.GetLength(2);
            int m0 = Math.Min(n0, output.GetLength(0));
            int m1 = Math.Min(n1, output.GetLength(1));

            for (int i = 0; i < m0; i++)
                for (int j = 0; j < m1; j++)
                    output[(n0 - i)%n0, (n1 - j)%n1] = input[i, j, 0];

            for (int k = 1; k < n2; k++)
                for (int i = 0; i < m0; i++)
                    for (int j = 0; j < m1; j++)
                        output[(n0 - i)%n0, (n1 - j)%n1] += input[i, j, k];
        }

        /// <summary>
        ///     Find maximum element in array
        /// </summary>
        /// <param name="data">Array of values</param>
        /// <param name="x">Index of maximum element</param>
        /// <param name="y">Index of maximum element</param>
        /// <param name="value">Value of maximum element</param>
        public void Max(double[,] data, out int x, out int y, out double value)
        {
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
    }
}