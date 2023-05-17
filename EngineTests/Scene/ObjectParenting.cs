using MonoEngine.Scenes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineTests.Scene
{
    [TestFixture]
    internal class SceneObjectTest
    {
        HierarchyObject parent;
        HierarchyObject child;

        [Test]
        public void Parenting()
        {
            SetUpHierarchy();

            Assert.AreSame(parent, child.Parent);

            var children = parent.Children;

            Assert.AreEqual(1, children.Count);
            Assert.IsTrue(children.Contains(child));
        }

        [Test]
        public void TransformParenting() 
        {
            SetUpHierarchy();

            Assert.AreSame(parent.Transform, child.Transform.Parent);
            Assert.AreSame(null, parent.Transform.Parent);
        }

        [Test]
        public void ManualTransformChange()
        {
            SetUpHierarchy();

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    parent.Transform.Parent = child.Transform.Parent;
                });
        }   

        private void SetUpHierarchy()
        {
            parent = new HierarchyObject();
            child = new HierarchyObject();

            Assert.DoesNotThrow(() => child.Parent = parent);
        }
    }
}
