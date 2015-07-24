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
    ///     Resize bitmap with the Fastest Fourier Transform
    /// </summary>
    public class StretchBuilder : IDisposable
    {
        private readonly Size _size;

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="size">Bitmap new size</param>
        public StretchBuilder(Size size)
        {
            _size = size;
        }

        public void Dispose()
        {
        }

        /// <summary>
        ///     Copy arrays
        /// </summary>
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        private static void Copy(Complex[,,] input, ref Complex[,,] output)
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
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        private static void Copy(Complex[,,] input, ref Complex[] output)
        {
            int n0 = input.GetLength(0);
            int n1 = input.GetLength(1);
            int n2 = input.GetLength(2);
            int index = 0;
            for (int i = 0; i < n0; i++)
                for (int j = 0; j < n1; j++)
                    for (int k = 0; k < n2; k++)
                        output[index++] = input[i, j, k];
        }

        /// <summary>
        ///     Copy arrays
        /// </summary>
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        private static void Copy(Complex[] input, ref Complex[,,] output)
        {
            int n0 = output.GetLength(0);
            int n1 = output.GetLength(1);
            int n2 = output.GetLength(2);
            int index = 0;
            for (int i = 0; i < n0; i++)
                for (int j = 0; j < n1; j++)
                    for (int k = 0; k < n2; k++)
                        output[i, j, k] = input[index++];
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public Image<Bgr, Byte> Stretch(Image<Bgr, Byte> bitmap)
        {
            using (Image<Bgr, double> image = bitmap.Convert<Bgr, double>())
            {
                var doubles = new double[image.Data.Length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, Buffer.ByteLength(image.Data));
                double power = Math.Sqrt(doubles.Average(x => x*x));

                var input = new fftw_complexarray(doubles.Select(x => new Complex(x, 0)).ToArray());
                var output = new fftw_complexarray(doubles.Length);
                fftw_plan.dft_3d(image.Data.GetLength(0),
                    image.Data.GetLength(1),
                    image.Data.GetLength(2),
                    input,
                    output,
                    fftw_direction.Forward,
                    fftw_flags.Estimate).Execute();
                Complex[] complex = output.GetData_Complex();

                using (var image2 = new Image<Bgr, double>(_size))
                {
                    var complex2 = new Complex[image2.Data.Length];

                    var data =
                        new Complex[image.Data.GetLength(0),
                            image.Data.GetLength(1),
                            image.Data.GetLength(2)];
                    var data2 =
                        new Complex[image2.Data.GetLength(0),
                            image2.Data.GetLength(1),
                            image2.Data.GetLength(2)];

                    Copy(complex, ref data);
                    Copy(data, ref data2);
                    Copy(data2, ref complex2);

                    var input2 = new fftw_complexarray(complex2.ToArray());
                    var output2 = new fftw_complexarray(complex2.Length);
                    fftw_plan.dft_3d(image2.Data.GetLength(0),
                        image2.Data.GetLength(1),
                        image2.Data.GetLength(2),
                        input2,
                        output2,
                        fftw_direction.Backward,
                        fftw_flags.Estimate).Execute();
                    double[] array2 = output2.GetData_Complex().Select(x => x.Magnitude).ToArray();
                    double power2 = Math.Sqrt(array2.Average(x => x*x));
                    double[] doubles2 = array2.Select(x => x*power/power2).ToArray();
                    Buffer.BlockCopy(doubles2, 0, image2.Data, 0, Buffer.ByteLength(image2.Data));
                    return image2.Convert<Bgr, Byte>();
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
                var doubles = new double[image.Data.Length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, Buffer.ByteLength(image.Data));
                double power = Math.Sqrt(doubles.Average(x => x*x));

                var input = new fftw_complexarray(doubles.Select(x => new Complex(x, 0)).ToArray());
                var output = new fftw_complexarray(doubles.Length);
                fftw_plan.dft_3d(image.Data.GetLength(0),
                    image.Data.GetLength(1),
                    image.Data.GetLength(2),
                    input,
                    output,
                    fftw_direction.Forward,
                    fftw_flags.Estimate).Execute();
                Complex[] complex = output.GetData_Complex();

                using (var image2 = new Image<Bgr, double>(_size))
                {
                    var complex2 = new Complex[image2.Data.Length];

                    var data =
                        new Complex[image.Data.GetLength(0),
                            image.Data.GetLength(1),
                            image.Data.GetLength(2)];
                    var data2 =
                        new Complex[image2.Data.GetLength(0),
                            image2.Data.GetLength(1),
                            image2.Data.GetLength(2)];

                    Copy(complex, ref data);
                    Copy(data, ref data2);
                    Copy(data2, ref complex2);

                    var input2 = new fftw_complexarray(complex2.ToArray());
                    var output2 = new fftw_complexarray(complex2.Length);
                    fftw_plan.dft_3d(image2.Data.GetLength(0),
                        image2.Data.GetLength(1),
                        image2.Data.GetLength(2),
                        input2,
                        output2,
                        fftw_direction.Backward,
                        fftw_flags.Estimate).Execute();
                    double[] array2 = output2.GetData_Complex().Select(x => x.Magnitude).ToArray();
                    double power2 = Math.Sqrt(array2.Average(x => x*x));
                    double[] doubles2 = array2.Select(x => x*power/power2).ToArray();
                    Buffer.BlockCopy(doubles2, 0, image2.Data, 0, Buffer.ByteLength(image2.Data));
                    return image2.Bitmap;
                }
            }
        }
    }
}