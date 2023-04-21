using MonoEngine.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Scenes
{
    public class SceneObject
    {
        public SceneObject Parent
        {
            get => parent;
            set
            {
                parent = value;
                if (parent != null)
                {
                    transform.Parent = parent.Transform;
                }
                else
                { 
                    transform.Parent = null;
                }
            }
        }

        public Transform Transform
        {
            get => transform;
        }

        private SceneObject parent = null;
        private readonly Transform transform = new Transform();
    }
}
