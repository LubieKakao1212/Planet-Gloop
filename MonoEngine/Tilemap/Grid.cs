using Microsoft.Xna.Framework;
using MonoEngine.Math;
using MonoEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Tilemap
{
    public class Grid : HierarchyObject
    {
        public Vector2 CellSize;
        
        public Grid(Vector2 cellSize)
        {
            CellSize = cellSize;
        }

        public Point WorldToCell(Vector2 worldPos)
        {
            var localPos = Transform.WorldToLocal.TransformPoint(worldPos);

            return LocalToCell(localPos);
        }

        public Point LocalToCell(Vector2 localPos)
        {
            localPos /= CellSize;
            
            return localPos.FloorToInt();
        }

        public Vector2 GridToCellCornerLocal(Point gridPos)
        {
            return new Vector2(gridPos.X * CellSize.X, gridPos.Y * CellSize.Y);
        }

        public Vector2 GridToCellCornerWorld(Point gridPos)
        {
            return Transform.LocalToWorld.TransformPoint(GridToCellCornerLocal(gridPos));
        }

        public Vector2 GridToCellCenterLocal(Point gridPos)
        {
            return GridToCellCornerLocal(gridPos) + (CellSize / 2f);
        }

        public Vector2 GridToCellCenterWorld(Point gridPos)
        {
            return Transform.LocalToWorld.TransformPoint(GridToCellCenterLocal(gridPos));
        }

    }
}
