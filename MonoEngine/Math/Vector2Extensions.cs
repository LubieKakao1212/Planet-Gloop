using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Math
{
    public static class Vector2Extensions
    {
        public static Point FloorToInt(this Vector2 vect)
        {
            vect.Floor();

            return new Point((int)vect.X, (int)vect.Y);
        }

        public static float Get(this Vector2 vect, int idx) => idx switch { 0 => vect.X, 1 => vect.Y, _ => throw new IndexOutOfRangeException() };

        public static void Set(this ref Vector2 vect, int i, float value)
        {
            switch (i) 
            { 
                case 0: 
                    vect.X = value;
                    break;
                case 1: 
                    vect.Y = value;
                    break;
                default: throw new IndexOutOfRangeException();
            }
        }

        public static Point FloorDiv(this Point lhs, int rhs)
        {
            return new Point(
                MathUtil.FloorDiv(lhs.X, rhs),
                MathUtil.FloorDiv(lhs.Y, rhs)
                );
        }

    }
}
