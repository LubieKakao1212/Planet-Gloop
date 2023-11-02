using GlobalLoopGame.Globals;
using Microsoft.Xna.Framework;
using Custom2d_Engine.Physics;
using Custom2d_Engine.Scenes;
using Custom2d_Engine.Util;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Custom2d_Engine.Util.Ticking;

namespace GlobalLoopGame.Asteroid
{
    public class ExplosionParticleObject : PhysicsBodyObject
    {
        private float lifetime;

        private AutoTimeMachine despawner;

        private float timeAlive = 0;
        public float sizeModifier = 1f;

        public ExplosionParticleObject(World world) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Kinematic);

            PhysicsBody.Tag = this;

            lifetime = Random.Shared.NextSingle() * 0.9f + 0.1f;

            despawner = new AutoTimeMachine(Despawn, lifetime);
        }

        public ExplosionParticleObject InitializeParticle(Vector2 spawnPos)
        {
            Transform.GlobalPosition = spawnPos;

            Transform.LocalScale = Vector2.Zero;

            var drawable = new DrawableObject(Color.White, 2f);

            drawable.Transform.LocalRotation = Random.Shared.NextSingle() * 2 * MathF.PI;

            drawable.Transform.LocalPosition = Vector2.Zero;

            drawable.Transform.LocalScale = GameSprites.SmallExplosionSize;// * spawner.Transform.LocalScale.X;

            drawable.Parent = this;

            drawable.Sprite = GameSprites.SmallExplosion;

            return this;
        }

        public override void Update(GameTime time)
        {
            despawner.Forward(time.ElapsedGameTime.TotalSeconds);

            var d = Children[0] as DrawableObject;

            d.Color = Color.Lerp(Color.White, new Color(0f, 0f, 0f, 0f), timeAlive / lifetime);

            timeAlive += (float)time.ElapsedGameTime.TotalSeconds;

            Transform.LocalScale = Vector2.One * (timeAlive / lifetime) * sizeModifier;

            base.Update(time);
        }

        private void Despawn()
        {
            PhysicsBody.World.RemoveAsync(PhysicsBody);

            CurrentScene.RemoveObject(this);
        }
    }
}
