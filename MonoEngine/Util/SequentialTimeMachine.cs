using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Util
{
    public class SequentialAutoTimeMachine
    {
        public List<(Action action, double interval)> Sequence { get; private set; }

        private readonly TimeMachine machine;

        private int idx = 0;

        public SequentialAutoTimeMachine(params (Action action, double interval)[] sequence)
        {
            machine = new TimeMachine();
            Sequence = new List<(Action action, double interval)>(sequence);
        }

        /// <summary>
        /// Forwards the time by given amount, triggers assigned action relevant amount of times
        /// </summary>
        /// <param name="time">Amount of time to forward by</param>
        public void Forward(double time)
        {
            machine.Accumulate(time);
            while (machine.TryRetrieve(Sequence[idx].interval))
            {
                Sequence[idx].action();
                idx++;
                idx = idx % Sequence.Count;
            }
        }
    }
}
