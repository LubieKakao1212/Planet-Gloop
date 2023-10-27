using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Globals;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision;
using nkast.Aether.Physics2D.Dynamics;
using System;
namespace GlobalLoopGame.Spaceship.Turret
{
    public static class TurretTargetingHelper
    {
        public static ITargettable FindTargetAsteroid(this AsteroidManager manager, World world, Vector2 position, float range, Func<ITargettable, float, float> fitness)
        {
            ITargettable target = null;
            float maxFitness = float.NegativeInfinity;
            foreach (var asteroid in manager.asteroids)
            {
                var delta = asteroid.Transform.GlobalPosition - position;
                var distance = delta.Length();
                if (distance < range)
                {
                    var fit = fitness(asteroid, distance);
                    if (fit > maxFitness)
                    {
                        bool lineOfSight = true;
                        world.RayCast((fixture, point, normal, fraction) =>
                        {
                            if (fixture.CollisionCategories.HasFlag(CollisionCats.Planet) ||
                                fixture.CollisionCategories.HasFlag(CollisionCats.Shield))
                            {
                                lineOfSight = false;
                                return 0f;
                            }
                            return 1f;
                        }, position, asteroid.Transform.GlobalPosition);
                        if (!lineOfSight)
                            continue;

                        target = asteroid;
                        maxFitness = fit;
                    }
                }
            }
            return target;
        }

        public static ITargettable FindTargetPhysicsBased(this World world, Vector2 position, float range, Func<ITargettable, float, float> fitness)
        {
            ITargettable target = null;
            float maxFitness = float.NegativeInfinity;
            world.QueryAABB((fixture) =>
            {
                if (fixture is not ITargettable localTarget)
                {
                    return true;
                }
                var delta = localTarget.Transform.GlobalPosition - position;
                var distance = delta.Length();
                if (distance < range)
                {
                    var fit = fitness(localTarget, distance);
                    if (fit > maxFitness)
                    {
                        bool lineOfSight = true;
                        world.RayCast((fixture, point, normal, fraction) =>
                        {
                            if (fixture.CollisionCategories.HasFlag(CollisionCats.Planet) ||
                                fixture.CollisionCategories.HasFlag(CollisionCats.Shield))
                            {
                                lineOfSight = false;
                                return 0f;
                            }
                            return 1f;
                        }, position, localTarget.Transform.GlobalPosition);
                        if (!lineOfSight)
                            return true;

                        target = localTarget;            
                        maxFitness = fit;
                    }
                }
                return true;
            }, new AABB(position - Vector2.One * range, position + Vector2.One * range));
            return target;
        }
    }
}
