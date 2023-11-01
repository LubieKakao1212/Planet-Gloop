using GlobalLoopGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Custom2d_Engine.Rendering;
using Custom2d_Engine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;

namespace GlobalLoopGame.Planet
{
    public class ShieldDisplay : SpecialRenderedObject
    {
        private static VertexDeclaration segmentInstanceVertex = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.Color, 1)
                );

        private int segmentCount;

        private VertexBuffer transformBuffer;
        private VertexBuffer segmentBuffer;

        private SegmentData[] segments;
        private bool dirty;

        private Effect effect = GameEffects.Shield.Clone();

        public ShieldDisplay(RenderPipeline pipeline, int segmentCount, float radius, float thickness, float spacingDeg, float drawOrder) : base(pipeline, Color.White, drawOrder)
        {
            transformBuffer = new VertexBuffer(pipeline.Graphics, pipeline.InstanceVertexDeclaration, segmentCount, BufferUsage.WriteOnly);
            segmentBuffer = new VertexBuffer(pipeline.Graphics, segmentInstanceVertex, segmentCount, BufferUsage.WriteOnly);
            this.segmentCount = segmentCount;
            segments = new SegmentData[segmentCount];

            float arc = MathHelper.TwoPi / segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                segments[i].Color = Color.Cyan.ToVector4();
                segments[i].SegmentSize = new Vector4(radius, thickness, i * arc + arc / 2f - MathF.PI, arc / 2f - spacingDeg * MathF.PI / 180f);
            }

            effect.Parameters[GameEffects.Falloffs].SetValue(new Vector2(1f, MathHelper.ToRadians(5f)));
        }

        public void SetSegmentColor(int idx, Color color)
        {
            segments[idx].Color = color.ToVector4();
            dirty = true;
        }

        public override void Render(Camera camera)
        {
            var instance = new RenderPipeline.InstanceData(Transform.LocalToWorld, Color.Transparent);
            transformBuffer.SetData(Enumerable.Repeat(instance, segmentCount).ToArray());
            
            if (dirty)
            {
                segmentBuffer.SetData(segments);
            }

            using var effectScope = new RenderPipeline.EffectScope(Pipeline, effect);
            Pipeline.Rendering.DrawInstancedQuads(segments.Length,
                    new VertexBufferBinding(transformBuffer, 0, 1),
                    new VertexBufferBinding(segmentBuffer, 0, 1));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SegmentData
        {
            public Vector4 SegmentSize;
            public Vector4 Color;
        }
    }
}
