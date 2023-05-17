using System;
using System.Collections.Generic;

namespace MonoEngine.Utils
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
        public static V GetOrSetToDefaultLazy<K, V>(this IDictionary<K, V> dict, K key, Func<V> defaultValue)
        {
            if (dict.TryGetValue(key, out V value))
            {
                return value;
            }
            var val = defaultValue();
            dict.Add(key, val);

            return val;
        }
    }
}
