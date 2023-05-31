using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Rendering;
using MonoEngine.Scenes;
using MonoEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MonoEngine.Tilemap
{
    public class TilemapRenderer : SpecialRenderedObject
    {
        private static VertexDeclaration TileInstanceDeclaration { get; } = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1), //2x2 RotScale
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector2, VertexElementUsage.Position, 2), //2 Position
            new VertexElement(sizeof(float) * (4 + 2), VertexElementFormat.Vector4, VertexElementUsage.Color, 0), //Position
            new VertexElement(sizeof(float) * (4 + 2 + 4), VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0) //[2, 2] Texture Rext
            );

        private Tilemap tilemap;
        private Grid grid;

        private DynamicVertexBuffer instanceBuffer;
        
        public TilemapRenderer(Tilemap tilemap, Grid grid, RenderPipeline pipeline, Color color, float drawOrder) : base(pipeline, color, drawOrder)
        {
            this.tilemap = tilemap;
            this.grid = grid;
            instanceBuffer = new DynamicVertexBuffer(pipeline.Graphics, TileInstanceDeclaration, RenderPipeline.MaxInstanceCount, BufferUsage.WriteOnly);
        }

        public override void Render(Camera camera)
        {
            var gridWTL = grid.Transform.WorldToLocal;
            var camProjInv = camera.ProjectionMatrix.Inverse();

            var mat = camProjInv * gridWTL;

            var rect = Camera.CullingRect.Transformed(mat);

            var dataOrders = new List<Ordered<TileInstanceRenderData>>();

            foreach (var chunk in tilemap.GetChunksAt(rect.ToInt(), false))
            {
                //TODO Cache Tiles
                dataOrders.Capacity += Chunk.tileCount;

                var chunkPos = chunk.ChunkPos;
                var chunkGridPos = Tilemap.ChunkToGridPos(chunkPos);

                var i = 0;
                
                dataOrders.AddRange(chunk.ChunkData.SelectMany((tile) =>
                {
                    if(tile.Tile == null)
                    {
                        return Enumerable.Empty<Ordered<TileInstanceRenderData>>();
                    }
                    return Enumerable.Repeat(new Ordered<TileInstanceRenderData>() { Value = new TileInstanceRenderData()
                    {
                        RotScale = tile.Transform.Flat,
                        Position = Chunk.IndexToPos(i++).ToVector2() + chunkGridPos.ToVector2(),
                        Color = tile.Tile.Tint.ToVector4(),
                        TexCoord = tile.Tile.Sprite.TextureRect.Flat
                    }, Order = tile.Tile.Order }
                    , 1);
                }));
            }

            var effect = Effects.TilemapDefault;

            effect.CurrentTechnique = effect.Techniques["Unlit"];
            effect.Parameters[Effects.GridRS].SetValue(grid.Transform.LocalToWorld.RS.Flat);
            effect.Parameters[Effects.GridT].SetValue(grid.Transform.LocalToWorld.T);

            using var effectScope = new RenderPipeline.EffectScope(Pipeline, effect);
            using var cameraScope = new RenderPipeline.CameraScope(Pipeline, camera);

            Pipeline.Rendering.DrawSortedLayerQuads(instanceBuffer, dataOrders.ToArray());
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TileInstanceRenderData
        {
            public Vector4 RotScale;
            public Vector2 Position;
            public Vector4 Color;
            public Vector4 TexCoord;
        }
    }
}
