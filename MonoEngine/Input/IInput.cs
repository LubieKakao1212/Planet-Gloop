using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Input
{
    public interface IInput
    {
        public string FriendlyName { get; }

        event Action<IInput> Started;
        event Action<IInput> Performed;
        event Action<IInput> Canceled;

        T GetCurrentValue<T>();
    }
}
