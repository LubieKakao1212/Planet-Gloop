using Microsoft.Xna.Framework;
using System;

namespace MonoEngine.Util
{
    public static class RectangleArrayExtensions
    {
        public static void SetRectUnchecked<T>(this T[] destinationArray, int arrayWidth, T[] source, Rectangle destinationRect)
        {
            var w = destinationRect.Width;
            var h = destinationRect.Height;
            var targetX = destinationRect.X;
            var targetY = destinationRect.Y;

            var z = targetX / arrayWidth;

            for (int y = 0; y < h; y++)
            {
                var sourceIdx = y * w;
                //TODO This is a dirty fix
                //var destinationIdx = (targetY + y) * arrayWidth + targetX;
                var destinationIdx = (targetY + y + (z*arrayWidth)) * arrayWidth + targetX;
                var sourceSpan = new Span<T>(source, sourceIdx, w);
                var destinationSpan = new Span<T>(destinationArray, destinationIdx, w);

                sourceSpan.CopyTo(destinationSpan);
            }
        }
    }
}
