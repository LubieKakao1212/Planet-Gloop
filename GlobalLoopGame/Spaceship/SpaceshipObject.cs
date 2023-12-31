﻿using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Globals;
using GlobalLoopGame.Spaceship.Dragging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Custom2d_Engine.Physics;
using Custom2d_Engine.Scenes;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Joints;
using System;
using System.Collections.Generic;

namespace GlobalLoopGame.Spaceship
{
    public class SpaceshipObject : PhysicsBodyObject, IDragger, IResettable
    {
        public const float DragDistance = 10f;
        public const float DragInteractionDistance = 15f;

        public float ThrustMultiplier { get; set; }

        public Joint CurrentDrag { get; set; }

        public DrawableObject magnetObject;

        public event Action<float> BoostUpdated;

        public PhysicsBodyObject ThisObject => this;
        public float BoostLeft { get; private set; }
        public float DisplayedBoost
        {
            get
            {
                BoostUpdated?.Invoke(BoostLeft);
                return (BoostLeft / maxBoost);
            }
        }

        private bool movable = false;

        private float maxBoost = 2f;
        private float boostThrust = 2f;
        private float boostRegeneration = 1f;
        private float targetThrusterVolume = 0f;
        private float targetSideThrusterVolume = 0f;
        private float targetBoosterVolume = 0f;

        /// <summary>
        /// BottomLeft, BottomRight, TopLeft, TopRight
        /// </summary>
        private List<HierarchyObject> thrusters = new List<HierarchyObject>();

        private List<float> thrust = new List<float>();
        private List<bool> boost = new List<bool>();

        public HierarchyObject magnetPivot;

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
            fixture.CollisionCategories = CollisionCats.Spaceship;
            fixture.CollidesWith = CollisionCats.CollisionsSpaceship;

            PhysicsBody.OnCollision += (sender, other, contact) =>
            {
                AsteroidObject asteroid = other.Body.Tag as AsteroidObject;

                if (asteroid != null)
                {
                    GameSounds.PlaySound(GameSounds.playerHurtSound, 2);
                }

                return true;
            };

            shipBody.DrawOrder = drawOrder + 0.01f;
            var t = GameSprites.SpaceshipBodySize;
            t.X *= 12f / 32f;
            AddThruster(new(-t.X, -t.Y / 2f), 0f);
            AddThruster(new(t.X, -t.Y / 2f), 0f);
            AddThruster(new(-t.X, t.Y / 3f), MathF.PI);
            AddThruster(new(t.X, t.Y / 3f), MathF.PI);

            magnetPivot = new HierarchyObject();
            magnetPivot.Parent = this;
            magnetPivot.Transform.LocalPosition = Vector2.Zero;
            magnetPivot.Transform.LocalRotation = MathHelper.ToRadians(180f);

            // add magnet
            magnetObject = new DrawableObject(Color.White, 1f);
            magnetObject.Sprite = GameSprites.SpaceshipMagnet;
            magnetObject.Parent = magnetPivot;
            magnetObject.Transform.LocalScale = GameSprites.SpaceshipMagnetSize;
            magnetObject.Transform.LocalPosition = new Vector2(0f, 2f);
        }
    
        /*public void IncrementThruster(int idx)
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
        }*/

        public void SetThrust(int thrusterIdx, float value)
        {
            if (!movable)
            {
                return;
            }
            thrust[thrusterIdx] = value;
            UpdateThruster(thrusterIdx);
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

                                    targetBoosterVolume = 0f;

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

                    magnetPivot.Transform.GlobalRotation = MathF.Atan2(dir.Y, dir.X) - MathF.PI / 2f;

                    if (dir.Length() > 4)
                    {
                        magnetObject.Color = Color.White;
                    }
                    else
                    {
                        magnetObject.Color = Color.Transparent;
                    }
                }
            }

            ProcessSounds();

            // fade engine sound
            GameSounds.boostEmitter.Volume = GameSounds.boostEmitter.Volume + MathF.Sign(targetBoosterVolume - GameSounds.boostEmitter.Volume) * (float)time.ElapsedGameTime.TotalSeconds / 2f;
            
            base.Update(time);
        }

        public void BoostThruster(int idx)
        {
            if (!movable)
            {
                return;
            }

            if (BoostLeft <= 0f)
            {
                GameSounds.boostUnableSound.Play();
                return;
            }

            if (thrusters[0].Transform.LocalScale.Y > 0f ||
                thrusters[1].Transform.LocalScale.Y > 0f ||
                thrusters[2].Transform.LocalScale.Y > 0f ||
                thrusters[3].Transform.LocalScale.Y > 0f)
            {
                boost[idx] = true;

                UpdateThruster(idx);

                if (GameSounds.boostEmitter.State != SoundState.Playing)
                {
                    GameSounds.boostEmitter.Play();

                    targetBoosterVolume = 0.15f;
                }
            }
            
        }

        public void DisableBoost(int idx)
        {
            boost[idx] = false;

            UpdateThruster(idx);

            if (GameSounds.boostEmitter.State == SoundState.Playing)
            {
                GameSounds.boostEmitter.Pause();

                targetBoosterVolume = 0f;
            }
        }

        public void OnGameEnd()
        {
            PhysicsBody.LinearVelocity = Vector2.Zero;
            PhysicsBody.AngularVelocity = 0f;

            targetThrusterVolume = 0f;

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
            /*if (thrusters.Count < 4)
            {
                return;
            }*/
            PlayThrusterSound(0, 1, GameSounds.thrusterEmitter);

            PlayThrusterSound(2, 3, GameSounds.sideThrusterEmitter);
        }

        private void PlayThrusterSound(int thrusterOne, int thrusetTwo, SoundEffectInstance sound)
        {
            var volumeMul = 1f / 16;
            float volume = MathF.Sqrt(thrust[thrusterOne] + thrust[thrusetTwo]) * volumeMul;
            sound.Volume = MathHelper.Clamp(volume, 0f, 1f);
            sound.Play();
        }
    }
}
