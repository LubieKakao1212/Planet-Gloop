using GlobalLoopGame.Globals;
using GlobalLoopGame.Spaceship;
using Microsoft.Xna.Framework;
using Custom2D_Engine.Scenes;
using Custom2D_Engine.Scenes.Events;
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
        private DrawableObject background;
        private Color positiveColor;
        private Color negativeColor;
        private Color backgroundColor;

        private bool visible = true;

        public Bar(Func<float> fillAmount, Color positiveColor, Color negativeColor, Color backgroundColor)
        {
            this.fillAmount = fillAmount;

            background = new DrawableObject(backgroundColor, 2f);
            background.Parent = this;
            background.Sprite = GameSprites.NullSprite;
            background.Transform.LocalScale = new Vector2(4f, 1f);
            background.Transform.LocalPosition = new Vector2(1f, 0f);

            bar = new DrawableObject(positiveColor, 3f);
            bar.Parent = background;
            bar.Sprite = GameSprites.NullSprite;
            bar.Transform.LocalScale = Vector2.One;
            bar.Transform.LocalPosition = Vector2.Zero;
            
            this.positiveColor = positiveColor;
            this.negativeColor = negativeColor;
            this.backgroundColor = backgroundColor;
        }

        public void Update(GameTime time)
        {
            if (!visible) return;

            var fill = fillAmount();

            bar.Transform.LocalScale = new Vector2(fill, 1f * MathF.Sign(fill));
            bar.Transform.LocalPosition = new Vector2((fill / 2f) - 0.5f, 0);

            if (fill > 0)
            {
                bar.Color = positiveColor;
            }
            else
            {
                bar.Color = negativeColor;
            }
        }

        public void ToggleVisibility(bool newVisibility)
        {
            visible = newVisibility;

            bar.Color = newVisibility ? positiveColor : Color.Transparent;
            background.Color = newVisibility ? backgroundColor : Color.Transparent;
        }
    }
}
