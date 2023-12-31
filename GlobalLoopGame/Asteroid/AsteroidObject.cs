using Custom2d_Engine.Physics;
using Custom2d_Engine.Scenes;
using Custom2d_Engine.Ticking;
using GlobalLoopGame.Globals;
using GlobalLoopGame.Planet;
using GlobalLoopGame.Spaceship.Turret;
using GlobalLoopGame.UI;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using System;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidObject : PhysicsBodyObject, ITargettable
    {
        private AsteroidManager manager;
        public Vector2 velocity { get; private set; }
        public float speed { get; private set; }
        public float health { get; private set; }
        public float healthToDisplay { get => (health / maxHealth); }
        public Vector2 size { get; private set; }
        public DrawableObject asteroidDrawable { get; private set; }

        float ITargettable.Health => health;

        private bool isDead = false;

        private float maxHealth = 100f;
        private int damage = 1;

        private AutoTimeMachine despawner;
        private Bar healthBar;

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

                    contact.GetWorldManifold(out var normal, out var points);

                    ExplosionParticleObject epo = new ExplosionParticleObject(PhysicsBody.World).InitializeParticle(points[0]);

                    epo.sizeModifier = 4f;

                    CurrentScene.AddObject(epo);

                    Die();

                    return false;
                }
                else if (other.CollisionCategories.HasFlag(CollisionCats.Shield))
                {
                    manager.game.cameraManager.SetCameraShake(true, 6);

                    Die();
                }

                return true;
            };

            despawner = new AutoTimeMachine(Die, 60f);
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
            PhysicsBody.AngularVelocity = Random.Shared.NextSingle() * 0.5f;

            asteroidDrawable = AddDrawableRectFixture(placement.size, new(0f, 0f), Random.Shared.NextSingle() * 2 * MathF.PI, out var fixture);
            asteroidDrawable.Color = Color.Gray;
            asteroidDrawable.Sprite = placement.size.X > 5f ? GameSprites.LargeAsteroid : GameSprites.SmallAsteroid;

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3, bullets - 4
            fixture.CollisionCategories = CollisionCats.Asteroids;
            fixture.CollidesWith = CollisionCats.CollisionsAsteroids;

            healthBar = new Bar(() => healthToDisplay, Color.Red, Color.Red, Color.White);
            healthBar.Parent = this;
            healthBar.ToggleVisibility(false);

            if (health > 200)
            {
                damage = 2;
            }
            else
            {
                damage = 1;
            }
        }

        public override void Update(GameTime time)
        {
            base.Update(time);

            despawner.Forward(time.ElapsedGameTime.TotalSeconds);

            healthBar.Transform.LocalRotation = -Transform.LocalRotation;

            Vector2 thetaVector = new Vector2(MathF.Cos(healthBar.Transform.LocalRotation), MathF.Sin(healthBar.Transform.LocalRotation));

            healthBar.Transform.LocalPosition = (thetaVector);
        }

        public void ModifyHealth(float healthModification)
        {
            health = MathHelper.Clamp(health + healthModification, 0, maxHealth);

            healthBar.ToggleVisibility(health > 0 && health < maxHealth);

            if (health <= 0)
            {
                // score modification is based on maxHealth and speed
                int pointModification = (int)MathF.Round(maxHealth * (20f - speed)/2);

                manager.ModifyPoints(pointModification);

                Die();
            }
            else
            {
                //GameSounds.asteroidHurtSound.Play();
                GameSounds.PlaySound(GameSounds.asteroidHurtSound, 2);
            }
        }

        public void Die()
        {
            if (isDead)
                return;

            isDead = true;

            if (size.X > 5f)
            {
                // GameSounds.bigAsteroidDeath.Play();
                GameSounds.PlaySound(GameSounds.bigAsteroidDeath, 2);
            }
            else
            {
                //GameSounds.smallAsteroidDeath.Play();
                GameSounds.PlaySound(GameSounds.smallAsteroidDeath, 2);
            }

            var u = Transform.Up;
            var r = Transform.Right;

            CurrentScene.AddObject(new AsteroidParticleObject(PhysicsBody.World).InitializeParticle(this, u + r));
            CurrentScene.AddObject(new AsteroidParticleObject(PhysicsBody.World).InitializeParticle(this, -u + r));
            CurrentScene.AddObject(new AsteroidParticleObject(PhysicsBody.World).InitializeParticle(this, -u - r));
            CurrentScene.AddObject(new AsteroidParticleObject(PhysicsBody.World).InitializeParticle(this, u - r));

            //CurrentScene.RemoveObject(healthBar);

            manager.RemoveAsteroid(this);
            PhysicsBody.World.RemoveAsync(PhysicsBody);
            CurrentScene.RemoveObject(this);
        }
    }
}
