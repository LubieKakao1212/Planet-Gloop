using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return a / b + (a >> 31);
        }

    }
}
