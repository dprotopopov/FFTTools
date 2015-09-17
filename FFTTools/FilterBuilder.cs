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
    ///     Filter bitmap with the Fastest Fourier Transform
    /// </summary>
    public class FilterBuilder : BuilderBase, IBuilder
    {
        private readonly Complex[,] _filterKernel; // filter kernel
        private readonly FilterMode _filterMode; // builder mode
        private readonly double _filterPower; // filter power
        private readonly Size _filterSize; // filter size
        private readonly int _filterStep; // filter step
        private readonly KeepOption _keepOption; // energy save options

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="filterStep">Bitmap filter step</param>
        /// <param name="filterPower"></param>
        /// <param name="keepOption"></param>
        public FilterBuilder(int filterStep, double filterPower = 1,
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
        /// <param name="filterKernel"></param>
        /// <param name="filterPower"></param>
        /// <param name="keepOption"></param>
        public FilterBuilder(Complex[,] filterKernel, double filterPower = 1,
            KeepOption keepOption = KeepOption.AverageAndDelta)
        {
            _filterMode = FilterMode.FilterKernel;
            _filterKernel = filterKernel;
            _filterPower = filterPower;
            _keepOption = keepOption;
        }

        /// <summary>
        ///     Builder constructor
        /// </summary>
        /// <param name="filterSize">Bitmap filter size</param>
        /// <param name="filterPower"></param>
        /// <param name="keepOption"></param>
        public FilterBuilder(Size filterSize, double filterPower = 1,
            KeepOption keepOption = KeepOption.AverageAndDelta)
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

        /// <summary>
        ///     Vizualize builder
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Bitmap ToBitmap(Bitmap source)
        {
            int width = source.Width;
            int height = source.Height;
            int length = width*height;

            var kernel = new Complex[height, width]; // Kernel values
            GetKernelFourier(kernel);
            Fourier(kernel, FourierDirection.Backward);

            var doubles = new double[kernel.Length];
            int index = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    doubles[index++] = 1 + kernel[i, j].Magnitude;

            double max = doubles.Max();
            doubles = doubles.Select(x => Math.Round(255.0*x/max)).ToArray();
            using (var image = new Image<Gray, double>(width, height))
            {
                Buffer.BlockCopy(doubles, 0, image.Data, 0, length*sizeof (double));
                return image.Convert<Bgr, Byte>().ToBitmap();
            }
        }

        /// <summary>
        ///     Copy 2D array to 2D array (sizes can be different)
        /// </summary>
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        private static void Copy(Complex[,] input, Complex[,] output)
        {
            int n0 = output.GetLength(0);
            int n1 = output.GetLength(1);
            int m0 = Math.Min(n0, input.GetLength(0));
            int m1 = Math.Min(n1, input.GetLength(1));

            for (int i = 0; i < m0; i++)
                for (int j = 0; j < m1; j++)
                    output[i, j] = input[i, j];
        }

        private void GetKernelFourier(Complex[,] kernelFourier)
        {
            int length = kernelFourier.Length; // Kernel length = height*width
            int n0 = kernelFourier.GetLength(0); // Kernel height
            int n1 = kernelFourier.GetLength(1); // Kernel width
            switch (_filterMode)
            {
                case FilterMode.FilterKernel:
                    Copy(_filterKernel, kernelFourier);
                    break;
                case FilterMode.FilterSize:
                    Fill(kernelFourier, _filterSize, Complex.One);
                    break;
                case FilterMode.FilterStep:
                    int filterStep = _filterStep;
                    var filterSize = new Size(MulDiv(n1, 1, filterStep + 1),
                        MulDiv(n0, 1, filterStep + 1));
                    Fill(kernelFourier, filterSize, Complex.One);
                    break;
                default:
                    throw new NotImplementedException();
            }
            Fourier(kernelFourier, FourierDirection.Forward);
            for (int i = 0; i < n0; i++)
                for (int j = 0; j < n1; j++)
                {
                    Complex value = kernelFourier[i, j];
                    value = Complex.Pow(value*Complex.Conjugate(value), _filterPower/2);
                    kernelFourier[i, j] = value;
                }
        }

        /// <summary>
        ///     Filter bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Blurred bitmap</returns>
        private double[,,] Apply(double[,,] imageData, FilterAction filterAction)
        {
            Complex f = Complex.One;
            int length = imageData.Length; // Image length = height*width*colors
            int n0 = imageData.GetLength(0); // Image height
            int n1 = imageData.GetLength(1); // Image width
            int n2 = imageData.GetLength(2); // Image colors

            var kernelFourier = new Complex[n0, n1]; // Filter values
            GetKernelFourier(kernelFourier);

            // Filter main loop

            var doubles = new double[length];
            Buffer.BlockCopy(imageData, 0, doubles, 0, length*sizeof (double));

            double average;
            double delta;
            AverageAndDelta(out average, out delta, doubles, _keepOption);

            Complex[] complex = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, n2, complex, FourierDirection.Forward);

            // Apply filter values
            int index = 0;
            switch (filterAction)
            {
                case FilterAction.Multiply:
                    for (int i = 0; i < n0; i++)
                        for (int j = 0; j < n1; j++)
                        {
                            Complex value = kernelFourier[i, j];
                            for (int k = 0; k < n2; k++)
                                complex[index++] *= f + value;
                        }
                    break;
                case FilterAction.Divide:
                    for (int i = 0; i < n0; i++)
                        for (int j = 0; j < n1; j++)
                        {
                            Complex value = kernelFourier[i, j];
                            for (int k = 0; k < n2; k++)
                                complex[index++] /= f + value;
                        }
                    break;
                default:
                    throw new NotImplementedException();
            }

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

        /// <summary>
        ///     Blur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Blurred bitmap</returns>
        public Image<Bgr, Byte> Blur(Image<Bgr, Byte> bitmap)
        {
            using (Image<Bgr, double> image = bitmap.Convert<Bgr, double>())
            {
                image.Data = Apply(image.Data, FilterAction.Multiply);
                return image.Convert<Bgr, Byte>();
            }
        }

        /// <summary>
        ///     Blur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Blurred bitmap</returns>
        public Image<Gray, Byte> Blur(Image<Gray, Byte> bitmap)
        {
            using (Image<Gray, double> image = bitmap.Convert<Gray, double>())
            {
                image.Data = Apply(image.Data, FilterAction.Multiply);
                return image.Convert<Gray, Byte>();
            }
        }

        /// <summary>
        ///     Blur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Blurred bitmap</returns>
        public Bitmap Blur(Bitmap bitmap)
        {
            using (var image = new Image<Bgr, double>(bitmap))
            {
                image.Data = Apply(image.Data, FilterAction.Multiply);
                return image.ToBitmap();
            }
        }

        /// <summary>
        ///     Fill region of array by value
        /// </summary>
        /// <param name="filter">Output array</param>
        /// <param name="size"></param>
        /// <param name="value">Value to replace copied data</param>
        private static void Fill(Complex[,] filter, Size size, Complex value)
        {
            int n0 = filter.GetLength(0);
            int n1 = filter.GetLength(1);
            int m0 = Math.Min(n0 - 1, size.Height);
            int m1 = Math.Min(n1 - 1, size.Width);

            Array.Clear(filter, 0, filter.Length);

            for (int i = 0; i <= m0; i++)
                for (int j = 0; j <= m1; j++)
                    filter[i, j] = value;
        }

        /// <summary>
        ///     Sharp bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Sharped bitmap</returns>
        public Image<Gray, byte> Sharp(Image<Gray, byte> bitmap)
        {
            using (Image<Gray, double> image = bitmap.Convert<Gray, double>())
            {
                image.Data = Apply(image.Data, FilterAction.Divide);
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
                image.Data = Apply(image.Data, FilterAction.Divide);
                return image.ToBitmap();
            }
        }

        /// <summary>
        ///     Sharp bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Sharped bitmap</returns>
        public Image<Bgr, byte> Sharp(Image<Bgr, byte> bitmap)
        {
            using (Image<Bgr, double> image = bitmap.Convert<Bgr, double>())
            {
                image.Data = Apply(image.Data, FilterAction.Divide);
                return image.Convert<Bgr, Byte>();
            }
        }
    }
}