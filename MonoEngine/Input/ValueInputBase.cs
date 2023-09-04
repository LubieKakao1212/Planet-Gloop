using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Input
{
    public abstract class ValueInputBase<T> : IInput
    {
        public event Action<IInput> Started;
        public event Action<IInput> Performed;
        public event Action<IInput> Canceled;

        protected abstract T Value { get; }

        public abstract string FriendlyName { get; }

        public T1 GetCurrentValue<T1>()
        {
            var v = Value;
            if (v is T1 v1)
            {
                return v1;
            }
            else
            {
                throw new ArgumentException($"Cannot get ${typeof(T1).Name} from input of with value type ${typeof(T).Name}");
            }
        }

        protected void InvokeEvents(bool state, bool changed)
        {
            if (!state && changed)
            {
                Canceled?.Invoke(this);
            }
            else if(state)
            {
                if (changed)
                {
                    Started?.Invoke(this);
                }
                else
                {
                    Performed?.Invoke(this);
                }
            }
        }

        protected void PassStarted(IInput _) => Started?.Invoke(this);
        protected void PassPerformed(IInput _) => Performed?.Invoke(this);
        protected void PassCanceled(IInput _) => Canceled?.Invoke(this);
    }
}
