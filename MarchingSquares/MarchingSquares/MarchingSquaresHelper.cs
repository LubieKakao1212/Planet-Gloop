using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoEngine.Util;

namespace MarchingSquares.MarchingSquares
{
    using EdgePoint = System.ValueTuple<sbyte, CardinalDirection>;

    public static class MarchingSquaresHelper
    {
        private static Dictionary<byte, PatternProvider> patterns = new Dictionary<byte, PatternProvider>();

        static MarchingSquaresHelper()
        {
            /*
             * **
             * **
             * 
             * *-----* 7--0--1
             * |     | |     |
             * |     | 6     2
             * |     | |     |
             * *-----* 5--4--3
             */
            patterns.Add(0b0000, new PatternProvider(new Pattern.Builder().Build()));

            /*
             * *#
             * **
             * 
             * *--0--1 7--0--1
             * |   \#| |     |
             * |    \2 6     2
             * |     | |     |
             * *-----* 5--4--3
             */
            patterns.Add(0b0001, new PatternProvider(new Pattern.Builder()
                .AddVertex(0) // 0 Top
                .AddVertex(1) // 1 Top Right
                .AddVertex(2) // 2 Right
                .AddTriangle(0, 1, 2)
                .AddEdge(CardinalDirection.Top, CardinalDirection.Right)
                .Build()));

            /*
             * **
             * *#
             * 
             * *-----* 7--0--1
             * |     | |     |
             * |    /0 6     2
             * |   /#| |     |
             * *--2--1 5--4--3
             */
            patterns.Add(0b0010, new PatternProvider(new Pattern.Builder()
                .AddVertex(2) // 0 Right
                .AddVertex(3) // 1 Bottom Right
                .AddVertex(4) // 2 Bottom
                .AddTriangle(0, 1, 2)
                .AddEdge(CardinalDirection.Bottom, CardinalDirection.Right)
                .Build()));

            /*
             * *#
             * *#
             * 
             * *--0--1 7--0--1
             * |  |##| |     |
             * |  |##| 6     2
             * |  |##| |     |
             * *--3--2 5--4--3
             */
            patterns.Add(0b0011, new PatternProvider(new Pattern.Builder()
                .AddVertex(0) // 0 Top
                .AddVertex(1) // 1 Top Right
                .AddVertex(3) // 2 Bottom Right
                .AddVertex(4) // 3 Bottom
                .AddTriangle(0, 1, 2)
                .AddTriangle(0, 2, 3)
                .AddEdge(CardinalDirection.Top, CardinalDirection.Bottom)
                .Build()));

            /*
             * **
             * #*
             * 
             * *-----* 7--0--1
             * |     | |     |
             * 2\    | 6     2
             * |#\   | |     |
             * 1--0--* 5--4--3
             */
            patterns.Add(0b0100, new PatternProvider(new Pattern.Builder()
                .AddVertex(4) // 0 Bottom
                .AddVertex(5) // 1 Bottom Left
                .AddVertex(6) // 2 Left
                .AddTriangle(0, 1, 2)
                .AddEdge(CardinalDirection.Left, CardinalDirection.Bottom)
                .Build()));

            /*
             * *#
             * #*
             * 
             * *--0--1 7--0--1
             * |   \#| |     |
             * 5\   \2 6     2
             * |#\   | |     |
             * 4--3--* 5--4--3
             * 
             * *--0--1 7--0--1
             * | /###| |     |
             * 5/###/2 6     2
             * |###/ | |     |
             * 4--3--* 5--4--3
             */
            var builder0101 = new Pattern.Builder()
                .AddVertex(0) // 0 Top
                .AddVertex(1) // 1 Top Right
                .AddVertex(2) // 2 Right
                .AddVertex(4) // 3 Bottom
                .AddVertex(5) // 4 Bottom Left
                .AddVertex(6) // 5 Left;
                .AddTriangle(0, 1, 2)
                .AddTriangle(3, 4, 5);
            patterns.Add(0b0101, new DualPatternProvider(
                builder0101
                .Copy()
                .AddEdge(CardinalDirection.Left, CardinalDirection.Bottom)
                .AddEdge(CardinalDirection.Top, CardinalDirection.Right)
                .Build(),
                builder0101
                .AddEdge(CardinalDirection.Left, CardinalDirection.Top)
                .AddEdge(CardinalDirection.Right, CardinalDirection.Bottom)
                .AddTriangle(0, 2, 3)
                .AddTriangle(3, 5, 0)
                .Build()
                ));

            /*
             * **
             * ##
             * 
             * *-----* 7--0--1
             * |     | |     |
             * 3-----0 6     2
             * |#####| |     |
             * 2-----1 5--4--3
             */
            patterns.Add(0b0110, new PatternProvider(new Pattern.Builder()
                .AddVertex(2) // 0 Right
                .AddVertex(3) // 1 Bottom Right
                .AddVertex(5) // 2 Bottom Left
                .AddVertex(6) // 3 Left
                .AddTriangle(0, 1, 2)
                .AddTriangle(0, 2, 3)
                .AddEdge(CardinalDirection.Left, CardinalDirection.Right)
                .Build()));

            /*
             * *#
             * ##
             * 
             * *--0--1 7--0--1
             * | /###| |     |
             * 4/####| 6     2
             * |#####| |     |
             * 3-----2 5--4--3
             */
            patterns.Add(0b0111, new PatternProvider(new Pattern.Builder()
                .AddVertex(0) // 0 Top
                .AddVertex(1) // 1 Top Right
                .AddVertex(3) // 2 Bottom Right
                .AddVertex(5) // 3 Bottom Left
                .AddVertex(6) // 4 Left
                .AddTriangle(1, 2, 3)
                .AddTriangle(0, 1, 3)
                .AddTriangle(0, 3, 4)
                .AddEdge(CardinalDirection.Left, CardinalDirection.Top)
                .Build()));

            /*
             * #*
             * **
             * 
             * 1--2--* 7--0--1 
             * |#/   | |     |
             * 0/    | 6     2
             * |     | |     |
             * *-----* 5--4--3
             */
            patterns.Add(0b1000, new PatternProvider(new Pattern.Builder()
                .AddVertex(6) // 0 Left
                .AddVertex(7) // 1 Top Left
                .AddVertex(0) // 2 Top
                .AddTriangle(0, 1, 2)
                .AddEdge(CardinalDirection.Left, CardinalDirection.Top)
                .Build()));

            /*
             * ##
             * **
             * 
             * 1-----2 7--0--1
             * |#####| |     |
             * 0-----3 6     2
             * |     | |     |
             * *-----* 5--4--3
             */
            patterns.Add(0b1001, new PatternProvider(new Pattern.Builder()
                .AddVertex(6) // 0 Left
                .AddVertex(7) // 1 Top Left
                .AddVertex(1) // 2 Top Right
                .AddVertex(2) // 3 Right
                .AddTriangle(0, 1, 2)
                .AddTriangle(0, 2, 3)
                .AddEdge(CardinalDirection.Left, CardinalDirection.Right)
                .Build()));

            /*
             * #*
             * *#
             * 
             * 1--2--* 7--0--1
             * |#/   | |     |
             * 0/   /3 6     2
             * |   /#| |     |
             * *--5--4 5--4--3
             * 
             * 1--2--* 7--0--1
             * |###\ | |     |
             * 0\###\3 6     2
             * | \###| |     |
             * *--5--4 5--4--3
             * 
             * 5--0--* 7--0--1
             * |###\ | |     |
             * 4\###\1 6     2
             * | \###| |     |
             * *--3--2 5--4--3
             */
            var builder1010 = new Pattern.Builder()
                .AddVertex(6) // 0 Left
                .AddVertex(7) // 1 Top Left
                .AddVertex(0) // 2 Top
                .AddVertex(2) // 3 Right
                .AddVertex(3) // 4 Bottom Right
                .AddVertex(4) // 5 Bottom;
                .AddTriangle(4, 5, 0)
                .AddTriangle(1, 2, 3);
            patterns.Add(0b1010, new DualPatternProvider(
                builder1010
                .Copy()
                .AddEdge(CardinalDirection.Left, CardinalDirection.Top)
                .AddEdge(CardinalDirection.Right, CardinalDirection.Bottom)
                .Build(),
                builder1010
                .AddTriangle(4, 0, 3)
                .AddTriangle(1, 3, 0)
                .AddEdge(CardinalDirection.Left, CardinalDirection.Bottom)
                .AddEdge(CardinalDirection.Right, CardinalDirection.Top)
                .Build()
                ));

            /*
             * ##
             * *#
             * 
             * 0-----1 7--0--1
             * |#####| |     |
             * 4\####| 6     2
             * | \###| |     |
             * *--3--2 5--4--3
             */
            patterns.Add(0b1011, new PatternProvider(new Pattern.Builder()
                .AddVertex(7) // 0 Top Left
                .AddVertex(1) // 1 Top Right
                .AddVertex(3) // 2 Bottom Right
                .AddVertex(4) // 3 Bottom
                .AddVertex(6) // 4 Left
                .AddTriangle(0, 1, 2)
                .AddTriangle(0, 2, 3)
                .AddTriangle(0, 3, 4)
                .AddEdge(CardinalDirection.Left, CardinalDirection.Bottom)
                .Build()));

            /*
             * #*
             * #*
             * 
             * 2--3--* 7--0--1
             * |##|  | |     |
             * |##|  | 6     2
             * |##|  | |     |
             * 1--0--* 5--4--3
             */
            patterns.Add(0b1100, new PatternProvider(new Pattern.Builder()
                .AddVertex(4) // 0 Bottom
                .AddVertex(5) // 2 Bottom Left
                .AddVertex(7) // 3 Top Left
                .AddVertex(0) // 3 Top
                .AddTriangle(0, 1, 2)
                .AddTriangle(0, 2, 3)
                .AddEdge(CardinalDirection.Top, CardinalDirection.Bottom)
                .Build()));

            /*
             * ##
             * #*
             * 
             * 1-----2 7--0--1
             * |#####| |     |
             * |####/3 6     2
             * |###/ | |     |
             * 0--4--* 5--4--3
             */
            patterns.Add(0b1101, new PatternProvider(new Pattern.Builder()
                .AddVertex(5) // 0 Bottom Left
                .AddVertex(7) // 1 Top Left
                .AddVertex(1) // 2 Top Right
                .AddVertex(2) // 3 Right
                .AddVertex(4) // 4 Bottom
                .AddTriangle(0, 1, 2)
                .AddTriangle(0, 2, 3)
                .AddTriangle(0, 3, 4)
                .AddEdge(CardinalDirection.Right, CardinalDirection.Bottom)
                .Build()));

            /*
             * #*
             * ##
             * 
             * 2--3--* 7--0--1
             * |###\ | |     |
             * |####\4 6     2
             * |#####| |     |
             * 1-----0 5--4--3
             */
            patterns.Add(0b1110, new PatternProvider(new Pattern.Builder()
                .AddVertex(3) // 0 Bottom Right
                .AddVertex(5) // 1 Bottom Left
                .AddVertex(7) // 2 Top Left
                .AddVertex(0) // 3 Top
                .AddVertex(2) // 4 Right
                .AddTriangle(0, 1, 2)
                .AddTriangle(0, 2, 3)
                .AddTriangle(0, 3, 4)
                .AddEdge(CardinalDirection.Top, CardinalDirection.Right)
                .Build()));

            /*
             * ##
             * ##
             * 
             * 3-----0 7--0--1
             * |#####| |     |
             * |#####| 6     2
             * |#####| |     |
             * 2-----1 5--4--3
             */
            patterns.Add(0b1111, new PatternProvider(new Pattern.Builder()
                .AddVertex(1) // 3 Top Right
                .AddVertex(3) // 0 Bottom Right
                .AddVertex(5) // 1 Bottom Left
                .AddVertex(7) // 2 Top Left
                .AddTriangle(0, 1, 2)
                .AddTriangle(2, 3, 0)
                .Build()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config">
        /// 1 << 0: TopRight<br/>
        /// 1 << 1: BottomRight<br/>
        /// 1 << 2: BottomLeft<br/>
        /// 1 << 3: TopLeft
        /// </param>
        public static PatternProvider GetPattern(byte config)
        {
            return patterns[(byte)(config & 15)];
        }

        public class Pattern
        {
            public List<int> Triangles { get; }

            public Dictionary<CardinalDirection, CardinalDirection> Connections { get; }

            /// <summary>
            /// Order: { Top, TopRight, Right, BottomRight, ... }
            /// </summary>
            private byte verticies;

            private (EdgePoint p1, EdgePoint p2)? edge1;
            private (EdgePoint p1, EdgePoint p2)? edge2;

            private Pattern(byte verticies, List<int> triangles)
            {
                this.verticies = verticies;
                this.Triangles = triangles;
            }

            public void InterpolatedVerticies(float[] ratios, out Vector2[] verts, out Edge? edge1, out Edge? edge2)
            {
                verts = InterpolatedVerticies(ratios).ToArray();
                Edges(verts, out edge1, out edge2);
            }

            public void Edges(Vector2[] verts, out Edge? e1, out Edge? e2)
            {
                Edge(verts, edge1, out e1);
                Edge(verts, edge2, out e2);
            }

            /// <summary>
            /// Order: { top, right, bottom, left }
            /// </summary>
            public IEnumerable<Vector2> InterpolatedVerticies(float[] ratios)
            {
                return InterpolatedVerticies(ratios, new Vector2[] {
                    new Vector2(-1, 1),
                    new Vector2(1, 1),
                    new Vector2(1, -1),
                    new Vector2(-1, -1) });
            }

            /// <summary>
            /// Order: { top, right, bottom, left }
            /// </summary>
            public IEnumerable<Vector2> InterpolatedVerticies(float[] ratios, Vector2[] values)
            {
                return
                    InterpolateSegment(0, ratios[0], values[0], values[1])
                    .Concat(InterpolateSegment(1, ratios[1], values[1], values[2]))
                    .Concat(InterpolateSegment(2, ratios[2], values[2], values[3]))
                    .Concat(InterpolateSegment(3, ratios[3], values[3], values[0]));
            }

            private IEnumerable<Vector2> InterpolateSegment(int corner, float ratio, Vector2 corner1, Vector2 corner2)
            {
                if ((verticies & (1 << (2 * corner))) > 0)
                    yield return Vector2.Lerp(corner1, corner2, ratio);
                if ((verticies & (1 << (2 * corner + 1))) > 0)
                    yield return corner2;
            }

            public sbyte GetVertexIndex(Direction side)
            {
                var s = (byte)side;
                var mask = 1 << s;

                if ((verticies & mask) == 0)
                {
                    return -1;
                }

                sbyte r = 0;

                for (byte i = 0; i < s; i++)
                {
                    var m = (1 << i) & verticies;
                    r += (sbyte)(m >> i);
                }

                return r;
            }

            private void Edge(Vector2[] verts, (EdgePoint p1, EdgePoint p2)? edge, out Edge? edgeOut)
            {
                edgeOut = edge == null ? null : new Edge()
                {
                    p1 = verts[edge.Value.p1.Item1],
                    p1Side = edge.Value.p1.Item2,

                    p2 = verts[edge.Value.p2.Item1],
                    p2Side = edge.Value.p2.Item2
                };
            }

            public class Builder
            {
                private List<int> triangles = new List<int>();
                private byte verticies;
                private int vertexCount;

                private (CardinalDirection p1, CardinalDirection p2)? edge1;
                private (CardinalDirection p1, CardinalDirection p2)? edge2;

                /// <summary>
                /// Order: { Top, TopRight, Right, BottomRight, ... }
                /// </summary>
                public Builder AddTriangle(int i1, int i2, int i3)
                {
                    if (!(i1 < vertexCount && i2 < vertexCount && i3 < vertexCount))
                    {
                        throw new System.ArgumentException("Triangle index out of bounds");
                    }
                    triangles.Add(i1);
                    triangles.Add(i2);
                    triangles.Add(i3);
                    return this;
                }

                /// <summary>
                /// Order: { Top, TopRight, Right, BottomRight, ... }
                /// </summary>
                public Builder AddVertex(byte vertex)
                {
                    verticies = (byte)(verticies | (1 << vertex));
                    vertexCount++;
                    return this;
                }

                public Builder AddEdge(CardinalDirection p1, CardinalDirection p2)
                {
                    if (edge1 == null)
                    {
                        edge1 = (p1, p2);
                    }
                    else if (edge2 == null)
                    {
                        edge2 = (p1, p2);
                    }
                    else
                        throw new ApplicationException("Cannot add more than two edges to a pattern.");
                    return this;
                }

                public Pattern Build()
                {
                    var pattern = new Pattern(verticies, triangles);
                    pattern.edge1 = edge1 == null ? null :
                        (
                            (pattern.GetVertexIndex(edge1.Value.p1.ToDirection()), edge1.Value.p1),
                            (pattern.GetVertexIndex(edge1.Value.p2.ToDirection()), edge1.Value.p2)
                        );

                    pattern.edge2 = edge2 == null ? null :
                        (
                            (pattern.GetVertexIndex(edge2.Value.p1.ToDirection()), edge2.Value.p1),
                            (pattern.GetVertexIndex(edge2.Value.p2.ToDirection()), edge2.Value.p2)
                        );
                    return pattern;
                }

                public Builder Copy()
                {
                    return new Builder() { triangles = new List<int>(triangles), verticies = verticies, vertexCount = vertexCount, edge1 = edge1, edge2 = edge2 };
                }
            }
        }

        public class PatternProvider
        {
            protected Pattern pattern;

            public PatternProvider(Pattern pattern)
            {
                this.pattern = pattern;
            }

            public virtual Pattern Get(bool dense)
            {
                return pattern;
            }
        }

        public class DualPatternProvider : PatternProvider
        {
            protected Pattern secondaryPattern;

            public DualPatternProvider(Pattern sparsePattern, Pattern densePattern) : base(densePattern)
            {
                secondaryPattern = sparsePattern;
            }

            public override Pattern Get(bool dense)
            {
                return dense ? pattern : secondaryPattern;
            }
        }
    }

    public struct Edge
    {
        public Vector2 p1;
        public Vector2 p2;

        public CardinalDirection p1Side;
        public CardinalDirection p2Side;

        public (Vector2 exitPoint, CardinalDirection exitSide) GetExit(CardinalDirection entrySide)
        {
            if (entrySide == p1Side)
            {
                return (p2, p2Side);
            }
            if (entrySide == p2Side)
            {
                return (p1, p1Side);
            }
            return (default, CardinalDirection.None);
        }
    }
}
