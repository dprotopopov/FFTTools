using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;

namespace FFTTools
{
    /// <summary>
    ///     Resize bitmap with the Fastest Fourier Transform
    /// </summary>
    public class StretchBuilder : BuilderBase, IBuilder
    {
        private readonly FilterMode _filterMode;
        private readonly Size _filterSize;
        private readonly int _filterStep;
        private readonly KeepOption _keepOption;

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="filterStep"></param>
        /// <param name="keepOption"></param>
        public StretchBuilder(int filterStep, KeepOption keepOption = KeepOption.AverageAndDelta)
        {
            _filterMode = FilterMode.FilterStep;
            _filterStep = filterStep;
            _keepOption = keepOption;
        }

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="filterSize">Bitmap new size</param>
        /// <param name="keepOption"></param>
        public StretchBuilder(Size filterSize, KeepOption keepOption = KeepOption.AverageAndDelta)
        {
            _filterMode = FilterMode.FilterSize;
            _filterSize = filterSize;
            _keepOption = keepOption;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        public Bitmap ToBitmap(Bitmap source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Copy arrays
        /// </summary>
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        private static void Copy(Complex[,,] input, Complex[,,] output)
        {
            var n0 = input.GetLength(0);
            var n1 = input.GetLength(1);
            var n2 = input.GetLength(2);
            var m0 = output.GetLength(0);
            var m1 = output.GetLength(1);
            var m2 = output.GetLength(2);
            var ex0 = Math.Min(n0, m0)/2;
            var ex1 = Math.Min(n1, m1)/2;
            var ex2 = Math.Min(n2, m2);
            Debug.Assert(n2 == m2);
            for (var k = 0; k < ex2; k++)
            {
                for (var i = 0; i <= ex0; i++)
                {
                    for (var j = 0; j <= ex1; j++)
                    {
                        var ni = n0 - i - 1;
                        var nj = n1 - j - 1;
                        var mi = m0 - i - 1;
                        var mj = m1 - j - 1;
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
            var ex0 = Math.Min(n0, m0)/2;
            var ex1 = Math.Min(n1, m1)/2;
            var nj = n1 - ex1;
            var mj = m1 - ex1;
            for (var i = 0; i <= ex0; i++)
            {
                var ni = n0 - i - 1;
                var mi = m0 - i - 1;
                Array.Copy(input, i*n1*nm2, output, i*m1*nm2, ex1*nm2);
                Array.Copy(input, ni*n1*nm2, output, mi*m1*nm2, ex1*nm2);
                Array.Copy(input, (i*n1 + nj)*nm2, output, (i*m1 + mj)*nm2, ex1*nm2);
                Array.Copy(input, (ni*n1 + nj)*nm2, output, (mi*m1 + mj)*nm2, ex1*nm2);
            }
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public Array Stretch(Array imageData)
        {
            var length = imageData.Length;
            var n0 = imageData.GetLength(0);
            var n1 = imageData.GetLength(1);
            var n2 = imageData.GetLength(2);

            var doubles = new double[length];

            var handle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
            Marshal.Copy(handle.AddrOfPinnedObject(), doubles, 0, doubles.Length);
            handle.Free();

            double average;
            double delta;
            AverageAndDelta(out average, out delta, doubles, _keepOption);

            var newSize = _filterSize;
            switch (_filterMode)
            {
                case FilterMode.FilterSize:
                    break;
                case FilterMode.FilterStep:
                    var filterStep = _filterStep;
                    newSize = new Size(MulDiv(n1, filterStep + filterStep + 1, filterStep + filterStep),
                        MulDiv(n0, filterStep + filterStep + 1, filterStep + filterStep));
                    break;
                default:
                    throw new NotImplementedException();
            }

            var imageData2 = new double[newSize.Height, newSize.Width, n2];
            var length2 = imageData2.Length;
            var m0 = imageData2.GetLength(0);
            var m1 = imageData2.GetLength(1);
            var m2 = imageData2.GetLength(2);

            var complex = doubles.Select(x => new Complex(x, 0)).ToArray();
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
            var a = (_keepOption == KeepOption.AverageAndDelta) ? (delta/delta2) : (average/average2);
            var b = (_keepOption == KeepOption.AverageAndDelta) ? (average - a*average2) : 0;
            Debug.Assert(Math.Abs(a*average2 + b - average) < 0.1);
            doubles = doubles.Select(x => Math.Round(a*x + b)).ToArray();


            handle = GCHandle.Alloc(imageData2, GCHandleType.Pinned);
            Marshal.Copy(doubles, 0, handle.AddrOfPinnedObject(), doubles.Length);
            handle.Free();

            return imageData2;
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public Image<Bgr, byte> Stretch(Image<Bgr, byte> bitmap)
        {
            using (var image = bitmap.Convert<Bgr, double>())
            using (var image2 = new Image<Bgr, double>(Stretch(image.Data) as double[,,]))
                return image2.Convert<Bgr, byte>();
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public Image<Gray, byte> Stretch(Image<Gray, byte> bitmap)
        {
            using (var image = bitmap.Convert<Gray, double>())
            using (var image2 = new Image<Gray, double>(Stretch(image.Data) as double[,,]))
                return image2.Convert<Gray, byte>();
        }

        /// <summary>
        ///     Resize bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Resized bitmap</returns>
        public Bitmap Stretch(Bitmap bitmap)
        {
            using (var image = new Image<Bgr, double>(bitmap))
            using (var image2 = new Image<Bgr, double>(Stretch(image.Data) as double[,,]))
                return image2.Convert<Bgr, byte>().ToBitmap();
        }
    }
}