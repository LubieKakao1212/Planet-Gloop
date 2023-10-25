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
    public class AsteroidManager : IGameComponent, IUpdateable, IResettable
    {
        private World _world;

        private Hierarchy _hierarchy;

        public int points { get; private set; }
        public bool Enabled 
        { 
            get => enabled; 
            set 
            { 
                enabled = value; 
                EnabledChanged?.Invoke(null, EventArgs.Empty); 
            } 
        }
        private bool enabled = false; 

        public int UpdateOrder { get; }

        public event EventHandler<EventArgs> EnabledChanged;

        public event EventHandler<EventArgs> UpdateOrderChanged;
        
        public int difficulty { get; private set; } = 0;
        private float waveInterval = 5f;
        private float waveWarningTime = 5f;
        private int waveNumber = 0;
        private bool dirty = false;
        private bool active = false;

        public GlobalLoopGame game;

        private AsteroidWave selectedWave;

        public List<AsteroidObject> asteroids { get; private set; } = new List<AsteroidObject>();
        public List<AsteroidWarning> asteroidWarnings { get; private set; } = new List<AsteroidWarning>();

        SequentialAutoTimeMachine waveMachine;

        public AsteroidManager(World world, Hierarchy hierarchy)
        {
            _world = world;

            _hierarchy = hierarchy;
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

        public void SelectWaveAndPlaceWarning(int diff)
        {
            if (!active) return;

            // Console.WriteLine("selecting wave and placing warning");

            waveNumber++; 
            
            List<AsteroidWave> sortedwaves = waves.Where(wave => wave.difficultyStage == diff).ToList();

            if (sortedwaves.Count > 0)
            {
                int rand = Random.Shared.Next(0, sortedwaves.Count);

                if (rand < sortedwaves.Count)
                {
                    selectedWave = sortedwaves[rand];

                    GameSounds.warningSound.Play();

                    foreach (float loc in selectedWave.warningPlacements)
                    {
                        AsteroidWarning warning = new AsteroidWarning(_world, this);

                        _hierarchy.AddObject(warning);

                        if (!asteroidWarnings.Contains(warning))
                        {
                            asteroidWarnings.Add(warning);
                        }

                        warning.InitializeWarning(loc, waveWarningTime);
                    }
                }
            }
            // If it can't find a wave of asteroids with the given difficulty, it tries again with a lower difficulty
            else if (diff > 0)
            {
                SelectWaveAndPlaceWarning(diff - 1);
            }
        }

        private void SpawnAsteroidsInPlacement(AsteroidWave wave)
        {
            if (wave is null || !active)
                return;

            // Console.WriteLine("spawning asteroids in placement");

            foreach (AsteroidPlacement aPlacement in wave.asteroidPlacements)
            {
                CreateAsteroid(aPlacement);
            }
            
            if (difficulty < 2 || waveNumber % difficulty == 0)
            {
                ModifyDifficulty(1);
            }

            SetInterval(10, 7);
        }

        public void ModifyDifficulty(int difficultyModification)
        {
            difficulty = MathHelper.Clamp(difficulty + difficultyModification, 0, 10);

            // Console.WriteLine("setting difficulty to " + difficulty.ToString());
        }

        public void ModifyPoints(int pointModification)
        {
            points += pointModification;

            if (points < 0)
            {
                points = 0;
            }

            Console.WriteLine("points " + points.ToString());
        }

        public void SetInterval(float interval, float warningTime)
        {
            waveInterval = interval;

            waveWarningTime = warningTime;

            //  Console.WriteLine(waveInterval.ToString());

            dirty = true;
        }

        public void Initialize()
        {

        }

        public void Update(GameTime gameTime)
        {
            if (!active)
                return;

            if (dirty)
            {
                var a = waveMachine.Sequence[0];
                a.interval = waveInterval;
                waveMachine.Sequence[0] = a;

                a = waveMachine.Sequence[1];
                a.interval = waveWarningTime;
                waveMachine.Sequence[1] = a;
                dirty = false;
            }

            waveMachine.Forward(gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void OnGameEnd()
        {
            active = false;

            List<AsteroidObject> asteroidsCopy = new List<AsteroidObject>(asteroids);

            foreach (AsteroidObject asteroid in asteroidsCopy)
            {
                asteroid.Die();
            }

            List<AsteroidWarning> asteroidWarningsCopy = new List<AsteroidWarning>(asteroidWarnings);

            foreach (AsteroidWarning asteroidWarning in asteroidWarningsCopy)
            {
                asteroidWarning.Despawn();
            }
        }

        public void Reset()
        {
            active = true; 
            
            difficulty = 0;
            SetInterval(3, 3);
            waveNumber = 0;
            points = 0;

            waveMachine = new SequentialAutoTimeMachine(
                (() => SelectWaveAndPlaceWarning(this.difficulty), this.waveWarningTime),
                (() => SpawnAsteroidsInPlacement(this.selectedWave), this.waveInterval)
                );
        }

        public List<AsteroidWave> waves = new List<AsteroidWave>()
        {
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 10f, 10f, 8f, 75)
            },
            0,
            new List<float>()
            {
                10f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 90f, 90f, 8f, 75)
            },
            0,
            new List<float>()
            {
                90f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 170f, 170f, 8f, 75)
            },
            0,
            new List<float>()
            {
                170f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 250f, 250f, 7f, 85)
            },
            1,
            new List<float>()
            {
                250f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 4f, 35, 77f, 7f, 85),
                new AsteroidPlacement(Vector2.One * 3f, 39f, 67f, 9f, 85)
            },
            2,
            new List<float>()
            {
                50f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 10f, 30f, 8f, 85),
                new AsteroidPlacement(Vector2.One * 3f, 0f, 20f, 8f, 85)
            },
            2,
            new List<float>()
            {
                10f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 180, 200f, 6f, 120),
                new AsteroidPlacement(Vector2.One * 3f, 170f, 190f, 10f, 80)
            },
            2,
            new List<float>()
            {
                175f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 45f, 90f, 5.5f, 130),
                new AsteroidPlacement(Vector2.One * 3f, 50f, 80f, 10f, 80),
                new AsteroidPlacement(Vector2.One * 3f, 35f, 70f, 9f, 100)
            },
            2,
            new List<float>()
            {
                50f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 4f, 90f, 120f, 6f, 100),
                new AsteroidPlacement(Vector2.One * 3f, 80f, 100f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 100f, 130f, 8f, 120)
            },
            2,
            new List<float>()
            {
                95f,
                125f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 140f, 150f, 5.5f, 90),
                new AsteroidPlacement(Vector2.One * 3.3f, 130f, 140f, 6f, 115),
                new AsteroidPlacement(Vector2.One * 3.5f, 150f, 160f, 7.5f, 105)
            },
            3,
            new List<float>()
            {
                145f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 175f, 190f, 6f, 90),
                new AsteroidPlacement(Vector2.One * 3f, 190f, 205f, 8f, 75),
                new AsteroidPlacement(Vector2.One * 6f, 160f, 175f, 5f, 130)
            },
            3,
            new List<float>()
            {
                165f,
                200f,
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3.3f, 225f, 240f, 6.5f, 80),
                new AsteroidPlacement(Vector2.One * 3.5f, 215f, 230f, 7.5f, 90),
                new AsteroidPlacement(Vector2.One * 5f, 205f, 220f, 6f, 120)
            },
            3,
            new List<float>()
            {
                220f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 17f, 0f, 30f, 5f, 220)
            },
            3,
            new List<float>()
            {
                0f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3.5f, 0f, 30f, 6.5f, 100),
                new AsteroidPlacement(Vector2.One * 3.2f, 10f, 40f, 7.5f, 90),
                new AsteroidPlacement(Vector2.One * 3f, 0f, 45f, 7f, 120)
            },
            3,
            new List<float>()
            {
                0f,
                45f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 45f, 90f, 6.5f, 100),
                new AsteroidPlacement(Vector2.One * 3.2f, 45f, 90f, 8f, 100),
                new AsteroidPlacement(Vector2.One * 3.5f, 90f, 135f, 8f, 100)
            },
            4,
            new List<float>()
            {
                70f,
                110f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 4f, 180f, 215f, 8f, 100),
                new AsteroidPlacement(Vector2.One * 3f, 215f, 270f, 9f, 100),
                new AsteroidPlacement(Vector2.One * 6f, 215f, 270f, 7f, 100)
            },
            4,
            new List<float>()
            {
                180f,
                240f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 4f, 270f, 315f, 10f, 100),
                new AsteroidPlacement(Vector2.One * 7f, 315f, 359f, 6f, 130),
                new AsteroidPlacement(Vector2.One * 6f, 315f, 350f, 7f, 120),
                new AsteroidPlacement(Vector2.One * 5f, 270f, 350f, 9f, 110)
            },
            4,
            new List<float>()
            {
                280f,
                350f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 18f, 80f, 100f, 5.4f, 300)
            },
            4,
            new List<float>()
            {
                90f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 0f, 30f, 10f, 100),
                new AsteroidPlacement(Vector2.One * 4f, 20f, 45f, 6f, 130),
                new AsteroidPlacement(Vector2.One * 7f, 90f, 120f, 7f, 140),
                new AsteroidPlacement(Vector2.One * 3f, 100f, 145f, 8f, 120)
            },
            5,
            new List<float>()
            {
                20f,
                100f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 170f, 230f, 9f, 100),
                new AsteroidPlacement(Vector2.One * 3f, 180f, 230f, 6f, 130),
                new AsteroidPlacement(Vector2.One * 6f, 290f, 320f, 9f, 140),
                new AsteroidPlacement(Vector2.One * 3f, 285f, 325f, 8f, 120)
            },
            5,
            new List<float>()
            {
                190f,
                310f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 16f, 160f, 200f, 5.2f, 280)
            },
            5,
            new List<float>()
            {
                180f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 4f, 0f, 45f, 9f, 100),
                new AsteroidPlacement(Vector2.One * 3f, 45f, 90f, 6f, 130),
                new AsteroidPlacement(Vector2.One * 8f, 0f, 90f, 9f, 160),
                new AsteroidPlacement(Vector2.One * 3f, 135f, 215f, 8f, 100),
                new AsteroidPlacement(Vector2.One * 4f, 135f, 215f, 8f, 100)
            },
            6,
            new List<float>()
            {
                45f,
                180f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 45f, 135f, 10f, 100),
                new AsteroidPlacement(Vector2.One * 4f, 45f, 135f, 6f, 130),
                new AsteroidPlacement(Vector2.One * 7f, 215f, 305f, 8f, 140),
                new AsteroidPlacement(Vector2.One * 3f, 215f, 305f, 7f, 120),
                new AsteroidPlacement(Vector2.One * 4f, 215f, 305f, 8f, 120)
            },
            6,
            new List<float>()
            {
                90f,
                270f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 90f, 180f, 7f, 100),
                new AsteroidPlacement(Vector2.One * 6f, 90f, 180f, 6f, 150),
                new AsteroidPlacement(Vector2.One * 6f, 270f, 359f, 8f, 140),
                new AsteroidPlacement(Vector2.One * 3f, 270f, 359f, 10.5f, 90),
                new AsteroidPlacement(Vector2.One * 4f, 270f, 359f, 8f, 110)
            },
            6,
            new List<float>()
            {
                135f,
                305f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 18f, 250f, 290f, 5f, 300)
            },
            6,
            new List<float>()
            {
                270f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 0f, 90f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 6f, 0f, 90f, 8f, 150),
                new AsteroidPlacement(Vector2.One * 8f, 90f, 180f, 8f, 140),
                new AsteroidPlacement(Vector2.One * 3f, 90f, 180f, 10f, 100),
                new AsteroidPlacement(Vector2.One * 4f, 180f, 270f, 8f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 270f, 9f, 110)
            },
            7,
            new List<float>()
            {
                45f,
                135f,
                215f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 8f, 45f, 135f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 170f, 230f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 7f, 170f, 230f, 8f, 140),
                new AsteroidPlacement(Vector2.One * 3f, 170f, 230f, 10f, 120),
                new AsteroidPlacement(Vector2.One * 4f, 300f, 350f, 8f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 290f, 350f, 9f, 110)
            },
            7,
            new List<float>()
            {
                90f,
                200f,
                330f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 20f, 20f, 70f, 5f, 300)
            },
            7,
            new List<float>()
            {
                45f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 0f, 45f, 10f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 0f, 45f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 7f, 90f, 180f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 3f, 90f, 180f, 8f, 120),
                new AsteroidPlacement(Vector2.One * 4f, 180f, 350f, 8f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 350f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 180f, 350f, 9f, 110)
            },
            8,
            new List<float>()
            {
                30f,
                135f,
                270f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 235f, 300f, 11f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 190f, 350f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 7f, 235f, 300f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 3f, 190f, 350f, 8f, 120),
                new AsteroidPlacement(Vector2.One * 4f, 235f, 300f, 12f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 190f, 350f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 235f, 300f, 9f, 110)
            },
            8,
            new List<float>()
            {
                270f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 21f, 90f, 180f, 4.5f, 350)
            },
            8,
            new List<float>()
            {
                135f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 0f, 90f, 10f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 0f, 90f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 7f, 90f, 180f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 3f, 90f, 180f, 11f, 120),
                new AsteroidPlacement(Vector2.One * 4f, 135f, 215f, 8f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 135f, 215f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 0f, 180f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 0f, 180f, 9f, 110)
            },
            9,
            new List<float>()
            {
                45f,
                90f,
                135f,
                180f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 180f, 270f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 270f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 10f, 270f, 315f, 12f, 130),
                new AsteroidPlacement(Vector2.One * 3f, 300, 350f, 8f, 120),
                new AsteroidPlacement(Vector2.One * 4f, 300f, 359f, 8f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 215f, 350f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 215f, 350f, 10f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 215f, 350f, 9f, 110)
            },
            9,
            new List<float>()
            {
                215f,
                270f,
                305f,
                350f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 0f, 90f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 0f, 90f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 6f, 90f, 180f, 9f, 130),
                new AsteroidPlacement(Vector2.One * 3f, 90f, 180f, 8f, 120),
                new AsteroidPlacement(Vector2.One * 4f, 180f, 270f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 270f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 4f, 270f, 359f, 10f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 270f, 359f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 6f, 270f, 359f, 10f, 110)
            },
            9,
            new List<float>()
            {
                45f,
                135f,
                215f,
                305f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 200f, 230f, 12f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 270f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 6f, 200f, 230f, 9f, 130),
                new AsteroidPlacement(Vector2.One * 3f, 200f, 230f, 8f, 120),
                new AsteroidPlacement(Vector2.One * 4f, 180f, 270f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 270f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 4f, 180f, 270f, 10f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 180f, 270f, 9f, 110)
            },
            9,
            new List<float>()
            {
                215f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 55f, 125f, 11f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 45f, 135f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 6f, 55f, 125f, 9f, 130),
                new AsteroidPlacement(Vector2.One * 3f, 45f, 135f, 8f, 120),
                new AsteroidPlacement(Vector2.One * 4f, 55f, 125f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 45f, 135f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 4f, 55f, 125f, 10f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 45f, 135f, 9f, 110)
            },
            9,
            new List<float>()
            {
                90f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 17f, 180f, 270f, 5f, 350)
            },
            9,
            new List<float>()
            {
                215f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 11f, 0f, 90f, 12f, 180),
                new AsteroidPlacement(Vector2.One * 5f, 50f, 135f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 6f, 50f, 135f, 9f, 130),
                new AsteroidPlacement(Vector2.One * 4f, 50f, 135f, 7f, 120),
                new AsteroidPlacement(Vector2.One * 6f, 135f, 215f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 135f, 215f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 4f, 135f, 215f, 10f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 270f, 359f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 6f, 270f, 359f, 11f, 110)
            },
            10,
            new List<float>()
            {
                45f,
                95f,
                185f,
                300f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 0f, 90f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 6f, 0f, 90f, 9f, 130),
                new AsteroidPlacement(Vector2.One * 4f, 0f, 90f, 7f, 120),
                new AsteroidPlacement(Vector2.One * 6f, 0f, 90f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 0f, 90f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 4f, 0f, 90f, 10f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 0f, 90f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 6f, 0f, 90f, 12f, 110)
            },
            10,
            new List<float>()
            {
                40f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 135f, 205f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 6f, 135f, 205f, 9f, 130),
                new AsteroidPlacement(Vector2.One * 4f, 135f, 205f, 7f, 120),
                new AsteroidPlacement(Vector2.One * 6f, 135f, 205f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 135f, 205f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 4f, 135f, 205f, 10f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 135f, 205f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 6f, 135f, 205f, 12f, 110)
            },
            10,
            new List<float>()
            {
                180f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 245f, 335f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 6f, 245f, 335f, 9f, 130),
                new AsteroidPlacement(Vector2.One * 4f, 245f, 335f, 7f, 120),
                new AsteroidPlacement(Vector2.One * 6f, 245f, 335f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 245f, 335f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 4f, 245f, 335f, 10f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 245f, 335f, 9f, 110),
                new AsteroidPlacement(Vector2.One * 6f, 245f, 335f, 11f, 110)
            },
            10,
            new List<float>()
            {
                290f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 22f, 270f, 359f, 5f, 500)
            },
            10,
            new List<float>()
            {
                305f
            })
        };
    }
}
