using GlobalLoopGame.Asteroid;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Spaceship
{
    public class ShotgunTurret : TurretStation
    {
        public ShotgunTurret(World world, AsteroidManager asteroids, float cooldown) : base(world, asteroids, cooldown)
        {
            bulletCount = 16;
            spread = 30f * MathF.PI / 180f;
        }

        protected override BulletObject CreateProjectile(Vector2 dir, Vector2 pos)
        {
            var bo = new BulletObject(PhysicsBody.World);
            return bo.InitializeBullet(pos, dir, Random.Shared.NextSingle() * 16f + 64f).SetLifetime(0.5f);
        }
    }
}
