using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Scenes.Events
{
    public interface IOrdered
    {
        public float Order { get; }
    }
}
