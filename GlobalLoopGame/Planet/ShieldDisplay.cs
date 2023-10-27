using GlobalLoopGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Rendering;
using MonoEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
        }

        public void SetSegmentColor(int idx, Color color)
        {
            segments[idx].Color = color.ToVector4();
        }

        public override void Render(Camera camera)
        {
            var instance = new RenderPipeline.InstanceData(Transform.LocalToWorld, Color.Transparent);
            transformBuffer.SetData(Enumerable.Repeat(instance, segmentCount).ToArray());

            segmentBuffer.SetData(segments);

            VertexBufferBinding[] bindings = new VertexBufferBinding[]
            {
                    new VertexBufferBinding(Pipeline.quadVerts),
                    new VertexBufferBinding(transformBuffer, 0, 1),
                    new VertexBufferBinding(segmentBuffer, 0, 1)
            };

            var graphics = Pipeline.Graphics;
            var effect = GameEffects.Shield;//pipeline.CurrentState.CurrentEffect;
            var cameraMatrixInv = camera.ProjectionMatrix;
            graphics.BlendState = BlendState.AlphaBlend;

            //effect.CurrentTechnique = effect.Techniques["Unlit"];

            //effect.Parameters[Effects.SpriteAtlas].SetValue(Pipeline.CurrentState.SpriteAtlas);
            //effect.Parameters[Effects.AtlasSize].SetValue(Pipeline.CurrentState.SpriteAtlas.Depth);
            effect.Parameters[Effects.CameraRS].SetValue(cameraMatrixInv.RS.Flat);
            effect.Parameters[Effects.CameraT].SetValue(cameraMatrixInv.T);
            //TODO
            effect.Parameters["Falloffs"].SetValue(new Vector2(1f, MathHelper.ToRadians(5f)));

            effect.CurrentTechnique.Passes[0].Apply();

            graphics.Indices = Pipeline.quadInds;

            Pipeline.Graphics.SamplerStates[1] = SamplerState.PointClamp;
            //pipeline.Graphics.Textures[0] = pipeline.CurrentState.SpriteAtlas;

            graphics.SetVertexBuffers(bindings);
            graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, segmentCount);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SegmentData
        {
            public Vector4 SegmentSize;
            public Vector4 Color;
        }
    }
}
