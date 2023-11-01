using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Globals;
using Microsoft.Xna.Framework;
using Custom2d_Engine.Rendering;
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
            MinTargettingDistance = 24f;
            UpdateText();
        }

        protected override ITargettable FindTarget()
        {
            var world = PhysicsBody.World;
            ITargettable target = null;
            target ??= asteroids.FindTargetAsteroid(world, Transform.GlobalPosition, RangeRadius, (target, distance) => distance < MinTargettingDistance ? float.NegativeInfinity : target.Health);
            return target;
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
