using System;
using System.Collections.Generic;

namespace MonoEngine.Scenes
{
    using MonoEngine.Scenes.Events;
    using Util;

    //TODO Optimise callbacks
    public class Hierarchy
    {
        public IReadOnlyCollection<DrawableObject> Drawables => CustomOrderedInstancesOf<DrawableObject>((obj) => obj.DrawOrder);
        
        private HashSet<HierarchyObject> rootsSet = new HashSet<HierarchyObject>();
        private List<HierarchyObject> roots = new List<HierarchyObject>();

        private List<HierarchyObject> objectsToAdd = new();
        private List<HierarchyObject> objectsToRemove = new();

        private bool isUpdating;

        public Hierarchy()
        {
        }

        public void AddObject(HierarchyObject obj)
        {
            if (isUpdating)
            {
                objectsToAdd.Add(obj);
                if (objectsToAdd.Contains(obj))
                {
                    return;
                }
                if (objectsToRemove.Contains(obj))
                {
                    objectsToRemove.Remove(obj);
                }
                objectsToAdd.Add(obj);
                return;
            }

            InsertObject(obj, roots.Count);
        }

        public void InsertObject(HierarchyObject obj, int order)
        {
            if (isUpdating)
            {
                throw new ApplicationException("Not usable during updates");
            }
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
            obj.CurrentScene?.RemoveObjectInternal(obj);

            obj.CurrentScene = this;
            rootsSet.Add(obj);
            roots.Insert(order, obj);
        }

        public void RemoveObject(HierarchyObject obj)
        {
            if (isUpdating)
            {
                if (objectsToRemove.Contains(obj))
                {
                    return;
                }
                if (objectsToAdd.Contains(obj))
                {
                    objectsToAdd.Remove(obj);
                }
                else
                {
                    objectsToRemove.Add(obj);
                }
                return;
            }
            if (!rootsSet.Contains(obj))
            {
                throw new InvalidOperationException("Invalid object removal");
            }
            RemoveObjectInternal(obj);

            obj.CurrentScene = null;
        }

        public IReadOnlyCollection<T> AllInstancesOf<T>()
        {
            var listOut = new List<T>();
            foreach (var root in roots)
            {
                foreach (var obj in root.ChildrenDeepAndSelf)
                {
                    if (obj is T instance)
                    {
                        listOut.Add(instance);
                    }
                }
            }
            return listOut;
        }

        public IReadOnlyCollection<T> OrderedInstancesOf<T>() where T : IOrdered
        {
            //TODO use Ordered<T>
            IDictionary<float, List<T>> orderedDrawables = new SortedDictionary<float, List<T>>();

            int count = 0;

            foreach (var root in roots)
            {
                foreach (var obj in root.ChildrenDeepAndSelf)
                {
                    if (obj is T instance)
                    {
                        var list = orderedDrawables.GetOrSetToDefault(instance.Order, new());
                        list.Add(instance);
                        count++;
                    }
                }
            }

            var listOut = new List<T>(count);

            foreach (var order in orderedDrawables)
            {
                listOut.AddRange(order.Value);
            }

            return listOut;
        }
        
        public IReadOnlyCollection<T> CustomOrderedInstancesOf<T>(Func<T, float> orderer)
        {
            IDictionary<float, List<T>> orderedDrawables = new SortedDictionary<float, List<T>>();

            int count = 0;

            int i = 0;
            foreach (var root in roots)
            {
                i++;
                foreach (var obj in root.ChildrenDeepAndSelf)
                {
                    if (obj is T instance)
                    {
                        var list = orderedDrawables.GetOrSetToDefault(orderer(instance), new());
                        list.Add(instance);
                        count++;
                    }
                }
            }

            var listOut = new List<T>(count);

            foreach (var order in orderedDrawables)
            {
                listOut.AddRange(order.Value);
            }

            return listOut;
        }

        public void BeginUpdate()
        {
            isUpdating = true;
        }

        public void EndUpdate()
        {
            isUpdating = false;

            foreach (var addition in objectsToAdd)
            {
                AddObject(addition);
            }
            objectsToAdd.Clear();

            foreach (var removal in objectsToRemove)
            {
                RemoveObject(removal);
            }
            objectsToRemove.Clear();
        }

        private void RemoveObjectInternal(HierarchyObject obj) 
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
