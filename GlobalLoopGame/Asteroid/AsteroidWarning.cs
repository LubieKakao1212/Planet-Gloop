using MonoEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoEngine.Scenes.Events;
using MonoEngine.Util;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidWarning : DrawableObject, IGameComponent, IUpdateable
    {
        public bool Enabled => true;
        public int UpdateOrder { get; }

        public event EventHandler<EventArgs> EnabledChanged;

        public event EventHandler<EventArgs> UpdateOrderChanged;

        private AutoTimeMachine despawner;

        public AsteroidWarning(Color color, float drawOrder) : base(color, drawOrder)
        {
            Transform.LocalScale = new Vector2(2f, 6f);
        }

        public void Initialize()
        {

        }

        public void InitializeWarning(float location, float lifetime)
        {
            float radians = MathHelper.ToRadians(location);

            Vector2 thetaVector = new Vector2(MathF.Cos(radians), MathF.Sin(radians));

            Transform.LocalPosition = (thetaVector * 58f);

            despawner = new AutoTimeMachine(Despawn, lifetime);
        }

        public void Update(GameTime gameTime)
        {
            despawner.Forward(gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void Despawn()
        {
            CurrentScene.RemoveObject(this);
        }
    }
}
