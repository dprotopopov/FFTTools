using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFTTools.UnitTest
{
    [TestClass]
    public class ConvolutionUnitTest
    {
        [TestMethod]
        public void ConvolutionTestMethod()
        {
            const int count = 10;
            var values = new double[count];
            var vxv = new double[2*values.Length-1];
            Array.Clear(values, 0, values.Length);
            Array.Clear(vxv, 0, vxv.Length);

            BinomialBuilder.GetDoubles(values);

            for (int i = 0; i < values.Length; i++)
                for (int j = 0; j < values.Length; j++)
                    vxv[i + j] += values[i] * values[j];

            var fxf = new double[2 * values.Length];
            ConvolutionBuilder.Convolution(values, fxf);

            Console.WriteLine(
                string.Join(Environment.NewLine,
                    vxv.Zip(fxf, (x, y) => string.Format("{0} - {1} = {2}", x, y, x - y))) +
                Environment.NewLine);
            Assert.IsTrue(vxv.Zip(fxf, (x, y) => x - y).All(x => Math.Abs(x) < 0.0001));
        }
    }
}