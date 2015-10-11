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
    ////////////////////////////////
    /// <summary>
    /// </summary>
    public class DeblurBuilder : BuilderBase, IBuilder
    {
        private readonly FilterMode _filterMode; // builder mode
        private readonly double _filterPower; // filter power
        private readonly Size _filterSize; // filter size
        private readonly int _filterStep; // filter step
        private readonly KeepOption _keepOption; // energy save options

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="filterStep"></param>
        /// <param name="filterPower"></param>
        /// <param name="keepOption"></param>
        public DeblurBuilder(int filterStep = 1, double filterPower = 1,
            KeepOption keepOption = KeepOption.AverageAndDelta)
        {
            _filterMode = FilterMode.FilterStep;
            _filterStep = filterStep;
            _filterPower = filterPower;
            _keepOption = keepOption;
        }

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="filterSize"></param>
        /// <param name="filterPower"></param>
        /// <param name="keepOption"></param>
        public DeblurBuilder(Size filterSize, double filterPower = 1, KeepOption keepOption = KeepOption.AverageAndDelta)
        {
            _filterMode = FilterMode.FilterSize;
            _filterSize = filterSize;
            _filterPower = filterPower;
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
            using (var image = new Image<Bgr, double>(source))
            {
                var length = image.Data.Length;
                var n0 = image.Data.GetLength(0);
                var n1 = image.Data.GetLength(1);
                var n2 = image.Data.GetLength(2);
                var doubles = new double[length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, length*sizeof (double));

                var complex = doubles.Select(x => new Complex(x, 0)).ToArray();

                Fourier(n0, n1, n2, complex, FourierDirection.Forward);

                var array = complex.Select(x => x*Complex.Conjugate(x)).ToArray();
                array = array.Select(x => x/array[0]).ToArray();
                array = array.Select(Complex.Sqrt).ToArray();

                Debug.Assert(array.All(x => x.Magnitude <= 1));

                Fourier(n0, n1, n2, array, FourierDirection.Backward);
                doubles = array.Select(x => x.Magnitude).ToArray();

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
                BlindInner(n0, n1, doubles, filterSize, n2);

                doubles = array.Select(x => x.Magnitude/length).ToArray();
                var max = doubles.Max();
                doubles = doubles.Select(x => Math.Round(255.0*x/max)).ToArray();

                var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                Marshal.Copy(doubles, 0, handle.AddrOfPinnedObject(), doubles.Length);
                handle.Free();

                return image.Convert<Bgr, byte>().ToBitmap();
            }
        }

        /// <summary>
        ///     Deblur bitmap with the Fastest Fourier Transform
        /// </summary>
        private Array Deblur(Array data)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            var f = Complex.One;
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

            var array = complex.Select(x => x*Complex.Conjugate(x)).ToArray();
            array = array.Select(x => x/array[0]).ToArray();
            array = array.Select(Complex.Sqrt).ToArray();

            Debug.Assert(array.All(x => x.Magnitude <= 1));

            Fourier(n0, n1, n2, array, FourierDirection.Backward);
            doubles = array.Select(x => x.Magnitude/length).ToArray();

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
            BlindInner(n0, n1, doubles, filterSize, n2);

            array = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, n2, array, FourierDirection.Forward);

            array = array.Select(x => Complex.Pow(x*Complex.Conjugate(x), _filterPower/2)).ToArray();
            array = array.Select(x => f + x).ToArray();
            array = array.Select(Complex.Reciprocal).ToArray();

            var level = complex[0];
            complex = complex.Select(x => f + x).ToArray();
            complex = complex.Zip(array, (x, y) => (x*y)).ToArray();
            complex[0] = level;

            Fourier(n0, n1, n2, complex, FourierDirection.Backward);
            doubles = complex.Select(x => x.Magnitude/length).ToArray();

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
        ///     Deblur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Deblurred bitmap</returns>
        public Bitmap Deblur(Bitmap bitmap)
        {
            using (var image = new Image<Bgr, double>(bitmap))
            {
                image.Data = Deblur(image.Data) as double[,,];
                return image.ToBitmap();
            }
        }

        /// <summary>
        ///     Deblur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Deblurred bitmap</returns>
        public Image<TColor, TDepth> Deblur<TColor, TDepth>(Image<TColor, TDepth> bitmap)
            where TColor : struct, IColor
            where TDepth : new()
        {
            using (var image = bitmap.Convert<TColor, double>())
            {
                image.Data = Deblur(image.Data) as double[,,];
                return image.Convert<TColor, TDepth>();
            }
        }
    }
}