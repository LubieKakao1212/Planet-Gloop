using Microsoft.Xna.Framework;

namespace MonoEngine.Scenes
{
    public class DrawableObject : SceneObject
    {
        public Color Color => color;

        private Color color;

        public DrawableObject(Color color)
        {
            this.color = color;
        }
    }
}
