using Custom2d_Engine.Physics;
using Custom2d_Engine.Scenes;
using Custom2d_Engine.Util.Ticking;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidParticleObject : PhysicsBodyObject
    {
        public DrawableObject asteroidFixture;

        private float lifetime;
        private AutoTimeMachine despawner;
        private float timeAlive = 0;

        public AsteroidParticleObject(World world) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Kinematic);
            PhysicsBody.Tag = this;

            lifetime = Random.Shared.NextSingle() * 0.9f + 0.1f;
            despawner = new AutoTimeMachine(Despawn, lifetime);

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3, bullets - 4
        }

        public AsteroidParticleObject InitializeParticle(AsteroidObject spawner, Vector2 startingVelocity)
        {
            Transform.GlobalPosition = spawner.Transform.GlobalPosition;
            //Chese
            Transform.LocalScale = spawner.Children[0].Transform.LocalScale * 0.5f;
            PhysicsBody.LinearVelocity = startingVelocity * (Random.Shared.NextSingle() * 8f + 4f);
            PhysicsBody.AngularVelocity = Random.Shared.NextSingle() * 5f - 2.5f;
            
            var drawable = new DrawableObject(Color.DarkSlateGray, 0f);
            drawable.Transform.LocalRotation = Random.Shared.NextSingle() * 2 * MathF.PI;
            drawable.Parent = this;
            drawable.Sprite = ((DrawableObject)spawner.Children[0]).Sprite;

            return this;
        }

        public override void Update(GameTime time)
        {
            despawner.Forward(time.ElapsedGameTime.TotalSeconds);
            var d = (Children[0] as DrawableObject);
            d.Color = Color.Lerp(Color.Gray, new Color(0f,0f,0f,0f), (timeAlive / lifetime));
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
