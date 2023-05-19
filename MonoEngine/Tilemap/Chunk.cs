using Microsoft.Xna.Framework;
using MonoEngine.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Tilemap
{
    /// <summary>
    /// Represents a tilemap chunk
    /// </summary>
    public class Chunk
    {
        public const int chunkSize = 32;
        public const int chunkSizeMask = 31;
        public const int tileCount = chunkSize * chunkSize;

        public TileInstance[] ChunkData => chunkData;

        private TileInstance[] chunkData;

        public Chunk()
        {
            chunkData = new TileInstance[tileCount];
        }

        public TileInstance GetTile(Vector2Int pos)
        {
            return chunkData[PosToIndex(pos)];
        }

        public void SetTile(Vector2Int pos, TileInstance tile)
        {
            chunkData[PosToIndex(pos)] = tile;
        }

        /// <summary>
        /// Sets a rectangle of tiles in chunk
        /// Much faster than multiple <see cref="SetTile(Vector2Int, TileInstance)"/> calls
        /// Does perform bounds and data size checks
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="slice"></param>
        public void SetTilesRect(Rectangle area, in ReadOnlySpan<TileInstance> data)
        {
            var x = area.X;
            var y = area.Y;
            var width = area.Width;
            var height = area.Height;

            if (x < 0 || (x + width > chunkSize) || y < 0 || (y + height > chunkSize))
            {
                throw new IndexOutOfRangeException("Outside of chunk");
            }

            if (data.Length < width * height)
            {
                throw new ArgumentException("Not enough tiles for desired area");
            }

            //Special case, we can do faster
            if (width == chunkSize)
            {
                SetSliceUnsafe(y * chunkSize, data);
                return;
            }

            for (int i = 0; i < area.Y; i++)
            {
                var slice = data.Slice(i * width, width);
                
                SetSliceUnsafe(new Vector2Int(x, y + i), slice);
            }
        }

        /// <summary>
        /// Sets a horizontal slice of tiles to this chunk <br/>
        /// Much faster than multiple <see cref="SetTile(Vector2Int, TileInstance)"/> calls <br/>
        /// DOES NOT perform any bounds checks
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="slice"></param>
        internal void SetSliceUnsafe(Vector2Int start, in ReadOnlySpan<TileInstance> slice)
        {
            SetSliceUnsafe(PosToIndex(start), slice);
        }

        /// <inheritdoc cref="SetSliceUnsafe(Vector2Int, in ReadOnlySpan{TileInstance})"/>
        internal void SetSliceUnsafe(int startIdx, in ReadOnlySpan<TileInstance> slice)
        {
            var dstSpan = new Span<TileInstance>(chunkData, startIdx, slice.Length);
            slice.CopyTo(dstSpan);
        }


        public static int PosToIndex(Vector2Int pos)
        {
            return pos.Y * chunkSize + pos.X;
        }

        public static Vector2Int IndexToPos(int index)
        {
            return new Vector2Int(index % chunkSize, index / chunkSize);
        }
    }
}
