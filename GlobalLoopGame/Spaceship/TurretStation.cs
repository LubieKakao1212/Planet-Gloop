using GlobalLoopGame.Spaceship.Dragging;
using Microsoft.Xna.Framework;
using MonoEngine.Physics;
using nkast.Aether.Physics2D.Dynamics;

namespace GlobalLoopGame.Spaceship
{
    public class TurretStation : PhysicsBodyObject, IDraggable
    {
        public TurretStation(World world) : base(null)
        {
            PhysicsBody = world.CreateBody(bodyType: BodyType.Dynamic);
            PhysicsBody.Tag = this;
            PhysicsBody.AngularDamping = 1f;
            PhysicsBody.LinearDamping = 1f;
            var drawable = AddDrawableRectFixture(new Vector2(3f, 3f), Vector2.Zero, 0f, out var fixture, 1f);

            // Asteroids are collision Category 1, Player is collision Category 2, and Turrets are collision Category 3
            fixture.CollisionCategories = Category.Cat3;
            fixture.CollidesWith = Category.None;
            fixture.CollidesWith |= Category.Cat2;
            fixture.CollidesWith |= Category.Cat3;
        }
    }
}
