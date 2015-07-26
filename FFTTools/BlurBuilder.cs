using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
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
        private readonly Size _blenderSize; //blender size

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="blenderSize">Bitmap blur blender size</param>
        public BlurBuilder(Size blenderSize)
        {
            _blenderSize = blenderSize;
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
        ///     Blur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Blured bitmap</returns>
        public Image<Bgr, Byte> Blur(Image<Bgr, Byte> bitmap)
        {
            using (Image<Bgr, double> image = bitmap.Convert<Bgr, double>())
            {
                int length = image.Data.Length;
                int n0 = image.Data.GetLength(0);
                int n1 = image.Data.GetLength(1);
                int n2 = image.Data.GetLength(2);

                var doubles = new double[length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, Buffer.ByteLength(image.Data));
                double power = Math.Sqrt(doubles.Average(x => x*x));

                var input = new fftw_complexarray(doubles.Select(x => new Complex(x, 0)).ToArray());
                var output = new fftw_complexarray(length);
                fftw_plan.dft_3d(n0, n1, n2,
                    input,
                    output,
                    fftw_direction.Forward,
                    fftw_flags.Estimate).Execute();
                Complex[] complex = output.GetData_Complex();

                var data = new Complex[n0, n1, n2];

                Copy(complex, ref data);
                Blind(ref data, _blenderSize);
                Copy(data, ref complex);

                input.SetData(complex);

                fftw_plan.dft_3d(n0, n1, n2,
                    input,
                    output,
                    fftw_direction.Backward,
                    fftw_flags.Estimate).Execute();
                double[] array2 = output.GetData_Complex().Select(x => x.Magnitude).ToArray();
                double power2 = Math.Sqrt(array2.Average(x => x*x));
                double[] doubles2 = array2.Select(x => x*power/power2).ToArray();
                Buffer.BlockCopy(doubles2, 0, image.Data, 0, Buffer.ByteLength(image.Data));
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
                int length = image.Data.Length;
                int n0 = image.Data.GetLength(0);
                int n1 = image.Data.GetLength(1);
                int n2 = image.Data.GetLength(2);

                var doubles = new double[length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, Buffer.ByteLength(image.Data));
                double power = Math.Sqrt(doubles.Average(x => x*x));

                var input = new fftw_complexarray(doubles.Select(x => new Complex(x, 0)).ToArray());
                var output = new fftw_complexarray(length);
                fftw_plan.dft_3d(n0, n1, n2,
                    input,
                    output,
                    fftw_direction.Forward,
                    fftw_flags.Estimate).Execute();
                Complex[] complex = output.GetData_Complex();

                var data = new Complex[n0, n1, n2];

                Copy(complex, ref data);
                Blind(ref data, _blenderSize);
                Copy(data, ref complex);

                input.SetData(complex);

                fftw_plan.dft_3d(n0, n1, n2,
                    input,
                    output,
                    fftw_direction.Backward,
                    fftw_flags.Estimate).Execute();
                double[] array2 = output.GetData_Complex().Select(x => x.Magnitude).ToArray();
                double power2 = Math.Sqrt(array2.Average(x => x*x));
                double[] doubles2 = array2.Select(x => x*power/power2).ToArray();
                Buffer.BlockCopy(doubles2, 0, image.Data, 0, Buffer.ByteLength(image.Data));
                return image.Convert<Gray, Byte>();
            }
        }

        /// <summary>
        ///     Clear internal region of array
        /// </summary>
        /// <param name="data">Array of values</param>
        /// <param name="size">Internal blind region size</param>
        private static void Blind(ref Complex[,,] data, Size size)
        {
            int n0 = data.GetLength(0);
            int n1 = data.GetLength(1);
            int n2 = data.GetLength(2);
            int s0 = Math.Max(0, (n0 - size.Height) / 2);
            int s1 = Math.Max(0, (n1 - size.Width) / 2);
            int e0 = Math.Min((n0 + size.Height) / 2, n0 - 1);
            int e1 = Math.Min((n1 + size.Width) / 2, n1 - 1);
            for (int k = 0; k < n2; k++)
            {
                for (int i = 0; i < n0; i++)
                {
                    for (int j = s1; j <= e1; j++)
                    {
                        data[i, j, k] -= data[i, j, k];
                    }
                }
                for (int i = s0; i <= e0; i++)
                {
                    for (int j = 0; j < n1; j++)
                    {
                        data[i, j, k] -= data[i, j, k];
                    }
                }
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
                int length = image.Data.Length;
                int n0 = image.Data.GetLength(0);
                int n1 = image.Data.GetLength(1);
                int n2 = image.Data.GetLength(2);

                var doubles = new double[length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, Buffer.ByteLength(image.Data));
                double power = Math.Sqrt(doubles.Average(x => x*x));

                var input = new fftw_complexarray(doubles.Select(x => new Complex(x, 0)).ToArray());
                var output = new fftw_complexarray(length);
                fftw_plan.dft_3d(n0, n1, n2,
                    input,
                    output,
                    fftw_direction.Forward,
                    fftw_flags.Estimate).Execute();
                Complex[] complex = output.GetData_Complex();

                var data = new Complex[n0, n1, n2];

                Copy(complex, ref data);
                Blind(ref data, _blenderSize);
                Copy(data, ref complex);

                input.SetData(complex);

                fftw_plan.dft_3d(n0, n1, n2,
                    input,
                    output,
                    fftw_direction.Backward,
                    fftw_flags.Estimate).Execute();
                double[] array2 = output.GetData_Complex().Select(x => x.Magnitude).ToArray();
                double power2 = Math.Sqrt(array2.Average(x => x*x));
                double[] doubles2 = array2.Select(x => x*power/power2).ToArray();
                Buffer.BlockCopy(doubles2, 0, image.Data, 0, Buffer.ByteLength(image.Data));
                return image.Bitmap;
            }
        }
    }
}