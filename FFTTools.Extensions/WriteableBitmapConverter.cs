using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace FFTTools.Extensions
{
    internal static class WriteableBitmapConverter
    {
        public static WriteableBitmap ToWriteableBitmap(this Bitmap src)
        {
            return new WriteableBitmap(src.ToBitmapSource());
        }

        public static WriteableBitmap ToWriteableBitmap(this IImage src)
        {
            return new WriteableBitmap(src.ToBitmapSource());
        }

        public static IImage ToImage(this WriteableBitmap src)
        {
            return src.ToBitmap().ToImage();
        }
        public static Bitmap ToBitmap(this WriteableBitmap src)
        {
            throw new NotImplementedException();
        }
    }
}