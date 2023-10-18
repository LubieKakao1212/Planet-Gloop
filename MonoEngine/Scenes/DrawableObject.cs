using Microsoft.Xna.Framework;
using MonoEngine.Scenes.Events;

namespace MonoEngine.Scenes
{
    public class DrawableObject : HierarchyObject
    {
        public Color Color { get; set; }
        public float DrawOrder { get; set; }
        public long DrawLayerMask => drawLayerMask;
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
