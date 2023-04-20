using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;

namespace MonoEngine.Math
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix2x2
    {
        public float
            m00, m10,
            m01, m11;

        public Matrix2x2(Vector2 i, Vector2 j)
        {
            m00 = i.X;
            m01 = i.Y;

            m10 = j.X;
            m11 = j.Y;
        }

        public static Vector2 operator *(in Matrix2x2 lhs, in Vector2 rhs)
        {
            Mul(lhs, rhs, out var vOut);
            return vOut;
        }

        public static Matrix2x2 operator *(in Matrix2x2 lhs, in Matrix2x2 rhs)
        {
            Mul(lhs, rhs, out var mOut);
            return mOut;
        }

        public static void Mul(in Matrix2x2 mat, in Vector2 vIn, out Vector2 vOut)
        {
            vOut.X = mat.m00 * vIn.X + mat.m01 * vIn.Y;
            vOut.Y = mat.m10 * vIn.X + mat.m11 * vIn.Y;
        }

        public static void Mul(in Matrix2x2 lhs, in Matrix2x2 rhs, out Matrix2x2 mOut)
        {
            mOut.m00 = lhs.m00 * rhs.m00 + lhs.m10 * rhs.m01;
            mOut.m10 = lhs.m00 * rhs.m10 + lhs.m10 * rhs.m11;

            mOut.m01 = lhs.m01 * rhs.m00 + lhs.m11 * rhs.m01;
            mOut.m11 = lhs.m01 * rhs.m10 + lhs.m11 * rhs.m11;
        }

        public static void Rotation(float radians, out Matrix2x2 mat)
        {
            var sin = MathF.Sin(radians);
            var cos = MathF.Cos(radians);
            mat.m00 = cos;
            mat.m10 = -sin;
            mat.m01 = sin;
            mat.m11 = cos;
        }

        public static void Scale(Vector2 scale, out Matrix2x2 mat)
        {
            mat.m00 = scale.X;
            mat.m10 = 0;
            mat.m01 = 0;
            mat.m11 = scale.Y;
        }
    }
}
