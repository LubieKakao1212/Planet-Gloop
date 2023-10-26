using Microsoft.Xna.Framework;
using MonoEngine.Scenes;
using MonoEngine.Scenes.Events;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Runtime.Serialization;

namespace MonoEngine.Physics
{
    public class PhysicsBodyObject : HierarchyObject, IUpdatable
    {
        public float Order { get; set; }

        public Body PhysicsBody { get; protected set; }

        private bool dirty;
        private bool isUpdating;

        public PhysicsBodyObject(Body physicsBody)
        {
            PhysicsBody = physicsBody;
            Transform.Changed += () =>
            {
                if (isUpdating)
                {
                    return;
                }
                dirty = true;
            };
        }

        public virtual void Update(GameTime time)
        {
            if (PhysicsBody.World == null)
            {
                return;
            }

            if (dirty)
            {
                var p = Transform.LocalPosition;
                PhysicsBody.SetTransformIgnoreContacts(ref p, Transform.LocalRotation);
                dirty = false;
            }

            isUpdating = true;
            var bodyTransform = PhysicsBody.GetTransform();

            var angle = bodyTransform.q.Phase;
            var pos = bodyTransform.p;
            Transform.LocalRotation = angle;
            Transform.LocalPosition = pos;
            isUpdating = false;
        }

        public DrawableObject AddDrawableRectFixture(Vector2 size, Vector2 offset, float rotation, out Fixture fixture, float density = 1f)
        {
            var verts = PolygonTools.CreateRectangle(size.X / 2f, size.Y / 2f);
            verts.Rotate(rotation);
            verts.Translate(offset);
            fixture = PhysicsBody.CreatePolygon(verts, density);
            var drawable = new DrawableObject(Color.WhiteSmoke, 0f);
            drawable.Parent = this;
            drawable.Transform.LocalPosition = offset;
            drawable.Transform.LocalRotation = rotation;
            drawable.Transform.LocalScale = size;
            return drawable;
        }

        public void RemoveFixture(Fixture fixture)
        {
            PhysicsBody.Remove(fixture);
        }
    }
}
