using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Rendering;
using MonoEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoEngine.Tilemap
{
    public class TilemapRenderer : SpecialRenderedObject
    {
        private Tilemap tilemap;
        private Grid grid;

        private VertexBuffer instanceBuffer;
        
        public TilemapRenderer(RenderPipeline pipeline, Color color, float drawOrder) : base(pipeline, color, drawOrder)
        {
            
        }

        public override void Render(Camera camera)
        {
            
        }
    }
}
