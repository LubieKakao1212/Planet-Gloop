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

        //private HashSet<SceneObject> roots = new HashSet<SceneObject>();
        private HashSet<DrawableObject> drawables = new HashSet<DrawableObject>();

        public Scene(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("Cannot create a scene without a camera");
            }

            this.camera = camera;
        }

        public void RegisterDrawable(DrawableObject obj)
        {
            this.drawables.Add(obj);
        }
    }
}
