using System;
using System.Collections.Generic;

namespace MonoEngine.Util
{
    //One of my premade utilities
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Adds given value to a dictionary if there was no element at given <paramref name="key"/>, replaces element with <paramref name="value"> otherwise.
        /// </summary>
        /// <returns>true if element was added, false if it was replaced</returns>
        public static bool AddOrUpdate<K, V>(this IDictionary<K, V> dict, K key, V value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
                return false;
            }
            else
            {
                dict.Add(key, value);
                return true;
            }
        }


        /// <summary>
        /// Gets a value from the dictionary under a specified key or adds it if did not exist and returns <paramref name="defaultValue"/>.
        /// For complex objects use <see cref="GetOrSetToDefaultLazy{K, V}(IDictionary{K, V}, K, Func{V})"/>
        /// </summary>
        /// <returns>value under a given <paramref name="key"/> if it exists, <paramref name="defaultValue"/> otherwise</returns>
        public static V GetOrSetToDefault<K, V>(this IDictionary<K, V> dict, K key, V defaultValue)
        {
            if (dict.TryGetValue(key, out V value))
            {
                return value;
            }
            dict.Add(key, defaultValue);

            return defaultValue;
        }

        /// <summary>
        /// Alternative overload to <see cref="GetOrSetToDefault{K, V}(IDictionary{K, V}, K, V)"/>, with lazy object construction
        /// </summary>
        /// <returns>value under a given <paramref name="key"/> if it exists, result of <paramref name="defaultValue"/> otherwise</returns>
        public static V GetOrSetToDefaultLazy<K, V>(this IDictionary<K, V> dict, K key, Func<K, V> defaultValue)
        {
            if (dict.TryGetValue(key, out V value))
            {
                return value;
            }
            var val = defaultValue(key);
            dict.Add(key, val);

            return val;
        }

        /// <summary>
        /// Enumerates given dictionary nested entries, as if it wa a MultiDictionary
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static IEnumerable<(K, V)> EnumerateNestedEntries<K, V>(this IDictionary<K, List<V>> dict)
        {
            foreach (var (key, list) in dict)
            {
                foreach (var element in list)
                {
                    yield return (key, element);
                }
            }
        }

        /// <summary>
        /// Similar to <see cref="EnumerateNestedEntries{K, V}(IDictionary{K, List{V}})"/> but enumerates only values
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static IEnumerable<V> EnumerateNestedValues<K, V>(this IDictionary<K, List<V>> dict)
        {
            foreach (var list in dict.Values)
            {
                foreach (var element in list)
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Adds an element to given dictionary nesting it in a list, emulates a multi dictionary
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddNested<K, V>(this IDictionary<K, List<V>> dict, K key, V value)
        {
            var list = dict.GetOrSetToDefaultLazy(key, (k) => new());
            list.Add(value);
        }
    }
}
