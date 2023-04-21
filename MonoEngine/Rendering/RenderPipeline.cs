using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Math;
using MonoEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MonoEngine.Rendering
{
    public class RenderPipeline
    {
        private Vector2 quadScale = new Vector2(1f, 1f);
        private VertexBuffer quadVerts;
        private IndexBuffer quadInds;

        private DynamicVertexBuffer instanceBuffer;
        private const int MaxInstanceCount = 4096;
        private VertexDeclaration InstanceVertexDeclaration;
        
        private Effect effect;

        private VertexBufferBinding[] bindings;

        [StructLayout(LayoutKind.Sequential)]
        private struct InstanceData
        {
            public Vector4 sr;
            public Vector2 t;
            public Vector4 color;

            public InstanceData(TransformMatrix transform, Color color)
            {
                this.sr = transform.RS.Flat();
                this.t = transform.T;
                this.color = color.ToVector4();
            }
        }

        public void Init(GraphicsDevice graphics, ContentManager content)
        {
            effect = content.Load<Effect>("Unlit");

            #region quad
            quadVerts = new VertexBuffer(graphics, VertexPosition.VertexDeclaration, 4, BufferUsage.WriteOnly);

            quadVerts.SetData(new VertexPosition[4] 
            {
                new VertexPosition(new Vector3(-quadScale.X, -quadScale.Y, 0)),
                new VertexPosition(new Vector3(quadScale.X, -quadScale.Y, 0)),
                new VertexPosition(new Vector3(quadScale.X, quadScale.Y, 0)),
                new VertexPosition(new Vector3(-quadScale.X, quadScale.Y, 0))
            });

            quadInds = new IndexBuffer(graphics, typeof(short), 6, BufferUsage.WriteOnly);
            quadInds.SetData(new short[6]
            {
                1, 0, 2, 2, 0, 3
            });
            #endregion

            #region Instances

            InstanceVertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementUsage.Color, 0)
                );

            instanceBuffer = new DynamicVertexBuffer(graphics, InstanceVertexDeclaration, MaxInstanceCount, BufferUsage.WriteOnly);

            InstanceData[] instanceData = new InstanceData[MaxInstanceCount];

            TransformMatrix zeroTransform = new TransformMatrix();

            for (int i = 0; i< MaxInstanceCount; i++)
            {
                instanceData[i] = new InstanceData(
                    zeroTransform,
                    Color.White
                    );
            }

            instanceBuffer.SetData(instanceData);
            #endregion

            bindings = new VertexBufferBinding[2] 
            { 
                new VertexBufferBinding(quadVerts),
                new VertexBufferBinding(instanceBuffer, 0 , 1)
            };
        }

        public void RenderScene(GraphicsDevice graphics, Scene scene)
        {
            var cameraMatrixInv = scene.Camera.CameraMatrix.Inverse();
            foreach (var instanceCount in SetupSceneInstances(scene))
            {
                Render(graphics, cameraMatrixInv, instanceCount);
            }
        }

        public void Render(GraphicsDevice graphics, TransformMatrix cameraMatrixInv, int instanceCount)
        {
            graphics.Clear(Color.Black);

            effect.CurrentTechnique = effect.Techniques["Unlit"];
            effect.Parameters["CameraRS"].SetValue(cameraMatrixInv.RS.Flat());
            effect.Parameters["CameraT"].SetValue(cameraMatrixInv.T);

            graphics.Indices = quadInds;

            effect.CurrentTechnique.Passes[0].Apply();

            graphics.SetVertexBuffers(bindings);
            graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, instanceCount);
        }

        public IEnumerable<int> SetupSceneInstances(Scene scene)
        {
            int i = 0;
            var instances = new InstanceData[MathHelper.Min(scene.Drawables.Count, MaxInstanceCount)];
            foreach (var drawable in scene.Drawables)
            {
                var ltw = drawable.Transform.LocalToWorld;
                InstanceData data = new InstanceData(ltw, drawable.Color);
                instances[i++] = data;
                if (i == MaxInstanceCount)
                {
                    instanceBuffer.SetData(0, instances, 0, i, InstanceVertexDeclaration.VertexStride, SetDataOptions.None);
                    yield return i;
                    i = 0;
                }
            }
            instanceBuffer.SetData(instances, 0, i, SetDataOptions.None);
            yield return i;
            yield break;
        }
    }
}
