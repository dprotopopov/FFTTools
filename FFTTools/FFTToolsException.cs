using System;

namespace FFTTools
{
#if LANG_JP
    /// <summary>
    /// OpenCvSharpから投げられる例外
    /// </summary>
#else
    /// <summary>
    ///     The exception that is thrown by OpenCvSharp.
    /// </summary>
#endif
    public class FFTToolsException : Exception
    {
        /// <summary>
        /// </summary>
        public FFTToolsException()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        public FFTToolsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="messageFormat"></param>
        /// <param name="args"></param>
        public FFTToolsException(string messageFormat, params object[] args)
            : base(string.Format(messageFormat, args))
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public FFTToolsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}