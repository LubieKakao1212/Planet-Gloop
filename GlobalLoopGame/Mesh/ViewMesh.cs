using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Math;
using MonoEngine.Rendering.Data;
using nkast.Aether.Physics2D.Dynamics;
using System;

namespace GlobalLoopGame.Mesh
{
    public static class ViewMesh
    {
        public static void CalculateMesh(World world, Vector2 origin, float minRadius, float maxRadius, int rays, out Vertex2DPosition[] verticies, out int[] indicies, Category collidesWith)
        {
            verticies = new Vertex2DPosition[rays * 2];
            indicies = new int[6 * rays];

            //verticies[0].Pos = Vector2.Zero;

            var i0 = 0;

            for (int i = 0; i < rays; i++)
            {
                var fraction = i / (float)rays;
                var angle = fraction * MathHelper.TwoPi;

                var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

                var position = origin + dir * maxRadius;

                world.RayCast(
                    (fixture, point, normal, fraction) =>
                    {
                        if ((fixture.CollisionCategories & collidesWith) != Category.None)
                        {
                            position = point;
                            return fraction;
                        }
                        //TODO Check if works (I think it works)
                        return -1;
                    }, origin, position);

                var outerRadius = MathF.Max((position - origin).Length(), minRadius);

                verticies[2 * i].Pos = dir * minRadius;
                verticies[2 * i + 1].Pos = dir * outerRadius;
                //Skip this in first iteration
                if (i > 0)
                {
                    AddTriangles(indicies, i - 1, i);
                }
            }
            AddTriangles(indicies, rays - 1, 0);
            /*
            i0 = (rays - 1) * 3;
            indicies[i0] = 0;
            indicies[i0 + 1] = 1;
            indicies[i0 + 2] = rays;*/
        }

        private static void AddTriangles(int[] indicies, int lowIdx, int highIdx)
        {
            int i = lowIdx * 6;

            int i0 = lowIdx * 2;
            int i1 = highIdx * 2;
            //i0 = (i - 1) * 3;
            indicies[i] = i0 + 1;
            indicies[i + 1] = i0;
            indicies[i + 2] = i1;

            indicies[i + 3] = i1 + 1;
            indicies[i + 4] = i0 + 1;
            indicies[i + 5] = i1;
        }
    }
}
