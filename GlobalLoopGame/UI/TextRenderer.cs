using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Math;
using MonoEngine.Scenes;
using nkast.Aether.Physics2D.Common;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.UI
{
    public static class TextRenderer
    {
        public static TransformMatrix ViewToPixel = TransformMatrix.TranslationRotationScale(new Vector2(GlobalLoopGame.WindowSize) / 2f, 0f, new Vector2(GlobalLoopGame.WindowSize / 2f, -GlobalLoopGame.WindowSize / 2f));

        public static void DrawTextAt(this SpriteBatch sb, SpriteFont font, string text, Camera cam, Vector2 worldPos, Color color)
        {
            var textSize = font.MeasureString(text);
            var pos = (ViewToPixel * cam.ProjectionMatrix).TransformPoint(worldPos);

            sb.DrawString(font, text, pos - textSize / 2f, color);
        }

        public static void DrawAllText(this SpriteBatch sb, Hierarchy hierarchy, Font font, Camera cam)
        {
            sb.Begin(samplerState: SamplerState.PointWrap);
            foreach (var text in hierarchy.AllInstancesOf<TextObject>())
            {
                sb.DrawTextAt(font.GetSize(text.FontSize), text.Text, cam, text.Transform.GlobalPosition, text.Color);
            }
            sb.End();
        }
    }
}
