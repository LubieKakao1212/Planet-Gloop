using Custom2d_Engine.Math;
using Custom2d_Engine.Physics;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Spaceship.Turret
{
    public interface ITargettable
    {
        public Transform Transform { get; }

        public Body PhysicsBody { get; }

        public float Health { get; }
    }
}
