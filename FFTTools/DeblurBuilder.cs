using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
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
                int length = image.Data.Length;
                int n0 = image.Data.GetLength(0);
                int n1 = image.Data.GetLength(1);
                int n2 = image.Data.GetLength(2);
                var doubles = new double[length];
                Buffer.BlockCopy(image.Data, 0, doubles, 0, length*sizeof (double));

                Complex[] complex = doubles.Select(x => new Complex(x, 0)).ToArray();

                Fourier(n0, n1, n2, complex, FourierDirection.Forward);

                Complex[] array = complex.Select(x => x*Complex.Conjugate(x)).ToArray();
                array = array.Select(x => x/array[0]).ToArray();
                array = array.Select(Complex.Sqrt).ToArray();

                Debug.Assert(array.All(x => x.Magnitude <= 1));

                Fourier(n0, n1, n2, array, FourierDirection.Backward);
                doubles = array.Select(x => x.Magnitude).ToArray();

                Size filterSize = _filterSize;
                switch (_filterMode)
                {
                    case FilterMode.FilterSize:
                        break;
                    case FilterMode.FilterStep:
                        int filterStep = _filterStep;
                        filterSize = new Size(MulDiv(n1, filterStep, filterStep + 1),
                            MulDiv(n0, filterStep, filterStep + 1));
                        break;
                    default:
                        throw new NotImplementedException();
                }
                BlindInner(n0, n1, doubles, filterSize, n2);

                doubles = array.Select(x => x.Magnitude/length).ToArray();
                double max = doubles.Max();
                doubles = doubles.Select(x => Math.Round(255.0*x/max)).ToArray();
                Buffer.BlockCopy(doubles, 0, image.Data, 0, length*sizeof (double));
                return image.Convert<Bgr, Byte>().ToBitmap();
            }
        }

        /// <summary>
        ///     Deblur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Deblurred bitmap</returns>
        public Bitmap Deblur(Bitmap bitmap)
        {
            using (var image = new Image<Bgr, double>(bitmap))
            {
                image.Data = Deblur(image.Data);
                return image.ToBitmap();
            }
        }

        /// <summary>
        ///     Deblur bitmap with the Fastest Fourier Transform
        /// </summary>
        private double[,,] Deblur(double[,,] imageData)
        {
            Complex f = Complex.One;
            int length = imageData.Length;
            int n0 = imageData.GetLength(0);
            int n1 = imageData.GetLength(1);
            int n2 = imageData.GetLength(2);
            var doubles = new double[length];
            Buffer.BlockCopy(imageData, 0, doubles, 0, length*sizeof (double));

            double average;
            double delta;
            AverageAndDelta(out average, out delta, doubles, _keepOption);

            Complex[] complex = doubles.Select(x => new Complex(x, 0)).ToArray();

            Fourier(n0, n1, n2, complex, FourierDirection.Forward);

            Complex[] array = complex.Select(x => x*Complex.Conjugate(x)).ToArray();
            array = array.Select(x => x/array[0]).ToArray();
            array = array.Select(Complex.Sqrt).ToArray();

            Debug.Assert(array.All(x => x.Magnitude <= 1));

            Fourier(n0, n1, n2, array, FourierDirection.Backward);
            doubles = array.Select(x => x.Magnitude/length).ToArray();

            Size filterSize = _filterSize;
            switch (_filterMode)
            {
                case FilterMode.FilterSize:
                    break;
                case FilterMode.FilterStep:
                    int filterStep = _filterStep;
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

            Complex level = complex[0];
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
            double a = (_keepOption == KeepOption.AverageAndDelta) ? (delta/delta2) : (average/average2);
            double b = (_keepOption == KeepOption.AverageAndDelta) ? (average - a*average2) : 0;
            Debug.Assert(Math.Abs(a*average2 + b - average) < 0.1);
            doubles = doubles.Select(x => Math.Round(a*x + b)).ToArray();

            Buffer.BlockCopy(doubles, 0, imageData, 0, length*sizeof (double));
            return imageData;
        }
    }
}