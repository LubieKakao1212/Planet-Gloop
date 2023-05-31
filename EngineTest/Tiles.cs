using Microsoft.Xna.Framework;
using MonoEngine.Math;
using MonoEngine.Tilemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineTest
{
    public static class Tiles
    {
        public static readonly Tile Green = new Tile() { Order = -1f, Tint = Color.Green };
        public static readonly Tile OversizedRed = new Tile() { Order = 0.1f, Tint = Color.Red };
        public static readonly Tile OversizedTransparentYellow = new Tile() { Order = 0.2f, Tint = new Color(1f, 1f, 0f, 0.5f) };

        public static readonly Matrix2x2 GreenTransform = new Matrix2x2(1);
        public static readonly Matrix2x2 OversizedRedTransform = new Matrix2x2(1.25f);
        public static readonly Matrix2x2 OversizedTransparentYellowTransform = new Matrix2x2(Vector2.UnitX, Vector2.One);
    }
}
