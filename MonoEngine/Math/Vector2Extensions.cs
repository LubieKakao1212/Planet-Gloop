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
        public static Vector2Int FloorToInt(this Vector2 vect)
        {
            vect.Floor();
            return new Vector2Int((int)vect.X, (int)vect.Y);
        }
    }
}
