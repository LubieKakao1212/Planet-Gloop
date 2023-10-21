using Microsoft.Xna.Framework;
using System;
using static Microsoft.Xna.Framework.MathHelper;

namespace MonoEngine.Math
{
    public static class MathUtil
    {

        /// <summary>
        /// Performs a integer division rounding down
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b">Bust be positive</param>
        public static int FloorDiv(int a, int b)
        {
            return (a - (a >> 31)) / b + (a >> 31);
        }

        public static float Loop(float t, float period)
        {
            return t - MathF.Floor(t / period) * period;
        }

        public static float LoopAngle(float t)
        {
            return Loop(t, MathHelper.TwoPi);
        }

        public static float LoopedDistance(float a, float b, float period)
        {
            a = Loop(a, period);
            b = Loop(b, period);
            float d1 = MathF.Abs(a - b);
            float d2 = MathF.Abs(a + period - b);
            float d3 = MathF.Abs(a - period - b);
            return Min(Min(d1, d2), d3);
        }

        public static float AngleDistance(float a, float b)
        {
            return LoopedDistance(a, b, TwoPi);
        }

    }
}
