using System;
using System.Linq;
using System.Runtime.InteropServices;
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

            var image = new Image<Gray, byte>(height, width);
            var bytes = new byte[image.Data.Length];

            Rng.GetBytes(bytes);

            var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
            Marshal.Copy(bytes, 0, handle.AddrOfPinnedObject(), bytes.Length);
            handle.Free();

            var average = bytes.Average(x => (double) x);
            var delta = Math.Sqrt(bytes.Average(x => (double) x*x) - average*average);
            var minValue = bytes.Min(x => (double) x);
            var maxValue = bytes.Max(x => (double) x);
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Length {0}", bytes.Length));
            sb.AppendLine(string.Format("Average {0}", average));
            sb.AppendLine(string.Format("Delta {0}", delta));
            sb.AppendLine(string.Format("minValue {0}", minValue));
            sb.AppendLine(string.Format("maxValue {0}", maxValue));
            Console.WriteLine(sb.ToString());

            using (var blurBuilder = new BlurBuilder(filterStep))
                image = blurBuilder.Blur(image);

            handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
            Marshal.Copy(handle.AddrOfPinnedObject(), bytes, 0, bytes.Length);
            handle.Free();

            var average1 = bytes.Average(x => (double) x);
            var delta1 = Math.Sqrt(bytes.Average(x => (double) x*x) - average1*average1);
            var minValue1 = bytes.Min(x => (double) x);
            var maxValue1 = bytes.Max(x => (double) x);
            var sb1 = new StringBuilder();
            sb1.AppendLine(string.Format("Length {0}", bytes.Length));
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

            var image = new Image<Gray, byte>(height, width);
            var bytes = new byte[image.Data.Length];

            Rng.GetBytes(bytes);

            var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
            Marshal.Copy(bytes, 0, handle.AddrOfPinnedObject(), bytes.Length);
            handle.Free();

            var average = bytes.Average(x => (double) x);
            var delta = Math.Sqrt(bytes.Average(x => (double) x*x) - average*average);
            var minValue = bytes.Min(x => (double) x);
            var maxValue = bytes.Max(x => (double) x);
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Length {0}", bytes.Length));
            sb.AppendLine(string.Format("Average {0}", average));
            sb.AppendLine(string.Format("Delta {0}", delta));
            sb.AppendLine(string.Format("minValue {0}", minValue));
            sb.AppendLine(string.Format("maxValue {0}", maxValue));
            Console.WriteLine(sb.ToString());

            using (var sharpBuilder = new SharpBuilder(filterStep))
                image = sharpBuilder.Sharp(image);

            handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
            Marshal.Copy(handle.AddrOfPinnedObject(), bytes, 0, bytes.Length);
            handle.Free();

            var average1 = bytes.Average(x => (double) x);
            var delta1 = Math.Sqrt(bytes.Average(x => (double) x*x) - average1*average1);
            var minValue1 = bytes.Min(x => (double) x);
            var maxValue1 = bytes.Max(x => (double) x);
            var sb1 = new StringBuilder();
            sb1.AppendLine(string.Format("Length {0}", bytes.Length));
            sb1.AppendLine(string.Format("Average {0}", average1));
            sb1.AppendLine(string.Format("Delta {0}", delta1));
            sb1.AppendLine(string.Format("minValue {0}", minValue1));
            sb1.AppendLine(string.Format("maxValue {0}", maxValue1));
            Console.WriteLine(sb1.ToString());

            Assert.IsTrue(delta1 < delta);
        }
    }
}