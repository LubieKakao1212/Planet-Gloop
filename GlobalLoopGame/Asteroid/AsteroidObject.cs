using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using MonoEngine.Scenes;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidObject : PhysicsBodyObject
    {
        Vector2 velocity;
        float speed;

        public AsteroidObject(World world, float drawOrder) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Tag = this;
            PhysicsBody.AngularDamping = 4f;
            PhysicsBody.LinearDamping = 4f;
        }

        public override void Update(GameTime time)
        {
            PhysicsBody.ApplyForce(velocity * speed, Transform.GlobalPosition);

            base.Update(time);
        }
    }
}
