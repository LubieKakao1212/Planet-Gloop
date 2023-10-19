using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoEngine.Math
{
    /// <summary>
    /// Represents a 3x3 matrix with <see cref="TransformMatrix.rotationScale"/> as upper left 2x2, <see cref="TransformMatrix.translation"/> as bottom left row and 0,0,1 as right column<br/>
    /// </summary>
    //m00 m10 tx 
    //m01 m11 ty
    //0   0   1
    [StructLayout(LayoutKind.Sequential)]
    public struct TransformMatrix
    {
        public static TransformMatrix Identity => new TransformMatrix(new Matrix2x2(1f), Vector2.Zero);

        public float this[int i, int j] => i switch
        {
            0 => RS[0, j],
            1 => RS[1, j],
            2 => j switch { 0 => T.X, 1 => T.Y, _ => throw new IndexOutOfRangeException($"j: {i}") },
            _ => throw new IndexOutOfRangeException($"i: {i}")
        };

        internal Matrix2x2 RS => rotationScale;
        internal Vector2 T => translation;

        private Matrix2x2 rotationScale;
        private Vector2 translation;

        public TransformMatrix(Matrix2x2 rotationAndScale, Vector2 translation)
        {
            this.rotationScale = rotationAndScale;
            this.translation = translation;
        }

        public TransformMatrix Inverse()
        {
            Inverse(this, out var mOut);
            return mOut;
        }
        
        public float Determinant()
        {
            return Determinant(this);
        }

        public void SetIdentity()
        {
            rotationScale.SetIdentity();
            translation.X = 0;
            translation.Y = 0;
        }

        public TransformMatrix Mul(in TransformMatrix rhs)
        {
            Mul(this, rhs, out var mOut);
            return mOut;
        }

        public TransformMatrix Mul(in Matrix2x2 rhs)
        {
            Mul(this, rhs, out var mOut);
            return mOut;
        }
        
        public TransformMatrix Translate(in Vector2 rhs)
        {
            Translate(this, rhs, out var mOut);
            return mOut;
        }

        public Vector2 TransformPoint(in Vector2 point)
        {
            TransformPoint(this, point, out var pOut);
            return pOut;
        }

        public Vector2 TransformDirection(in Vector2 direction)
        {
            TransformDirection(this, direction, out var dOut);
            return dOut;
        }

        public Matrix ToMatrixXNA()
        {
            return new Matrix(
                new Vector4(RS.m00, RS.m10, 0, 0),
                new Vector4(RS.m01, RS.m11, 0, 0),
                new Vector4(T.X   , T.Y   , 0, 0),
                new Vector4(0     , 0     , 0, 1)
                );
        }

        public void Deconstruct(out Vector2 translation, out float rotation, out float xShear, out Vector2 scale)
        {
            Deconstruct(this, out translation, out rotation, out xShear, out scale);
        }
        
        public static TransformMatrix operator *(in TransformMatrix lhs, in TransformMatrix rhs)
        {
            Mul(lhs, rhs, out var mOut);
            return mOut;
        }

        public static TransformMatrix operator *(in TransformMatrix lhs, in Matrix2x2 rhs)
        {
            Mul(lhs, rhs, out var mOut);
            return mOut;
        }

        public static TransformMatrix operator *(in Matrix2x2 lhs, in TransformMatrix rhs)
        {
            Mul(lhs, rhs, out var mOut);
            return mOut;
        }

        public static void Mul(in TransformMatrix lhs, in TransformMatrix rhs, out TransformMatrix mOut)
        {
            mOut = lhs;
            mOut.rotationScale *= rhs.rotationScale;
            /*mOut.translation.X = 
                lhs.RS.m00 * rhs.T.X +
                lhs.RS.m10 * rhs.T.Y +
                lhs.T.X;

            mOut.translation.Y =
                lhs.RS.m01 * rhs.T.X +
                lhs.RS.m11 * rhs.T.Y +
                lhs.T.Y;*/
            mOut.translation += lhs.rotationScale * rhs.translation;
        }

        public static void Mul(in TransformMatrix lhs, in Matrix2x2 rhs, out TransformMatrix mOut)
        {
            mOut = lhs;
            mOut.rotationScale *= rhs;
        }

        public static void Mul(in Matrix2x2 lhs, in TransformMatrix rhs, out TransformMatrix mOut)
        {
            mOut = rhs;
            mOut.rotationScale = lhs * rhs.rotationScale;
            mOut.translation *= lhs;
        }

        public static void Translate(in TransformMatrix lhs, in Vector2 rhs, out TransformMatrix mOut)
        {
            mOut = lhs;
            mOut.translation += rhs;
        }
        
        public static void TransformPoint(in TransformMatrix lhs, in Vector2 rhs, out Vector2 mOut)
        {
            mOut = (lhs.rotationScale * rhs) + lhs.translation;
        }

        public static void TransformDirection(in TransformMatrix lhs, in Vector2 rhs, out Vector2 mOut)
        {
            mOut = (lhs.rotationScale * rhs);
        }

        public static void Deconstruct(in TransformMatrix mat, out Vector2 translation, out float rotation, out float xShear, out Vector2 scale)
        {
            translation = mat.translation;
            (rotation, xShear, scale) = mat.rotationScale;
        }

        /// <summary>
        /// Creates a <see cref="TransformMatrix"/> which transfroms spase from <paramref name="fromMat"/> to <paramref name="toMat"/> <br/>
        /// <paramref name="fromToMat"/> * <paramref name="fromMat"/> = <paramref name="toMat"/>
        /// </summary>
        /// <returns></returns>
        public static void FromTo(in TransformMatrix fromMat, in TransformMatrix toMat, out TransformMatrix fromToMat)
        {
            Inverse(fromMat, out var fromInv);
            fromToMat = fromInv * toMat;
        }

        /// <summary>
        /// Apparently as simple as taking <see cref="TransformMatrix.rotationScale"/> determinant
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static float Determinant(in TransformMatrix mat)
        {
            //Every component except these two zeros out
            // m00 * m11 * 1 - m10 * m01 * 1
            return mat.rotationScale.Determinant();
        }

        public static void Inverse(in TransformMatrix mat, out TransformMatrix mOut)
        {
            //mat.m00 * mat.m11 -
            //mat.m01 * mat.m10

            //Not Transpose
            //m00 m10 0 
            //m01 m11 0
            //tx  ty  1

            //Transpose
            //m00 m01 tx 
            //m10 m11 ty
            //0   0   1

            var det = Determinant(mat);

            var rs = mat.rotationScale;

            //Inverse the 2x2
            Matrix2x2 rsOut;

            rsOut.m00 = rs.m11 / det;
            rsOut.m10 = -rs.m10 / det;
            rsOut.m01 = -rs.m01 / det;
            rsOut.m11 = rs.m00 / det;

            var t = mat.translation;

            //Inverse translation
            Vector2 tOut;

            //tOut.X = (rs.m01 * t.Y - rs.m11 * t.X) / det;
            //tOut.Y = -(rs.m00 * t.Y - rs.m10 * t.X) / det;
            var rs2 = rsOut;
            rs2.m00 = -rs2.m00;
            rs2.m10 = -rs2.m10;
            rs2.m01 = -rs2.m01;
            rs2.m11 = -rs2.m11;
            tOut = rs2 * t;

            //Put everything together
            mOut.rotationScale = rsOut;
            mOut.translation = tOut;
        }
    
        public static TransformMatrix TranslationRotationScale(Vector2 translation, float rotationRadians, Vector2 scale)
        {
            TransformMatrix trs;
            trs.rotationScale = Matrix2x2.RotationScale(rotationRadians, scale);
            trs.translation = translation;
            return trs;
        }

        public static TransformMatrix TranslationRotationShearScale(Vector2 translation, float rotationRadians, float xShear, Vector2 scale)
        {
            TransformMatrix trss;
            trss.rotationScale = Matrix2x2.RotationShearScale(rotationRadians, xShear, scale);
            trss.translation = translation;
            return trss;
        }
    }
}
