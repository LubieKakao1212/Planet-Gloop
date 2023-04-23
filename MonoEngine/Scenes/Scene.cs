using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Scenes
{
    public class Scene
    {
        public Camera Camera => camera;

        //public IReadOnlySet<SceneObject> Roots => roots;
        public IReadOnlySet<DrawableObject> Drawables => drawables;

        private Camera camera;

        private SortedSet<DrawableObject> drawables = new SortedSet<DrawableObject>();

        private HashSet<SceneObject> objectsOnScene = new HashSet<SceneObject>();
        private List<SceneObject> roots = new List<SceneObject>();

        public Scene(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("Cannot create a scene without a camera");
            }

            this.camera = camera;
        }

        internal void RegisterDrawable(DrawableObject obj)
        {
            drawables.Add(obj);
        }
    }
}
