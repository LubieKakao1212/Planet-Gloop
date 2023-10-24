using GlobalLoopGame.Spaceship.Dragging;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using MonoEngine.Scenes;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Joints;
using System;
using System.Collections.Generic;

namespace GlobalLoopGame.Spaceship
{
    public class SpaceshipObject : PhysicsBodyObject, IDragger, IResettable
    {
        public float ThrustMultiplier { get; set; }

        public Joint CurrentDrag { get; set; }

        public PhysicsBodyObject ThisObject => this;

        private bool movable = false;

        /// <summary>
        /// BottomLeft, BottomRight, TopLeft, TopRight
        /// </summary>
        private List<HierarchyObject> thrusters = new List<HierarchyObject>();

        private List<int> thrust = new List<int>();

        public SpaceshipObject(World world, float drawOrder) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Tag = this;
            PhysicsBody.AngularDamping = 10f;
            PhysicsBody.LinearDamping = 3.5f;
            Transform.GlobalPosition = new Vector2(0f, -48f);
            
            var shipBody = AddDrawableRectFixture(new(3f, 1f), new(0f, 0f), 0, out var fixture);

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3
            fixture.CollisionCategories = Category.Cat2;
            fixture.CollidesWith = Category.None;
            fixture.CollidesWith |= Category.Cat1;
            fixture.CollidesWith |= Category.Cat3;
            fixture.CollidesWith |= Category.Cat5;

            shipBody.DrawOrder = drawOrder + 0.01f;
            AddThruster(new(-1f, -0.5f), 0f);
            AddThruster(new(1f, -0.5f), 0f);
            AddThruster(new(-1f, 0.5f), MathF.PI);
            AddThruster(new(1f, 0.5f), MathF.PI);
        }
        
        public void IncrementThruster(int idx)
        {
            if (movable)
            {
                thrust[idx] += 1;
                UpdateThruster(idx);
            }

            // CurrentDrag.BodyB.Tag as IDraggable
        }

        public void DecrementThruster(int idx)
        {
            if (movable)
            {
                thrust[idx] -= 1;
                UpdateThruster(idx);
            }
        }

        public void AddThruster(Vector2 pos, float rotation)
        {
            var drawable = new DrawableObject(Color.Cyan, 0f);
            drawable.Transform.LocalPosition = -Vector2.UnitY / 2f;
            var root = new HierarchyObject();
            drawable.Parent = root;
            root.Transform.LocalPosition = pos;
            root.Transform.LocalRotation = rotation;
            root.Parent = this;
            thrusters.Add(root);
            thrust.Add(0);
            UpdateThruster(thrusters.Count - 1);
        }

        private void UpdateThruster(int idx)
        {
            var s = thrusters[idx].Transform.LocalScale;
            s.Y = thrust[idx];
            thrusters[idx].Transform.LocalScale = s;
        }

        public override void Update(GameTime time)
        {
            if (movable)
            {
                for (int i = 0; i < thrusters.Count; i++)
                {
                    PhysicsBody.ApplyForce(thrusters[i].Transform.Up * ThrustMultiplier, thrusters[i].Transform.GlobalPosition);
                }
            }

            base.Update(time);
        }
        public void OnGameEnd()
        {
            PhysicsBody.LinearVelocity = Vector2.Zero;
            PhysicsBody.AngularVelocity = 0f;
            movable = false;
        }

        public void Reset()
        {
            Transform.GlobalPosition = new Vector2(0f, -48f);
            movable = true;
        }
    }
}
