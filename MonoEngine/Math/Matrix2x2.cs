using Microsoft.Xna.Framework;
using System;
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
        
        /// <inheritdoc cref="Deconstruct(in Matrix2x2, out float, out Vector2, out float)"/>
        public void Deconstruct(out float rotation, out float xShear, out Vector2 scale)
        {
            Deconstruct(this, out rotation, out xShear, out scale);
        }

        public override string ToString()
        {
            return $"[{m00}, {m10} | {m01}, {m11}";
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
            //      |   x   |
            //      |   y   |
            // |a b| ax + by 
            // |c d| cx + dy
            vOut.X = mat.m00 * vIn.X + mat.m10 * vIn.Y;
            vOut.Y = mat.m01 * vIn.X + mat.m11 * vIn.Y;
        }

        public static void Mul(in Matrix2x2 lhs, in Matrix2x2 rhs, out Matrix2x2 mOut)
        {
            // rotataion * xShear
            //               | r00             r10 |
            //               | r01             r11 |
            //
            // |l00 l10| |l00r00 + l10r01 l00r10 + l10r11|
            // |l01 l11| |l01r00 + l11r01 l01r10 + l11r11|

            mOut.m00 = rhs.m00 * lhs.m00 + rhs.m01 * lhs.m10;
            mOut.m10 = rhs.m10 * lhs.m00 + rhs.m11 * lhs.m10;
            
            mOut.m01 = rhs.m00 * lhs.m01 + rhs.m01 * lhs.m11;
            mOut.m11 = rhs.m10 * lhs.m01 + rhs.m11 * lhs.m11;
            

            //mOut.m00 = rhs.m00 * lhs.m00 + rhs.m10 * lhs.m01;
            //mOut.m10 = rhs.m00 * lhs.m10 + rhs.m10 * lhs.m11;

            //mOut.m01 = rhs.m01 * lhs.m00 + rhs.m11 * lhs.m01;
            //mOut.m11 = rhs.m01 * lhs.m10 + rhs.m11 * lhs.m11;
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

        /// <summary>
        /// Creates a <see cref="Matrix2x2"/> which transfroms spase from <paramref name="fromMat"/> to <paramref name="toMat"/>
        /// </summary>
        /// <returns></returns>
        public static void FromTo(in Matrix2x2 fromMat, in Matrix2x2 toMat, out Matrix2x2 fromToMat)
        {
            Inverse(fromMat, out var fromInv);
            fromToMat = fromInv * toMat;
        }

        /// <summary>
        /// Creates a <see cref="Matrix2x2"/> which transfroms spase from <paramref name="fromMat"/> to <paramref name="toMat"/>
        /// </summary>
        /// <returns></returns>
        public static Matrix2x2 FromTo(in Matrix2x2 fromMat, in Matrix2x2 toMat)
        {
            Inverse(fromMat, out var fromInv);
            return fromInv * toMat;
        }

        public static Matrix2x2 Rotation(float theta)
        {
            Rotation(theta, out var mat);
            return mat;
        }

        public static Matrix2x2 Scale(Vector2 scale)
        {
            Scale(scale, out var mat);
            return mat;
        }

        public static Matrix2x2 Skew(float xAngle, float yAngle)
        {
            Skew(xAngle, yAngle, out var mat);
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

        public static void Skew(float xAngle, float yAngle, out Matrix2x2 mat)
        {
            mat.m00 = 1f;
            mat.m10 = MathF.Tan(MathF.PI - xAngle);
            mat.m01 = MathF.Tan(MathF.PI - yAngle);
            mat.m11 = 1f;
        }

        /// <summary>
        /// Deconstructs a <see cref="Matrix2x2"/> into rotation, shear and scale. Original matrix can be recreated using <see cref="RotationShearScale(float, float, Vector2)"/> with values returned by this function 
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="rotation"></param>
        /// <param name="xShear"></param>
        /// <param name="scale"></param>
        public static void Deconstruct(in Matrix2x2 mat, out float rotation, out float xShear, out Vector2 scale)
        {
            // rotataion * xShear
            //         | 1     t |
            //         | 0     1 |
            //
            // |c -s|  | c   ct-s| 
            // |s  c|  | s   st+c|

            rotation = MathF.Atan2(mat.m01, mat.m00);

            xShear = MathUtil.Loop(MathF.Atan2(mat.m11, mat.m10) - MathF.PI / 2f - rotation, MathF.PI * 2f);

            scale = new Vector2(
                MathF.Sqrt(mat.m00 * mat.m00 + mat.m01 * mat.m01),
                MathF.Sqrt(mat.m10 * mat.m10 + mat.m11 * mat.m11) * MathF.Cos(xShear)
                );
        }

        public static Matrix2x2 RotationScale(float rotationRadians, Vector2 scale)
        {
            return Rotation(rotationRadians) * Scale(scale);
        }

        /// <summary>
        /// Composes a <see cref="Matrix2x2"/> from represents first scaling by <paramref name="scale"/>, than shearing by <paramref name="xShear"/> and then rotating by <paramref name="rotationRadians"/>
        /// </summary>
        /// <param name="rotationRadians">Angle to rotate by</param>
        /// <param name="xShear">Shear angle</param>
        /// <param name="scale">Per axis scale</param>
        /// <returns></returns>
        public static Matrix2x2 RotationShearScale(float rotationRadians, float xShear, Vector2 scale)
        {
            return Rotation(rotationRadians) * Skew(xShear, 0f) * Scale(scale);
        }
    }
}
