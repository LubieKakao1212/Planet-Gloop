using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Globals;
using GlobalLoopGame.Mesh;
using GlobalLoopGame.Spaceship.Dragging;
using GlobalLoopGame.UI;
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

namespace GlobalLoopGame.Spaceship.Turret
{
    public class TurretStation : PhysicsBodyObject, IDraggable, IResettable
    {
        public float RangeRadius { get; set; } = 32f;
        public float CloseTargetRange { get; set; } = 16f;

        public float MinTargettingDistance { get; set; } = 7.5f;

        public bool IsDestroyed => false;

        private const int meshResolution = 1024;//4095;

        protected TimeMachine chargeTimer = new TimeMachine();
        protected TimeMachine cooldownTimer = new TimeMachine();

        protected AsteroidManager asteroids;
        protected HierarchyObject barrelPivot;

        protected DrawableObject turretBaseDrawable;
        protected DrawableObject barrelBaseDrawable;
        protected DrawableObject barrelDrawable;

        protected MeshObject rangeDisplay;
        protected MeshObject innerRangeDisplay;

        protected float spread = 5f * MathF.PI / 180f;

        protected int bulletCount = 1;
        protected bool willReload = false;

        private bool canShoot = false;

        private Vector2 startingPosition = Vector2.Zero;
        private float startingRotation = 0f;

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

        protected ITargettable target;

        protected float barrelOffset;

        protected Vector2 predictedTargetDirection;

        public TurretStation(World world, AsteroidManager asteroids, RenderPipeline renderer, float cooldown = 0.125f, float chargeTime = 0f) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Tag = this;
            PhysicsBody.AngularDamping = 1f;
            PhysicsBody.LinearDamping = 1f;

            turretBaseDrawable = AddDrawableRectFixture(GameSprites.TurretBaseSize, Vector2.Zero, 0f, out var fixture, 0.125f);
            turretBaseDrawable.Sprite = GameSprites.TurretBase;

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3
            fixture.CollisionCategories = CollisionCats.Turrets;
            fixture.CollidesWith = CollisionCats.CollisionsTurrets;

            barrelBaseDrawable = new DrawableObject(Color.White, 0.1f);
            barrelDrawable = new DrawableObject(Color.White, 0.1f);

            popupDescription = new TextObject();
            popupDescription.Color = Color.White;
            popupDescription.Parent = this;
            popupDescription.FontSize = 12;
            UpdateText();

            rangeDisplay = MeshObject.CreateNew(renderer, Vertex2DPosition.VertexDeclaration, new Vertex2DPosition[meshResolution * 2], new int[meshResolution * 6], Color.White, -10f, GameEffects.Custom, GameEffects.DSS);
            rangeDisplay.Parent = this;

            innerRangeDisplay = MeshObject.CreateNew(renderer, Vertex2DPosition.VertexDeclaration, new Vertex2DPosition[meshResolution * 2], new int[meshResolution * 6], Color.White, -10f, GameEffects.CustomRed, GameEffects.DSS);
            innerRangeDisplay.Parent = this;


            barrelPivot = new HierarchyObject();
            barrelPivot.Transform.LocalScale = Vector2.One;
            barrelBaseDrawable.Parent = barrelPivot;
            barrelDrawable.Parent = barrelPivot;
            barrelPivot.Parent = this;

            this.asteroids = asteroids;
            this.cooldown = cooldown;
            this.chargeTime = chargeTime;
        }

        /// <summary>
        /// Shoots, does not check if it can
        /// </summary>
        protected virtual void Shoot()
        {
            var spawnPos = Transform.GlobalPosition + barrelPivot.Transform.Up * barrelLength / 2f;

            GameSounds.PlaySound(GameSounds.shotSounds[shotIndex], 2);

            for (int i = 0; i < bulletCount; i++)
            {
                CurrentScene.AddObject(CreateProjectile(predictedTargetDirection, spawnPos));
            }

            barrelOffset = 2f;

            willReload = true;
        }

        protected virtual void Reload()
        {
            willReload = false;
        }

        protected virtual ITargettable FindTarget()
        {
            var world = PhysicsBody.World;

            //Closest
            Func<ITargettable, float, float> fitness = (target, distance) =>
            {
                return distance < MinTargettingDistance ? float.NegativeInfinity : -distance;
            };

            ITargettable target = world.FindTargetPhysicsBased(Transform.GlobalPosition, CloseTargetRange, fitness);
            target ??= asteroids.FindTargetAsteroid(world, Transform.GlobalPosition, RangeRadius, fitness);
            return target;
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
            barrelPivot.Transform.GlobalRotation = MathF.Atan2(dir.Y, dir.X) - MathF.PI / 2f;
        }

        public override void Update(GameTime time)
        {
            GlobalLoopGame.Profiler.Push(GetType().Name);
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
            }
            else
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

            if (grabTimer > 0f)
            {
                UpdateRangeMesh(grabTimer * RangeRadius);
            }


            if (barrelOffset < 0.001f)
            {
                barrelOffset = 0f;
            }
            else
            {
                barrelOffset = barrelOffset + MathF.Sign(-barrelOffset) * (float)time.ElapsedGameTime.TotalSeconds * 8;
            }

            barrelBaseDrawable.Transform.LocalPosition = new Vector2(0, -barrelOffset);
            barrelDrawable.Transform.LocalPosition = new Vector2(0, -barrelOffset);
            
            popupDescription.Transform.GlobalPosition = Transform.GlobalPosition + Vector2.UnitY * 10f;
            popupDescription.Transform.GlobalRotation = 0;
            popupDescription.Color = Color.Lerp(Color.White, Color.Transparent, 1f - Math.Min(grabTimer * 3f, 1f));

            base.Update(time);
            
            GlobalLoopGame.Profiler.Pop(GetType().Name);
        }

        private void UpdateRangeMesh(float factor)
        {
            ViewMesh.CalculateMesh(PhysicsBody.World, Transform.GlobalPosition, MinTargettingDistance, MathF.Max(factor, MinTargettingDistance), meshResolution, out var verts, out var inds, CollisionCats.Planet | CollisionCats.Shield);
            rangeDisplay.UpdateMesh(verts, inds);
            rangeDisplay.Transform.GlobalRotation = 0f;

            ViewMesh.CalculateMesh(PhysicsBody.World, Transform.GlobalPosition, 0.1f, MathF.Min(factor, MinTargettingDistance), meshResolution, out verts, out inds, CollisionCats.Planet | CollisionCats.Shield);
            innerRangeDisplay.UpdateMesh(verts, inds);
            innerRangeDisplay.Transform.GlobalRotation = 0f;
        }

        public void SetStartingPosition(Vector2 pos)
        {
            startingPosition = pos;

            Transform.LocalPosition = startingPosition;

            startingRotation = Transform.LocalRotation;
        }

        public TurretStation SetSprites(Sprite[] sprites, Vector2[] sizes, Vector2 pivot)
        {
            this.sprites = sprites;

            barrelDrawable.Transform.LocalScale = sizes[0];
            barrelBaseDrawable.Transform.LocalScale = sizes[0];

            barrelBaseDrawable.Transform.LocalPosition = pivot;
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

            pickupInstance.Pitch = Random.Shared.Next(-10, 10) * 1f / 10f;

            pickupInstance.Play();

            GameSounds.magnetEmitter.Play();

            UpdateSprite(1);

            grabbed = true;

            Color faded = new Color(0.8f, 0.8f, 0.8f, 0.6f);

            turretBaseDrawable.Color = faded;
            barrelDrawable.Color = faded;
            barrelBaseDrawable.Color = faded;

            SpaceshipObject spaceship = dragger.ThisObject as SpaceshipObject;

            if (spaceship != null)
            {
                spaceship.magnetObject.Sprite = GameSprites.SpaceshipMagnetActive;
                spaceship.magnetObject.Transform.LocalScale = GameSprites.SpaceshipMagnetSizeActive;
                spaceship.magnetObject.Transform.LocalPosition = new Vector2(0f, 17f) / GameSprites.pixelsPerUnit;
            }
        }

        public void OnBecomeDropped(IDragger dragger)
        {
            canShoot = true;

            SoundEffectInstance dropInstance = GameSounds.dropTurretSound.CreateInstance();

            dropInstance.Volume = 0.5f;

            dropInstance.Pitch = Random.Shared.Next(-10, 10) * 1f / 10f;

            dropInstance.Play();

            GameSounds.magnetEmitter.Pause();

            UpdateSprite(0);

            grabbed = false;

            turretBaseDrawable.Color = Color.White;
            barrelDrawable.Color = Color.White;
            barrelBaseDrawable.Color = Color.White;

            SpaceshipObject spaceship = dragger.ThisObject as SpaceshipObject;

            if (spaceship != null)
            {
                spaceship.magnetObject.Sprite = GameSprites.SpaceshipMagnet;
                spaceship.magnetObject.Transform.LocalScale = GameSprites.SpaceshipMagnetSize;

                spaceship.magnetPivot.Transform.LocalRotation = MathHelper.ToRadians(0f);

                spaceship.magnetObject.Transform.LocalPosition = new Vector2(0f, 2f);
                spaceship.magnetObject.Color = Color.White;
            }
        }

        public void OnGameEnd()
        {
            canShoot = false;
        }

        public void Reset()
        {
            Transform.LocalPosition = startingPosition;

            Transform.LocalRotation = startingRotation;

            barrelPivot.Transform.LocalRotation = 0f;

            canShoot = true;
        }

        private void UpdateSprite(int idx)
        {
            barrelDrawable.Sprite = sprites[idx];
            barrelBaseDrawable.Sprite = sprites[2];
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
