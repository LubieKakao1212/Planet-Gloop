using GlobalLoopGame.Globals;
using GlobalLoopGame.Planet;
using GlobalLoopGame.Spaceship.Dragging;
using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using nkast.Aether.Physics2D.Dynamics;

namespace GlobalLoopGame.Spaceship.Item
{
    public class RepairCharge : PhysicsBodyObject, IDraggable
    {
        public bool IsDestroyed => !isAlive;
       
        private bool isAlive = true;

        private IDragger dragger;

        public RepairCharge(World world) : base(null)
        {
            var body = world.CreateBody(bodyType: BodyType.Dynamic);
            body.Tag = this;
            PhysicsBody = body;

            AddDrawableRectFixture(GameSprites.RepairChargeSize, Vector2.Zero, 0, out var fixture).Sprite = GameSprites.RepairCharge;

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
                    if (shield.GetSegmentHealth(segmentIdx) < shield.maxHealth)
                    {
                        shield.ModifySegment(segmentIdx, 1);
                        Despawn();
                    }
                    return false;
                }
                return true;
            };
        }
        
        public void OnBecomeDragged(IDragger dragger)
        {
            this.dragger = dragger;
        }

        public void OnBecomeDropped(IDragger dragger)
        {
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
    }
}
