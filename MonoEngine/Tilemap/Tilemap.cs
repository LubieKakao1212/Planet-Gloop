using MonoEngine.Math;
using MonoEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Tilemap
{
    public class Tilemap
    {
        public IEnumerable<KeyValuePair<Vector2Int, Chunk>> Chunks => chunks;

        private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

        public Tilemap()
        {
            
        }

        public void SetTile(Vector2Int pos, TileInstance tile)
        {
            var chunk = GetChunkAt(pos);

        }

        public Chunk GetChunkAt(Vector2Int pos)
        {
            return chunks.GetOrSetToDefaultLazy(Vector2Int.FloorDiv(pos, Chunk.chunkSize), () => new Chunk());
        }

        public static Vector2Int WorldToPosInChunk(Vector2Int worldPos)
        {
            return new Vector2Int(worldPos.X & Chunk.chunkSizeMask, worldPos.Y & Chunk.chunkSizeMask);
        }

    }
}
