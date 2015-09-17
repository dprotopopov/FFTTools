using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
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
                int length = image.Data.Length;
                int n0 = image.Data.GetLength(0);
                int n1 = image.Data.GetLength(1);
                int n2 = image.Data.GetLength(2);
                double[] doubles = Enumerable.Repeat(1.0, length).ToArray();
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
                BlindOuter(n0, n1, doubles, filterSize, n2);
                double max = doubles.Max();
                doubles = doubles.Select(x => Math.Round(255.0*x/max)).ToArray();
                Buffer.BlockCopy(doubles, 0, image.Data, 0, length*sizeof (double));
                return image.Convert<Bgr, Byte>().ToBitmap();
            }
        }

        /// <summary>
        ///     Sharp bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Sharped bitmap</returns>
        public Image<Bgr, Byte> Sharp(Image<Bgr, Byte> bitmap)
        {
            using (Image<Bgr, double> image = bitmap.Convert<Bgr, double>())
            {
                image.Data = Sharp(image.Data);
                return image.Convert<Bgr, Byte>();
            }
        }

        /// <summary>
        ///     Sharp bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Sharped bitmap</returns>
        private double[,,] Sharp(double[,,] imageData)
        {
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
            Complex level = complex[0];
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
            BlindOuter(n0, n1, complex, filterSize, n2);
            complex[0] = level;
            Fourier(n0, n1, n2, complex, FourierDirection.Backward);
            doubles = complex.Select(x => x.Magnitude).ToArray();

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

        /// <summary>
        ///     Sharp bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Sharped bitmap</returns>
        public Image<Gray, Byte> Sharp(Image<Gray, Byte> bitmap)
        {
            using (Image<Gray, double> image = bitmap.Convert<Gray, double>())
            {
                image.Data = Sharp(image.Data);
                return image.Convert<Gray, Byte>();
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
                image.Data = Sharp(image.Data);
                return image.ToBitmap();
            }
        }
    }
}