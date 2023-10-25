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
        public static void CalculateMesh(World world, Vector2 origin, float radius, int rays, out Vertex2DPosition[] verticies, out int[] indicies, Category collidesWith)
        {
            verticies = new Vertex2DPosition[rays + 1];
            indicies = new int[3 * rays];

            verticies[0].Pos = Vector2.Zero;

            var i0 = 0;

            for (int i = 0; i < rays; i++)
            {
                var fraction = i / (float)rays;
                var angle = fraction * MathHelper.TwoPi;

                var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

                var position = origin + dir * radius;

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

                verticies[i + 1].Pos = position - origin;
                //Skip this in first iteration
                if (i > 0)
                {
                    i0 = (i - 1) * 3;
                    indicies[i0    ] = 0;
                    indicies[i0 + 1] = i + 1;
                    indicies[i0 + 2] = i;
                }
            }

            i0 = (rays - 1) * 3;
            indicies[i0] = 0;
            indicies[i0 + 1] = 1;
            indicies[i0 + 2] = rays;
        }
    }
}
