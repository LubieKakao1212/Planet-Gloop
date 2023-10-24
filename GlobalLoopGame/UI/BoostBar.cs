using GlobalLoopGame.Spaceship;
using Microsoft.Xna.Framework;
using MonoEngine.Scenes;
using MonoEngine.Scenes.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.UI
{
    public class BoostBar : HierarchyObject, IUpdatable
    {
        public float Order => 0f;

        private SpaceshipObject spaceship;
        private DrawableObject bar;

        public BoostBar(SpaceshipObject spaceship)
        {
            this.spaceship = spaceship;
            bar = new DrawableObject(Color.Green, 0f);
            bar.Parent = this;
            bar.Sprite = GameSprites.NullSprite;
            bar.Transform.LocalScale = new Vector2(2f, 1f);
            bar.Transform.LocalPosition = new Vector2(1f, 0f);
        }

        public void Update(GameTime time)
        {
            Transform.LocalScale = new Vector2(spaceship.BoostLeft, 1f * MathF.Sign(spaceship.BoostLeft));
            if (spaceship.BoostLeft > 0)
            {
                bar.Color = Color.Green;
            }
            else
            {
                bar.Color = Color.Red;
            }
        }
    }
}
