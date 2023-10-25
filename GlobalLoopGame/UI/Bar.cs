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
    public class Bar : HierarchyObject, IUpdatable
    {
        public float Order => 0f;

        private Func<float> fillAmount;
        private DrawableObject bar;
        private Color positiveColor;
        private Color negativeColor;

        public Bar(Func<float> fillAmount, Color positiveColor, Color negativeColor)
        {
            this.fillAmount = fillAmount;
            bar = new DrawableObject(Color.Green, 0f);
            bar.Parent = this;
            bar.Sprite = GameSprites.NullSprite;
            bar.Transform.LocalScale = new Vector2(2f, 1f);
            bar.Transform.LocalPosition = new Vector2(1f, 0f);

            this.positiveColor = positiveColor;
            this.negativeColor = negativeColor;
        }

        public void Update(GameTime time)
        {
            var fill = fillAmount();
            Transform.LocalScale = new Vector2(fill, 1f * MathF.Sign(fill));
            if (fill > 0)
            {
                bar.Color = positiveColor;
            }
            else
            {
                bar.Color = negativeColor;
            }
        }
    }
}
