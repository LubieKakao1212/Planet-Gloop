using System;
using System.Collections.Generic;

namespace MonoEngine.Scenes
{
    using Utils;

    public class Scene
    {
        public Camera Camera => camera;

        //public IReadOnlySet<SceneObject> Roots => roots;
        
        public IReadOnlyCollection<DrawableObject> Drawables
        {
            get
            {
                IDictionary<float, List<DrawableObject>> orderedDrawables = new SortedDictionary<float, List<DrawableObject>>();
                
                int count = 0;

                foreach (var root in roots)
                {
                    foreach (var obj in root.ChildrenDeepAndSelf)
                    {
                        if (obj is DrawableObject drawable)
                        {
                            var list = orderedDrawables.GetOrSetToDefault(drawable.DrawOrder, new());
                            list.Add(drawable);
                            count++;
                        }
                    }
                }

                var listOut = new List<DrawableObject>(count);

                foreach (var order in orderedDrawables)
                {
                    listOut.AddRange(order.Value);
                }

                return listOut;
            }
        }

        private Camera camera;

        //private HashSet<DrawableObject> drawables = new HashSet<DrawableObject>();

        private HashSet<SceneObject> rootsSet = new HashSet<SceneObject>();
        private List<SceneObject> roots = new List<SceneObject>();

        public Scene(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("Cannot create a scene without a camera");
            }

            this.camera = camera;
        }

        public void AddObject(SceneObject obj)
        {
            InsertObject(obj, roots.Count);
        }

        public void InsertObject(SceneObject obj, int order)
        {
            if (obj.Parent != null)
            {
                throw new InvalidOperationException("Cannot change scene of non-root object");
            }
            if (rootsSet.Contains(obj))
            {
                //TODO Inplement logger
                Console.Out.WriteLine("Assigning object to scene it is alredy assigned, this may be a mistake");
                return;
            }
            obj.CurrentScene?.RemoveObject(obj);

            obj.CurrentScene = this;
            rootsSet.Add(obj);
            roots.Insert(order, obj);
        }

        /*internal void RegisterDrawable(DrawableObject obj)
        {
            drawables.Add(obj);
        }*/

        private void RemoveObject(SceneObject obj) 
        {
            if (!rootsSet.Contains(obj))
            {
                throw new InvalidOperationException("Invalid object removal");
            }
            rootsSet.Remove(obj);
            roots.Remove(obj);
        }
    }
}
