﻿using Microsoft.Xna.Framework;
using Custom2d_Engine.Physics;
using nkast.Aether.Physics2D.Collision;
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
            // var joint = new DistanceJoint(obj.PhysicsBody, dragged.PhysicsBody, Vector2.Zero, Vector2.Zero);
            var joint = new RopeJoint(obj.PhysicsBody, dragged.PhysicsBody, Vector2.Zero, Vector2.Zero);
            //joint.DampingRatio = 0.5f;
            //joint.Frequency = 1f;
            //joint.Length = distance;
            joint.MaxLength = distance;
            joint.CollideConnected = false;

            obj.PhysicsBody.World.Add(joint);
            dragger.CurrentDrag = joint;

            IDraggable iDraggable = dragged.PhysicsBody.Tag as IDraggable;
            if (iDraggable != null)
            {
                iDraggable.OnBecomeDragged(dragger);
            }
        }

        public static void ToggleDragging(this IDragger dragger, float distance, float interactionDistance)
        {
            if (dragger.CurrentDrag == null)
            {
                TryInitDragging(dragger, distance, interactionDistance);
            }
            else
            {
                StopDragging(dragger);   
            }
        }

        public static void TryInitDragging(this IDragger dragger, float distance, float interactionDistance)
        {
            if (dragger.CurrentDrag != null)
            {
                return;
            }
            var obj = dragger.ThisObject;

            
            var world = obj.PhysicsBody.World;
            var pos1 = obj.Transform.GlobalPosition;
            var pos2 = obj.Transform.Up * interactionDistance + pos1;
            Body result = null;
            /*world.RayCast((fixture, point, normal, fraction) =>
            {
                if (fixture.Body.Tag is IDraggable)
                {
                    result = fixture.Body;
                    return 0;
                }
                return 1;
            }, pos1, pos2);*/
            float d = interactionDistance;
            world.QueryAABB((fixture) =>
                {
                    if (fixture.Body == obj.PhysicsBody)
                    {
                        return true;
                    }
                    if (fixture.Body.Tag is not IDraggable)
                    {
                        return true;
                    }
                    var dist = (fixture.Body.Position - obj.PhysicsBody.Position).Length();
                    if (dist < d)
                    {
                        d = dist;
                        result = fixture.Body;
                    }
                    return true;
                }, new AABB(
                    obj.Transform.GlobalPosition - Vector2.One * interactionDistance, 
                    obj.Transform.GlobalPosition + Vector2.One * interactionDistance));
            
            if (result != null)
            {
                var pbo = result.Tag as PhysicsBodyObject;
                dragger.ConnectForDragging(pbo, distance);
            }
        }

        public static void StopDragging(this IDragger dragger)
        {
            if (dragger.CurrentDrag != null)
            {
                var obj = dragger.ThisObject;
                IDraggable iDraggable = dragger.CurrentDrag.BodyB.Tag as IDraggable;
                if (iDraggable != null)
                {
                    iDraggable.OnBecomeDropped(dragger);
                }

                if (!iDraggable.IsDestroyed)
                {
                    obj.PhysicsBody.World.Remove(dragger.CurrentDrag);
                }
                dragger.CurrentDrag = null;
                return;
            }
        }
    }
}
