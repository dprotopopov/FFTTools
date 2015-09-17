using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace FFTTools
{
    /// <summary>
    ///     The convolution of f and g is written fxg, using an asterisk or star. It is defined as the integral of the product
    ///     of the two functions after one is reversed and shifted.
    ///     FT(fxg) = FT(f)*FT(g)
    ///     FT(a*f+b*g) = a*FT(f)+b*FT(g)
    /// </summary>
    public class ConvolutionBuilder : BuilderBase, IBuilder
    {
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
        ///     Calculate function self-convolution values
        /// </summary>
        /// <param name="f">Input values</param>
        /// <param name="fxf">Output values</param>
        public static void Convolution(double[] f, double[] fxf)
        {
            int length = fxf.Length;
            double[] doubles = f.Concat(Enumerable.Repeat(0.0, length - f.Length)).ToArray();
            Complex[] complex = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(complex, FourierDirection.Forward);
            complex = complex.Select(x => x*x/length).ToArray();
            Fourier(complex, FourierDirection.Backward);
            doubles = complex.Select(x => x.Real).ToArray();
            Array.Copy(doubles, 0, fxf, 0, length);
        }

        /// <summary>
        ///     Calculate function self-convolution values
        /// </summary>
        /// <param name="f">Input values</param>
        /// <param name="fxf">Output values</param>
        public static void Convolution(Complex[] f, Complex[] fxf)
        {
            int length = fxf.Length;
            Complex[] complex = f.Concat(Enumerable.Repeat(Complex.Zero, length - f.Length)).ToArray();
            Fourier(complex, FourierDirection.Forward);
            complex = complex.Select(x => x*x/length).ToArray();
            Fourier(complex, FourierDirection.Backward);
            Array.Copy(complex, 0, fxf, 0, length);
        }

        /// <summary>
        ///     Calculate convolution values
        /// </summary>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fxg">Output values</param>
        public static void Convolution(double[] f, double[] g, double[] fxg)
        {
            int length = fxg.Length;
            double[] doubles = f.Concat(Enumerable.Repeat(0.0, length - f.Length)).ToArray();
            Complex[] complex = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(complex, FourierDirection.Forward);
            doubles = g.Concat(Enumerable.Repeat(0.0, length - g.Length)).ToArray();
            Complex[] complex1 = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(complex1, FourierDirection.Backward);
            int index = 0;
            foreach (Complex value in complex1)
                complex[index++] *= value/length;
            Fourier(complex, FourierDirection.Backward);
            doubles = complex.Select(x => x.Real).ToArray();
            Array.Copy(doubles, 0, fxg, 0, length);
        }

        /// <summary>
        ///     Calculate convolution values
        /// </summary>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fxg">Output values</param>
        public static void Convolution(Complex[] f, Complex[] g, Complex[] fxg)
        {
            int length = fxg.Length;
            Complex[] complex = f.Concat(Enumerable.Repeat(Complex.Zero, length - f.Length)).ToArray();
            Fourier(complex, FourierDirection.Forward);
            Complex[] complex1 = g.Concat(Enumerable.Repeat(Complex.Zero, length - g.Length)).ToArray();
            Fourier(complex1, FourierDirection.Forward);
            int index = 0;
            foreach (Complex value in complex1)
                complex[index++] *= value/length;
            Fourier(complex, FourierDirection.Backward);
            Array.Copy(complex, 0, fxg, 0, length);
        }

        /// <summary>
        ///     Calculate convolution values
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="n2">Array size</param>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fxg">Output values</param>
        public static void Convolution(int n0, int n1, int n2, double[] f, double[] g, double[] fxg)
        {
            Debug.Assert(fxg.Length == n0*n1*n2);
            Debug.Assert(f.Length == fxg.Length);
            Debug.Assert(g.Length == fxg.Length);
            int length = fxg.Length;
            double[] doubles = f.ToArray();
            Complex[] complex = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, n2, complex, FourierDirection.Forward);
            doubles = g.ToArray();
            Complex[] complex1 = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, n2, complex1, FourierDirection.Forward);
            int index = 0;
            foreach (Complex value in complex1)
                complex[index++] *= value/length;
            Fourier(n0, n1, n2, complex, FourierDirection.Backward);
            doubles = complex.Select(x => x.Real).ToArray();
            Array.Copy(doubles, 0, fxg, 0, length);
        }

        /// <summary>
        ///     Calculate convolution values
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fxg">Output values</param>
        public static void Convolution(int n0, int n1, double[] f, double[] g, double[] fxg)
        {
            Debug.Assert(fxg.Length == n0*n1);
            Debug.Assert(f.Length == fxg.Length);
            Debug.Assert(g.Length == fxg.Length);
            int length = fxg.Length;
            double[] doubles = f.ToArray();
            Complex[] complex = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, complex, FourierDirection.Forward);
            doubles = g.ToArray();
            Complex[] complex1 = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, complex1, FourierDirection.Forward);
            int index = 0;
            foreach (Complex value in complex1)
                complex[index++] *= value/length;
            Fourier(n0, n1, complex, FourierDirection.Backward);
            doubles = complex.Select(x => x.Real).ToArray();
            Array.Copy(doubles, 0, fxg, 0, length);
        }

        /// <summary>
        ///     Calculate convolution values
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fxg">Output values</param>
        public static void Convolution(int n0, int n1, Complex[] f, Complex[] g, Complex[] fxg)
        {
            Debug.Assert(fxg.Length == n0*n1);
            Debug.Assert(f.Length == fxg.Length);
            Debug.Assert(g.Length == fxg.Length);
            int length = fxg.Length;
            Complex[] complex = f.ToArray();
            Fourier(n0, n1, complex, FourierDirection.Forward);
            Complex[] complex1 = g.ToArray();
            Fourier(n0, n1, complex1, FourierDirection.Forward);
            int index = 0;
            foreach (Complex value in complex1)
                complex[index++] *= value/length;
            Fourier(n0, n1, complex, FourierDirection.Backward);
            Array.Copy(complex, 0, fxg, 0, length);
        }

        /// <summary>
        ///     Calculate convolution values
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="n2">Array size</param>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fxg">Output values</param>
        public static void Convolution(int n0, int n1, int n2, Complex[] f, Complex[] g, Complex[] fxg)
        {
            Debug.Assert(fxg.Length == n0*n1*n2);
            Debug.Assert(f.Length == fxg.Length);
            Debug.Assert(g.Length == fxg.Length);
            int length = fxg.Length;
            Complex[] complex = f.ToArray();
            Fourier(n0, n1, n2, complex, FourierDirection.Forward);
            Complex[] complex1 = g.ToArray();
            Fourier(n0, n1, n2, complex1, FourierDirection.Forward);
            int index = 0;
            foreach (Complex value in complex1)
                complex[index++] *= value/length;
            Fourier(n0, n1, n2, complex, FourierDirection.Backward);
            Array.Copy(complex, 0, fxg, 0, length);
        }
    }
}