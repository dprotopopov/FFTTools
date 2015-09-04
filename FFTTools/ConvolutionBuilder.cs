using System;
using System.Linq;
using System.Numerics;
using FFTWSharp;

namespace FFTTools
{
    /// <summary>
    ///     The convolution of f and g is written f∗g, using an asterisk or star. It is defined as the integral of the product
    ///     of the two functions after one is reversed and shifted.
    ///     FT(fxg) = FT(f)*FT(g)
    ///     FT(a*f+b*g) = a*FT(f)+b*FT(g)
    /// </summary>
    public class ConvolutionBuilder : IDisposable
    {
        private readonly FunctionType _functionType;

        /// <summary>
        ///     Builder constructor
        /// </summary>
        public ConvolutionBuilder(FunctionType functionType = FunctionType.Periodic)
        {
            _functionType = functionType;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///     Calculate function self-convolution values
        /// </summary>
        /// <param name="f">Function values</param>
        /// <returns></returns>
        public double[] Build(double[] f)
        {
            int length = (_functionType == FunctionType.Periodic) ? f.Length : (f.Length + f.Length - 1);

            var input = new fftw_complexarray(length);
            var output = new fftw_complexarray(length);
            fftw_plan forward = fftw_plan.dft_1d(length, input, output,
                fftw_direction.Forward,
                fftw_flags.Estimate);
            fftw_plan backward = fftw_plan.dft_1d(length, input, output,
                fftw_direction.Backward,
                fftw_flags.Estimate);

            var complex = new Complex[length];
            for (int i = 0; i < f.Length; i++) complex[i] = f[i];
            input.SetData(complex);
            forward.Execute();
            complex = output.GetData_Complex();
            input.SetData(complex.Select(x => x*x/length).ToArray());
            backward.Execute();
            complex = output.GetData_Complex();

            return complex.Select(x => x.Magnitude).ToArray();
        }
    }
}