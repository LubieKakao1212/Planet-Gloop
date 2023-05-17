using Microsoft.Xna.Framework;
using MonoEngine.Math;
using MonoEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Tilemap
{
    public class Grid : HierarchyObject
    {
        public Vector2 CellSize;

        public Vector2Int WorldToCell(Vector2 worldPos)
        {
            var localPos = Transform.WorldToLocal.TransformPoint(worldPos);

            return LocalToCell(localPos);
        }

        public Vector2Int LocalToCell(Vector2 localPos)
        {
            localPos /= CellSize;

            return localPos.FloorToInt();
        }

        public Vector2 CellCornerToLocal(Vector2Int gridPos)
        {
            return (gridPos * CellSize);
        }

        public Vector2 CellCornerToWorld(Vector2Int gridPos)
        {
            return Transform.LocalToWorld.TransformPoint(CellCornerToLocal(gridPos));
        }

        public Vector2 CellCenterToLocal(Vector2Int gridPos)
        {
            return CellCornerToLocal(gridPos) + (CellSize / 2f);
        }

        public Vector2 CellCenterToWorld(Vector2Int gridPos)
        {
            return Transform.LocalToWorld.TransformPoint(CellCenterToLocal(gridPos));
        }

    }
}
