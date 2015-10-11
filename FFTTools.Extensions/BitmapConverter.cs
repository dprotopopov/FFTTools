using System;
using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;

namespace FFTTools.Extensions
{
    public static class BitmapConverter
    {
        public static Image<TColor, TDepth> ToImage<TColor, TDepth>(this Bitmap src)
            where TColor : struct, IColor
            where TDepth : new() => new Image<TColor, TDepth>(src);

        public static Array ToArray<TColor, TDepth>(this Bitmap src)
            where TColor : struct, IColor
            where TDepth : new() => src.ToImage<TColor, TDepth>().Data;

        public static IImage ToImage(this Bitmap src)
        {
            switch (src.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    return src.ToImage<Bgr, byte>();
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return src.ToImage<Bgra, byte>();
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    return src.ToImage<Gray, byte>();
                default:
                    throw new NotImplementedException();
            }
        }

        public static Array ToArray(this Bitmap src)
        {
            switch (src.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                    return src.ToImage<Bgr, byte>().Convert<Bgr, double>().Data;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return src.ToImage<Bgra, byte>().Convert<Bgra, double>().Data;
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    return src.ToImage<Gray, byte>().Convert<Gray, double>().Data;
                default:
                    throw new NotImplementedException();
            }
        }

        public static Bitmap ToBitmap(this Array src)
        {
            switch (src.Rank)
            {
                case 3:
                    switch (src.GetLength(2))
                    {
                        case 1:
                            return new Image<Gray, double>(src as double[,,]).Convert<Gray, byte>().ToBitmap();
                        case 3:
                            return new Image<Bgr, double>(src as double[,,]).Convert<Bgr, byte>().ToBitmap();
                        case 4:
                            return new Image<Bgra, double>(src as double[,,]).Convert<Bgra, byte>().ToBitmap();
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}