using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Spaceship.Dragging;
using Microsoft.Xna.Framework;
using MonoEngine.Math;
using MonoEngine.Physics;
using MonoEngine.Scenes;
using MonoEngine.Util;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Dynamics;
using System;

namespace GlobalLoopGame.Spaceship
{
    public class TurretStation : PhysicsBodyObject, IDraggable, IResettable
    {
        public float Range { get; set; } = 32f;

        protected AutoTimeMachine shootingTimer;
        protected AsteroidManager asteroids;
        protected HierarchyObject barrel;

        protected float spread = 5f * MathF.PI / 180f;

        protected int bulletCount = 1;

        private bool canShoot = false;

        private Vector2 startingPosition = Vector2.Zero;

        private float barrelLength;

        //TODO pass textures
        public TurretStation(World world, AsteroidManager asteroids, float cooldown = 0.125f) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Tag = this;
            PhysicsBody.AngularDamping = 1f;
            PhysicsBody.LinearDamping = 1f;
            var drawable = AddDrawableRectFixture(GameSprites.TurretBaseSize, Vector2.Zero, 0f, out var fixture, 0.125f);
            drawable.Sprite = GameSprites.TurretBase;

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3
            fixture.CollisionCategories = Category.Cat3;
            fixture.CollidesWith = Category.None;
            fixture.CollidesWith |= Category.Cat2;
            fixture.CollidesWith |= Category.Cat3;
            fixture.CollidesWith |= Category.Cat5;

            var barrel = new DrawableObject(Color.White, 0.1f);
            barrel.Sprite = GameSprites.TurretCannon[0];

            var ratio = 17f / GameSprites.pixelsPerUnit; 
            barrel.Transform.LocalPosition = new Vector2(0f, ratio);
            barrel.Transform.LocalScale = GameSprites.TurretCannonSizes[0];

            barrelLength = barrel.Transform.LocalPosition.Y + barrel.Transform.LocalScale.Y;

            var barrelRoot = new HierarchyObject();
            barrel.Parent = barrelRoot;
            barrelRoot.Parent = this;
            this.barrel = barrelRoot;
            
            this.asteroids = asteroids;

            shootingTimer = new AutoTimeMachine(TargetAndShoot, cooldown);
        }

        protected virtual void TargetAndShoot()
        {
            if (!canShoot)
            {
                return;
            }

            var target = FindTarget();

            if (target != null)
            {
                var dir = target.Transform.GlobalPosition - Transform.GlobalPosition;
                dir.Normalize();
                
                barrel.Transform.GlobalRotation = MathF.Atan2(dir.Y, dir.X) - MathF.PI / 2f;

                // spread gets narrower the smaller the target is
                //spread = (30f - target.size.X) * MathF.PI / 180f;
                for (int i = 0; i < bulletCount; i++)
                {
                    var d1 = Matrix2x2.Rotation(Random.Shared.NextSingle() * spread - spread / 2f) * dir;
                    CurrentScene.AddObject(CreateProjectile(d1, Transform.GlobalPosition + barrel.Transform.Up * barrelLength / 2f));
                }
            }
        }

        protected virtual AsteroidObject FindTarget()
        {
            AsteroidObject closest = null;

            var closestDist = Range;

            foreach (var asteroid in asteroids.asteroids)
            {
                var delta = asteroid.Transform.GlobalPosition - Transform.GlobalPosition;
                var distance = delta.Length();
                if (distance < closestDist)
                {
                    closestDist = distance;
                    closest = asteroid;
                }
            }

            return closest;
        }

        protected virtual BulletObject CreateProjectile(Vector2 dir, Vector2 spawnPos)
        {
            return new BulletObject(PhysicsBody.World).InitializeBullet(spawnPos, dir, 128f);
        }

        public override void Update(GameTime time)
        {
            if (canShoot)
            {
                shootingTimer.Forward(time.ElapsedGameTime.TotalSeconds);
            }

            base.Update(time);
        }

        public void SetStartingPosition(Vector2 pos)
        {
            startingPosition = pos;

            Transform.LocalPosition = startingPosition;
        }

        public void OnBecomeDragged()
        {
            canShoot = false;

            GameSounds.pickupTurretSound.Play();

            GameSounds.magnetEmitter.Play();
        }

        public void OnBecomeDropped()
        {
            canShoot = true;

            GameSounds.dropTurretSound.Play();

            GameSounds.magnetEmitter.Pause();
        }

        public void OnGameEnd()
        {
            canShoot = false;
        }

        public void Reset()
        {
            Transform.LocalPosition = startingPosition;

            canShoot = true;
        }
    }
}
