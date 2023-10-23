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
        World _world;
        Hierarchy _hierarchy;

        public List<AsteroidObject> asteroids = new List<AsteroidObject>();

        public AsteroidManager(World world, Hierarchy hierarchy)
        {
            _world = world;
            _hierarchy = hierarchy;
        }

        // public void CreateAsteroid(Vector2 pos, Vector2 vel, float spe)
        public void CreateAsteroid(AsteroidPlacement placement)
        {
            AsteroidObject initializedAsteroid = new AsteroidObject(_world, 0f);

            initializedAsteroid.InitializeAsteroid(this, placement);

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

        public void SpawnWave(int difficulty)
        {
            List<AsteroidWave> sortedwaves = waves.Where(wave => wave.difficultyStage == difficulty).ToList();

            int rand = Random.Shared.Next(0, sortedwaves.Count);

            if (rand < sortedwaves.Count)
            {
                AsteroidWave wave = sortedwaves[rand];

                foreach (AsteroidPlacement aPlacement in wave.asteroidPlacements)
                {
                    CreateAsteroid(aPlacement);
                }
            }
        }

        public List<AsteroidWave> waves = new List<AsteroidWave>()
        {
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 2f, 45f, 16f, 100)
            },
            0),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, MathHelper.ToRadians(235), 16f, 120),
                new AsteroidPlacement(Vector2.One * 1.5f, MathHelper.ToRadians(240), 20f, 120)
            },
            1)
        };
    }
}
