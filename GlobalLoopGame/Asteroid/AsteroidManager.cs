using Microsoft.Xna.Framework;
using MonoEngine.Scenes;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using MonoEngine.Util;
using GlobalLoopGame.Audio;
using MonoEngine.Math;
using GlobalLoopGame.Spaceship.Item;
using GlobalLoopGame.Globals;
using Util;
using GlobalLoopGame.Planet;

namespace GlobalLoopGame.Asteroid
{
    public class AsteroidManager : IGameComponent, IUpdateable, IResettable
    {
        private World _world;

        private Hierarchy _hierarchy;

        private SegmentedShield _planetShield;

        public event Action<int> PointsUpdated;
        public event Action<int> WavesUpdated;

        public int Points 
        { 
            get => points; 
            private set 
            { 
                points = value; 
                PointsUpdated?.Invoke(value); 
            } 
        }

        private int points;

        public int WaveNumber
        {
            get => waveNumber;
            private set
            {
                waveNumber = value;
                WavesUpdated?.Invoke(value);
            }
        }

        private int waveNumber = 0;

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
        
        public static int Difficulty = 0;

        private float waveInterval = 5f;
        private float waveWarningTime = 5f;
        private bool dirty = false;
        private bool active = false;

        public GlobalLoopGame game;

        private AsteroidWave selectedWave;

        public List<AsteroidObject> asteroids { get; private set; } = new List<AsteroidObject>();
        public List<AsteroidWarning> asteroidWarnings { get; private set; } = new List<AsteroidWarning>();

        SequentialAutoTimeMachine waveMachine;

        public AsteroidManager(World world, Hierarchy hierarchy, PlanetObject planet)
        {
            _world = world;
            
            _hierarchy = hierarchy;
            this._planetShield = planet.Shield;
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

            WaveNumber++; 
            
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

            if (Difficulty < 2 || WaveNumber % (Difficulty - 1) == 0) 
            {
                ModifyDifficulty(1);
            }

            SpawnPowerups();

            SetInterval(10, 7);
        }

        private void SpawnPowerups()
        {
            var shieldHpMissing = _planetShield.TotalSegmentHealth - _planetShield.TotalHealthLeft;

            var rolls = (1 + shieldHpMissing) / 2;
            rolls = MathHelper.Min(rolls, maxRechargeRolls);

            for (int i = 0; i < rolls; i++)
            {
                if (shieldRechargeRandom.GetRandom())
                {
                    Console.WriteLine($"Spawning Recharge");

                    SpawnShieldRecharge();
                }
            }
        }

        /*private void SpawnPowerup()
        {
            if (rand.NextSingle() < 0.5f)
            {
                Console.WriteLine("Spawning powerup");
            }
        }*/

        public void SpawnShieldRecharge()
        {
            var rand = Random.Shared;

            var spawnAngle = rand.NextSingle() * MathHelper.TwoPi;
            var spawnDir = new Vector2(MathF.Cos(spawnAngle), MathF.Sin(spawnAngle));

            var spawnPos = spawnDir * (GlobalLoopGame.MapRadius + 4f);

            var r = rand.NextSingle() * 2f - 1f;
            var minAngle = 10f * MathF.PI / 180f;
            var maxAngle = 30f * MathF.PI / 180f;
            var spawnVel = -spawnDir * Matrix2x2.Rotation((maxAngle - minAngle) * r + minAngle * MathF.Sign(r));

            var spawnSpeed = 3f;
            spawnVel *= spawnSpeed;

            var powerup = new RepairCharge(_world);
            powerup.Transform.LocalPosition = spawnPos;
            powerup.PhysicsBody.LinearVelocity = spawnVel;

            r = rand.NextSingle() * 2f - 1f;
            var minAngVel = 5f;
            var maxAngVel = 10f;
            var angVel = (maxAngVel - minAngVel) * r + minAngle * MathF.Sign(r);

            GameSounds.PlaySound(GameSounds.chargeAlert, 2);

            powerup.PhysicsBody.AngularVelocity = angVel;
            _hierarchy.AddObject(powerup);
        }

        public static void ModifyDifficulty(int difficultyModification)
        {
            Difficulty = MathHelper.Clamp(Difficulty + difficultyModification, 0, 10);

            switch (Difficulty)
            {
                //case 0:
                //    MusicManager.SetIntensity(0);
                //    break;
                //case 5:
                //    MusicManager.SetIntensity(1);
                //    break;
                //case 7:
                //    MusicManager.SetIntensity(2);
                //    break;
                //default:
                //    break;

                case 0:
                    MusicManager.SetIntensity(0);
                    break;
                case 1:
                    MusicManager.SetIntensity(1);
                    break;
                case 2:
                    MusicManager.SetIntensity(2);
                    break;
                default:
                    break;
            }
        }

        public void ModifyPoints(int pointModification)
        {
            Points += pointModification;

            if (Points < 0)
            {
                Points = 0;
            }

            Console.WriteLine("points " + Points.ToString());
        }

        public void SetInterval(float interval, float warningTime)
        {
            waveInterval = interval;

            waveWarningTime = warningTime;

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

            Difficulty = 10;
            WaveNumber = 0;
            Points = 0;

            SetInterval(3, 3);

            waveMachine = new SequentialAutoTimeMachine(
                (() => SelectWaveAndPlaceWarning(Difficulty), this.waveWarningTime),
                (() => SpawnAsteroidsInPlacement(this.selectedWave), this.waveInterval)
                );
        }

        public List<AsteroidWave> waves = new List<AsteroidWave>()
        {
            //*************
            // Difficulty 0
            // ************
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 330f, 330f, 8f, 100)
            },
            0,
            new List<float>()
            {
                330f
            }),

            //*************
            // Difficulty 1
            // ************
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 250f, 250f, 7f, 100)
            },
            1,
            new List<float>()
            {
                250f
            }),

            //*************
            // Difficulty 2
            // ************
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 4f, 35, 77f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 39f, 67f, 8f, 110)
            },
            2,
            new List<float>()
            {
                50f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 10f, 30f, 8f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 0f, 20f, 7f, 110)
            },
            2,
            new List<float>()
            {
                10f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 180, 200f, 6f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 170f, 190f, 7f, 110)
            },
            2,
            new List<float>()
            {
                175f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 45f, 90f, 5f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 50f, 80f, 8f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 35f, 70f, 9f, 110)
            },
            2,
            new List<float>()
            {
                50f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 4f, 90f, 120f, 6f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 80f, 100f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 100f, 130f, 8f, 110)
            },
            2,
            new List<float>()
            {
                95f,
                125f
            }),

            //*************
            // Difficulty 3
            // ************
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 140f, 150f, 5f, 110),
                new AsteroidPlacement(Vector2.One * 3.3f, 130f, 140f, 6f, 110),
                new AsteroidPlacement(Vector2.One * 3.5f, 150f, 160f, 7f, 110)
            },
            3,
            new List<float>()
            {
                145f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 175f, 190f, 6f, 110),
                new AsteroidPlacement(Vector2.One * 3f, 190f, 205f, 8f, 110),
                new AsteroidPlacement(Vector2.One * 6f, 160f, 175f, 5f, 110)
            },
            3,
            new List<float>()
            {
                165f,
                200f,
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3.3f, 225f, 240f, 6f, 110),
                new AsteroidPlacement(Vector2.One * 3.5f, 215f, 230f, 7f, 110),
                new AsteroidPlacement(Vector2.One * 5f, 205f, 220f, 6f, 110)
            },
            3,
            new List<float>()
            {
                220f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 17f, 0f, 30f, 5f, 250)
            },
            3,
            new List<float>()
            {
                0f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3.5f, 0f, 30f, 6f, 120),
                new AsteroidPlacement(Vector2.One * 3.2f, 10f, 40f, 7f, 120),
                new AsteroidPlacement(Vector2.One * 3f, 0f, 45f, 5f, 120)
            },
            3,
            new List<float>()
            {
                0f,
                45f
            }),

            //*************
            // Difficulty 4
            // ************
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 45f, 90f, 6f, 120),
                new AsteroidPlacement(Vector2.One * 3.2f, 45f, 90f, 8f, 120),
                new AsteroidPlacement(Vector2.One * 3.5f, 90f, 135f, 7f, 120)
            },
            4,
            new List<float>()
            {
                70f,
                110f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 4f, 180f, 215f, 8f, 120),
                new AsteroidPlacement(Vector2.One * 3f, 215f, 270f, 6f, 120),
                new AsteroidPlacement(Vector2.One * 6f, 215f, 270f, 7f, 120)
            },
            4,
            new List<float>()
            {
                180f,
                240f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 4f, 270f, 315f, 8f, 120),
                new AsteroidPlacement(Vector2.One * 7f, 315f, 359f, 5f, 120),
                new AsteroidPlacement(Vector2.One * 6f, 315f, 350f, 7f, 120),
                new AsteroidPlacement(Vector2.One * 5f, 270f, 350f, 9f, 120)
            },
            4,
            new List<float>()
            {
                280f,
                350f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 18f, 80f, 100f, 5.4f, 350)
            },
            4,
            new List<float>()
            {
                90f
            }),

            //*************
            // Difficulty 5
            // ************
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 0f, 30f, 9f, 130),
                new AsteroidPlacement(Vector2.One * 4f, 20f, 45f, 6f, 130),
                new AsteroidPlacement(Vector2.One * 7f, 90f, 120f, 7f, 130),
                new AsteroidPlacement(Vector2.One * 3f, 100f, 145f, 7f, 130)
            },
            5,
            new List<float>()
            {
                20f,
                100f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 170f, 230f, 8f, 130),
                new AsteroidPlacement(Vector2.One * 3f, 180f, 230f, 6f, 130),
                new AsteroidPlacement(Vector2.One * 6f, 290f, 320f, 7f, 130),
                new AsteroidPlacement(Vector2.One * 3f, 285f, 325f, 8f, 130)
            },
            5,
            new List<float>()
            {
                190f,
                310f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 16f, 160f, 200f, 5.2f, 380)
            },
            5,
            new List<float>()
            {
                180f
            }),

            //*************
            // Difficulty 6
            // ************
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 4f, 0f, 45f, 8f, 140),
                new AsteroidPlacement(Vector2.One * 3f, 45f, 90f, 6f, 140),
                new AsteroidPlacement(Vector2.One * 8f, 0f, 90f, 8f, 140),
                new AsteroidPlacement(Vector2.One * 3f, 135f, 215f, 8f, 140),
                new AsteroidPlacement(Vector2.One * 4f, 135f, 215f, 7f, 140)
            },
            6,
            new List<float>()
            {
                45f,
                180f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 45f, 135f, 9f, 140),
                new AsteroidPlacement(Vector2.One * 4f, 45f, 135f, 6f, 140),
                new AsteroidPlacement(Vector2.One * 7f, 215f, 305f, 8f, 140),
                new AsteroidPlacement(Vector2.One * 3f, 215f, 305f, 7f, 140),
                new AsteroidPlacement(Vector2.One * 4f, 215f, 305f, 7f, 140)
            },
            6,
            new List<float>()
            {
                90f,
                270f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 90f, 180f, 6f, 140),
                new AsteroidPlacement(Vector2.One * 6f, 90f, 180f, 6f, 140),
                new AsteroidPlacement(Vector2.One * 6f, 270f, 359f, 8f, 140),
                new AsteroidPlacement(Vector2.One * 3f, 270f, 359f, 10f, 140),
                new AsteroidPlacement(Vector2.One * 4f, 270f, 359f, 7f, 140)
            },
            6,
            new List<float>()
            {
                135f,
                305f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 18f, 250f, 290f, 5f, 350)
            },
            6,
            new List<float>()
            {
                270f
            }),

            //*************
            // Difficulty 7
            // ************
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 3f, 0f, 90f, 6f, 150),
                new AsteroidPlacement(Vector2.One * 6f, 0f, 90f, 7f, 150),
                new AsteroidPlacement(Vector2.One * 8f, 90f, 180f, 8f, 150),
                new AsteroidPlacement(Vector2.One * 3f, 90f, 180f, 9f, 150),
                new AsteroidPlacement(Vector2.One * 4f, 180f, 270f, 8f, 150),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 270f, 9f, 150)
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
                new AsteroidPlacement(Vector2.One * 8f, 45f, 135f, 6f, 150),
                new AsteroidPlacement(Vector2.One * 5f, 170f, 230f, 8f, 150),
                new AsteroidPlacement(Vector2.One * 7f, 170f, 230f, 7f, 150),
                new AsteroidPlacement(Vector2.One * 3f, 170f, 230f, 9f, 150),
                new AsteroidPlacement(Vector2.One * 4f, 300f, 350f, 7f, 150),
                new AsteroidPlacement(Vector2.One * 5f, 290f, 350f, 8f, 150)
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
                new AsteroidPlacement(Vector2.One * 20f, 20f, 70f, 5f, 370)
            },
            7,
            new List<float>()
            {
                45f
            }),

            //*************
            // Difficulty 8
            // ************
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 0f, 45f, 9f, 160),
                new AsteroidPlacement(Vector2.One * 5f, 0f, 45f, 6f, 160),
                new AsteroidPlacement(Vector2.One * 7f, 90f, 180f, 8f, 160),
                new AsteroidPlacement(Vector2.One * 3f, 90f, 180f, 7f, 160),
                new AsteroidPlacement(Vector2.One * 4f, 180f, 350f, 7f, 160),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 350f, 9f, 160),
                new AsteroidPlacement(Vector2.One * 3f, 180f, 350f, 8f, 160)
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
                new AsteroidPlacement(Vector2.One * 6f, 235f, 300f, 10f, 160),
                new AsteroidPlacement(Vector2.One * 5f, 190f, 350f, 6f, 160),
                new AsteroidPlacement(Vector2.One * 7f, 235f, 300f, 8f, 160),
                new AsteroidPlacement(Vector2.One * 3f, 190f, 350f, 7f, 160),
                new AsteroidPlacement(Vector2.One * 4f, 235f, 300f, 10f, 160),
                new AsteroidPlacement(Vector2.One * 5f, 190f, 350f, 9f, 160),
                new AsteroidPlacement(Vector2.One * 3f, 235f, 300f, 8f, 160)
            },
            8,
            new List<float>()
            {
                270f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 21f, 90f, 180f, 4.5f, 410)
            },
            8,
            new List<float>()
            {
                135f
            }),

            //*************
            // Difficulty 9
            // ************
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 0f, 90f, 9f, 170),
                new AsteroidPlacement(Vector2.One * 5f, 0f, 90f, 8f, 170),
                new AsteroidPlacement(Vector2.One * 4f, 135f, 215f, 7f, 170),
                new AsteroidPlacement(Vector2.One * 5f, 135f, 215f, 9f, 170),
                new AsteroidPlacement(Vector2.One * 3f, 0f, 180f, 8f, 170),
                new AsteroidPlacement(Vector2.One * 3f, 0f, 180f, 8f, 170)
            },
            9,
            new List<float>()
            {
                45f,
                90f,
                180f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 180f, 270f, 7f, 180),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 270f, 6f, 180),
                new AsteroidPlacement(Vector2.One * 3f, 300, 350f, 8f, 180),
                new AsteroidPlacement(Vector2.One * 4f, 300f, 359f, 8f, 180),
                new AsteroidPlacement(Vector2.One * 5f, 215f, 350f, 8f, 180),
                new AsteroidPlacement(Vector2.One * 3f, 215f, 350f, 9f, 180),
                new AsteroidPlacement(Vector2.One * 3f, 215f, 350f, 7f, 180)
            },
            9,
            new List<float>()
            {
                215f,
                305f,
                350f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 90f, 180f, 9f, 180),
                new AsteroidPlacement(Vector2.One * 3f, 90f, 180f, 8f, 180),
                new AsteroidPlacement(Vector2.One * 4f, 180f, 270f, 7f, 180),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 270f, 7f, 180),
                new AsteroidPlacement(Vector2.One * 4f, 270f, 359f, 9f, 180),
                new AsteroidPlacement(Vector2.One * 3f, 270f, 359f, 8f, 180),
                new AsteroidPlacement(Vector2.One * 6f, 270f, 359f, 9f, 180)
            },
            9,
            new List<float>()
            {
                135f,
                215f,
                305f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 200f, 230f, 11f, 180),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 270f, 7f, 180),
                new AsteroidPlacement(Vector2.One * 6f, 200f, 230f, 8f, 180),
                new AsteroidPlacement(Vector2.One * 3f, 200f, 230f, 8f, 180),
                new AsteroidPlacement(Vector2.One * 4f, 180f, 270f, 7f, 180),
                new AsteroidPlacement(Vector2.One * 5f, 180f, 270f, 8f, 180),
                new AsteroidPlacement(Vector2.One * 4f, 180f, 270f, 9f, 180)
            },
            9,
            new List<float>()
            {
                215f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 6f, 55f, 125f, 10f, 180),
                new AsteroidPlacement(Vector2.One * 5f, 45f, 135f, 7f, 180),
                new AsteroidPlacement(Vector2.One * 6f, 55f, 125f, 8f, 180),
                new AsteroidPlacement(Vector2.One * 3f, 45f, 135f, 8f, 180),
                new AsteroidPlacement(Vector2.One * 4f, 55f, 125f, 7f, 180),
                new AsteroidPlacement(Vector2.One * 5f, 45f, 135f, 9f, 180),
                new AsteroidPlacement(Vector2.One * 4f, 55f, 125f, 8f, 180)
            },
            9,
            new List<float>()
            {
                90f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 17f, 180f, 270f, 5f, 450)
            },
            9,
            new List<float>()
            {
                215f
            }),

            //*************
            // Difficulty 10
            // ************
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 11f, 0f, 90f, 11f, 190),
                new AsteroidPlacement(Vector2.One * 5f, 50f, 135f, 8f, 190),
                new AsteroidPlacement(Vector2.One * 6f, 50f, 135f, 8f, 190),
                new AsteroidPlacement(Vector2.One * 4f, 50f, 135f, 7f, 190),
                new AsteroidPlacement(Vector2.One * 6f, 135f, 215f, 6f, 190),
                new AsteroidPlacement(Vector2.One * 5f, 135f, 215f, 8f, 190)
            },
            10,
            new List<float>()
            {
                45f,
                95f,
                185f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 0f, 90f, 7f, 190),
                new AsteroidPlacement(Vector2.One * 6f, 0f, 90f, 8f, 190),
                new AsteroidPlacement(Vector2.One * 4f, 0f, 90f, 6f, 190),
                new AsteroidPlacement(Vector2.One * 6f, 0f, 90f, 6f, 190),
                new AsteroidPlacement(Vector2.One * 5f, 0f, 90f, 8f, 190),
                new AsteroidPlacement(Vector2.One * 4f, 0f, 90f, 9f, 190),
                new AsteroidPlacement(Vector2.One * 3f, 0f, 90f, 8f, 190)
            },
            10,
            new List<float>()
            {
                40f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 135f, 205f, 7f, 190),
                new AsteroidPlacement(Vector2.One * 6f, 135f, 205f, 8f, 190),
                new AsteroidPlacement(Vector2.One * 4f, 135f, 205f, 6f, 190),
                new AsteroidPlacement(Vector2.One * 6f, 135f, 205f, 6f, 190),
                new AsteroidPlacement(Vector2.One * 5f, 135f, 205f, 8f, 190),
                new AsteroidPlacement(Vector2.One * 4f, 135f, 205f, 9f, 190),
                new AsteroidPlacement(Vector2.One * 3f, 135f, 205f, 8f, 190)
            },
            10,
            new List<float>()
            {
                180f
            }),
            new AsteroidWave(new List<AsteroidPlacement>()
            {
                new AsteroidPlacement(Vector2.One * 5f, 245f, 335f, 7f, 200),
                new AsteroidPlacement(Vector2.One * 6f, 245f, 335f, 8f, 200),
                new AsteroidPlacement(Vector2.One * 4f, 245f, 335f, 6f, 200),
                new AsteroidPlacement(Vector2.One * 6f, 245f, 335f, 6f, 200),
                new AsteroidPlacement(Vector2.One * 5f, 245f, 335f, 8f, 200),
                new AsteroidPlacement(Vector2.One * 4f, 245f, 335f, 9f, 200),
                new AsteroidPlacement(Vector2.One * 3f, 245f, 335f, 8f, 200)
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

        private int maxRechargeRolls = 5;

        //17 false, 3 true
        private BucketRandom<bool> shieldRechargeRandom = new BucketRandom<bool>(Random.Shared, 
            false, false, false, false, false,
            false, false, false, false, false,
            false, false, false, false, false,
            false, false, true , true , true 
            );
    }

    /*public enum PowerupTye 
    { 
        None = 0,
        ShieldCharge = 1,
    }*/

}
