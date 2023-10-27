using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalLoopGame.Globals;
using Microsoft.Xna.Framework;
using MonoEngine.Scenes;

namespace GlobalLoopGame.UI
{
    internal class StarryBackground : DrawableObject
    {
        private float localScale = 136f;
        public StarryBackground(Color color, float drawOrder, int spawnNumber, float scaleMult) : base(color, drawOrder)
        {
            this.Sprite = GameSprites.NullSprite;
            this.Transform.LocalScale = new Vector2(localScale);
            for(int i = 0; i < spawnNumber; i++)
            {
                DrawableSine star = AddStar(Color.White, drawOrder + 0.1f, Random.Shared.NextSingle());
                Vector2 scale = new Vector2(Random.Shared.NextSingle() / localScale);
                star.Transform.LocalScale = scale * scaleMult;
                Vector2 position = new Vector2(Random.Shared.NextSingle() - 0.5f, Random.Shared.NextSingle() - 0.5f) / localScale;
                star.Transform.LocalPosition = position * 136;

                star.Parent = this;
            }
        }

        public DrawableSine AddStar(Color color, float drawOrder, float phase)
        {
            phase *= (float)Math.PI;
            var drawableSine = new DrawableSine(color, drawOrder, phase);
            drawableSine.Sprite = GameSprites.NullSprite;
            return drawableSine;
        }
    }
}
