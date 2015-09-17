using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace FFTTools
{
    /// <summary>
    ///     FFT((f,g)) = FFT(fxg') = FFT(f)*FFT(g)' = FFT(f)*BFT(g)
    ///     (a*f+b*g,h) = a*(f,h)+b*(g,h)
    ///     (f,g)=(g,f)'
    /// </summary>
    public class ScalarBuilder : BuilderBase, IBuilder
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
        ///     Calculate function self-scalar values
        /// </summary>
        /// <param name="f">Input values</param>
        /// <param name="ff">Output values</param>
        public static void Scalar(double[] f, double[] ff)
        {
            int length = ff.Length;
            double[] doubles = f.Concat(Enumerable.Repeat(0.0, length - f.Length)).ToArray();
            Complex[] complex = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(complex, FourierDirection.Forward);
            complex = complex.Select(x => x*Complex.Conjugate(x)/length).ToArray();
            Fourier(complex, FourierDirection.Backward);
            doubles = complex.Select(x => x.Real).ToArray();
            Array.Copy(doubles, 0, ff, 0, length);
        }

        /// <summary>
        ///     Calculate function self-scalar values
        /// </summary>
        /// <param name="f">Input values</param>
        /// <param name="ff">Output values</param>
        public static void Scalar(Complex[] f, Complex[] ff)
        {
            int length = ff.Length;
            Complex[] complex = f.Concat(Enumerable.Repeat(Complex.Zero, length - f.Length)).ToArray();
            Fourier(complex, FourierDirection.Forward);
            complex = complex.Select(x => x*Complex.Conjugate(x)/length).ToArray();
            Fourier(complex, FourierDirection.Backward);
            Array.Copy(complex, 0, ff, 0, length);
        }

        /// <summary>
        ///     Calculate scalar values
        /// </summary>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fg">Output values</param>
        public static void Scalar(double[] f, double[] g, double[] fg)
        {
            int length = fg.Length;
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
            Array.Copy(doubles, 0, fg, 0, length);
        }

        /// <summary>
        ///     Calculate scalar values
        /// </summary>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fg">Output values</param>
        public static void Scalar(Complex[] f, Complex[] g, Complex[] fg)
        {
            int length = fg.Length;
            Complex[] complex = f.Concat(Enumerable.Repeat(Complex.Zero, length - f.Length)).ToArray();
            Fourier(complex, FourierDirection.Forward);
            Complex[] complex1 = g.Concat(Enumerable.Repeat(Complex.Zero, length - g.Length)).ToArray();
            Fourier(complex1, FourierDirection.Backward);
            int index = 0;
            foreach (Complex value in complex1)
                complex[index++] *= value/length;
            Fourier(complex, FourierDirection.Backward);
            Array.Copy(complex, 0, fg, 0, length);
        }

        /// <summary>
        ///     Calculate scalar values
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="n2">Array size</param>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fg">Output values</param>
        public static void Scalar(int n0, int n1, int n2, double[] f, double[] g, double[] fg)
        {
            Debug.Assert(fg.Length == n0*n1*n2);
            Debug.Assert(f.Length == fg.Length);
            Debug.Assert(g.Length == fg.Length);
            int length = fg.Length;
            double[] doubles = f.ToArray();
            Complex[] complex = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, n2, complex, FourierDirection.Forward);
            doubles = g.ToArray();
            Complex[] complex1 = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, n2, complex1, FourierDirection.Backward);
            int index = 0;
            foreach (Complex value in complex1)
                complex[index++] *= value/length;
            Fourier(n0, n1, n2, complex, FourierDirection.Backward);
            doubles = complex.Select(x => x.Real).ToArray();
            Array.Copy(doubles, 0, fg, 0, length);
        }

        /// <summary>
        ///     Calculate scalar values
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fg">Output values</param>
        public static void Scalar(int n0, int n1, double[] f, double[] g, double[] fg)
        {
            Debug.Assert(fg.Length == n0*n1);
            Debug.Assert(f.Length == fg.Length);
            Debug.Assert(g.Length == fg.Length);
            int length = fg.Length;
            double[] doubles = f.ToArray();
            Complex[] complex = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, complex, FourierDirection.Forward);
            doubles = g.ToArray();
            Complex[] complex1 = doubles.Select(x => new Complex(x, 0)).ToArray();
            Fourier(n0, n1, complex1, FourierDirection.Backward);
            int index = 0;
            foreach (Complex value in complex1)
                complex[index++] *= value/length;
            Fourier(n0, n1, complex, FourierDirection.Backward);
            doubles = complex.Select(x => x.Real).ToArray();
            Array.Copy(doubles, 0, fg, 0, length);
        }

        /// <summary>
        ///     Calculate scalar values
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fg">Output values</param>
        public static void Scalar(int n0, int n1, Complex[] f, Complex[] g, Complex[] fg)
        {
            Debug.Assert(fg.Length == n0*n1);
            Debug.Assert(f.Length == fg.Length);
            Debug.Assert(g.Length == fg.Length);
            int length = fg.Length;
            Complex[] complex = f.ToArray();
            Fourier(n0, n1, complex, FourierDirection.Forward);
            Complex[] complex1 = g.ToArray();
            Fourier(n0, n1, complex1, FourierDirection.Backward);
            int index = 0;
            foreach (Complex value in complex1)
                complex[index++] *= value/length;
            Fourier(n0, n1, complex, FourierDirection.Backward);
            Array.Copy(complex, 0, fg, 0, length);
        }

        /// <summary>
        ///     Calculate scalar values
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="n2">Array size</param>
        /// <param name="f">Input values</param>
        /// <param name="g">Input values</param>
        /// <param name="fg">Output values</param>
        public static void Scalar(int n0, int n1, int n2, Complex[] f, Complex[] g, Complex[] fg)
        {
            Debug.Assert(fg.Length == n0*n1*n2);
            Debug.Assert(f.Length == fg.Length);
            Debug.Assert(g.Length == fg.Length);
            int length = fg.Length;
            Complex[] complex = f.ToArray();
            Fourier(n0, n1, n2, complex, FourierDirection.Forward);
            Complex[] complex1 = g.ToArray();
            Fourier(n0, n1, n2, complex1, FourierDirection.Backward);
            int index = 0;
            foreach (Complex value in complex1)
                complex[index++] *= value/length;
            Fourier(n0, n1, n2, complex, FourierDirection.Backward);
            Array.Copy(complex, 0, fg, 0, length);
        }
    }
}