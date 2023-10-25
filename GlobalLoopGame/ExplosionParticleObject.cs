﻿using GlobalLoopGame.Asteroid;
using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using MonoEngine.Scenes;
using MonoEngine.Util;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame
{
    public class ExplosionParticleObject : PhysicsBodyObject
    {
        private float lifetime;

        private AutoTimeMachine despawner;

        private float timeAlive = 0;

        public ExplosionParticleObject(World world) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Kinematic);

            PhysicsBody.Tag = this;

            lifetime = Random.Shared.NextSingle() * 0.9f + 0.1f;

            despawner = new AutoTimeMachine(Despawn, lifetime);
        }

        public ExplosionParticleObject InitializeParticle(PhysicsBodyObject spawner)
        {
            Transform.GlobalPosition = spawner.Transform.GlobalPosition;

            Transform.LocalScale = Vector2.One;

            var drawable = new DrawableObject(Color.White, 2f);

            drawable.Transform.LocalRotation = Random.Shared.NextSingle() * 2 * MathF.PI;

            drawable.Transform.LocalPosition = Vector2.Zero;

            drawable.Transform.LocalScale = GameSprites.SmallExplosionSize;

            drawable.Parent = this;

            drawable.Sprite = GameSprites.SmallExplosion;

            return this;
        }

        public override void Update(GameTime time)
        {
            despawner.Forward(time.ElapsedGameTime.TotalSeconds);

            var d = (Children[0] as DrawableObject);

            d.Color = Color.Lerp(Color.White, new Color(0f, 0f, 0f, 0f), (timeAlive / lifetime));

            timeAlive += (float)time.ElapsedGameTime.TotalSeconds;

            base.Update(time);
        }

        private void Despawn()
        {
            PhysicsBody.World.RemoveAsync(PhysicsBody);

            CurrentScene.RemoveObject(this);
        }
    }
}
