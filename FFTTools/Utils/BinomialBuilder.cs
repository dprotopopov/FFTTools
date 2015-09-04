using System;

namespace FFTTools.Utils
{
    public class BinomialBuilder : IDisposable
    {
        public void GetLongs(long[] array)
        {
            if (array.Length > 0) array[0] = 1;
            for (int i = 1; i < array.Length; i++)
                for (int j = i; j-- > 0; )
                    array[j + 1] += array[j];
        }
        public void GetDoubles(double[] array)
        {
            if(array.Length>0) array[0] = 1;
            for (int i = 1; i < array.Length; i++)
                for (int j = i; j-- > 0; )
                    array[j + 1] += array[j];
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}