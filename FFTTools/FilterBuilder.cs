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
            var width = source.Width;
            var height = source.Height;
            var length = width*height;

            var kernel = new Complex[height, width]; // Kernel values
            GetKernelFourier(kernel);
            Fourier(kernel, FourierDirection.Backward);

            var doubles = new double[kernel.Length];
            var index = 0;
            for (var i = 0; i < height; i++)
                for (var j = 0; j < width; j++)
                    doubles[index++] = 1 + kernel[i, j].Magnitude;

            var max = doubles.Max();
            doubles = doubles.Select(x => Math.Round(255.0*x/max)).ToArray();
            using (var image = new Image<Gray, double>(width, height))
            {
                Buffer.BlockCopy(doubles, 0, image.Data, 0, length*sizeof (double));
                return image.Convert<Bgr, byte>().ToBitmap();
            }
        }

        /// <summary>
        ///     Copy 2D array to 2D array (sizes can be different)
        /// </summary>
        /// <param name="input">Input array</param>
        /// <param name="output">Output array</param>
        private static void Copy(Complex[,] input, Complex[,] output)
        {
            var n0 = output.GetLength(0);
            var n1 = output.GetLength(1);
            var m0 = Math.Min(n0, input.GetLength(0));
            var m1 = Math.Min(n1, input.GetLength(1));

            for (var i = 0; i < m0; i++)
                for (var j = 0; j < m1; j++)
                    output[i, j] = input[i, j];
        }

        private void GetKernelFourier(Complex[,] kernelFourier)
        {
            var length = kernelFourier.Length; // Kernel length = height*width
            var n0 = kernelFourier.GetLength(0); // Kernel height
            var n1 = kernelFourier.GetLength(1); // Kernel width
            switch (_filterMode)
            {
                case FilterMode.FilterKernel:
                    Copy(_filterKernel, kernelFourier);
                    break;
                case FilterMode.FilterSize:
                    Fill(kernelFourier, _filterSize, Complex.One);
                    break;
                case FilterMode.FilterStep:
                    var filterStep = _filterStep;
                    var filterSize = new Size(MulDiv(n1, 1, filterStep + 1),
                        MulDiv(n0, 1, filterStep + 1));
                    Fill(kernelFourier, filterSize, Complex.One);
                    break;
                default:
                    throw new NotImplementedException();
            }
            Fourier(kernelFourier, FourierDirection.Forward);
            for (var i = 0; i < n0; i++)
                for (var j = 0; j < n1; j++)
                {
                    var value = kernelFourier[i, j];
                    value = Complex.Pow(value*Complex.Conjugate(value), _filterPower/2);
                    kernelFourier[i, j] = value;
                }
        }

        /// <summary>
        ///     Filter bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Blurred bitmap</returns>
        private Array Apply(Array data, FilterAction filterAction)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            var f = Complex.One;
            var length = data.Length; // Image length = height*width*colors
            var n0 = data.GetLength(0); // Image height
            var n1 = data.GetLength(1); // Image width
            var n2 = data.GetLength(2); // Image colors

            var kernelFourier = new Complex[n0, n1]; // Filter values
            GetKernelFourier(kernelFourier);

            // Filter main loop

            var doubles = new double[length];

            Marshal.Copy(handle.AddrOfPinnedObject(), doubles, 0, doubles.Length);

            double average;
            double delta;
            AverageAndDelta(out average, out delta, doubles, _keepOption);

            var complex = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, n2, complex, FourierDirection.Forward);

            // Apply filter values
            var index = 0;
            switch (filterAction)
            {
                case FilterAction.Multiply:
                    for (var i = 0; i < n0; i++)
                        for (var j = 0; j < n1; j++)
                        {
                            var value = kernelFourier[i, j];
                            for (var k = 0; k < n2; k++)
                                complex[index++] *= f + value;
                        }
                    break;
                case FilterAction.Divide:
                    for (var i = 0; i < n0; i++)
                        for (var j = 0; j < n1; j++)
                        {
                            var value = kernelFourier[i, j];
                            for (var k = 0; k < n2; k++)
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
            var a = (_keepOption == KeepOption.AverageAndDelta) ? (delta/delta2) : (average/average2);
            var b = (_keepOption == KeepOption.AverageAndDelta) ? (average - a*average2) : 0;
            Debug.Assert(Math.Abs(a*average2 + b - average) < 0.1);
            doubles = doubles.Select(x => Math.Round(a*x + b)).ToArray();

            Marshal.Copy(doubles, 0, handle.AddrOfPinnedObject(), doubles.Length);

            handle.Free();

            return data;
        }

        /// <summary>
        ///     Fill region of array by value
        /// </summary>
        /// <param name="filter">Output array</param>
        /// <param name="size"></param>
        /// <param name="value">Value to replace copied data</param>
        private static void Fill(Complex[,] filter, Size size, Complex value)
        {
            var n0 = filter.GetLength(0);
            var n1 = filter.GetLength(1);
            var m0 = Math.Min(n0 - 1, size.Height);
            var m1 = Math.Min(n1 - 1, size.Width);

            Array.Clear(filter, 0, filter.Length);

            for (var i = 0; i <= m0; i++)
                for (var j = 0; j <= m1; j++)
                    filter[i, j] = value;
        }

        /// <summary>
        ///     Blur bitmap with the Fastest Fourier Transform
        /// </summary>
        /// <returns>Blurred bitmap</returns>
        public Image<TColor, TDepth> Blur<TColor, TDepth>(Image<TColor, TDepth> bitmap)
            where TColor : struct, IColor
            where TDepth : new()
        {
            using (var image = bitmap.Convert<TColor, double>())
            {
                image.Data = Apply(image.Data, FilterAction.Multiply) as double[,,];
                return image.Convert<TColor, TDepth>();
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
                image.Data = Apply(image.Data, FilterAction.Multiply) as double[,,];
                return image.ToBitmap();
            }
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
                image.Data = Apply(image.Data, FilterAction.Divide) as double[,,];
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
                image.Data = Apply(image.Data, FilterAction.Divide) as double[,,];
                return image.ToBitmap();
            }
        }
    }
}