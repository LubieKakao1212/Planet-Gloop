using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Spaceship.Dragging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MonoEngine.Math;
using MonoEngine.Physics;
using MonoEngine.Rendering.Sprites;
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
        protected DrawableObject barrelDrawable;

        protected float spread = 5f * MathF.PI / 180f;

        protected int bulletCount = 1;

        private bool canShoot = false;

        private Vector2 startingPosition = Vector2.Zero;

        private float barrelLength;
        private Sprite[] sprites;
        private Vector2[] spriteScales;

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

            barrelDrawable = new DrawableObject(Color.White, 0.1f);
            barrelDrawable.Sprite = GameSprites.TurretCannon[0];

            //var ratio = 17f / GameSprites.pixelsPerUnit;
            // = GameSprites.TurretCannonSizes[0];

            var barrelRoot = new HierarchyObject();
            barrelDrawable.Parent = barrelRoot;
            barrelRoot.Parent = this;
            barrel = barrelRoot;
            
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

        public TurretStation SetSprites(Sprite[] sprites, Vector2[] sizes, Vector2 pivot)
        {
            this.sprites = sprites;
            //this.spriteSizes = sizes;

            barrelDrawable.Transform.LocalScale = sizes[0];
            barrelDrawable.Transform.LocalPosition = pivot;
            spriteScales = new Vector2[2];
            spriteScales[0] = Vector2.One;
            spriteScales[1] = sizes[1] / sizes[0];

            barrelLength = sizes[0].Y - pivot.Y;

            UpdateSprite(0);
            return this;
        }
        
        public void OnBecomeDragged()
        {
            canShoot = false;

            SoundEffectInstance pickupInstance = GameSounds.pickupTurretSound.CreateInstance();

            pickupInstance.Volume = 0.5f;

            pickupInstance.Play();

            GameSounds.magnetEmitter.Play();

            UpdateSprite(1);
        }
        
        public void OnBecomeDropped()
        {
            canShoot = true;

            SoundEffectInstance dropInstance = GameSounds.dropTurretSound.CreateInstance();

            dropInstance.Volume = 0.5f;

            dropInstance.Play();

            GameSounds.magnetEmitter.Pause();

            UpdateSprite(0);
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

        private void UpdateSprite(int idx)
        {
            barrelDrawable.Sprite = sprites[idx];
            barrel.Transform.LocalScale = spriteScales[idx];
        }
    }
}
