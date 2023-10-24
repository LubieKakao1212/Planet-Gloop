using GlobalLoopGame.Spaceship;
using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Updaters
{
    public class BoundryFieldComponent : IGameComponent, IUpdateable
    {
        public bool Enabled => true;

        public int UpdateOrder => 0;

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        private PhysicsBodyObject[] affectedObjects;
        private float force;
        private float radius;

        public BoundryFieldComponent(float radius, float force, params PhysicsBodyObject[] affectedObjects)
        {
            this.force = force;
            this.affectedObjects = affectedObjects;
            this.radius = radius;
        }

        public void Update(GameTime gameTime)
        {
            foreach (var obj in affectedObjects)
            {
                var pos = obj.Transform.GlobalPosition;
                var force = pos.Length() - radius;
                force = MathF.Max(force, 0) * this.force;
                if (force > 0)
                {
                    pos.Normalize();

                    obj.PhysicsBody.ApplyForce(-pos * force);
                }
            }
        }

        public void Initialize()
        {
        }
    }
}
