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
    public class TurretStation : PhysicsBodyObject, IDraggable
    {
        public float Range { get; set; } = 32f;

        private AutoTimeMachine shootingTimer;
        private AsteroidManager asteroids;
        private HierarchyObject barrel;

        private float spread = 27f * MathF.PI / 180f;

        private bool canShoot = false;

        public TurretStation(World world, AsteroidManager asteroids) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Tag = this;
            PhysicsBody.AngularDamping = 1f;
            PhysicsBody.LinearDamping = 1f;
            var drawable = AddDrawableRectFixture(new Vector2(3f, 3f), Vector2.Zero, 0f, out var fixture, 0.25f);

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3
            fixture.CollisionCategories = Category.Cat3;
            fixture.CollidesWith = Category.None;
            fixture.CollidesWith |= Category.Cat2;
            fixture.CollidesWith |= Category.Cat3;

            var barrel = new DrawableObject(Color.Gray, 0.1f);
            barrel.Transform.LocalPosition = new Vector2(0f, 1f);
            barrel.Transform.LocalScale = new Vector2(1f, 4f);
            var barrelRoot = new HierarchyObject();
            barrel.Parent = barrelRoot;
            barrelRoot.Parent = this;
            this.barrel = barrelRoot;
            
            this.asteroids = asteroids;

            canShoot = true;

            shootingTimer = new AutoTimeMachine(TargetAndShoot, 0.125f);
        }

        private void TargetAndShoot()
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
                dir = Matrix2x2.Rotation(Random.Shared.NextSingle() * spread - spread / 2f) * dir;
                CurrentScene.AddObject(new BulletObject(PhysicsBody.World).InitializeBullet(Transform.GlobalPosition, dir, 128f));
            }
        }

        private AsteroidObject FindTarget()
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

        public override void Update(GameTime time)
        {
            if (canShoot)
            {
                shootingTimer.Forward(time.ElapsedGameTime.TotalSeconds);
            }

            base.Update(time);
        }

        public void OnBecomeDragged()
        {
            canShoot = false;
        }

        public void OnBecomeDropped()
        {
            canShoot = true;
        }
    }
}
