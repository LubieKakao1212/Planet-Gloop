using GlobalLoopGame.Planet;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MonoEngine.Physics;
using MonoEngine.Scenes;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using static System.Net.Mime.MediaTypeNames;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidObject : PhysicsBodyObject
    {
        private AsteroidManager manager;
        public Vector2 velocity { get; private set; }
        public float speed { get; private set; }
        public float health { get; private set; }
        public Vector2 size { get; private set; }
        public DrawableObject asteroidDrawable { get; private set; }
        private bool isDead = false;

        private float maxHealth = 100f;
        private int damage = 1;

        public AsteroidObject(World world, float drawOrder) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Position = new Vector2(9001f, 9001f);
            PhysicsBody.Tag = this;

            PhysicsBody.OnCollision += (sender, other, contact) =>
            {
                if (isDead)
                    return false;

                PlanetObject planet = other.Body.Tag as PlanetObject;

                if (planet != null)
                {
                    planet.ModifyHealth(-damage);

                    Die();
                }

                return false;
            };
        }

        public void InitializeAsteroid(AsteroidManager aManager, AsteroidPlacement placement)
        {
            manager = aManager;
            velocity = placement.velocity;
            speed = placement.speed;
            Transform.LocalPosition = placement.location;
            maxHealth = placement.maxHealth;
            size = placement.size;

            health = maxHealth;
            PhysicsBody.LinearVelocity = velocity * speed;

            asteroidDrawable = AddDrawableRectFixture(placement.size, new(0f, 0f), Random.Shared.NextSingle() * 2 * MathF.PI, out var fixture);
            asteroidDrawable.Color = Color.Gray;
            asteroidDrawable.Sprite = GameSprites.NullSprite;

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3, bullets - 4
            fixture.CollisionCategories = Category.Cat1;
            fixture.CollidesWith = Category.None;
            fixture.CollidesWith |= Category.Cat2;
            fixture.CollidesWith |= Category.Cat4;
            fixture.CollidesWith |= Category.Cat5;
        }

        public void ModifyHealth(float healthModification)
        {
            health = MathHelper.Clamp(health + healthModification, 0, maxHealth);

            if (health <= 0)
            {
                // score modification is based on maxHealth and speed
                int pointModification = (int)MathF.Round(maxHealth * (20f - speed)/2);

                manager.ModifyPoints(pointModification);

                GameSounds.asteroidDeathSound.Play();

                Die();
            }
        }

        public void Die()
        {
            if (isDead)
                return;

            isDead = true;

            var u = Transform.Up;
            var r = Transform.Right;

            CurrentScene.AddObject(new AsteroidParticleObject(PhysicsBody.World).InitializeParticle(this, u + r));
            CurrentScene.AddObject(new AsteroidParticleObject(PhysicsBody.World).InitializeParticle(this, -u + r));
            CurrentScene.AddObject(new AsteroidParticleObject(PhysicsBody.World).InitializeParticle(this, -u - r));
            CurrentScene.AddObject(new AsteroidParticleObject(PhysicsBody.World).InitializeParticle(this, u - r));

            manager.RemoveAsteroid(this);
            PhysicsBody.World.RemoveAsync(PhysicsBody);
            CurrentScene.RemoveObject(this);
        }
    }
}
