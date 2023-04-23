using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Math
{
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public int X, Y;

        public Vector2Int(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X.GetHashCode(), Y.GetHashCode());
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is Vector2Int v)
            {
                return Equals(v);
            }
            return false;
        }

        public bool Equals(Vector2Int other)
        {
            return X == other.X && Y == other.Y;
        }
    }
}
