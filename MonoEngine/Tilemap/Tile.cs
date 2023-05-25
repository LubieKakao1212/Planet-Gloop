using Microsoft.Xna.Framework;
using MonoEngine.Math;
using MonoEngine.Rendering;
using System.Data;

namespace MonoEngine.Tilemap
{
    public class Tile
    {
        public Sprite Sprite { get; init; }
        //private Matrix2x2 transform;
        public float Order { get; init; }

        public Color Tint => tint;
        private Color tint;
    }

    public readonly struct TileInstance
    {
        public Tile Tile { get; init; }
        public Matrix2x2 Transform { get; init; }

        public TileInstance()
        {
            Transform = new Matrix2x2(1f);

            Tile = null;
        }

        public TileInstance(Tile tile, Matrix2x2 transfrom)
        {
            Tile = tile;
            Transform = transfrom;
        }

        public static implicit operator TileInstance(Tile tile)
        {
            return new TileInstance(tile, new Matrix2x2(1f));
        }
    }
}
