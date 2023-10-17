using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MonoEngine.Util
{
    public static class DirectionUtil
    {
        public static Direction ToDirection(this CardinalDirection dir)
        {
            if (dir == CardinalDirection.None)
            {
                return Direction.None;
            }
            return (Direction)(((byte)dir - 1) * 2);
        }

        public static Directions ToDirections(this Direction dir)
        {
            return (Directions)(1 << (byte)dir);
        }

        public static CardinalDirection Invert(this CardinalDirection dir) => dir switch
        {
            CardinalDirection.Top => CardinalDirection.Bottom,
            CardinalDirection.Left => CardinalDirection.Right,
            CardinalDirection.Right => CardinalDirection.Left,
            CardinalDirection.Bottom => CardinalDirection.Top,
            _ => CardinalDirection.None
        };

        public static Direction Invert(this Direction dir) => dir switch
        {
            Direction.Top => Direction.Bottom,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Bottom => Direction.Top,
            Direction.TopRight => Direction.BottomLeft,
            Direction.BottomRight => Direction.TopLeft,
            Direction.BottomLeft => Direction.TopRight,
            Direction.TopLeft => Direction.BottomRight,
            _ => Direction.None
        };

        public static Point ToVector(this CardinalDirection dir) => dir switch
        {
            CardinalDirection.Top => new Point(0, 1),
            CardinalDirection.Left => new Point(-1, 0),
            CardinalDirection.Right => new Point(1, 0),
            CardinalDirection.Bottom => new Point(0, -1),
            _ => Point.Zero
        };

        public static Point ToVector(this Direction dir) => dir switch
        {
            Direction.Top => new Point(0, 1),
            Direction.Left => new Point(-1, 0),
            Direction.Right => new Point(1, 0),
            Direction.Bottom => new Point(0, -1),
            Direction.TopRight => new Point(1, 1),
            Direction.BottomRight => new Point(1, -1),
            Direction.BottomLeft => new Point(-1, -1),
            Direction.TopLeft => new Point(-1, 1),
            _ => new Point(0, 0)
        };
    }

    public enum CardinalDirection : byte
    {
        None = 0,
        Top = 1,
        Right = 2,
        Bottom = 3,
        Left = 4
    }

    public enum Direction : byte
    {
        Top = 0,
        TopRight = 1,
        Right = 2,
        BottomRight = 3,
        Bottom = 4,
        BottomLeft = 5,
        Left = 6,
        TopLeft = 7,
        None = 255
    }

    [Flags]
    public enum Directions : byte
    {
        None = 0,
        Top = 1,
        TopRight = 2,
        Right = 4,
        BottomRight = 8,
        Bottom = 16,
        BottomLeft = 32,
        Left = 64,
        TopLeft = 128
    }
}
