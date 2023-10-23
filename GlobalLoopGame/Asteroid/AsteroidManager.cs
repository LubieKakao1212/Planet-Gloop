using Microsoft.Xna.Framework;
using MonoEngine.Scenes;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidManager
    {
        private World _world;
        private Hierarchy _hierarchy;

        public List<AsteroidObject> asteroids { get; private set; } = new List<AsteroidObject>();

        public AsteroidManager(World world, Hierarchy hierarchy)
        {
            _world = world;
            _hierarchy = hierarchy;
        }

        public void CreateAsteroid(Vector2 pos, Vector2 vel, float spe)
        {
            AsteroidObject initializedAsteroid = new AsteroidObject(_world, 0f);

            initializedAsteroid.InitializeAsteroid(pos, vel, spe);

            _hierarchy.AddObject(initializedAsteroid);

            if (!asteroids.Contains(initializedAsteroid))
            {
                asteroids.Add(initializedAsteroid);
            }
            
        }

        public void RemoveAsteroid(AsteroidObject asteroid)
        {
            if (asteroids.Contains(asteroid))
            {
                asteroids.Remove(asteroid);
            }
        }
    }
}
