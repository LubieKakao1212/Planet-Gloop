using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Math;
using MonoEngine.Rendering.Sprites;
using MonoEngine.Scenes;
using MonoEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame
{
    public class MultiIconDisplay : HierarchyObject
    {
        private int maxIcons = 4;

        private Vector2 iconSize;
        private Sprite icon;

        private int fontSize;

        private int iconCount;
        private TextObject overflowMessage;
        private DrawableObject[] icons;

        private float drawOrder;

        public MultiIconDisplay(Sprite icon, int maxIcons, float spacing, float textOffset, float drawOrder)
        {
            this.icon = icon;
            iconSize = GameSprites.GetRelativeSize(GameSprites.Planet, GameSprites.PlanetSize, icon);
            fontSize = (int)(iconSize.Y * 12) / 2;
            Console.WriteLine($"FontSize: {fontSize}");
            iconCount = 0;
            this.maxIcons = maxIcons;
            this.drawOrder = drawOrder;

            CreateIcons(spacing);

            overflowMessage = new TextObject();
            overflowMessage.Transform.LocalPosition = new Vector2((iconSize.X + spacing) * (maxIcons) + textOffset, 0f);
            overflowMessage.Color = Color.White;
            overflowMessage.Text = "";
            overflowMessage.FontSize = fontSize;
            overflowMessage.Parent = this;
        }

        public void UpdateCount(int newCount)
        {
            var oldCount = iconCount;
            iconCount = newCount;

            if (newCount == oldCount)
            {
                return;
            }
            if (newCount > maxIcons)
            {
                overflowMessage.Text = $"+{newCount - maxIcons}";
            }
            else
            {
                overflowMessage.Text = "";
            }
            if (oldCount > maxIcons && newCount > maxIcons) 
            {
                return;
            }
            var delta = newCount - oldCount;

            if (delta < 0)
            {
                SetIconsColor(Color.Transparent, newCount, MathHelper.Min(oldCount, maxIcons));
            }
            else
            {
                SetIconsColor(Color.White, oldCount, MathHelper.Min(newCount, maxIcons));
            }

        }

        private void CreateIcons(float spacing)
        {
            icons = new DrawableObject[maxIcons];

            for (int i = 0; i < maxIcons; i++)
            {
                var icon = new DrawableObject(Color.Transparent, 0f);
                icon.Parent = this;
                icon.Sprite = this.icon;
                icon.Transform.LocalScale = iconSize;
                icon.Transform.LocalPosition = new Vector2(i * (iconSize.X + spacing) + iconSize.X / 2f, 0f);
                icons[i] = icon;
            }
        }

        private void SetIconsColor(Color color, int startI, int endI)
        {
            for (int i =startI; i < endI; i++)
            {
                icons[i].Color = color;
            }
        }
    }
}
