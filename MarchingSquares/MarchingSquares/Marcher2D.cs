using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoEngine.Math;
using MonoEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarchingSquares.MarchingSquares
{
    public class Marcher2D
    {
        public static void MarchDepthGrid(FiniteGrid<float> gridIn, out VertexPosition[] verticies, out int[] indicies)
        {
            var vCount = (gridIn.Bounds.Width - 1) * (gridIn.Bounds.Height - 1) * 4;
            verticies = new VertexPosition[vCount];
            
            var iCount = (gridIn.Bounds.Width - 1) * (gridIn.Bounds.Height - 1) * 6;
            indicies = new int[iCount];

            var iV = 0;
            var iI = 0;
            var vertex = new VertexPosition();
            for (int y = 0; y < gridIn.Bounds.Height - 1; y++)
                for (int x = 0; x < gridIn.Bounds.Width - 1; x++)
                {
                    vertex.Position = new Vector3(x    , y    , gridIn[x    , y    ]); //-4
                    verticies[iV++] = vertex;
                    vertex.Position = new Vector3(x + 1, y    , gridIn[x + 1, y    ]); //-3  
                    verticies[iV++] = vertex;
                    vertex.Position = new Vector3(x + 1, y + 1, gridIn[x + 1, y + 1]); //-2
                    verticies[iV++] = vertex;
                    vertex.Position = new Vector3(x    , y + 1, gridIn[x    , y + 1]); //-1
                    verticies[iV++] = vertex;

                    //-4, -1, -2
                    indicies[iI++] = iV - 4;
                    indicies[iI++] = iV - 1;
                    indicies[iI++] = iV - 2;
                    
                    //-2, -3, -4
                    indicies[iI++] = iV - 2;
                    indicies[iI++] = iV - 3;
                    indicies[iI++] = iV - 4;
                }
        } 

        public struct RawEdges
        {
            private FiniteGrid<(Edge?, Edge?)> edges;

            public RawEdges(FiniteGrid<(Edge?, Edge?)> edges)
            {
                this.edges = edges;
            }

            public List<List<Vector2>> GetPaths()
            {
                var paths = new List<List<Vector2>>();

                var size = edges.Bounds.Size;

                var visited = new FiniteGrid<int>(size,
                    edges.Content.Select((e) => (e.Item1 == null ? 0 : 1) + (e.Item2 == null ? 0 : 1)).ToArray()
                    );

                var grid = new FiniteGrid<(Edge?, Edge?)>(size, edges.Content.ToArray());

                foreach (var startPos in edges.Bounds.AllPositionsIn())
                {
                    var vIdx = visited.PosToIndex(startPos);
                    if (visited[vIdx] <= 0)
                    {
                        continue;
                    }
                    else
                    {
                        var idx = grid.PosToIndex(startPos);
                        var startDualEdge = grid[idx];

                        foreach (var startEdge in startDualEdge.EnumerateNotNull())
                        {
                            //Check needed for dual edge tiles
                            if (visited[vIdx] <= 0)
                            {
                                break;
                            }
                            paths.Add(Path(startEdge, startPos, visited, grid));
                        }
                    }
                }

                return paths;
            }

            private List<Vector2> Path(Edge startEdge, Point startPos, FiniteGrid<int> visited, FiniteGrid<(Edge?, Edge?)> grid)
            {
                List<Vector2> path = new List<Vector2>();

                path.Add(startEdge.p1 / 2f + startPos.ToVector2());
                //decrement counter for source
                visited[startPos]--;

                //--Forward Pass--
                var res = PathPass(startPos, startEdge.p1Side, visited, grid, out var looped, out var interupted);

                if (interupted)
                {
                    throw new ApplicationException("Edge generation error");
                }

                path.Capacity += res.Count;
                path.AddRange(res);

                //--Backward Pass--
                if (!looped)
                {
                    path.Insert(0, startEdge.p2 / 2f + startPos.ToVector2());
                    res = PathPass(startPos, startEdge.p2Side, visited, grid, out looped, out interupted);

                    if (interupted)
                    {
                        throw new ApplicationException("Edge generation error");
                    }

                    path.Capacity += res.Count;

                    //Prepend path
                    res.Reverse();
                    path.InsertRange(0, res);
                }
                else
                {
                    path.Add(startEdge.p1 / 2f + startPos.ToVector2());
                }

                return path;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="startPos"></param>
            /// <param name="dir">direction where to look for next square</param>
            /// <param name="visited"></param>
            /// <param name="grid"></param>
            /// <param name="looped"></param>
            /// <param name="interupted"></param>
            /// <returns></returns>
            private List<Vector2> PathPass(Point startPos, CardinalDirection dir, FiniteGrid<int> visited, FiniteGrid<(Edge?, Edge?)> grid, out bool looped, out bool interupted)
            {
                var size = grid.Bounds.Size;

                //next iteration position
                var pos1 = startPos + dir.ToVector();
                var path = new List<Vector2>();

                //Initialize results to false
                looped = false;
                interupted = false;

                while (true)
                {
                    if (
                        pos1.X < 0 || pos1.X >= size.X ||
                        pos1.Y < 0 || pos1.Y >= size.Y)
                    {
                        //We reached map edge
                        break;
                    }

                    if (visited[pos1] <= 0)
                    {
                        //We looped back to the beginning
                        if (pos1 == startPos)
                        {
                            looped = true;
                            break;
                        }
                        //We didn't loop back, there is an error
                        interupted = true;
                        break;
                    }

                    var dualEdge = grid[pos1];
                    var solved = SolveEdge(dir.Invert(), ref dualEdge);

                    //second exit condidion check
                    //for cells with dual edge
                    if (solved == null)
                    {
                        if (pos1 == startPos)
                        {
                            looped = true;
                            break;
                        }
                        interupted = true;
                        break;
                    }

                    grid[pos1] = dualEdge;
                    visited[pos1]--;

                    var exit = solved.Value;
                    path.Add(exit.exitPoint / 2f + pos1.ToVector2());

                    dir = exit.exitSide;
                    pos1 += exit.exitSide.ToVector();
                }

                return path;
            }

            private (Vector2 exitPoint, CardinalDirection exitSide)? SolveEdge(CardinalDirection entryDirection, ref (Edge? e1, Edge? e2) dualEdge)
            {
                var v = dualEdge.e1?.GetExit(entryDirection);
                if (v != null && v.Value.exitSide != CardinalDirection.None)
                {
                    dualEdge.e1 = null;
                    return v;
                }

                v = dualEdge.e2?.GetExit(entryDirection);
                if (v != null && v.Value.exitSide != CardinalDirection.None)
                {
                    dualEdge.e2 = null;
                    return v;
                }

                return null;
            }
        }
    }
}
