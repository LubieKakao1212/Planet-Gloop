using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Math
{
    public static class RectangleExtensions
    {
        public static IEnumerable<Point> AllPositionsIn(this Rectangle rect)
        {
            var lb = rect.Location;
            var hb = lb + rect.Size;

            for (int x = lb.X; x < hb.X; x++)
                for (int y = lb.Y; y < hb.Y; y++) 
                    yield return new Point(x, y);
        }

        public static int CellCount(in this Rectangle rect)
        {
            return rect.Size.X * rect.Size.Y;
        }
    }
}
