using System;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFTTools.UnitTest
{
    [TestClass]
    public class FourierUnitTest
    {
        private static readonly Random Rnd = new Random((int) DateTime.Now.Ticks);

        [TestMethod]
        public void ConjugationTestMethod()
        {
            const int length = 1000;
            var f = Enumerable.Range(0, length).Select(i => Rnd.NextDouble()).ToArray();
            var complex = f.Select(x => new Complex(x, 0)).ToArray();
            BuilderBase.Fourier(complex, FourierDirection.Forward);
            complex = complex.Select(x => Complex.Conjugate(x)/length).ToArray();
            BuilderBase.Fourier(complex, FourierDirection.Backward);
            var f1 = complex.Select(x => x.Magnitude).ToArray();

            for (var i = 0; i < length; i++)
            {
                Assert.IsTrue(Math.Abs(f[i] - f1[(length - i)%length]) < 0.0000000000001);
            }
        }
    }
}