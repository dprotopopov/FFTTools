using System;
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

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="blinderSize">Bitmap blur blinder size</param>
        public BlurBuilder(Size blinderSize)
        {
            _blinderSize = blinderSize;
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
        public Image<Bgr, Byte> Blur(Image<Bgr, Byte> bitmap)
        {
            using (Image<Bgr, double> image = bitmap.Convert<Bgr, double>())
            {
                int length = image.Data.Length;
                int n0 = image.Data.GetLength(0);
                int n1 = image.Data.GetLength(1);
                int n2 = image.Data.GetLength(2);

                var doubles = new double[length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, length*sizeof (double));
                double power = Math.Sqrt(doubles.Average(x => x*x));

                var input = new fftw_complexarray(doubles.Select(x => new Complex(x, 0)).ToArray());
                var output = new fftw_complexarray(length);
                fftw_plan.dft_3d(n0, n1, n2, input, output,
                    fftw_direction.Forward,
                    fftw_flags.Estimate).Execute();
                Complex[] complex = output.GetData_Complex();

                var data = new Complex[n0, n1, n2];
                var buffer = new double[length*2];

                GCHandle complexHandle = GCHandle.Alloc(complex, GCHandleType.Pinned);
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr complexPtr = complexHandle.AddrOfPinnedObject();
                IntPtr dataPtr = dataHandle.AddrOfPinnedObject();

                Marshal.Copy(complexPtr, buffer, 0, buffer.Length);
                Marshal.Copy(buffer, 0, dataPtr, buffer.Length);
                Blind(data, _blinderSize);
                Marshal.Copy(dataPtr, buffer, 0, buffer.Length);
                Marshal.Copy(buffer, 0, complexPtr, buffer.Length);

                complexHandle.Free();
                dataHandle.Free();

                input.SetData(complex);

                fftw_plan.dft_3d(n0, n1, n2, input, output,
                    fftw_direction.Backward,
                    fftw_flags.Estimate).Execute();
                double[] array2 = output.GetData_Complex().Select(x => x.Magnitude).ToArray();
                double power2 = Math.Sqrt(array2.Average(x => x*x));
                doubles = array2.Select(x => x*power/power2).ToArray();
                Buffer.BlockCopy(doubles, 0, image.Data, 0, length*sizeof (double));
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
                Buffer.BlockCopy(image.Data, 0, doubles, 0, length*sizeof (double));
                double power = Math.Sqrt(doubles.Average(x => x*x));

                var input = new fftw_complexarray(doubles.Select(x => new Complex(x, 0)).ToArray());
                var output = new fftw_complexarray(length);
                fftw_plan.dft_3d(n0, n1, n2, input, output,
                    fftw_direction.Forward,
                    fftw_flags.Estimate).Execute();
                Complex[] complex = output.GetData_Complex();

                var data = new Complex[n0, n1, n2];
                var buffer = new double[length*2];

                GCHandle complexHandle = GCHandle.Alloc(complex, GCHandleType.Pinned);
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr complexPtr = complexHandle.AddrOfPinnedObject();
                IntPtr dataPtr = dataHandle.AddrOfPinnedObject();

                Marshal.Copy(complexPtr, buffer, 0, buffer.Length);
                Marshal.Copy(buffer, 0, dataPtr, buffer.Length);
                Blind(data, _blinderSize);
                Marshal.Copy(dataPtr, buffer, 0, buffer.Length);
                Marshal.Copy(buffer, 0, complexPtr, buffer.Length);

                complexHandle.Free();
                dataHandle.Free();

                input.SetData(complex);

                fftw_plan.dft_3d(n0, n1, n2, input, output,
                    fftw_direction.Backward,
                    fftw_flags.Estimate).Execute();
                double[] array2 = output.GetData_Complex().Select(x => x.Magnitude).ToArray();
                double power2 = Math.Sqrt(array2.Average(x => x*x));
                doubles = array2.Select(x => x*power/power2).ToArray();
                Buffer.BlockCopy(doubles, 0, image.Data, 0, length*sizeof (double));
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
                int length = image.Data.Length;
                int n0 = image.Data.GetLength(0);
                int n1 = image.Data.GetLength(1);
                int n2 = image.Data.GetLength(2);

                var doubles = new double[length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, length*sizeof (double));
                double power = Math.Sqrt(doubles.Average(x => x*x));

                var input = new fftw_complexarray(doubles.Select(x => new Complex(x, 0)).ToArray());
                var output = new fftw_complexarray(length);
                fftw_plan.dft_3d(n0, n1, n2, input, output,
                    fftw_direction.Forward,
                    fftw_flags.Estimate).Execute();
                Complex[] complex = output.GetData_Complex();

                var data = new Complex[n0, n1, n2];
                var buffer = new double[length*2];

                GCHandle complexHandle = GCHandle.Alloc(complex, GCHandleType.Pinned);
                GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr complexPtr = complexHandle.AddrOfPinnedObject();
                IntPtr dataPtr = dataHandle.AddrOfPinnedObject();

                Marshal.Copy(complexPtr, buffer, 0, buffer.Length);
                Marshal.Copy(buffer, 0, dataPtr, buffer.Length);
                Blind(data, _blinderSize);
                Marshal.Copy(dataPtr, buffer, 0, buffer.Length);
                Marshal.Copy(buffer, 0, complexPtr, buffer.Length);

                complexHandle.Free();
                dataHandle.Free();

                input.SetData(complex);

                fftw_plan.dft_3d(n0, n1, n2, input, output,
                    fftw_direction.Backward,
                    fftw_flags.Estimate).Execute();
                double[] array2 = output.GetData_Complex().Select(x => x.Magnitude).ToArray();
                double power2 = Math.Sqrt(array2.Average(x => x*x));
                doubles = array2.Select(x => x*power/power2).ToArray();
                Buffer.BlockCopy(doubles, 0, image.Data, 0, length*sizeof (double));
                return image.Bitmap;
            }
        }
    }
}