using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFTTools.UnitTest
{
    [TestClass]
    public class BlurAndSharpUnitTest
    {
        private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        [TestMethod]
        public void BlurTestMethod()
        {
            const int width = 100;
            const int height = 100;
            const int filterStep = 1;

            var image = new Image<Bgr, byte>(width, height);
            var bytes = new byte[image.Data.Length];

            Rng.GetBytes(bytes);
            Buffer.BlockCopy(bytes, 0, image.Data, 0, bytes.Length);

            double average = bytes.Average(x => (double) x);
            double delta = Math.Sqrt(bytes.Average(x => (double) x*x) - average*average);
            double minValue = bytes.Min(x => (double) x);
            double maxValue = bytes.Max(x => (double) x);
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Length {0}", image.Data.Length));
            sb.AppendLine(string.Format("Average {0}", average));
            sb.AppendLine(string.Format("Delta {0}", delta));
            sb.AppendLine(string.Format("minValue {0}", minValue));
            sb.AppendLine(string.Format("maxValue {0}", maxValue));
            Console.WriteLine(sb.ToString());

            using (var blurBuilder = new BlurBuilder(filterStep))
                image = blurBuilder.Blur(image);

            Buffer.BlockCopy(image.Data, 0, bytes, 0, bytes.Length);

            double average1 = bytes.Average(x => (double) x);
            double delta1 = Math.Sqrt(bytes.Average(x => (double) x*x) - average1*average1);
            double minValue1 = bytes.Min(x => (double) x);
            double maxValue1 = bytes.Max(x => (double) x);
            var sb1 = new StringBuilder();
            sb1.AppendLine(string.Format("Length {0}", image.Data.Length));
            sb1.AppendLine(string.Format("Average {0}", average1));
            sb1.AppendLine(string.Format("Delta {0}", delta1));
            sb1.AppendLine(string.Format("minValue {0}", minValue1));
            sb1.AppendLine(string.Format("maxValue {0}", maxValue1));
            Console.WriteLine(sb1.ToString());

            Assert.IsTrue(delta1 < delta);
        }

        [TestMethod]
        public void SharpTestMethod()
        {
            const int width = 100;
            const int height = 100;
            const int filterStep = 1;

            var image = new Image<Bgr, byte>(width, height);
            var bytes = new byte[image.Data.Length];

            Rng.GetBytes(bytes);
            Buffer.BlockCopy(bytes, 0, image.Data, 0, bytes.Length);

            double average = bytes.Average(x => (double) x);
            double delta = Math.Sqrt(bytes.Average(x => (double) x*x) - average*average);
            double minValue = bytes.Min(x => (double) x);
            double maxValue = bytes.Max(x => (double) x);
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Length {0}", image.Data.Length));
            sb.AppendLine(string.Format("Average {0}", average));
            sb.AppendLine(string.Format("Delta {0}", delta));
            sb.AppendLine(string.Format("minValue {0}", minValue));
            sb.AppendLine(string.Format("maxValue {0}", maxValue));
            Console.WriteLine(sb.ToString());

            using (var sharpBuilder = new SharpBuilder(filterStep))
                image = sharpBuilder.Sharp(image);

            Buffer.BlockCopy(image.Data, 0, bytes, 0, bytes.Length);

            double average1 = bytes.Average(x => (double) x);
            double delta1 = Math.Sqrt(bytes.Average(x => (double) x*x) - average1*average1);
            double minValue1 = bytes.Min(x => (double) x);
            double maxValue1 = bytes.Max(x => (double) x);
            var sb1 = new StringBuilder();
            sb1.AppendLine(string.Format("Length {0}", image.Data.Length));
            sb1.AppendLine(string.Format("Average {0}", average1));
            sb1.AppendLine(string.Format("Delta {0}", delta1));
            sb1.AppendLine(string.Format("minValue {0}", minValue1));
            sb1.AppendLine(string.Format("maxValue {0}", maxValue1));
            Console.WriteLine(sb1.ToString());

            Assert.IsTrue(delta1 < delta);
        }
    }
}