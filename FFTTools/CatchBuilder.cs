using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;
using FFTTools.Extensions;

namespace FFTTools
{
    /// <summary>
    ///     Catch pattern bitmap with the Fastest Fourier Transform
    /// </summary>
    public class CatchBuilder : BuilderBase, IBuilder
    {
        private readonly bool _fastMode;
        private readonly Array _patternData;

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="pattern">Pattern bitmap</param>
        /// <param name="fastMode">Do not calculate power. Do not divide to power.</param>
        public CatchBuilder(Bitmap pattern, bool fastMode = false)
        {
            _fastMode = fastMode;
            _patternData = pattern.ToImage<Gray, byte>().Convert<Gray, double>().Data;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        public Bitmap ToBitmap(Bitmap source)
        {
            var data = Catch(source.ToImage<Gray, byte>().Convert<Gray, double>().Data);
            var n0 = data.GetLength(0); // Image height
            var n1 = data.GetLength(1); // Image width
            var length = data.Length;
            var doubles = new double[length];
            Buffer.BlockCopy(data, 0, doubles, 0, length*sizeof (double));
            var max = doubles.Max();
            doubles = doubles.Select(x => Math.Round(255.0*x/max)).ToArray();
            using (var image = new Image<Gray, double>(n1, n0))
            {
                var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                Marshal.Copy(doubles, 0, handle.AddrOfPinnedObject(), doubles.Length);
                handle.Free();

                return image.Convert<Bgr, byte>().ToBitmap();
            }
        }

        /// <summary>
        ///     Copy 3D array to 2D array (sizes can be different)
        ///     Reduce last dimension
        /// </summary>
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        private static void Copy(double[,,] input, double[,] output)
        {
            var n0 = output.GetLength(0);
            var n1 = output.GetLength(1);
            var m0 = Math.Min(n0, input.GetLength(0));
            var m1 = Math.Min(n1, input.GetLength(1));
            var m2 = input.GetLength(2);

            var buffer = new double[m2];
            for (var i = 0; i < m0; i++)
                for (var j = 0; j < m1; j++)
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
            var n0 = output.GetLength(0);
            var n1 = output.GetLength(1);
            var m0 = Math.Min(n0, input.GetLength(0));
            var m1 = Math.Min(n1, input.GetLength(1));
            var m2 = input.GetLength(2);

            for (var i = 0; i < m0; i++)
                for (var j = 0; j < m1; j++)
                    output[i, j] = value;
        }

        /// <summary>
        ///     Find a maximum element in the matrix
        /// </summary>
        /// <param name="data"></param>
        /// <param name="x">Index of maximum element</param>
        /// <param name="y">Index of maximum element</param>
        /// <param name="value">Value of maximum element</param>
        public static void Max(double[,] data, out int x, out int y, out double value)
        {
            var n0 = data.GetLength(0); // Image height
            var n1 = data.GetLength(1); // Image width
            value = data[0, 0];
            x = y = 0;
            for (var i = 0; i < n0; i++)
            {
                for (var j = 0; j < n1; j++)
                {
                    var t = data[i, j];
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
        /// <returns>Matrix of values</returns>
        private double[,] Catch(Array array)
        {
            const double f = 1.0;
            var n0 = array.GetLength(0); // Image height
            var n1 = array.GetLength(1); // Image width
            var n2 = array.GetLength(2); // Image colors

            Debug.Assert(n2 == 1);

            var patternData = _patternData as double[,,];
            var imageData = array as double[,,];
            var data = new double[n0, n1];

            Array.Clear(data, 0, data.Length);
            var doubles = new double[data.Length];

            // Calculate Divisor
            Copy(patternData, data);
            Buffer.BlockCopy(data, 0, doubles, 0, data.Length*sizeof (double));
            var first = doubles.Select(x => new Complex(x, 0)).ToArray();
            Copy(imageData, data);
            Buffer.BlockCopy(data, 0, doubles, 0, data.Length*sizeof (double));
            var second = doubles.Select(x => new Complex(x, 0)).ToArray();

            Fourier(n0, n1, first, FourierDirection.Forward);
            Fourier(n0, n1, second, FourierDirection.Forward);

            first = first.Select(Complex.Conjugate).Zip(second,
                (x, y) => x*y).ToArray();

            Fourier(n0, n1, first, FourierDirection.Backward);
            var doubles1 = first.Select(x => x.Magnitude).ToArray();

            if (_fastMode)
            {
                // Fast Result
                Buffer.BlockCopy(doubles1, 0, data, 0, data.Length*sizeof (double));
                return data;
            }

            // Calculate Divider (aka Power)
            CopyAndReplace(patternData, data);
            Buffer.BlockCopy(data, 0, doubles, 0, data.Length*sizeof (double));
            first = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, first, FourierDirection.Forward);

            first = first.Select(Complex.Conjugate).Zip(second,
                (x, y) => x*y).Select(Complex.Conjugate).Zip(second,
                    (x, y) => x*y).ToArray();

            Fourier(n0, n1, first, FourierDirection.Backward);
            var doubles2 = first.Select(x => x.Magnitude).ToArray();

            // Result
            Buffer.BlockCopy(doubles1.Zip(doubles2, (x, y) => (f + x*x)/(f + y)).ToArray(), 0, data, 0,
                data.Length*sizeof (double));
            return data;
        }

        /// <summary>
        ///     Catch pattern bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Matrix of values</returns>
        public double[,] Catch<TColor, TDepth>(Image<TColor, TDepth> bitmap)
            where TColor : struct, IColor
            where TDepth : new() => Catch(bitmap.Convert<Gray, double>().Data);

        /// <summary>
        ///     Catch pattern bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Array of values</returns>
        public double[,] Catch(Bitmap bitmap) => Catch(bitmap.ToImage<Gray, byte>().Convert<Gray, double>().Data);
    }
}