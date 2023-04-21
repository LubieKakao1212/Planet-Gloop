using Microsoft.Xna.Framework;
using MonoEngine.Math;

namespace MonoEngine.Scenes
{
    public class Camera : SceneObject
    {
        public TransformMatrix CameraMatrix
        {
            get
            {
                return Transform.LocalToWorld * Matrix2x2.Scale(new Vector2(ViewSize, ViewSize));
            }
        }

        public float ViewSize { get; set; }
    }
}
