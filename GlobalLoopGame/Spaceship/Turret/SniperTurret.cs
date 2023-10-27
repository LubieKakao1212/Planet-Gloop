using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Globals;
using Microsoft.Xna.Framework;
using MonoEngine.Rendering;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Dynamics;
using System.Reflection.Metadata.Ecma335;

namespace GlobalLoopGame.Spaceship.Turret
{
    public class SniperTurret : TurretStation
    {
        public SniperTurret(World world, AsteroidManager asteroids, RenderPipeline renderer) : base(world, asteroids, renderer, 2.5f, 0.5f)
        {
            spread = 0;
            RangeRadius = 72f;
            damage = 200;
            shotIndex = 2;
            UpdateText();
        }

        protected override ITargettable FindTarget()
        {
            var world = PhysicsBody.World;
            ITargettable target = null; // world.FindTargetPhysicsBased(Transform.GlobalPosition, , () => );
            target ??= asteroids.FindTargetAsteroid(world, Transform.GlobalPosition, RangeRadius, (target, distance) => target.Health);
            return target;
            /*float maxHealth = 0f;
            var world = PhysicsBody.World;
            foreach (var asteroid in asteroids.asteroids)
            {
                if (asteroid.health > maxHealth && (asteroid.Transform.GlobalPosition - Transform.GlobalPosition).LengthSquared() < RangeRadius * RangeRadius)
                {
                    bool lineOfSight = true;
                    world.RayCast((fixture, point, normal, fraction) =>
                    {
                        if (fixture.CollisionCategories.HasFlag(CollisionCats.Planet) || fixture.CollisionCategories.HasFlag(CollisionCats.Shield))
                        {
                            lineOfSight = false;
                            return 0f;
                        }
                        return 1f;
                    }, Transform.GlobalPosition, asteroid.Transform.GlobalPosition);
                    if (!lineOfSight)
                        continue;

                    best = asteroid;
                    maxHealth = asteroid.health;
                }
            }*/
        }

        protected override BulletObject CreateBullet(Vector2 dir, Vector2 pos, float speed)
        {
            var bo = new BulletObject(PhysicsBody.World);
            bo.damageIsPierce = true;
            bo.pierce = 1;
            bo.damage = damage;
            return bo.InitializeBullet(pos, dir, speed).SetColor(Color.OrangeRed);
        }

        protected override float GetBulletSpeed()
        {
            return 256f;
        }
    }
}
