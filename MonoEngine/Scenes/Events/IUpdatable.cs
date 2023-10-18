using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Scenes.Events
{
    public interface IUpdatable : IOrdered
    {
        public void Update(GameTime time);
    }
}
