﻿using GlobalLoopGame.Asteroid;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Dynamics;

namespace GlobalLoopGame.Spaceship
{
    public class SniperTurret : TurretStation
    {
        public SniperTurret(World world, AsteroidManager asteroids) : base(world, asteroids, 2.5f)
        {
            spread = 0;
        }

        protected override AsteroidObject FindTarget()
        {
            AsteroidObject best = null;
            float maxHealth = 0f;
            var world = PhysicsBody.World;
            foreach (var asteroid in asteroids.asteroids)
            {
                if (asteroid.health > maxHealth)
                {
                    bool lineOfSight = true;
                    world.RayCast((fixture, point, normal, fraction) =>
                    {
                        //the planet
                        if(fixture.CollisionCategories.HasFlag(Category.Cat5))
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
            }
            return best;
        }

        protected override BulletObject CreateProjectile(Vector2 dir, Vector2 pos)
        {
            var bo = new BulletObject(PhysicsBody.World);
            bo.pierce = int.MaxValue;
            bo.damage = int.MaxValue;
            return bo.InitializeBullet(pos, dir, 256f);
        }
    }
}
