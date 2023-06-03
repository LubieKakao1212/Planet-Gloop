using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Tilemap
{
    using Utils;
    using Math;

    public class Tilemap
    {
        public IEnumerable<KeyValuePair<Point, Chunk>> Chunks => chunks;

        private Dictionary<Point, Chunk> chunks = new Dictionary<Point, Chunk>();

        public Tilemap()
        {
            
        }

        public void SetTile(Point pos, TileInstance tile)
        {
            var chunk = GetChunkAt(pos, true);
            var posInChunk = GridToPosInChunk(pos);

            chunk.SetTile(posInChunk, tile);
        }

        public void SetTilesBlock(Rectangle rect, ReadOnlySpan<TileInstance> instance)
        {
            //Todo implement
            throw new NotImplementedException("TODO, Not yet implemented");
        }

        public Chunk GetChunkAt(Point pos, bool createNew)
        {
            return GetChunk(GridToChunkPos(pos), createNew);
        }

        public IEnumerable<Chunk> GetChunksAt(Rectangle rect, bool createNew)
        {
            var c1 = GridToChunkPos(new Point(rect.X, rect.Y));
            var c2 = GridToChunkPos(new Point(rect.X + rect.Width, rect.Y + rect.Height));

            for(var x = c1.X; x <= c2.X; x++)
                for (var y = c1.Y; y <= c2.Y; y++)
                {
                    Chunk chunk = GetChunk(new Point(x, y), createNew);
                    if (chunk != null)
                    {
                        yield return chunk;
                    }
                }
        }

        public Chunk GetChunk(Point chunkPos, bool createNew)
        {
            return createNew ? chunks.GetOrSetToDefaultLazy(chunkPos, (cPos) => new Chunk(cPos)) : chunks.GetValueOrDefault(chunkPos);
        }

        public static Point GridToPosInChunk(Point gridPos)
        {
            return new Point(gridPos.X & Chunk.chunkSizeMask, gridPos.Y & Chunk.chunkSizeMask);
        }

        public static Point GridToChunkPos(Point gridPos)
        {
            return gridPos.FloorDiv(Chunk.chunkSize);
        }

        public static Point ChunkToGridPos(Point chunkPos)
        {
            return new Point(chunkPos.X * Chunk.chunkSize, chunkPos.Y * Chunk.chunkSize);
        }

    }
}
