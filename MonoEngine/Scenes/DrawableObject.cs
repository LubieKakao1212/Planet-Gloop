using Microsoft.Xna.Framework;

namespace MonoEngine.Scenes
{
    public class DrawableObject : SceneObject
    {
        public Color Color => color;
        public float DrawOrder => drawOrder;

        private Color color;

        private float drawOrder;

        public DrawableObject(Color color, float drawOrder)
        {
            this.color = color;
            this.drawOrder = drawOrder;
        }
    }
}
