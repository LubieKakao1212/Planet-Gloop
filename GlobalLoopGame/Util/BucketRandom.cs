using System;
using System.Collections;
using System.Collections.Generic;

namespace Util
{
    [Serializable]
    public class BucketRandom<T>
    {
        private T[] defaultBucket;

        private List<T> bucket = new List<T>();

        private Random random;

        public BucketRandom(Random random, params T[] defaultBucket)
        {
            this.defaultBucket = defaultBucket;
            this.random = random;
        }

        public T GetRandom()
        {
            if (bucket.Count == 0)
            {
                if (defaultBucket.Length == 0)
                {
                    Console.WriteLine("defaultBucket is empty!");
                    return default;
                }

                bucket.AddRange(defaultBucket);
            }

            var i = random.Next(0, bucket.Count);
            var result = bucket[i];
            bucket.RemoveAt(i);

            return result;
        }
    }
}
