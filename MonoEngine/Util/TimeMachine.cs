using static System.Math;

namespace MonoEngine.Util
{
    public class TimeMachine
    {
        private double time;

        public TimeMachine()
        {
            time = 0;
        }

        /// <summary>
        /// Adds time into the TimeMachine
        /// </summary>
        /// <param name="time">Amount of time to be added</param>
        public void Accumulate(double time) 
        {
            this.time += time;
        }

        /// <summary>
        /// Retrieves given amount of time from the TimeMachine
        /// </summary>
        /// <param name="maxTime"></param>
        /// <returns>Amount of time retrievend</returns>
        public double Retrieve(double maxTime) 
        {
            if(!double.IsFinite(maxTime))
            {
                double timeRetrieved = time;
                time = 0;
                return timeRetrieved;
            }
            double timeLeft = time - maxTime;
            time = Max(timeLeft, 0);
            return Min(maxTime + timeLeft, maxTime);
        }

        /// <summary>
        /// Attempts to retrieves given amount of time from the TimeMachine <br/>
        /// If there is enough <paramref name="time"/> accumulated in this machine subtructs that amount and returns true, otherwise returns false
        /// </summary>
        /// <param name="time"></param>
        public bool TryRetrieve(double time)
        {
            if (this.time >= time)
            {
                this.time -= time;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Result is equivalent to calling <see cref="TryRetrieve(double)"/> as many times as possible, but is faster for larger <paramref name="limit"/> values
        /// </summary>
        /// <param name="interval">Single unit of warp time, must be positive/param>
        /// <param name="limit">Maximum amount of warps, must be positive</param>
        /// <returns>Amount of warps</returns>
        public int RetrieveAll(double interval, int limit = int.MaxValue)
        {
            int result = (int)Floor((float)(time / interval));
            result = Clamp(result, 0, limit);
            time -= result * interval;
            return result;
        }
    }
}
