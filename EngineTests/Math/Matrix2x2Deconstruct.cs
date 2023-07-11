using Microsoft.Xna.Framework;
using MonoEngine.Math;
using NUnit.Framework;
using System;
using static MonoEngine.Math.MathUtil;

namespace EngineTests.Math
{
    [TestFixture]
    internal class Matrix2x2Deconstruct
    {
        float epsilon = 1f / 1024f;

        [Test]
        public void Rotation()
        {
            var r = MathF.PI / 8f;

            var mat = Matrix2x2.Rotation(r);

            var (r0, sk0, sc0) = mat;

            ValidateParams(r, r0, 0f, sk0, Vector2.One, sc0);
        }

        [Test]
        public void DeconstructRSS()
        {
            var r = MathF.PI / 8f;
            var sk = MathF.PI / 8f;
            var sc = new Vector2(2f, 0.5f);

            var mat = Matrix2x2.RotationSkewScale(r, sk, sc);

            var (r0, sk0, sc0) = mat;

            ValidateParams(r, r0, sk, sk0, sc, sc0);
        }

        [Test]
        public void DeconstructRSR()
        {
            var scale = new Vector2(1f, 0.5f);
            var r = Matrix2x2.Rotation(MathF.PI / 2f);
            var rs = Matrix2x2.RotationScale(MathF.PI / 4f, scale);

            var rsr = rs * r;

            var vX = rsr * Vector2.UnitX;
            var vY = rsr * Vector2.UnitY;

            var (theta, sh, sc) = rsr;
            /*Assert.AreEqual(MathF.PI / 2f, t, epsilon, "theta");
            Assert.AreEqual(MathF.PI / 4f, p, epsilon, "phi");

            Assert.AreEqual(s.X, scale.X, epsilon, "scale X");
            Assert.AreEqual(s.Y, scale.Y, epsilon, "scale Y");*/

            var reconstructed = Matrix2x2.RotationSkewScale(theta, sh, sc);

            var vX1 = reconstructed * Vector2.UnitX;
            var vY1 = reconstructed * Vector2.UnitY;

            Assert.AreEqual(epsilon, (vX - vX1).Length(), epsilon);
            Assert.AreEqual(epsilon, (vY - vY1).Length(), epsilon);
        }

        private void ValidateParams(float r, float rD, float sk, float skD, Vector2 sc, Vector2 scD)
        {
            Assert.AreEqual(0f, AngleDistance(r, rD), epsilon, $"Theta: {LoopAngle(rD)}, expected {LoopAngle(r)}");
            Assert.AreEqual(0f, AngleDistance(sk, skD), epsilon, $"xShear: {LoopAngle(skD)}, expected {LoopAngle(sk)}");
            Assert.AreEqual(sc.X, scD.X, epsilon, $"Scale X: {scD.X}, expected {sc.X}");
            Assert.AreEqual(sc.Y, scD.Y, epsilon, $"Scale Y: {scD.Y}, expected {sc.Y}");
        }
    }
}
