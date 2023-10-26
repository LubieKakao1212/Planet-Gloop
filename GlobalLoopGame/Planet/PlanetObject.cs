using GlobalLoopGame.Asteroid;
using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using MonoEngine.Rendering;
using MonoEngine.Scenes;
using MonoEngine.Util;
using nkast.Aether.Physics2D.Dynamics;
using System;

namespace GlobalLoopGame.Planet
{
    public class PlanetObject : PhysicsBodyObject, IResettable
    {
        public event Action<int> HealthChange;

        public GlobalLoopGame game;
        public int health {  get; private set; }
        private int maxHealth = 1;
        public bool isDead { get; private set; }
        public bool shouldDie { get; private set; }

        public PlanetObject(World world, RenderPipeline renderer) : base(null)
        {
            Order = 10f;

            PhysicsBody = world.CreateBody(bodyType: BodyType.Kinematic);
            PhysicsBody.Tag = this;
            PhysicsBody.AngularVelocity = 0.25f;

            var shield = new SegmentedShield(world, renderer, 6, 20f, MathHelper.TwoPi, 3);
            shield.Parent = this;

            var fixture = PhysicsBody.CreateCircle(12f, 0f);
            var drawable = new DrawableObject(Color.White, -1f);
            drawable.Parent = this;
            drawable.Transform.LocalPosition = Vector2.Zero;
            drawable.Transform.LocalRotation = 0f;
            drawable.Transform.LocalScale = Vector2.One * 24f;
            drawable.Sprite = GameSprites.Planet;

            fixture.Friction = 1f;
            fixture.Restitution = 0f;

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3, bullets - 4
            fixture.CollisionCategories = CollisionCats.Planet;
            fixture.CollidesWith = CollisionCats.CollisionsPlanet;
            
            health = maxHealth;
        }

        public void ModifyHealth(int healthModification)
        {
            health = MathHelper.Clamp(health + healthModification, 0, maxHealth);

            Console.WriteLine("health " + health.ToString());

            if (healthModification < 0)
            {
                //GameSounds.planetHurtSound.Play();

                GameSounds.PlaySound(GameSounds.planetHurtSound, 2);

                if (game.asteroidManager.difficulty > 3)
                {
                    game.asteroidManager.ModifyDifficulty(-1);
                }
            }

            HealthChange?.Invoke(health);

            if (!isDead && health <= 0)
            {
                shouldDie = true;
            }
        }

        public void OnGameEnd()
        {

        }

        public void Reset()
        {
            isDead = false;
            shouldDie = false;

            health = maxHealth;
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            if(shouldDie && !isDead)
            {
                Die();
            }
            //Transform.LocalRotation += (float)time.ElapsedGameTime.TotalSeconds / 3f;
        }

        void Die()
        {
            isDead = true;

            if (game != null)
            {
                game.EndGame();
            }
        }
    }
}
