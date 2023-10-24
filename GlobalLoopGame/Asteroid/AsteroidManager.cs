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
        public float waveInterval = 5f;
        public float waveWarningTime = 5f;

        private AsteroidWave selectedWave;

        public List<AsteroidObject> asteroids { get; private set; } = new List<AsteroidObject>();

        SequentialAutoTimeMachine waveMachine;

        public AsteroidManager(World world, Hierarchy hierarchy)
        {
            _world = world;
            _hierarchy = hierarchy;

            waveMachine = new SequentialAutoTimeMachine(
                (() => SelectWaveAndPlaceWarning(this.difficulty), this.waveInterval), 
                (() => SpawnAsteroidsInPlacement(this.selectedWave), this.waveWarningTime)
                );
        }

        public void Initialize()
        {
            waveMachine.Forward(waveInterval);
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

        public void SelectWaveAndPlaceWarning(int difficulty)
        {
            List<AsteroidWave> sortedwaves = waves.Where(wave => wave.difficultyStage == difficulty).ToList();

            int rand = Random.Shared.Next(0, sortedwaves.Count);

            if (rand < sortedwaves.Count)
            {
                selectedWave = sortedwaves[rand];
                
                foreach (float loc in selectedWave.warningPlacements)
                {
                    AsteroidWarning warning = new AsteroidWarning(_world);

                    _hierarchy.AddObject(warning);

                    warning.InitializeWarning(loc, waveInterval);
                }
            }
        }

        private void SpawnAsteroidsInPlacement(AsteroidWave wave)
        {
            foreach (AsteroidPlacement aPlacement in wave.asteroidPlacements)
            {
                CreateAsteroid(aPlacement);
            }
        }

        public void ModifyDifficulty(int difficultyModification)
        {
            difficulty = MathHelper.Clamp(difficulty + difficultyModification, 0, 10);

            waveInterval = MathHelper.Clamp(20 - difficulty, 6, 999);
        }

        public List<AsteroidWave> waves = new List<AsteroidWave>()
        {
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 2f, 45f, 45f, 8f, 100)
            },
            0,
            new List<float>()
            {
                45f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 235f, 237f, 7f, 120),
                new AsteroidPlacement(Vector2.One * 1.5f, 239f, 241f, 9f, 120)
            },
            1,
            new List<float>()
            {
                235f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 35, 77f, 7f, 120),
                new AsteroidPlacement(Vector2.One * 1.5f, 39f, 67f, 9f, 120)
            },
            1,
            new List<float>()
            {
                50f
            })
        };
    }
}
