using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Joints;
using System.Reflection.Metadata.Ecma335;

namespace GlobalLoopGame.Spaceship.Dragging
{
    public static class DraggingHelper
    {
        public static void ConnectForDragging(this IDragger dragger, PhysicsBodyObject dragged, float distance)
        {
            var obj = dragger.ThisObject;
            var joint = new DistanceJoint(obj.PhysicsBody, dragged.PhysicsBody, Vector2.Zero, Vector2.Zero);
            joint.DampingRatio = 0.5f;
            joint.Frequency = 1f;
            joint.Length = distance;

            obj.PhysicsBody.World.Add(joint);
            dragger.CurrentDrag = joint;
        }

        public static void TryInitDragging(this IDragger dragger, float distance, float interactionDistance)
        {
            var obj = dragger.ThisObject;
            if (dragger.CurrentDrag != null)
            {
                obj.PhysicsBody.World.Remove(dragger.CurrentDrag);
                dragger.CurrentDrag = null;
            }
            var world = obj.PhysicsBody.World;
            var pos1 = obj.Transform.GlobalPosition;
            var pos2 = obj.Transform.Up * interactionDistance + pos1;
            Body result = null;
            world.RayCast((fixture, point, normal, fraction) =>
            {
                if (fixture.Body.Tag is IDraggable)
                {
                    result = fixture.Body;
                    return 0;
                }
                return 1;
            }, pos1, pos2);

            if (result != null)
            {
                var pbo = result.Tag as PhysicsBodyObject;
                dragger.ConnectForDragging(pbo, distance);
            }
        }
    }
}
