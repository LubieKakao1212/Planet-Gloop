using Microsoft.Xna.Framework;
using MonoEngine;
using MonoEngine.Math;
using MonoEngine.Util;
using NUnit.Framework;
using System;
using static EngineTests.Math.TestHelper;

namespace EngineTests.Math
{
    [TestFixture]
    internal class TransformMatrixTests
    {
        Vector2 testVect1 = Vector2.One;
        Vector2 testVect2 = new(2f, -1f);

        Vector2 translation1 = new Vector2(1f, 2f);
        Vector2 translation2 = new Vector2(2f, 3f);

        static float angle90 = MathF.PI / 2;
        Matrix2x2 rotation90 = Matrix2x2.Rotation(angle90);

        static Vector2 scaleVect = new(2f, 3f);
        static Vector2 scaleVect2 = new(0.5f, 0.25f);
        Matrix2x2 scale = Matrix2x2.Scale(scaleVect);
        Matrix2x2 scale2 = Matrix2x2.Scale(scaleVect2);

        [Test]
        public void Translation()
        {
            var trans = new TransformMatrix(new Matrix2x2(1f), translation1);

            var v11 = Vector2.One;

            var o = trans.TransformPoint(v11);
            AssertVector(new(2f, 3f), o);
            
            o = trans.TransformDirection(v11);
            AssertVector(new(1f, 1f), o);
        }

        [Test]
        public void Rotation90()
        {
            var trans = new TransformMatrix(rotation90, Vector2.Zero);

            var v11 = Vector2.One;

            var o = trans.TransformPoint(v11);
            AssertVector(new(-1f, 1f), o);

            o = trans.TransformDirection(v11);
            AssertVector(new(-1f, 1f), o);
        }

        [Test]
        public void Scale()
        {
            var trans = new TransformMatrix(scale, Vector2.Zero);

            var v11 = Vector2.One;

            var o = trans.TransformPoint(v11);
            AssertVector(new(2f, 3f), o);

            o = trans.TransformDirection(v11);
            AssertVector(new(2f, 3f), o);
        }

        [Test]
        public void TranslationMulTranslation()
        {
            var trans1 = new TransformMatrix(new Matrix2x2(1f), translation1);
            var trans2 = new TransformMatrix(new Matrix2x2(1f), translation2)
                * trans1;

            var v11 = Vector2.One;

            var o = trans2.TransformPoint(v11);
            AssertVector(new(4f, 6f), o);

            o = trans2.TransformDirection(v11);
            AssertVector(new(1f, 1f), o);
        }

        [Test]
        public void Rotation90MulTranslation()
        {
            var trans1 = new TransformMatrix(new Matrix2x2(1f), translation1);

            var trans2 = new TransformMatrix(rotation90, Vector2.Zero) * trans1;

            var v11 = Vector2.One;

            //(1f, 1f) + (1f, 2f) rot 90
            //(2f, 3f) rot 90
            //(-3f, 2f)

            var o = trans2.TransformPoint(v11);
            AssertVector(new(-3f, 2f), o);

            o = trans2.TransformDirection(v11);
            AssertVector(new(-1f, 1f), o);
        }

        [Test]
        public void TranslationMulRotation90()
        {
            var trans1 = new TransformMatrix(rotation90, Vector2.Zero);
            var trans2 = new TransformMatrix(new Matrix2x2(1f), translation1) 
                * trans1;
            
            var v11 = Vector2.One;

            //(1f, 1f) rot 90 + (1f, 2f)
            //(-1f, 1f) + (1f, 2f)
            //(0f, 3f)

            var o = trans2.TransformPoint(v11);
            AssertVector(new(0f, 3f), o);

            o = trans2.TransformDirection(v11);
            AssertVector(new(-1f, 1f), o);
        }
        
        [Test]
        public void TRSSnoSkew()
        {
            var trss = TransformMatrix.TranslationRotationShearScale(translation1, angle90, 0f, scaleVect);
            
            //(1, 1) * (2, 3) rot 90 + (1, 2)
            //(2, 3) rot 90 + (1, 2)
            //(-3, 2) + (1, 2)
            //(-2, 4)
            TestTransform(new(-2f, 4f), new(-3f, 2f), trss, testVect1);

            //(2, -1) * (2, 3) rot 90 + (1, 2)
            //(4, -3) rot 90 + (1, 2)
            //(3, 4) + (1, 2)
            //(4, 6)
            TestTransform(new(4f, 6f), new(3f, 4f), trss, testVect2);
        }

        [Test]
        public void TRSSnoSkewMulTRSSnoSkew()
        {
            var trss1 = TransformMatrix.TranslationRotationShearScale(translation1, angle90, 0f, scaleVect);

            var trss2 = TransformMatrix.TranslationRotationShearScale(translation2, -angle90, 0f, scaleVect2) * trss1;

            //Common
            //(1, 1) * (2, 3) rot 90 + (1, 2) * (0.5, 0.25) rot -90 + (2, 3)
            //(2, 3) rot 90 + (1, 2) * (0.5, 0.25) rot -90 + (2, 3)
            //(-3, 2) + (1, 2) * (0.5, 0.25) rot -90 + (2, 3)
            //Point
            //(-2, 4) * (0.5, 0.25) rot -90 + (2, 3)
            //(-1, 1) rot -90 + (2, 3)
            //(1, 1) + (2, 3)
            //(3, 4)
            //Dir
            //(-3, 2) * (0.5, 0.25) rot -90
            //(-1.5, 0.5) rot -90
            //(0.5, 1.5)
            TestTransform(new(3f, 4f), new(0.5f, 1.5f), trss2, testVect1);

            //Common
            //(2, -1) * (2, 3) rot 90 + (1, 2) * (0.5, 0.25) rot -90 + (2, 3)
            //(4, -3) rot 90 + (1, 2) * (0.5, 0.25) rot -90 + (2, 3)
            //(3, 4) + (1, 2) * (0.5, 0.25) rot -90 + (2, 3)
            //Point
            //(4, 6) * (0.5, 0.25) rot -90 + (2, 3)
            //(2, 1.5) rot -90 + (2, 3)
            //(1.5, -2) + (2, 3)
            //(3.5, 1)
            //Dir
            //(3, 4) * (0.5, 0.25) rot -90
            //(1.5, 1) rot -90
            //(1, -1.5)
            TestTransform(new(3.5f, 1f), new(1f, -1.5f), trss2, testVect2);
        }

        [Test]
        public void TRSSMulInverse()
        {
            //var trss1 = TransformMatrix.TranslationRotationShearScale(translation1, angle90, 0f, scaleVect);

            var trss2 = TransformMatrix.TranslationRotationShearScale(translation2, -angle90 / 2f, 0f, new(1f,1f));// scaleVect2);// * trss1;

            var inv = trss2.Inverse();

            var result = inv * trss2;

            AssertIdentity(inv * trss2);
            AssertIdentity(trss2 * inv);
        }

        private void TestTransform(Vector2 expectedPoint, Vector2 expectedDirection, TransformMatrix transform, Vector2 input)
        {
            var o = transform.TransformPoint(input);
            AssertVector(expectedPoint, o);

            o = transform.TransformDirection(input);
            AssertVector(expectedDirection, o);
        }
    }
}
