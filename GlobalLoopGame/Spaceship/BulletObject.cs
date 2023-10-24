using GlobalLoopGame.Asteroid;
using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using MonoEngine.Scenes;
using MonoEngine.Util;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Spaceship
{
    public class BulletObject : PhysicsBodyObject
    {
        public DrawableObject asteroidFixture;

        public int damage = 10;

        private AutoTimeMachine despawner;

        private bool destroyted = false;

        public BulletObject(World world) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Tag = this;
            PhysicsBody.Position = Vector2.One * 1024f;

            var visuals = AddDrawableRectFixture(new(0.5f, 2f), new(0f, 0f), 0, out var fixture);
            visuals.Color = Color.Red;

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3, bullets - 4
            fixture.CollisionCategories = Category.Cat4;
            fixture.CollidesWith = Category.None;
            fixture.CollidesWith |= Category.Cat1;

            PhysicsBody.OnCollision += (sender, other, contact) =>
            {
                if (destroyted)
                    return false;

                destroyted = true;

                AsteroidObject otherAsteroid = other.Body.Tag as AsteroidObject;

                if (otherAsteroid != null)
                {
                    otherAsteroid.ModifyHealth(-damage);
                }

                Despawn();

                return false;
            };

            despawner = new AutoTimeMachine(Despawn, 10f);
        }

        public BulletObject InitializeBullet(Vector2 startingPosition, Vector2 startingVelocity, float startingSpeed)
        {
            Transform.LocalPosition = startingPosition;
            PhysicsBody.LinearVelocity = startingVelocity * startingSpeed;

            Transform.LocalRotation = MathF.Atan2(startingVelocity.Y, startingVelocity.X) + MathF.PI / 2f;
            
            return this;
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            despawner.Forward(time.ElapsedGameTime.TotalSeconds);
        }

        private void Despawn()
        {
            PhysicsBody.World.RemoveAsync(PhysicsBody);
            CurrentScene.RemoveObject(this);
        }
    }
}
