using GlobalLoopGame.Globals;
using GlobalLoopGame.Planet;
using GlobalLoopGame.Spaceship.Dragging;
using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using nkast.Aether.Physics2D.Dynamics;
using MonoEngine.Scenes;

namespace GlobalLoopGame.Spaceship.Item
{
    public class RepairCharge : PhysicsBodyObject, IDraggable, IResettable
    {
        public bool IsDestroyed => !isAlive;
       
        private bool isAlive = true;

        private IDragger dragger;

        public DrawableObject circle;

        public RepairCharge(World world) : base(null)
        {
            var body = world.CreateBody(bodyType: BodyType.Dynamic);
            body.Tag = this;
            body.AngularDamping = 0f;
            body.LinearDamping = 0f;
            PhysicsBody = body;

            AddDrawableRectFixture(GameSprites.RepairChargeSize, Vector2.Zero, 0, out var fixture, 0.01f).Sprite = GameSprites.RepairCharge;

            fixture.CollisionCategories = CollisionCats.RepairCharge;
            fixture.CollidesWith = CollisionCats.CollisionsRepairCharge;

            body.OnCollision += (thisBody, otherBody, contact) =>
            {
                if (!isAlive)
                {
                    return false;
                }
                if (otherBody.CollisionCategories == CollisionCats.Shield)
                {
                    var segmentIdx = (int)otherBody.Tag;
                    var shield = (SegmentedShield)otherBody.Body.Tag;
                    if (shield.GetSegmentHealth(segmentIdx) < shield.MaxSegmentHealth)
                    {
                        shield.ModifySegment(segmentIdx, 1);
                        Despawn();
                    }
                    return false;
                }
                return true;
            };

            circle = new DrawableObject(Color.Transparent, -1f);
            circle.Sprite = GameSprites.Circle;
            circle.Parent = this;
            circle.Transform.LocalPosition = Vector2.Zero;
            circle.Transform.LocalScale = Vector2.One * 6f;
        }
        
        public void OnBecomeDragged(IDragger dragger)
        {
            this.dragger = dragger;

            circle.Color = new Color(0.85f, 0.25f, 0.25f, 1.0f);
        }

        public void OnBecomeDropped(IDragger dragger)
        {
            SpaceshipObject spaceship = dragger.ThisObject as SpaceshipObject;

            if (spaceship != null)
            {
                spaceship.magnetPivot.Transform.LocalRotation = MathHelper.ToRadians(180f);
            }

            circle.Color = Color.Transparent;

            this.dragger = null;
        }

        private void Despawn()
        {
            isAlive = false;
            PhysicsBody.World.RemoveAsync(PhysicsBody);
            CurrentScene.RemoveObject(this);

            if (dragger != null)
            {
                dragger.TryInitDragging(0,0);
            }
        }

        public void OnGameEnd()
        {
            Despawn();
        }

        public void Reset()
        {

        }
    }
}
