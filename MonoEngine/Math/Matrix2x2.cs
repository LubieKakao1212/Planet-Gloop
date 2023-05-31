using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MonoEngine.Math
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix2x2
    {
        public float this[int i, int j] => j switch
        {
            0 => i switch { 0 => m00, 1 => m10, _ => throw new IndexOutOfRangeException() },
            1 => i switch { 0 => m01, 1 => m11, _ => throw new IndexOutOfRangeException() },
            _ => throw new IndexOutOfRangeException()
        };

        public Vector4 Flat => new Vector4(m00, m10, m01, m11);

        public float
            m00, m10,
            m01, m11;

        public Matrix2x2(float diag = 1)
        {
            m00 = diag;
            m10 = 0;
            m01 = 0;
            m11 = diag;
        }

        public Matrix2x2(Vector2 i, Vector2 j)
        {
            m00 = i.X;
            m01 = i.Y;

            m10 = j.X;
            m11 = j.Y;
        }

        public float Determinant() => Determinant(this);

        public Matrix2x2 Inverse()
        {
            Inverse(this, out var mOut);
            return mOut;
        }

        public void SetIdentity()
        {
            m00 = 1;
            m10 = 0;
            m01 = 0;
            m11 = 1;
        }

        public static Vector2 operator *(in Matrix2x2 lhs, in Vector2 rhs)
        {
            Mul(lhs, rhs, out var vOut);
            return vOut;
        }

        public static Vector2 operator *(in Vector2 lhs, in Matrix2x2 rhs)
        {
            Mul(rhs, lhs, out var vOut);
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
            mOut.m00 = rhs.m00 * lhs.m00 + rhs.m10 * lhs.m01;
            mOut.m10 = rhs.m00 * lhs.m10 + rhs.m10 * lhs.m11;

            mOut.m01 = rhs.m01 * lhs.m00 + rhs.m11 * lhs.m01;
            mOut.m11 = rhs.m01 * lhs.m10 + rhs.m11 * lhs.m11;
        }

        public static float Determinant(in Matrix2x2 mat)
        {
            return
                mat.m00 * mat.m11 -
                mat.m01 * mat.m10;
        }

        public static void Inverse(in Matrix2x2 mat, out Matrix2x2 mOut)
        {
            var det = Determinant(mat);
            mOut.m00 = mat.m11 / det;
            mOut.m01 = -mat.m01 / det;

            mOut.m10 = -mat.m10 / det;
            mOut.m11 = mat.m00 / det;
        }

        public static void Transpose(in Matrix2x2 mat, out Matrix2x2 mOut)
        {
            mOut.m00 = mat.m00;
            mOut.m10 = mat.m01;
            mOut.m01 = mat.m10;
            mOut.m11 = mat.m11;
        }

        public static Matrix2x2 Rotation(float radians)
        {
            Rotation(radians, out var mat);
            return mat;
        }

        public static Matrix2x2 Scale(Vector2 scale)
        {
            Scale(scale, out var mat);
            return mat;
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

        public static Matrix2x2 RotationScale(float rotationRadians, Vector2 scale)
        {
            return Rotation(rotationRadians) * Scale(scale);
        }
    }
}
