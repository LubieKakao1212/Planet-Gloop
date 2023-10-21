using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Util
{
    //TODO add more
    public static class Tuples
    {
        public static IEnumerable<T> EnumerateNotNull<T>(this (T?, T?) tuple) where T : struct
        {
            if (tuple.Item1 != null)
            {
                yield return tuple.Item1.Value;
            }
            if (tuple.Item2 != null)
            {
                yield return tuple.Item2.Value;
            }
        }

        public static IEnumerable<T> Enumerate<T>(this (T, T) tuple)
        {
            yield return tuple.Item1;
            yield return tuple.Item2;
        }

        public static IEnumerable<T> Enumerate<T>(this (T, T, T) tuple)
        {
            yield return tuple.Item1;
            yield return tuple.Item2;
            yield return tuple.Item3;
        }
    }
}
