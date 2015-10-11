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
    ///     Sharp bitmap with the Fastest Fourier Transform
    /// </summary>
    public class SharpBuilder : BuilderBase, IBuilder
    {
        private readonly FilterMode _filterMode;
        private readonly Size _filterSize; // blinder size
        private readonly int _filterStep;
        private readonly KeepOption _keepOption;

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="filterStep"></param>
        /// <param name="keepOption"></param>
        public SharpBuilder(int filterStep, KeepOption keepOption = KeepOption.AverageAndDelta)
        {
            _filterMode = FilterMode.FilterStep;
            _filterStep = filterStep;
            _keepOption = keepOption;
        }

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="filterSize">Bitmap sharp blinder size</param>
        /// <param name="keepOption"></param>
        public SharpBuilder(Size filterSize, KeepOption keepOption = KeepOption.AverageAndDelta)
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

        /// <summary>
        ///     Vizualize builder
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Bitmap ToBitmap(Bitmap source)
        {
            using (var image = new Image<Gray, double>(source.Width, source.Height))
            {
                var length = image.Data.Length;
                var n0 = image.Data.GetLength(0);
                var n1 = image.Data.GetLength(1);
                var n2 = image.Data.GetLength(2);
                var doubles = Enumerable.Repeat(1.0, length).ToArray();
                var filterSize = _filterSize;
                switch (_filterMode)
                {
                    case FilterMode.FilterSize:
                        break;
                    case FilterMode.FilterStep:
                        var filterStep = _filterStep;
                        filterSize = new Size(MulDiv(n1, filterStep, filterStep + 1),
                            MulDiv(n0, filterStep, filterStep + 1));
                        break;
                    default:
                        throw new NotImplementedException();
                }
                BlindOuter(n0, n1, doubles, filterSize, n2);
                var max = doubles.Max();
                doubles = doubles.Select(x => Math.Round(255.0*x/max)).ToArray();

                var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                Marshal.Copy(doubles, 0, handle.AddrOfPinnedObject(), doubles.Length);
                handle.Free();

                return image.Convert<Bgr, byte>().ToBitmap();
            }
        }

        /// <summary>
        ///     Sharp bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Sharped bitmap</returns>
        private Array Sharp(Array data)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            var length = data.Length;
            var n0 = data.GetLength(0);
            var n1 = data.GetLength(1);
            var n2 = data.GetLength(2);

            var doubles = new double[length];

            Marshal.Copy(handle.AddrOfPinnedObject(), doubles, 0, doubles.Length);

            double average;
            double delta;
            AverageAndDelta(out average, out delta, doubles, _keepOption);

            var complex = doubles.Select(x => new Complex(x, 0)).ToArray();

            Fourier(n0, n1, n2, complex, FourierDirection.Forward);
            var level = complex[0];
            var filterSize = _filterSize;
            switch (_filterMode)
            {
                case FilterMode.FilterSize:
                    break;
                case FilterMode.FilterStep:
                    var filterStep = _filterStep;
                    filterSize = new Size(MulDiv(n1, filterStep, filterStep + 1),
                        MulDiv(n0, filterStep, filterStep + 1));
                    break;
                default:
                    throw new NotImplementedException();
            }
            BlindOuter(n0, n1, complex, filterSize, n2);
            complex[0] = level;
            Fourier(n0, n1, n2, complex, FourierDirection.Backward);
            doubles = complex.Select(x => x.Magnitude).ToArray();

            double average2;
            double delta2;
            AverageAndDelta(out average2, out delta2, doubles, _keepOption);

            // a*average2 + b == average
            // a*delta2 == delta
            var a = (_keepOption == KeepOption.AverageAndDelta) ? (delta/delta2) : (average/average2);
            var b = (_keepOption == KeepOption.AverageAndDelta) ? (average - a*average2) : 0;
            Debug.Assert(Math.Abs(a*average2 + b - average) < 0.1);
            doubles = doubles.Select(x => Math.Round(a*x + b)).ToArray();

            Marshal.Copy(doubles, 0, handle.AddrOfPinnedObject(), doubles.Length);

            handle.Free();

            return data;
        }

        /// <summary>
        ///     Sharp bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Sharped bitmap</returns>
        public Image<TColor, TDepth> Sharp<TColor, TDepth>(Image<TColor, TDepth> bitmap)
            where TColor : struct, IColor
            where TDepth : new()
        {
            using (var image = bitmap.Convert<TColor, double>())
            {
                image.Data = Sharp(image.Data) as double[,,];
                return image.Convert<TColor, TDepth>();
            }
        }

        /// <summary>
        ///     Sharp bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Sharped bitmap</returns>
        public Bitmap Sharp(Bitmap bitmap)
        {
            using (var image = new Image<Bgr, double>(bitmap))
            {
                image.Data = Sharp(image.Data) as double[,,];
                return image.ToBitmap();
            }
        }
    }
}