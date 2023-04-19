using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Math;
using MonoEngine.Rendering.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Rendering
{
    public class RenderPipeline
    {
        private Vector2 quadScale = new Vector2(0.1f, 0.1f);
        private VertexBuffer quadVerts;
        private IndexBuffer quadInds;

        private VertexBuffer instances;
        private const int InstanceCount = 1024*4;
        private VertexDeclaration InstanceVertexDeclaration;
        
        private Effect effect;

        private VertexBufferBinding[] bindings;

        [StructLayout(LayoutKind.Sequential)]
        private struct InstanceData
        {
            public Vector4 matrix;
            public Vector2 pos;
            public Vector4 color;

            public InstanceData(Vector4 matrix, Vector2 pos, Color color)
            {
                this.matrix = matrix;
                this.pos = pos;
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

            instances = new VertexBuffer(graphics, InstanceVertexDeclaration, InstanceCount, BufferUsage.WriteOnly);

            InstanceData[] instanceData = new InstanceData[InstanceCount];
            
            var random = new Random();

            for(int i = 0; i< InstanceCount; i++)
            {
                instanceData[i] = new InstanceData(
                    //new Vector4(1,0,0,1),
                    new Vector4(
                        random.RandomNormalised(),
                        random.RandomNormalised(),
                        random.RandomNormalised(),
                        random.RandomNormalised()
                        ),
                    new Vector2(
                        random.RandomNormalised(),
                        random.RandomNormalised()
                        ),
                    random.RandomColor()
                    );
            }

            instances.SetData(instanceData);
            #endregion

            bindings = new VertexBufferBinding[2] 
            { 
                new VertexBufferBinding(quadVerts),
                new VertexBufferBinding(instances, 0 , 1)
            };
        }

        public void Render(GraphicsDevice graphics)
        {
            graphics.Clear(Color.Black);

            //effect.CurrentTechnique = effect.Techniques["Simple"];
            effect.CurrentTechnique = effect.Techniques["Unlit"];

            graphics.Indices = quadInds;

            effect.CurrentTechnique.Passes[0].Apply();

            graphics.SetVertexBuffers(bindings);
            graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, InstanceCount);
            //graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }
    }
}
