using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Util
{
    /// <remarks>No size checks!!!</remarks>
    public class FiniteGrid<T>
    {
        public Rectangle Bounds => new Rectangle(Point.Zero, size);

        public IEnumerable<T> Content => content;

        public T this[Point pos]
        {
            get => content[PosToIndex(pos)];
            set => content[PosToIndex(pos)] = value;
        }

        public T this[int x, int y]
        {
            get => content[PosToIndex(x, y)];
            set => content[PosToIndex(x, y)] = value;
        }

        public T this[int index]
        {
            get => content[index];
            set => content[index] = value;
        }

        private T[] content;
        private Point size;

        public FiniteGrid(Point size) : this(size, Enumerable.Repeat(default(T), size.X * size.Y).ToArray())
        {

        }

        public FiniteGrid(Point size, T[] content)
        {
            this.size = size;
            this.content = content;
        }

        public FiniteGrid(FiniteGrid<T> other)
        {
            size = other.size;
            content = new T[other.content.Length];
            other.content.CopyTo(content, 0);
        }

        public void Fill(Func<Vector2, T> filler)
        {
            for (int i = 0; i < content.Length; i++)
            {
                content[i] = filler(IndexToPos(i).ToVector2());
            }
        }

        public int PosToIndex(Point pos)
        {
            return PosToIndex(pos.X, pos.Y);
        }

        public int PosToIndex(int x, int y)
        {
            return size.X * y + x;
        }

        public Point IndexToPos(int idx)
        {
            return new Point(idx % size.X, idx / size.X);
        }
    }
}
