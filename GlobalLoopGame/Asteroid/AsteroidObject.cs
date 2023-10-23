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

        private AsteroidManager manager;
        public Vector2 velocity { get; private set; }
        public float speed { get; private set; }
        public float health { get; private set; }
        public DrawableObject asteroidDrawable { get; private set; }
        private bool isDead = false;

        private float maxHealth = 100f;

        public AsteroidObject(World world, float drawOrder) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Position = new Vector2(9001f, 9001f);
            PhysicsBody.Tag = this;
        }

        public void InitializeAsteroid(AsteroidManager aManager, AsteroidPlacement placement)
        {
            manager = aManager;
            velocity = placement.velocity;
            speed = placement.speed;
            Transform.LocalPosition = placement.location;
            maxHealth = placement.maxHealth;

            health = maxHealth;
            PhysicsBody.LinearVelocity = velocity * speed;

            asteroidDrawable = AddDrawableRectFixture(placement.size, new(0f, 0f), Random.Shared.NextSingle() * 2 * MathF.PI, out var fixture);
            asteroidDrawable.Color = Color.Gray;

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3, bullets - 4
            fixture.CollisionCategories = Category.Cat1;
            fixture.CollidesWith = Category.None;
            fixture.CollidesWith |= Category.Cat2;
            fixture.CollidesWith |= Category.Cat4;
        }

        public void ModifyHealth(float healthModification)
        {
            health = MathHelper.Clamp(health + healthModification, 0, maxHealth);

            if (health <= 0)
            {
                Die();
            }
        }

        void Die()
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
