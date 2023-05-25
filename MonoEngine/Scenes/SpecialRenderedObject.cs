using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Scenes
{
    public abstract class SpecialRenderedObject : DrawableObject, ISpecialRenderer
    {
        protected RenderPipeline Pipeline { get; }

        public SpecialRenderedObject(RenderPipeline pipeline, Color color, float drawOrder) : base(color, drawOrder) 
        { 
            this.Pipeline = pipeline;
        }

        public abstract void Render(Camera camera);
    }
}
