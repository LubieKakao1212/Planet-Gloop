using Microsoft.Xna.Framework;
using MonoEngine.Math;
using System;
using System.Data;

namespace MonoEngine.Scenes
{
    public class Camera : HierarchyObject
    {
        /*public BoundingRect WorldBounds 
        { 
            get
            {
                if (!worldBounds.HasValue)
                {
                    worldBounds = (Transform.LocalToWorld )).Inverse();
                }
                return worldBounds.Value;
            }
        }*/

        public static BoundingRect CullingRect { get; } = BoundingRect.Normal
            //For debugging
            //.Scaled(0.2f);
            ;

        public TransformMatrix ProjectionMatrix
        {
            get
            {
                if (!projectionMatrix.HasValue)
                {
                    projectionMatrix = /*TransformMatrix.TranslationRotationShearScale(Vector2.One * 8f, MathF.PI / 2f, 0f, Vector2.One * 8f).Inverse();*/(Transform.LocalToWorld * Matrix2x2.Scale(new Vector2(ViewSize * aspectRatio, ViewSize))).Inverse();
                }
                return projectionMatrix.Value;
            }
        }

        public float ViewSize { get => viewSize; set { viewSize = value; projectionMatrix = null; } }

        public float AspectRatio { get => aspectRatio; set { aspectRatio = value; projectionMatrix = null; } }

        private TransformMatrix? projectionMatrix;
        private BoundingRect? worldBounds;
        private float viewSize = 1f;
        private float aspectRatio = 1f;

        public Camera()
        {
            Transform.Changed += () =>
            {
                projectionMatrix = null;
                worldBounds = null;
            };
        }

        public bool Cull(BoundingRect rect)
        {
            rect = rect.Transformed(ProjectionMatrix);

            rect.Intersects(CullingRect);

            return false;
        }

        public Vector2 ViewToWorldPos(Vector2 viewPos)
        {
            //TODO Cache inverse?
            return ProjectionMatrix.Inverse().TransformPoint(viewPos);
        }
    }
}
