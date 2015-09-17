using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFTTools.UnitTest
{
    [TestClass]
    public class ScalarUnitTest
    {
        [TestMethod]
        public void ScalarTestMethod()
        {
            const int count = 10;
            var doubles = new double[count];
            BinomialBuilder.GetDoubles(doubles);
            var scalar = new double[2*doubles.Length];
            ScalarBuilder.Scalar(doubles, scalar);
            double sum = doubles.Select(x => Math.Pow(x, 2)).Sum();
            Assert.IsTrue(Math.Abs(sum - scalar[0]) < 0.0001);
        }
    }
}