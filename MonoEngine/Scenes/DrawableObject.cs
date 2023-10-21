using Microsoft.Xna.Framework;
using MonoEngine.Rendering.Sprites;
using MonoEngine.Scenes.Events;

namespace MonoEngine.Scenes
{
    using Math;

    public class DrawableObject : HierarchyObject
    {
        public Color Color { get; set; }
        public float DrawOrder { get; set; }
        public long DrawLayerMask => drawLayerMask;
        
        public Sprite Sprite { get; set; } = new Sprite() { TextureIndex = 0, TextureRect = new BoundingRect(Vector2.Zero, Vector2.Zero) };

        /// <summary>
        /// Can this object be batched with other Objects?
        /// </summary>
        public bool InteruptQueue
        {
            get;
            protected set;
        }

        //All
        private long drawLayerMask = -1;

        public DrawableObject(Color color, float drawOrder) : base()
        {
            Color = color;
            DrawOrder = drawOrder;
        }

        public virtual DrawableObject SetInterupQueue(bool interuptQueue)
        {
            InteruptQueue = interuptQueue;
            return this;
        }
    }
}
