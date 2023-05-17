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

        public Scene CurrentScene
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

        public IReadOnlyList<SceneObject> Children => children;

        /// <summary>
        /// Returns all children as well as their ChildrenDeep <br/>
        /// 
        /// Ordered as follows: <br/>
        /// Child1 -> ChildrenDeep Of Child1 -> Child2 -> ChildrenDeep Of Child2 -> ... -> ChildN -> ChildrenDeep Of ChildN
        /// </summary>
        // Potentialy a lot of allocations
        public IReadOnlyList<SceneObject> ChildrenDeep
        {
            get
            {
                var result = new List<SceneObject>();

                foreach (var child in children)
                {
                    result.Add(child);
                    result.AddRange(child.ChildrenDeep);
                }

                return result;
            }
        }

        public IReadOnlyList<SceneObject> ChildrenDeepAndSelf
        {
            get
            {
                var result = new List<SceneObject>
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
        private readonly List<SceneObject> children = new();

        private Scene currentScene;
        private SceneObject parent = null;
        private readonly Transform transform = new();

        public SceneObject()
        {
            transform.Changed += OnTransformChanged;
        }

        private void OnTransformChanged()
        {
            if (Transform.Parent != Parent?.Transform)
            {
                throw new InvalidOperationException($"Cannot change transform's parent directly, use {nameof(SceneObject)}'s {nameof(Parent)} instead");
            }
        }

        private void PrivateSetScene(Scene scene)
        {
            currentScene = scene;
            foreach (var child in children)
            {
                child.PrivateSetScene(currentScene);
            }
        }

        private void AddChild(SceneObject child)
        {
            if (children.Contains(child))
            {
                throw new ArgumentException("Adding existing child, this should never happen");
            }
            children.Add(child);
        }
        
        private void RemoveChild(SceneObject child)
        {
            if (child.parent != this)
            {
                throw new InvalidOperationException("Invalid Parenting");
            }
            children.Remove(child);
        }
    }
}
