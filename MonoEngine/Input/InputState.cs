using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Input
{
    public enum InputState
    {
        Idle = 0,
        Started = 1,
        Performed = 2,
        Canceled = 3
    }
}
