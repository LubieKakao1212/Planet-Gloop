using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Globals;
using Microsoft.Xna.Framework;
using Custom2d_Engine.Physics;
using Custom2d_Engine.Scenes;
using Custom2d_Engine.Util;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Custom2d_Engine.Ticking;

namespace GlobalLoopGame.Spaceship
{
    public class BulletObject : PhysicsBodyObject
    {
        public DrawableObject asteroidFixture;

        public int damage = 10;
        public int pierce = 0;
        public bool damageIsPierce;

        private AutoTimeMachine despawner;

        private bool destroyed = false;

        List<AsteroidObject> hitAsteroids = new List<AsteroidObject>();

        public BulletObject(World world) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Tag = this;
            PhysicsBody.Position = Vector2.One * 1024f;

            var visuals = AddDrawableRectFixture(GameSprites.LaserSize, new(0f, 0f), 0, out var fixture, 0.01f);
            visuals.Color = Color.White;
            visuals.Sprite = GameSprites.Laser;

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3, bullets - 4
            fixture.CollisionCategories = CollisionCats.Bullets;
            fixture.CollidesWith = CollisionCats.CollisionsBullets;

            PhysicsBody.OnCollision += (sender, other, contact) =>
            {
                if (destroyed)
                    return false;

                AsteroidObject otherAsteroid = other.Body.Tag as AsteroidObject;

                if (otherAsteroid != null)
                {
                    if (!hitAsteroids.Contains(otherAsteroid))
                    {
                        contact.GetWorldManifold(out var normal, out var points);

                        CurrentScene.AddObject(new ExplosionParticleObject(PhysicsBody.World).InitializeParticle(points[0]));

                        OnAsteroidHit(otherAsteroid);

                        hitAsteroids.Add(otherAsteroid);
                    }
                }
                else
                {
                    Despawn();
                }

                return false;
            };

            despawner = new AutoTimeMachine(Despawn, 1f);
        }

        public BulletObject InitializeBullet(Vector2 startingPosition, Vector2 startingVelocity, float startingSpeed)
        {
            Transform.LocalPosition = startingPosition;
            PhysicsBody.LinearVelocity = startingVelocity * startingSpeed;

            Transform.LocalRotation = MathF.Atan2(startingVelocity.Y, startingVelocity.X) - MathF.PI / 2f;
            
            return this;
        }

        public BulletObject SetLifetime(float lifetime)
        {
            despawner.Interval = lifetime;
            return this;
        }

        public BulletObject SetDamage(int damage)
        {
            this.damage = damage;
            return this;
        }

        public BulletObject SetColor(Color color)
        {
            ((DrawableObject)Children[0]).Color = color;
            return this;
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            despawner.Forward(time.ElapsedGameTime.TotalSeconds);
        }

        private void Despawn()
        {
            hitAsteroids.Clear();
            destroyed = true;
            PhysicsBody.World.RemoveAsync(PhysicsBody);
            CurrentScene.RemoveObject(this);
        }

        protected virtual void OnAsteroidHit(AsteroidObject asteroid)
        {
            var dmg = damage;

            if (damageIsPierce)
            {
                damage -= (int)asteroid.health;
            }
            else
            {
                pierce -= (int)asteroid.health;
            }

            asteroid.ModifyHealth(-dmg);

            if (pierce <= 0 || damage <= 0)
            {
                Despawn();
            }
        }
    }
}
