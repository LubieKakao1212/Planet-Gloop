using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Rendering.Data
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex2DPosition
    {
        public static VertexDeclaration VertexDeclaration { get; private set; }

        public Vector2 Pos;

        public Vertex2DPosition(Vector2 pos) { Pos = pos; }

        static Vertex2DPosition()
        {
            VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
        }
    }
}
