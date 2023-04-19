using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Math
{
    public static class RandomExtensions
    {
        public static float RandomNormalised(this Random random)
        {
            return (random.NextSingle() * 2f) - 1f;
        }

        public static Color RandomColor(this Random random)
        {
            return new Color(
                random.NextSingle(),
                random.NextSingle(),
                random.NextSingle(),
                1f
                );
        }
    }
}
