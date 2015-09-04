using System;
using System.Linq;
using System.Numerics;
using FFTWSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFTTools.UnitTest
{
    [TestClass]
    public class DftUnitTest
    {
        private static readonly Random Rnd = new Random((int) DateTime.Now.Ticks);

        [TestMethod]
        public void ConjugationTestMethod()
        {
            int length = 1000;
            double[] f = Enumerable.Range(0, length).Select(i => Rnd.NextDouble()).ToArray();
            var input = new fftw_complexarray(length);
            var output = new fftw_complexarray(length);
            fftw_plan forward = fftw_plan.dft_1d(length, input, output,
                fftw_direction.Forward,
                fftw_flags.Estimate);
            fftw_plan backward = fftw_plan.dft_1d(length, input, output,
                fftw_direction.Backward,
                fftw_flags.Estimate);

            input.SetData(f.Select(x => new Complex(x, 0)).ToArray());
            forward.Execute();
            input.SetData(output.GetData_Complex().Select(x => Complex.Conjugate(x) / length).ToArray());
            backward.Execute();

            double[] f1 = output.GetData_Complex().Select(x => x.Magnitude).ToArray();
            for (int i = 0; i < length; i++)
            {
                Assert.IsTrue(Math.Abs(f[i] - f1[(length - i)%length]) < 0.0000000000001);
            }
        }
    }
}