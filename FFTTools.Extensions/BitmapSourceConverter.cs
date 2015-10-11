using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace FFTTools.Extensions
{
#if LANG_JP
    /// <summary>
    /// System.Windows.Media.Imaging.WriteableBitmapとOpenCVのIplImageとの間の相互変換メソッドを提供するクラス
    /// </summary>
#else
    /// <summary>
    ///     Static class which provides conversion between System.Windows.Media.Imaging.BitmapSource and IplImage
    /// </summary>
#endif
    public static class BitmapSourceConverter
    {
        /// <summary>
        ///     Delete a GDI object
        /// </summary>
        /// <param name="hObject">The poniter to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr hObject);

#if LANG_JP
    /// <summary>
    /// MatをBitmapSourceに変換する. 
    /// </summary>
    /// <param name="src">変換するIplImage</param>
    /// <returns>WPFのBitmapSource</returns>
#else
        /// <summary>
        ///     Converts IImage to BitmapSource.
        /// </summary>
        /// <param name="src">Input IImage</param>
        /// <returns>BitmapSource</returns>
#endif
        public static BitmapSource ToBitmapSource(
            this IImage src)
        {
            return ToBitmapSource(src.Bitmap);
        }


#if LANG_JP
    /// <summary>
    /// System.Drawing.BitmapをBitmapSourceに変換する. 
    /// </summary>
    /// <param name="src">変換するBitmap</param>
    /// <returns>WPFのBitmapSource</returns>
#else
        /// <summary>
        ///     Converts System.Drawing.Bitmap to BitmapSource.
        /// </summary>
        /// <param name="src">Input System.Drawing.Bitmap</param>
        /// <returns>BitmapSource</returns>
#endif
        public static BitmapSource ToBitmapSource(this Bitmap src)
        {
            var hBitmap = IntPtr.Zero;
            try
            {
                hBitmap = src.GetHbitmap();
                var bs = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                return bs;
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        public static IImage ToImage(this BitmapSource src)
        {
            return src.ToBitmap().ToImage();
        }

        public static Bitmap ToBitmap(this BitmapSource src)
        {
            throw new NotImplementedException();
        }
    }
}