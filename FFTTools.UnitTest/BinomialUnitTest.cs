using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFTTools.UnitTest
{
    [TestClass]
    public class BinomialUnitTest
    {
        [TestMethod]
        public void BinomialTestMethod()
        {
            const int count = 10;
            var doubles = new double[count];
            var longs = new long[count];
            BinomialBuilder.GetLongs(longs);
            BinomialBuilder.GetDoubles(doubles);
            Console.WriteLine(
                string.Join(Environment.NewLine,
                    longs.Zip(doubles, (x, y) => string.Format("{0} - {1} = {2}", x, y, x - y))) +
                Environment.NewLine);
            Assert.IsTrue(doubles.Zip(longs, (x, y) => x - y).All(x => Math.Abs(x) < 0.001));
        }
    }
}