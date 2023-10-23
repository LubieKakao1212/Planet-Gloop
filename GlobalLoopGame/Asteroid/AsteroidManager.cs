using Microsoft.Xna.Framework;
using MonoEngine.Scenes;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoEngine.Util;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidManager : IGameComponent, IUpdateable
    {
        private World _world;

        private Hierarchy _hierarchy;

        public bool Enabled => true;

        public int UpdateOrder { get; }

        public event EventHandler<EventArgs> EnabledChanged;

        public event EventHandler<EventArgs> UpdateOrderChanged;

        public int difficulty = 1;

        public List<AsteroidObject> asteroids { get; private set; } = new List<AsteroidObject>();

        AutoTimeMachine waveMachine;

        public AsteroidManager(World world, Hierarchy hierarchy)
        {
            _world = world;
            _hierarchy = hierarchy;

            waveMachine = new AutoTimeMachine(() => SpawnWave(this.difficulty), 5f);
            waveMachine.Forward(4f);
        }

        public void Initialize()
        {

        }

        public void Update(GameTime gameTime)
        {
            waveMachine.Forward(gameTime.ElapsedGameTime.TotalSeconds);
        }

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

        public void ModifyDifficulty(int difficultyModification)
        {
            difficulty = MathHelper.Clamp(difficulty + difficultyModification, 0, 10);
        }

        public List<AsteroidWave> waves = new List<AsteroidWave>()
        {
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 2f, 45, 45, 16f, 100)
            },
            0),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 235, 237, 16f, 120),
                new AsteroidPlacement(Vector2.One * 1.5f, 239, 241, 20f, 120)
            },
            1)
        };
    }
}
