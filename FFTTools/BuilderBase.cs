using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using FFTWSharp;

namespace FFTTools
{
    public abstract class BuilderBase : fftw
    {
        private static readonly Mutex FftwLock = new Mutex();

        /// <summary>
        ///     Hartley transform
        /// </summary>
        /// <param name="array">Array</param>
        public static void Hartley(Array array)
        {
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            FftwLock.WaitOne();
            var plan = r2r(array.Rank, Enumerable.Range(0, array.Rank).Select(array.GetLength).ToArray(),
                handle.AddrOfPinnedObject(), handle.AddrOfPinnedObject(),
                Enumerable.Repeat(fftw_kind.DHT, array.Rank).ToArray(),
                fftw_flags.Estimate);
            execute(plan);
            destroy_plan(plan);
            FftwLock.ReleaseMutex();
            handle.Free();
        }

        /// <summary>
        ///     Hartley transform
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="n2">Array size</param>
        /// <param name="array">Array</param>
        public static void Hartley(int n0, int n1, int n2, Array array)
        {
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            FftwLock.WaitOne();
            var plan = r2r_3d(n0, n1, n2,
                handle.AddrOfPinnedObject(), handle.AddrOfPinnedObject(),
                fftw_kind.DHT, fftw_kind.DHT, fftw_kind.DHT,
                fftw_flags.Estimate);
            execute(plan);
            destroy_plan(plan);
            FftwLock.ReleaseMutex();
            handle.Free();
        }

        /// <summary>
        ///     Fourier transform
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="direction">Fourier direction</param>
        public static void Fourier(Array array, FourierDirection direction)
        {
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            FftwLock.WaitOne();
            var plan = dft(array.Rank, Enumerable.Range(0, array.Rank).Select(array.GetLength).ToArray(),
                handle.AddrOfPinnedObject(), handle.AddrOfPinnedObject(),
                (fftw_direction) direction,
                fftw_flags.Estimate);
            execute(plan);
            destroy_plan(plan);
            FftwLock.ReleaseMutex();
            handle.Free();
        }

        /// <summary>
        ///     Fourier transform
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="array">Array</param>
        /// <param name="direction">Fourier direction</param>
        public static void Fourier(int n0, int n1, Array array, FourierDirection direction)
        {
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            FftwLock.WaitOne();
            var plan = dft_2d(n0, n1,
                handle.AddrOfPinnedObject(), handle.AddrOfPinnedObject(),
                (fftw_direction) direction,
                fftw_flags.Estimate);
            execute(plan);
            destroy_plan(plan);
            FftwLock.ReleaseMutex();
            handle.Free();
        }

        /// <summary>
        ///     Fourier transform
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="n2">Array size</param>
        /// <param name="array">Array</param>
        /// <param name="direction">Fourier direction</param>
        public static void Fourier(int n0, int n1, int n2, Array array, FourierDirection direction)
        {
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            FftwLock.WaitOne();
            var plan = dft_3d(n0, n1, n2,
                handle.AddrOfPinnedObject(), handle.AddrOfPinnedObject(),
                (fftw_direction) direction,
                fftw_flags.Estimate);
            execute(plan);
            destroy_plan(plan);
            FftwLock.ReleaseMutex();
            handle.Free();
        }

        /// <summary>
        /// </summary>
        /// <param name="number"></param>
        /// <param name="numerator"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        public static int MulDiv(int number, int numerator, int denominator)
        {
            return (int) (((long) number*numerator)/denominator);
        }

        public static void AverageAndDelta(out double average, out double delta, double[] doubles,
            KeepOption keepOption = KeepOption.AverageAndDelta)
        {
            average = doubles.Average();
            delta = Math.Sqrt(doubles.Average(x => x*x) - average*average);
            switch (keepOption)
            {
                case KeepOption.AverageAndDelta:
                    break;
                case KeepOption.Sum:
                    average = doubles.Sum();
                    break;
                case KeepOption.Square:
                    average = Math.Sqrt(doubles.Sum(x => x*x));
                    break;
                case KeepOption.AverageSquare:
                    average = Math.Sqrt(doubles.Average(x => x*x));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Split array
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="data">Array of values</param>
        /// <param name="outer">Array of values</param>
        /// <param name="middle">Array of values</param>
        /// <param name="inner">Array of values</param>
        /// <param name="size">Region size</param>
        /// <param name="n2">Array size</param>
        public static void Split(int n0, int n1, Array data, Array outer, Array middle, Array inner,
            Size size, int n2)
        {
            var s0 = Math.Max(0, (n0 - size.Height)/2);
            var s1 = Math.Max(0, (n1 - size.Width)/2);
            var e0 = Math.Min((n0 + size.Height)/2, n0);
            var e1 = Math.Min((n1 + size.Width)/2, n1);
            for (var i = s0; i < e0; i++)
            {
                Array.Copy(data, (i*n1 + s1)*n2, inner, (i*n1 + s1)*n2, (e1 - s1)*n2);
                Array.Copy(data, i*n1*n2, middle, i*n1*n2, s1*n2);
                Array.Copy(data, (i*n1 + e1)*n2, middle, (i*n1 + e1)*n2, (n1 - e1)*n2);
            }
            for (var i = 0; i < s0; i++)
            {
                Array.Copy(data, (i*n1 + s1)*n2, middle, (i*n1 + s1)*n2, (e1 - s1)*n2);
            }
            for (var i = e0; i < n0; i++)
            {
                Array.Copy(data, (i*n1 + s1)*n2, middle, (i*n1 + s1)*n2, (e1 - s1)*n2);
            }
            for (var i = 0; i < s0; i++)
            {
                Array.Copy(data, i*n1*n2, outer, i*n1*n2, s1*n2);
                Array.Copy(data, (i*n1 + e1)*n2, outer, (i*n1 + e1)*n2, (n1 - e1)*n2);
            }
            for (var i = e0; i < n0; i++)
            {
                Array.Copy(data, i*n1*n2, outer, i*n1*n2, s1*n2);
                Array.Copy(data, (i*n1 + e1)*n2, outer, (i*n1 + e1)*n2, (n1 - e1)*n2);
            }
        }


        /// <summary>
        ///     Split array
        /// </summary>
        /// <param name="data">Array of values</param>
        /// <param name="outer">Array of values</param>
        /// <param name="middle">Array of values</param>
        /// <param name="inner">Array of values</param>
        /// <param name="size">Region size</param>
        public static void Split(Array data, Array outer, Array middle, Array inner, Size size)
        {
            var n0 = data.GetLength(0);
            var n1 = data.GetLength(1);
            var n2 = data.GetLength(2);
            var s0 = Math.Max(0, (n0 - size.Height)/2);
            var s1 = Math.Max(0, (n1 - size.Width)/2);
            var e0 = Math.Min((n0 + size.Height)/2, n0);
            var e1 = Math.Min((n1 + size.Width)/2, n1);
            for (var i = s0; i < e0; i++)
            {
                Array.Copy(data, (i*n1 + s1)*n2, inner, (i*n1 + s1)*n2, (e1 - s1)*n2);
                Array.Copy(data, i*n1*n2, middle, i*n1*n2, s1*n2);
                Array.Copy(data, (i*n1 + e1)*n2, middle, (i*n1 + e1)*n2, (n1 - e1)*n2);
            }
            for (var i = 0; i < s0; i++)
            {
                Array.Copy(data, (i*n1 + s1)*n2, middle, (i*n1 + s1)*n2, (e1 - s1)*n2);
            }
            for (var i = e0; i < n0; i++)
            {
                Array.Copy(data, (i*n1 + s1)*n2, middle, (i*n1 + s1)*n2, (e1 - s1)*n2);
            }
            for (var i = 0; i < s0; i++)
            {
                Array.Copy(data, i*n1*n2, outer, i*n1*n2, s1*n2);
                Array.Copy(data, (i*n1 + e1)*n2, outer, (i*n1 + e1)*n2, (n1 - e1)*n2);
            }
            for (var i = e0; i < n0; i++)
            {
                Array.Copy(data, i*n1*n2, outer, i*n1*n2, s1*n2);
                Array.Copy(data, (i*n1 + e1)*n2, outer, (i*n1 + e1)*n2, (n1 - e1)*n2);
            }
        }

        /// <summary>
        ///     Clear internal region of array
        /// </summary>
        /// <param name="data">Array of values</param>
        /// <param name="size">Internal blind region size</param>
        public static void BlindInner(Array data, Size size)
        {
            var n0 = data.GetLength(0);
            var n1 = data.GetLength(1);
            var n2 = data.GetLength(2);
            var s0 = Math.Max(0, (n0 - size.Height)/2);
            var s1 = Math.Max(0, (n1 - size.Width)/2);
            var e0 = Math.Min((n0 + size.Height)/2, n0);
            var e1 = Math.Min((n1 + size.Width)/2, n1);
            for (var i = s0; i < e0; i++)
            {
                Array.Clear(data, i*n1*n2, n1*n2);
            }
            for (var i = 0; i < s0; i++)
            {
                Array.Clear(data, i*n1*n2 + s1*n2, (e1 - s1)*n2);
            }
            for (var i = e0; i < n0; i++)
            {
                Array.Clear(data, i*n1*n2 + s1*n2, (e1 - s1)*n2);
            }
        }

        /// <summary>
        ///     Clear internal region of array
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="data">Array of values</param>
        /// <param name="size">External blind region size</param>
        /// <param name="n2">Array size</param>
        public static void BlindInner(int n0, int n1, Array data, Size size, int n2)
        {
            var s0 = Math.Max(0, (n0 - size.Height)/2);
            var s1 = Math.Max(0, (n1 - size.Width)/2);
            var e0 = Math.Min((n0 + size.Height)/2, n0);
            var e1 = Math.Min((n1 + size.Width)/2, n1);
            for (var i = s0; i < e0; i++)
            {
                Array.Clear(data, i*n1*n2, n1*n2);
            }
            for (var i = 0; i < s0; i++)
            {
                Array.Clear(data, i*n1*n2 + s1*n2, (e1 - s1)*n2);
            }
            for (var i = e0; i < n0; i++)
            {
                Array.Clear(data, i*n1*n2 + s1*n2, (e1 - s1)*n2);
            }
        }

        /// <summary>
        ///     Clear external region of array
        /// </summary>
        /// <param name="data">Array of values</param>
        /// <param name="size">External blind region size</param>
        public static void BlindOuter(Array data, Size size)
        {
            var n0 = data.GetLength(0);
            var n1 = data.GetLength(1);
            var n2 = data.GetLength(2);
            var s0 = Math.Max(0, (n0 - size.Height)/2);
            var s1 = Math.Max(0, (n1 - size.Width)/2);
            var e0 = Math.Min((n0 + size.Height)/2, n0);
            var e1 = Math.Min((n1 + size.Width)/2, n1);
            for (var i = 0; i < s0; i++)
            {
                Array.Clear(data, i*n1*n2, s1*n2);
                Array.Clear(data, i*n1*n2 + e1*n2, (n1 - e1)*n2);
            }
            for (var i = e0; i < n0; i++)
            {
                Array.Clear(data, i*n1*n2, s1*n2);
                Array.Clear(data, i*n1*n2 + e1*n2, (n1 - e1)*n2);
            }
        }

        /// <summary>
        ///     Clear external region of array
        /// </summary>
        /// <param name="n0">Array size</param>
        /// <param name="n1">Array size</param>
        /// <param name="data">Array of values</param>
        /// <param name="size">External blind region size</param>
        /// <param name="n2">Array size</param>
        public static void BlindOuter(int n0, int n1, Array data, Size size, int n2)
        {
            var s0 = Math.Max(0, (n0 - size.Height)/2);
            var s1 = Math.Max(0, (n1 - size.Width)/2);
            var e0 = Math.Min((n0 + size.Height)/2, n0);
            var e1 = Math.Min((n1 + size.Width)/2, n1);
            for (var i = 0; i < s0; i++)
            {
                Array.Clear(data, i*n1*n2, s1*n2);
                Array.Clear(data, i*n1*n2 + e1*n2, (n1 - e1)*n2);
            }
            for (var i = e0; i < n0; i++)
            {
                Array.Clear(data, i*n1*n2, s1*n2);
                Array.Clear(data, i*n1*n2 + e1*n2, (n1 - e1)*n2);
            }
        }
    }
}