using Microsoft.Xna.Framework;

namespace MonoEngine.Scenes
{
    public class DrawableObject : SceneObject
    {
        public Color Color => color;
        public float DrawOrder => drawOrder;
        public long DrawLayerMask => drawLayerMask;

        private Color color;

        private float drawOrder;

        private long drawLayerMask;

        public DrawableObject(Color color, float drawOrder) : base()
        {
            this.color = color;
            this.drawOrder = drawOrder;
        }
    }
}
