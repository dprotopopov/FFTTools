using System;
using System.Linq;
using FFTTools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFTTools.UnitTest
{
    [TestClass]
    public class ConvolutionUnitTest
    {
        [TestMethod]
        public void BinomialTestMethod()
        {
            var values = new double[4];
            var valuesXvalues = new double[7];
            Array.Clear(values, 0, values.Length);
            Array.Clear(valuesXvalues, 0, valuesXvalues.Length);

            using (var binomialBuilder = new BinomialBuilder())
                binomialBuilder.GetDoubles(values);

            for (int i = 0; i < values.Length; i++)
                for (int j = 0; j < values.Length; j++)
                    valuesXvalues[i + j] += values[i]*values[j];

            using (var convolutionBuilder = new ConvolutionBuilder(FunctionType.NonPeriodic))
            {
                double[] fxf = convolutionBuilder.Build(values);
                Console.WriteLine(
                    string.Join(Environment.NewLine,
                        valuesXvalues.Zip(fxf, (x, y) => string.Format("{0} - {1} = {2}", x, y, x - y))) +
                    Environment.NewLine);
                Assert.IsTrue(valuesXvalues.Zip(fxf, (x, y) => x - y).All(x => Math.Abs(x) < 0.0001));                
            }
        }
    }
}