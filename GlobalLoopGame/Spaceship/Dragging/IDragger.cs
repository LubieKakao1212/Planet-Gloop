using Custom2D_Engine.Physics;
using nkast.Aether.Physics2D.Dynamics.Joints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Spaceship.Dragging
{
    public interface IDragger
    {
        Joint CurrentDrag { get; set; }

        PhysicsBodyObject ThisObject { get; }
    }
}
