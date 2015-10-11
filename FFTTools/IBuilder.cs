using System;
using System.Drawing;

namespace FFTTools
{
    public interface IBuilder : IDisposable
    {
#if EMGU
    /// <summary>
    ///     Vizualize builder
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
        Bitmap ToBitmap(Bitmap source);
#endif
    }
}