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
        private float outerForce;
        private float innerForce;
        private float outerRadius;
        private float innerRadius;

        public BoundryFieldComponent(float outerRadius, float outerForce, float innerRadius, float innerForce, params PhysicsBodyObject[] affectedObjects)
        {
            this.outerForce = outerForce;
            this.innerForce = innerForce;
            this.affectedObjects = affectedObjects;
            this.outerRadius = outerRadius;
            this.innerRadius = innerRadius;
        }

        public void Update(GameTime gameTime)
        {
            foreach (var obj in affectedObjects)
            {
                var pos = obj.Transform.GlobalPosition;
                var distance = pos.Length();
                var forceO = (distance - outerRadius) * outerForce;
                var forceI = (distance - innerRadius) * innerForce;
                var force = MathF.Min(MathF.Max(-forceI, 0), -forceO);
                if (distance > 0.001f && force != 0)
                {
                    pos.Normalize();

                    obj.PhysicsBody.ApplyForce(pos * force);
                }
            }
        }

        public void Initialize()
        {
        }
    }
}
