using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Input.Binding
{
    public class UnboundInput : IInput
    {
        public static readonly UnboundInput Value = new UnboundInput();

        public string FriendlyName => "Unbound";

        public event Action<IInput> Started;
        public event Action<IInput> Performed;
        public event Action<IInput> Canceled;

        public T GetCurrentValue<T>()
        {
            return default;
        }
    }
}
