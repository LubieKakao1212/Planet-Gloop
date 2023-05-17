using MonoEngine.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEngine.Scenes
{
    public class HierarchyObject
    {
        public HierarchyObject Parent
        {
            get => parent;
            set
            {
                parent?.RemoveChild(value);

                parent = value;
                if (parent != null)
                {
                    transform.Parent = parent.Transform;
                    parent.AddChild(this);
                }
                else
                { 
                    transform.Parent = null;
                }
            }
        }

        public Hierarchy CurrentScene
        {
            get => currentScene;
            internal set
            {
                PrivateSetScene(value);
            }
        }
        
        public Transform Transform
        {
            get => transform;
        }

        public IReadOnlyList<HierarchyObject> Children => children;

        /// <summary>
        /// Returns all children as well as their ChildrenDeep <br/>
        /// 
        /// Ordered as follows: <br/>
        /// Child1 -> ChildrenDeep Of Child1 -> Child2 -> ChildrenDeep Of Child2 -> ... -> ChildN -> ChildrenDeep Of ChildN
        /// </summary>
        // Potentialy a lot of allocations
        public IReadOnlyList<HierarchyObject> ChildrenDeep
        {
            get
            {
                var result = new List<HierarchyObject>();

                foreach (var child in children)
                {
                    result.Add(child);
                    result.AddRange(child.ChildrenDeep);
                }

                return result;
            }
        }

        public IReadOnlyList<HierarchyObject> ChildrenDeepAndSelf
        {
            get
            {
                var result = new List<HierarchyObject>
                {
                    this
                };

                foreach (var child in children)
                {
                    result.AddRange(child.ChildrenDeepAndSelf);
                }

                return result;
            }
        }

        //Not optimal for larege amount of children
        private readonly List<HierarchyObject> children = new();

        private Hierarchy currentScene;
        private HierarchyObject parent = null;
        private readonly Transform transform = new();

        public HierarchyObject()
        {
            transform.Changed += OnTransformChanged;
        }

        private void OnTransformChanged()
        {
            if (Transform.Parent != Parent?.Transform)
            {
                throw new InvalidOperationException($"Cannot change transform's parent directly, use {nameof(HierarchyObject)}'s {nameof(Parent)} instead");
            }
        }

        private void PrivateSetScene(Hierarchy scene)
        {
            currentScene = scene;
            foreach (var child in children)
            {
                child.PrivateSetScene(currentScene);
            }
        }

        private void AddChild(HierarchyObject child)
        {
            if (children.Contains(child))
            {
                throw new ArgumentException("Adding existing child, this should never happen");
            }
            children.Add(child);
        }
        
        private void RemoveChild(HierarchyObject child)
        {
            if (child.parent != this)
            {
                throw new InvalidOperationException("Invalid Parenting");
            }
            children.Remove(child);
        }
    }
}
