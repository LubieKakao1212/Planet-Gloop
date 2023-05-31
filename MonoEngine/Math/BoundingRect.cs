using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Math
{
    public struct BoundingRect : IEquatable<BoundingRect>
    {
        public static BoundingRect Normal { get; } = new BoundingRect(new Vector2(-1, -1), new Vector2(2, 2));

        public Vector2 Min => new Vector2(X, Y);
        public Vector2 Max => new Vector2(X + Width, Y + Height);
        public Vector4 Flat => new Vector4(X, Y, Width, Height);

        public float X;
        public float Y;
        public float Width;
        public float Height;

        public BoundingRect(Vector2 pos, Vector2 size)
        {
            X = pos.X;
            Y = pos.Y;

            Width = size.X;
            Height = size.Y;
        }

        public void Inflate(float amount)
        {
            X -= amount;
            Y -= amount;

            Width += 2 * amount;
            Height += 2 * amount;
        }

        public void Inflate(Vector2 amount)
        {
            X -= amount.X;
            Y -= amount.Y;

            Width += 2 * amount.X;
            Height += 2 * amount.Y;
        }

        public void Scale(float amount)
        {
            X = -((Width  / 2f) * amount) - (X + Width  / 2f);
            Y = -((Height / 2f) * amount) - (Y + Height / 2f);

            Width *= amount;
            Height *= amount;
        }

        public BoundingRect Scaled(float amount)
        {
            var rect = this;
            rect.Scale(amount);
            return rect;
        }

        public void Transform(in TransformMatrix mat)
        {
            var min = Min;
            var max = Max;

            var rectPos = mat.T;

            var rectMin = rectPos;
            var rectMax = rectPos;

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                {
                    float a = mat[i, j] * min.Get(j);
                    float b = mat[i, j] * max.Get(j);
                    var minI = rectMin.Get(i);
                    var maxI = rectMax.Get(i);
                    rectMin.Set(i, minI + a < b ? a : b);
                    rectMax.Set(i, maxI + a < b ? b : a);
                }

            X = rectMin.X;
            Y = rectMin.Y;

            Width = rectMax.X - rectMin.X;
            Height = rectMax.Y - rectMin.Y;
        }

        public BoundingRect Transformed(in TransformMatrix mat)
        {
            var br = this;
            br.Transform(mat);
            return br;
        }

        public bool Intersects(in BoundingRect other)
        {
            var min1 = Min;
            var max1 = Max;

            var min2 = other.Min;
            var max2 = other.Max;

            return 
                min2.X < max1.X && min1.X < max2.X && 
                min2.Y < max1.Y && min1.Y < max2.Y;
        }

        public Rectangle ToInt()
        {
            return new Rectangle(
                (int) MathF.Floor(X),
                (int) MathF.Floor(Y),
                (int) MathF.Ceiling(Width),
                (int) MathF.Ceiling(Height)
                );
        }

        public bool Equals(BoundingRect other)
        {
            if (X == other.X && Y == other.Y && Width == other.Width)
            {
                return Height == other.Height;
            }
            return false;
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is BoundingRect rect ? rect.Equals(this) : false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X.GetHashCode(), Y.GetHashCode(), Width.GetHashCode(), Height.GetHashCode());
        }

        public static BoundingRect MinMaxRect(Vector2 min, Vector2 max)
        {
            return new BoundingRect(min, max - min);
        }
    }
}
