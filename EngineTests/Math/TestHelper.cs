using MonoEngine.Math;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using MonoEngine.Util;

namespace EngineTests.Math
{
    public static class TestHelper
    {
        public static float epsilon = 1f / 1024f;

        public static void AssertIdentity(Matrix2x2 mat)
        {
            Assert.AreEqual(1f, mat.m00, epsilon);
            Assert.AreEqual(0f, mat.m10, epsilon);
            Assert.AreEqual(0f, mat.m01, epsilon);
            Assert.AreEqual(1f, mat.m11, epsilon);
        }

        public static void AssertIdentity(TransformMatrix mat)
        {
            Assert.AreEqual(1f, mat[0, 0], epsilon);
            Assert.AreEqual(1f, mat[1, 1], epsilon);
            Assert.AreEqual(0f, mat[1, 0], epsilon);
            Assert.AreEqual(0f, mat[0, 1], epsilon);
            Assert.AreEqual(0f, mat[2, 0], epsilon);
            Assert.AreEqual(0f, mat[2, 1], epsilon);
        }

        public static void AssertVector(Vector2 expected, Vector2 actual)
        {
            actual.LogThis();
            Assert.AreEqual(expected.X, actual.X, epsilon);
            Assert.AreEqual(expected.Y, actual.Y, epsilon);
        }
    }
}
