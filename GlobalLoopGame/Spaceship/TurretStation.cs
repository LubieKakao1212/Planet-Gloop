using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Mesh;
using GlobalLoopGame.Spaceship.Dragging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Math;
using MonoEngine.Physics;
using MonoEngine.Rendering;
using MonoEngine.Rendering.Data;
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
        public float RangeRadius { get; set; } = 32f;

        private const int meshResolution = 4095;

        //protected AutoTimeMachine shootingTimer;
        //protected SequentialAutoTimeMachine shootingTimer;
        protected TimeMachine chargeTimer = new TimeMachine();
        protected TimeMachine cooldownTimer = new TimeMachine();
        //protected AutoTimeMachine targetSeekerMachine;

        protected AsteroidManager asteroids;
        protected HierarchyObject barrel;
        protected DrawableObject barrelDrawable;
        protected MeshObject rangeDisplay;

        protected float spread = 5f * MathF.PI / 180f;

        protected int bulletCount = 1;
        protected bool willReload = false;

        private bool canShoot = false;

        private Vector2 startingPosition = Vector2.Zero;

        private float barrelLength;
        private Sprite[] sprites;
        private Vector2[] spriteScales;

        private float grabTimer;
        private bool grabbed;

        private TextObject popupDescription;

        protected int damage = 10;
        protected int shotIndex = 0;

        protected float chargeTime; 
        protected float cooldown;
        protected bool onCooldown = false;

        protected AsteroidObject target;

        protected Vector2 predictedTargetDirection;

        public TurretStation(World world, AsteroidManager asteroids, RenderPipeline renderer, float cooldown = 0.125f, float chargeTime = 0f) : base(null)
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
            popupDescription = new TextObject();
            popupDescription.Color = Color.White;
            popupDescription.Parent = this;
            popupDescription.FontSize = 12;
            UpdateText();

            /*rangeDisplay = new DrawableObject(Color.White * 0.125f, -10f);
            rangeDisplay.Sprite = GameSprites.Circle;
            rangeDisplay.Transform.LocalScale = Vector2.Zero;
            rangeDisplay.Parent = this;*/

            rangeDisplay = MeshObject.CreateNew(renderer, Vertex2DPosition.VertexDeclaration, new Vertex2DPosition[meshResolution + 1], new int[meshResolution * 3], Color.White, -10f, GameEffects.Custom, GameEffects.DSS);
            rangeDisplay.Parent = this;

            var barrelRoot = new HierarchyObject();
            barrelDrawable.Parent = barrelRoot;
            barrelRoot.Parent = this;
            barrel = barrelRoot;
            
            this.asteroids = asteroids;

            this.cooldown = cooldown;
            this.chargeTime = chargeTime;

            //shootingTimer = new AutoTimeMachine(TargetAndShoot, cooldown);
            /*shootingTimer = new SequentialAutoTimeMachine(
                (TargetAndShoot, cooldown),
                (Reload, cooldown + 1)
                );*/
        }

        /// <summary>
        /// Shoots, does not check if it can
        /// </summary>
        protected virtual void Shoot()
        {
            var spawnPos = Transform.GlobalPosition + barrel.Transform.Up * barrelLength / 2f;

            GameSounds.shotSounds[shotIndex].Play();

            for (int i = 0; i < bulletCount; i++)
            {
                CurrentScene.AddObject(CreateProjectile(predictedTargetDirection, spawnPos));
            }

            willReload = true;
        }

        protected virtual void Reload()
        {
            willReload = false;
        }
        
        protected virtual AsteroidObject FindTarget()
        {
            AsteroidObject closest = null;

            var closestDist = RangeRadius;

            foreach (var asteroid in asteroids.asteroids)
            {
                var delta = asteroid.Transform.GlobalPosition - Transform.GlobalPosition;
                var distance = delta.Length();
                if (distance < closestDist)
                {
                    bool lineOfSight = true;
                    PhysicsBody.World.RayCast((fixture, point, normal, fraction) =>
                    {
                        //the planet
                        if (fixture.CollisionCategories.HasFlag(Category.Cat5))
                        {
                            lineOfSight = false;
                            return 0f;
                        }
                        return 1f;
                    }, Transform.GlobalPosition, asteroid.Transform.GlobalPosition);
                    if (!lineOfSight)
                        continue;

                    closestDist = distance;
                    closest = asteroid;
                }
            }

            return closest;
        }

        protected virtual BulletObject CreateProjectile(Vector2 dir, Vector2 spawnPos)
        {
            var bulletSpeed = GetBulletSpeed();
            dir = RandomizeDirection(dir, spread);
            return CreateBullet(dir, spawnPos, bulletSpeed);
        }

        /// <summary>
        /// Sets the target asteroid and points at it
        /// </summary>
        protected virtual void LockTarget()
        {
            var dir = target.Transform.GlobalPosition - Transform.GlobalPosition;
            var dist = dir.Length();

            dir = PredictAim(Transform.GlobalPosition, GetBulletSpeed(), target.Transform.GlobalPosition, target.PhysicsBody.LinearVelocity, dist);

            predictedTargetDirection = dir;
            barrel.Transform.GlobalRotation = MathF.Atan2(dir.Y, dir.X) - MathF.PI / 2f;          
        }

        public override void Update(GameTime time)
        {
            float dt = (float)time.ElapsedGameTime.TotalSeconds;
            if (canShoot)
            {
                if (!onCooldown)
                {
                    target = FindTarget();
                    if (target != null)
                    {
                        LockTarget();
                        chargeTimer.Accumulate(dt);
                        if (chargeTimer.TryRetrieve(chargeTime))
                        {
                            chargeTimer.Retrieve(float.PositiveInfinity);
                            Shoot();
                            onCooldown = true;
                        }
                    }
                    else
                    {
                        chargeTimer.Retrieve(float.PositiveInfinity);
                    }
                }
                else
                {
                    cooldownTimer.Accumulate(dt);
                    if (cooldownTimer.TryRetrieve(cooldown))
                    {
                        cooldownTimer.Retrieve(float.PositiveInfinity);
                        onCooldown = false;
                    }
                }
            }else
            {
                chargeTimer.Retrieve(float.PositiveInfinity);
            }

            if (grabbed)
            {
                grabTimer += dt;
            }
            else
            {
                grabTimer -= dt;
            }

            grabTimer = MathHelper.Clamp(grabTimer, 0f, 1f);
            //rangeDisplay.Transform.LocalScale = Vector2.Lerp(Vector2.Zero, Vector2.One * RangeRadius * 2f, grabTimer);

            UpdateRangeMesh(RangeRadius * 2f * grabTimer);

            popupDescription.Transform.GlobalPosition = Transform.GlobalPosition + Vector2.UnitY * 10f;
            popupDescription.Transform.GlobalRotation = 0;
            popupDescription.Color = Color.Lerp(Color.White, Color.Transparent, 1f - Math.Min(grabTimer * 3f, 1f));

            base.Update(time);
        }

        private void UpdateRangeMesh(float radius)
        {
            ViewMesh.CalculateMesh(PhysicsBody.World, Transform.GlobalPosition, radius, meshResolution, out var verts, out var inds, Category.Cat5);
            rangeDisplay.UpdateMesh(verts, inds);
            rangeDisplay.Transform.GlobalRotation = 0f;
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
        
        public void OnBecomeDragged(IDragger dragger)
        {
            canShoot = false;

            SoundEffectInstance pickupInstance = GameSounds.pickupTurretSound.CreateInstance();

            pickupInstance.Volume = 0.5f;

            pickupInstance.Play();

            GameSounds.magnetEmitter.Play();

            UpdateSprite(1);
            grabbed = true;

            SpaceshipObject spaceship = dragger.ThisObject as SpaceshipObject;

            if (spaceship != null)
            {
                spaceship.magnetObject.Sprite = GameSprites.SpaceshipMagnetActive;
                spaceship.magnetObject.Transform.LocalScale = GameSprites.SpaceshipMagnetSizeActive;
            }
        }
        
        public void OnBecomeDropped(IDragger dragger)
        {
            canShoot = true;

            SoundEffectInstance dropInstance = GameSounds.dropTurretSound.CreateInstance();

            dropInstance.Volume = 0.5f;

            dropInstance.Play();

            GameSounds.magnetEmitter.Pause();

            UpdateSprite(0);
            
            grabbed = false;

            SpaceshipObject spaceship = dragger.ThisObject as SpaceshipObject;

            if (spaceship != null)
            {
                spaceship.magnetObject.Sprite = GameSprites.SpaceshipMagnet;
                spaceship.magnetObject.Transform.LocalScale = GameSprites.SpaceshipMagnetSize;
                spaceship.magnetObject.Transform.LocalRotation = 0f;
            }
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

        protected Vector2 PredictAim(Vector2 spawnPos, float bulletSpeed, Vector2 targetPosition, Vector2 targetVelocity, float distanceToTarget)
        {
            var predictedTargetPos = targetPosition + targetVelocity * (distanceToTarget / bulletSpeed);
            var deltaPos = -(spawnPos - predictedTargetPos);
            var l = deltaPos.Length();
            if (l < 0.001f)
            {
                return Vector2.UnitY;
            }
            return deltaPos / l;
        }

        protected void UpdateText()
        {
            popupDescription.Text = $"Damage: {damage}";
        }

        protected Vector2 RandomizeDirection(Vector2 direction, float spread)
        {
            return Matrix2x2.Rotation(Random.Shared.NextSingle() * spread - spread / 2f) * direction;
        }

        protected virtual BulletObject CreateBullet(Vector2 dir, Vector2 pos, float bulletSpeed)
        {
            return new BulletObject(PhysicsBody.World).InitializeBullet(pos, dir, bulletSpeed).SetColor(Color.LightGray);
        }

        protected virtual float GetBulletSpeed()
        {
            return 128f;
        }
    }
}
