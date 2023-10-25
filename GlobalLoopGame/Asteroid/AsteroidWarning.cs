using MonoEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEngine.Scenes.Events;
using MonoEngine.Util;
using MonoEngine.Physics;
using nkast.Aether.Physics2D.Dynamics;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidWarning : PhysicsBodyObject
    {
        private AutoTimeMachine despawner;
        private float scale;
        private float lifetime;
        private AsteroidManager asteroidManager;

        public AsteroidWarning(World world, AsteroidManager manager) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Tag = this;
            PhysicsBody.Position = Vector2.One * 1024f;

            asteroidManager = manager;

            var visuals = AddDrawableRectFixture(new(6f, 6f), new(0f, 0f), 0, out var fixture);
            // visuals.Color = Color.OrangeRed;
            visuals.DrawOrder = 999;
            visuals.Sprite = GameSprites.Warning;

            fixture.CollisionCategories = Category.None;
            fixture.CollidesWith = Category.None;

            scale = 0.5f;
            Transform.LocalScale = Vector2.One * scale;
        }

        public void InitializeWarning(float location, float deathtime)
        {
            float radians = MathHelper.ToRadians(location);

            Vector2 thetaVector = new Vector2(MathF.Cos(radians), MathF.Sin(radians));

            Transform.LocalPosition = (thetaVector * 58f);

            lifetime = deathtime - 0.1f;

            despawner = new AutoTimeMachine(Despawn, lifetime);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime); 

            despawner.Forward(gameTime.ElapsedGameTime.TotalSeconds);

            scale += (1.5f / (lifetime*60));

            Transform.LocalScale = Vector2.One * scale;
        }

        public void Despawn()
        {
            if (asteroidManager.asteroidWarnings.Contains(this))
            {
                asteroidManager.asteroidWarnings.Remove(this);
            }

            PhysicsBody.World.RemoveAsync(PhysicsBody);

            CurrentScene.RemoveObject(this);
        }
    }
}
