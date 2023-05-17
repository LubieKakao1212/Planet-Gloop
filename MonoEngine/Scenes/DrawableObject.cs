using Microsoft.Xna.Framework;

namespace MonoEngine.Scenes
{
    public class DrawableObject : HierarchyObject
    {
        public Color Color => color;
        public float DrawOrder => drawOrder;
        public long DrawLayerMask => drawLayerMask;
        /// <summary>
        /// Can this object be batched with other Objects?
        /// </summary>
        public bool InteruptQueue
        {
            get;
            protected set;
        }

        private Color color;

        private float drawOrder;

        //All
        private long drawLayerMask = -1;

        protected bool interuptQueue;

        public DrawableObject(Color color, float drawOrder) : base()
        {
            this.color = color;
            this.drawOrder = drawOrder;
        }

        public virtual DrawableObject SetInterupQueue(bool interuptQueue)
        {
            this.interuptQueue = interuptQueue;
            return this;
        }
    }
}
