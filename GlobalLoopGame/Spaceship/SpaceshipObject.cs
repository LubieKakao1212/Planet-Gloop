using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Spaceship.Dragging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MonoEngine.Physics;
using MonoEngine.Scenes;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Joints;
using System;
using System.Collections.Generic;

namespace GlobalLoopGame.Spaceship
{
    public class SpaceshipObject : PhysicsBodyObject, IDragger, IResettable
    {
        public float ThrustMultiplier { get; set; }

        public Joint CurrentDrag { get; set; }

        public DrawableObject magnetObject;

        public PhysicsBodyObject ThisObject => this;
        public float BoostLeft { get; private set; }
        public float DisplayedBoost { get => (BoostLeft / maxBoost); }

        private bool movable = false;

        private float maxBoost = 2f;
        private float boostThrust = 2f;
        private float boostRegeneration = 1f;

        /// <summary>
        /// BottomLeft, BottomRight, TopLeft, TopRight
        /// </summary>
        private List<HierarchyObject> thrusters = new List<HierarchyObject>();

        private List<int> thrust = new List<int>();
        private List<bool> boost = new List<bool>();

        public SpaceshipObject(World world, float drawOrder) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Tag = this;
            PhysicsBody.AngularDamping = 2.5f;
            PhysicsBody.LinearDamping = 1.5f;

            Transform.GlobalPosition = new Vector2(0f, -48f);
            
            var shipBody = AddDrawableRectFixture(GameSprites.SpaceshipBodySize, new(0f, 0f), 0, out var fixture, 0.25f);
            shipBody.Sprite = GameSprites.SpaceshipBody;
            shipBody.Color = Color.White;

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3
            fixture.CollisionCategories = Category.Cat2;
            fixture.CollidesWith = Category.None;
            fixture.CollidesWith |= Category.Cat1;
            fixture.CollidesWith |= Category.Cat3;
            fixture.CollidesWith |= Category.Cat5;

            PhysicsBody.OnCollision += (sender, other, contact) =>
            {
                AsteroidObject asteroid = other.Body.Tag as AsteroidObject;

                if (asteroid != null)
                {
                    GameSounds.playerHurtSound.Play();
                }

                return true;
            };

            shipBody.DrawOrder = drawOrder + 0.01f;
            var t = GameSprites.SpaceshipBodySize;
            t.X *= 12f / 32f;
            AddThruster(new(-t.X, -t.Y / 2f), 0f);
            AddThruster(new(t.X, -t.Y / 2f), 0f);
            AddThruster(new(-t.X, t.Y / 2f), MathF.PI);
            AddThruster(new(t.X, t.Y / 2f), MathF.PI);

            // add magnet
            magnetObject = new DrawableObject(Color.White, 1f);
            magnetObject.Sprite = GameSprites.SpaceshipMagnet;
            magnetObject.Parent = this;
            magnetObject.Transform.LocalScale = GameSprites.SpaceshipMagnetSize;
            magnetObject.Transform.LocalPosition = Vector2.Zero;
            magnetObject.Transform.LocalRotation = MathHelper.ToRadians(180f);
        }
        
        public void IncrementThruster(int idx)
        {
            if (movable)
            {
                thrust[idx] += 1;

                UpdateThruster(idx);
            }
        }

        public void DecrementThruster(int idx)
        {
            if (movable)
            {
                thrust[idx] -= 1;

                UpdateThruster(idx);
            }
        }

        public void AddThruster(Vector2 pos, float rotation)
        {
            var drawable = new DrawableObject(Color.Cyan, 0f);
            //No animation for now
            drawable.Sprite = GameSprites.SpaceshipThrusterFrames[0];
            var size = GameSprites.SpaceshipThrusterFrameSize;
            drawable.Transform.LocalScale = size;
            drawable.Transform.LocalPosition = -Vector2.UnitY * size.Y / 2f;
            var root = new HierarchyObject();
            drawable.Parent = root;
            root.Transform.LocalPosition = pos;
            root.Transform.LocalRotation = rotation;
            root.Parent = this;
            thrusters.Add(root);
            thrust.Add(0);
            boost.Add(false);
            UpdateThruster(thrusters.Count - 1);
        }

        private void UpdateThruster(int idx)
        {
            var scale = new Vector2(1f, thrust[idx]);
            if (thrust[idx] > 0 && boost[idx] && BoostLeft > 0)
            {
                scale.Y += 1;
                scale.X += 0.5f;
            }
            thrusters[idx].Transform.LocalScale = scale;
        }

        public override void Update(GameTime time)
        {
            if (movable)
            {
                var boostAmount = 0;

                for (int i = 0; i < thrusters.Count; i++)
                {
                    if (thrust[i] > 0)
                    {
                        var thrustMultiplier = ThrustMultiplier;

                        if (boost[i])
                        {
                            boostAmount++;

                            if (BoostLeft > 0)
                            {
                                thrustMultiplier *= boostThrust;
                            }
                            else
                            { 
                                UpdateThruster(i);

                                if (GameSounds.boostEmitter.State == SoundState.Playing)
                                {
                                    GameSounds.boostEmitter.Pause();

                                    GameSounds.boostOverloadSound.Play();
                                }
                            }
                        }

                        PhysicsBody.ApplyForce(thrusters[i].Transform.Up * thrustMultiplier, thrusters[i].Transform.GlobalPosition);
                    }
                }

                if (boostAmount > 0)
                {
                    BoostLeft -= boostAmount * (float)time.ElapsedGameTime.TotalSeconds;

                    if (BoostLeft < 0f)
                    {
                        BoostLeft -= (float)time.ElapsedGameTime.TotalSeconds;
                    }

                    BoostLeft = MathF.Max(BoostLeft, -1);

                    if (GameSounds.boostChargingEmitter.State == SoundState.Playing)
                    {
                        GameSounds.boostChargingEmitter.Pause();
                    }
                }
                else
                {
                    // Regenerate boost here
                    BoostLeft += boostRegeneration * (float)time.ElapsedGameTime.TotalSeconds;
                    BoostLeft = MathF.Min(BoostLeft, maxBoost);

                    if (GameSounds.boostChargingEmitter.State != SoundState.Playing)
                    {
                        GameSounds.boostChargingEmitter.Play();
                    }
                }

                // rotate magnet towards turret if dragging
                if (CurrentDrag != null)
                {
                    var dir = CurrentDrag.BodyB.Position - magnetObject.Transform.GlobalPosition;
                    magnetObject.Transform.GlobalRotation = MathF.Atan2(dir.Y, dir.X) - MathF.PI / 2f;
                }
            }

            ProcessSounds();

            base.Update(time);
        }

        public void BoostThruster(int idx)
        {
            if (BoostLeft <= 0f)
            {
                GameSounds.boostUnableSound.Play();
                return;
            }

            boost[idx] = true;

            UpdateThruster(idx);

            if (GameSounds.boostEmitter.State != SoundState.Playing)
            {
                GameSounds.boostEmitter.Play();
            }
        }

        public void DisableBoost(int idx)
        {
            boost[idx] = false;

            UpdateThruster(idx);

            if (GameSounds.boostEmitter.State == SoundState.Playing)
            {
                GameSounds.boostEmitter.Pause();
            }
        }

        public void OnGameEnd()
        {
            PhysicsBody.LinearVelocity = Vector2.Zero;
            PhysicsBody.AngularVelocity = 0f;

            for (int i = 0; i < thrust.Count; i++)
            {
                thrust[i] = 0;
            }

            if (CurrentDrag != null)
            {
                DraggingHelper.TryInitDragging(this, 10f, 15f);
            }

            movable = false;
        }

        public void Reset()
        {
            BoostLeft = maxBoost;
            Transform.GlobalPosition = new Vector2(0f, -48f);
            Transform.GlobalRotation = 0f;
            movable = true;
        }

        private void ProcessSounds()
        {
            if (thrusters.Count < 4)
            {
                return;
            }

            PlayThrusterSound(0, 1, GameSounds.thrusterEmitter);
            PlayThrusterSound(2, 3, GameSounds.sideThrusterEmitter);
        }

        private void PlayThrusterSound(int thrusterOne, int thrusterTwo, SoundEffectInstance sound)
        {
            if (sound.State == SoundState.Playing)
            {
                if (thrust[thrusterOne] < 1 && thrust[thrusterTwo] < 1)
                {
                    sound.Pause();
                }
            }
            else
            {
                if (thrust[thrusterOne] >= 1 || thrust[thrusterTwo] >= 1)
                {
                    sound.Play();
                }
            }
        }
    }
}
