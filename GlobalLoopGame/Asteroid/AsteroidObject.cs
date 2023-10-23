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
        public Vector2 velocity;
        public float speed;
        public DrawableObject asteroidFixture;

        public AsteroidObject(World world, float drawOrder) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Tag = this;

            asteroidFixture = AddDrawableRectFixture(new(2f, 2f), new(0f, 0f), 0, out var fixture);
            fixture.CollisionCategories = Category.Cat1;
            fixture.CollidesWith |= Category.Cat2;
            fixture.CollidesWith |= Category.Cat3;
        }

        public void InitializeAsteroid(Vector2 startingPosition, Vector2 startingVelocity, float startingSpeed)
        {
            velocity = startingVelocity;
            speed = startingSpeed;
            Transform.LocalPosition = startingPosition;
            PhysicsBody.LinearVelocity = startingVelocity * startingSpeed;

            var drawable = new DrawableObject(Color.Red, 1f);
            drawable.Transform.LocalRotation = Random.Shared.NextSingle() * 2 * MathF.PI;
            drawable.Parent = this;
        }
    }
}
