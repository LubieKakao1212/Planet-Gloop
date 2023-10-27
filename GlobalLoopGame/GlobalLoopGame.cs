using GlobalLoopGame.Spaceship;
using GlobalLoopGame.Spaceship.Dragging;
using GlobalLoopGame.Updaters;
using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Planet;
using GlobalLoopGame.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoEngine.Input;
using MonoEngine.Rendering;
using MonoEngine.Rendering.Sprites;
using MonoEngine.Rendering.Sprites.Atlas;
using MonoEngine.Scenes;
using MonoEngine.Scenes.Events;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using GlobalLoopGame.UI;
using Microsoft.Xna.Framework.Audio;
using MonoEngine.Input.Binding;
using nkast.Aether.Physics2D.Diagnostics;
using GlobalLoopGame.Globals;
using GlobalLoopGame.Spaceship.Turret;
using GameUtils.Profiling;

namespace GlobalLoopGame
{
    public class GlobalLoopGame : Game
    {
        public const float MapRadius = 64f;
        public const float PlanetRadius = 12f;
        public const int WindowSize = 1000;

        private GraphicsDeviceManager _graphics;

        private RenderPipeline renderPipeline;
        private World world;

        //Physics Debug
        private DebugView physicsDebug;
        private Hierarchy hierarchyMenu;
        private Hierarchy hierarchyGame;
        private Hierarchy hierarchyUI;
        private Hierarchy hierarchyGameOver;
        private Hierarchy hierarchyPressEnter;
        private Hierarchy hierarchyPaused;
        private Hierarchy hierarchyStarrySky;

        private InputManager inputManager;

        private Camera uiCamera; 
        private Camera gameCamera;

        private SpriteAtlas<Color> spriteAtlas;

        private SpriteBatch textRenderer;

        private GameTime GameTime;

        private bool gameEnded = true;
        private bool menuDisplayed = true;

        private int enterKeyEnteredCounter = 0;

        public AsteroidManager asteroidManager { get; private set; }
        public MusicManager musicManager { get; private set; }
        public CameraManager cameraManager { get; private set; }

        public SpaceshipObject Spaceship { get; private set; }
        public PlanetObject Planet { get; private set; }

        public static TimeLogger Profiler { get; set; } = TimeLogger.Instance;

        public List<TurretStation> Turrets { get; private set; } = new List<TurretStation>();

        private TextObject pointsText, wavesText;

        public List<IResettable> AdditionalResettables { get; private set; } = new List<IResettable>();

        CompoundAxixBindingInput accelerate, decelerate, rotLeft, rotRight;

        public GlobalLoopGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = WindowSize;
            _graphics.PreferredBackBufferHeight = WindowSize;
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            Profiler.unitScale = 1f / 60f;
            Profiler.enabled = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            renderPipeline = new RenderPipeline();
            Effects.Init(Content);
            renderPipeline.Init(GraphicsDevice);
            inputManager = new InputManager(Window);
            textRenderer = new SpriteBatch(GraphicsDevice);

            LoadSounds();

            LoadSprites();
            
            LoadEffects();

            CreateWorld();

            CreateScene();

            CreateMenuScene();

            CreateGameOverScene();

            CreateBindings();

            CreateUpdateables();

            CreateUI();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.X))
                Exit();

            Profiler.Push("Update");

            GameTime = gameTime;

            inputManager.UpdateState();

            if (!menuDisplayed)
            {
                Profiler.Push("Physics");
                world.Step(gameTime.ElapsedGameTime);
                Profiler.Pop("Physics");

                Profiler.Push("Game");
                hierarchyGame.BeginUpdate();
                foreach (var updatable in hierarchyGame.OrderedInstancesOf<IUpdatable>())
                {
                    updatable.Update(gameTime);
                }
                hierarchyGame.EndUpdate();
                Profiler.Pop("Game");
                
                Profiler.Push("UI");
                hierarchyUI.BeginUpdate();
                foreach (var updatable in hierarchyUI.OrderedInstancesOf<IUpdatable>())
                {
                    updatable.Update(gameTime);
                }
                hierarchyUI.EndUpdate();
                Profiler.Pop("UI");
            }
            else
            {
                hierarchyMenu.BeginUpdate();
                foreach (var updatable in hierarchyMenu.OrderedInstancesOf<IUpdatable>())
                {
                    updatable.Update(gameTime);
                }
                hierarchyMenu.EndUpdate();

                if(gameEnded && enterKeyEnteredCounter == 0)
                {
                    hierarchyPressEnter.BeginUpdate();
                    foreach (var updatable in hierarchyPressEnter.OrderedInstancesOf<IUpdatable>())
                    {
                        updatable.Update(gameTime);
                    }
                    hierarchyPressEnter.EndUpdate();
                } 
                else if (gameEnded)
                {
                    hierarchyGameOver.BeginUpdate();
                    foreach (var updatable in hierarchyGameOver.OrderedInstancesOf<IUpdatable>())
                    {
                        updatable.Update(gameTime);
                    }
                    hierarchyGameOver.EndUpdate();
                } 
                else
                {
                    hierarchyPaused.BeginUpdate();
                    foreach (var updatable in hierarchyPaused.OrderedInstancesOf<IUpdatable>())
                    {
                        updatable.Update(gameTime);
                    }
                    hierarchyPaused.EndUpdate();
                }
            }

            Profiler.Pop("Update");

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            if (!menuDisplayed)
            {
                GraphicsDevice.Clear(new Color(0.1f, 0.1f, 0.1f, 1.0f));
                renderPipeline.RenderScene(hierarchyGame, gameCamera);
                renderPipeline.RenderScene(hierarchyUI, uiCamera);

                textRenderer.DrawAllText(hierarchyGame, GameSprites.Font, gameCamera);
                textRenderer.DrawAllText(hierarchyUI, GameSprites.Font, uiCamera);

                //physicsDebug.RenderDebugData(camera.ProjectionMatrix.ToMatrixXNA(), Matrix.Identity);
                if (gameEnded)
                {
                    //GraphicsDevice.Clear(Color.Red);
                    renderPipeline.RenderScene(hierarchyGameOver, uiCamera);
                    textRenderer.DrawAllText(hierarchyGameOver, GameSprites.Font, uiCamera);
                }
            }
            else
            {
                //GraphicsDevice.Clear(new Color(19, 18, 51));
                //GraphicsDevice.Clear(new Color(12, 11, 33));
                GraphicsDevice.Clear(new Color(15, 15, 15));
                //GraphicsDevice.Clear(Color.Black);
                renderPipeline.RenderScene(hierarchyMenu, uiCamera);
                textRenderer.DrawAllText(hierarchyMenu, GameSprites.Font, uiCamera);
                if (gameEnded && enterKeyEnteredCounter == 0)
                {
                    renderPipeline.RenderScene(hierarchyPressEnter, uiCamera);
                    textRenderer.DrawAllText(hierarchyPressEnter, GameSprites.Font, uiCamera);
                }
                else
                {
                    renderPipeline.RenderScene(hierarchyPaused, uiCamera);
                    textRenderer.DrawAllText(hierarchyPaused, GameSprites.Font, uiCamera);
                }
            }

            base.Draw(gameTime);
        }

        private void LoadSounds()
        {
            //Load sounds and songs here
            GameSounds.asteroidHurtSound = Content.Load<SoundEffect>("Sounds/AsteroidHurt");
            GameSounds.bigAsteroidDeath = Content.Load<SoundEffect>("Sounds/AsteroidBig");
            GameSounds.smallAsteroidDeath = Content.Load<SoundEffect>("Sounds/Asteroid");
            GameSounds.magnetSound = Content.Load<SoundEffect>("Sounds/Magnet");
            GameSounds.dropTurretSound = Content.Load<SoundEffect>("Sounds/DropTurret");
            GameSounds.pickupTurretSound = Content.Load<SoundEffect>("Sounds/PickupTurret");
            GameSounds.planetHurtSound = Content.Load<SoundEffect>("Sounds/PlanetHurt");
            GameSounds.playerHurtSound = Content.Load<SoundEffect>("Sounds/PlayerHurt");
            GameSounds.sideThrustSound = Content.Load<SoundEffect>("Sounds/SideThrust");
            GameSounds.thrusterSound = Content.Load<SoundEffect>("Sounds/Thruster");
            GameSounds.warningSound = Content.Load<SoundEffect>("Sounds/Warning");

            GameSounds.boostSound = Content.Load<SoundEffect>("Sounds/Boosting");
            GameSounds.boostChargingSound = Content.Load<SoundEffect>("Sounds/BoostCharging");
            GameSounds.boostOverloadSound = Content.Load<SoundEffect>("Sounds/BoostOverload");
            GameSounds.boostUnableSound = Content.Load<SoundEffect>("Sounds/BoostUnable");

            GameSounds.shotSounds[0] = Content.Load<SoundEffect>("Sounds/RegularShot");
            GameSounds.shotSounds[1] = Content.Load<SoundEffect>("Sounds/ShotgunShot");
            GameSounds.shotSounds[2] = Content.Load<SoundEffect>("Sounds/SniperShot");
            GameSounds.shotgunReloadSound = Content.Load<SoundEffect>("Sounds/ShotgunReload");

            GameSounds.shieldHurt = Content.Load<SoundEffect>("Sounds/ShieldHurt"); 
            GameSounds.shieldHeal = Content.Load<SoundEffect>("Sounds/ShieldHeal"); 
            GameSounds.shieldDestroy = Content.Load<SoundEffect>("Sounds/ShieldDestroy");

            GameSounds.chargePickup = Content.Load<SoundEffect>("Sounds/EnergyPickup");
            GameSounds.chargeAlert = Content.Load<SoundEffect>("Sounds/EnergyAlert");

            GameSounds.musicIntensity1 = Content.Load<SoundEffect>("Sounds/Music/Song0");
            GameSounds.musicIntensity2 = Content.Load<SoundEffect>("Sounds/Music/Song1");
            GameSounds.musicIntensity3 = Content.Load<SoundEffect>("Sounds/Music/Song2");
            GameSounds.musicIntensity4 = Content.Load<SoundEffect>("Sounds/Music/Song3");
            GameSounds.musicIntensity5 = Content.Load<SoundEffect>("Sounds/Music/Song4");
            GameSounds.musicIntensity6 = Content.Load<SoundEffect>("Sounds/Music/Song5");
            GameSounds.musicIntensity7 = Content.Load<SoundEffect>("Sounds/Music/Song6");
            GameSounds.musicIntensity8 = Content.Load<SoundEffect>("Sounds/Music/Song7");
            GameSounds.musicIntensity9 = Content.Load<SoundEffect>("Sounds/Music/Song8");
            GameSounds.musicIntensity10 = Content.Load<SoundEffect>("Sounds/Music/Song9");
            GameSounds.musicIntensity11 = Content.Load<SoundEffect>("Sounds/Music/Song10");
            GameSounds.musicIntensity12 = Content.Load<SoundEffect>("Sounds/Music/Song11");

            GameSounds.magnetEmitter = GameSounds.magnetSound.CreateInstance();
            GameSounds.magnetEmitter.IsLooped = true;
            GameSounds.magnetEmitter.Pause();

            GameSounds.thrusterEmitter = GameSounds.thrusterSound.CreateInstance();
            GameSounds.thrusterEmitter.IsLooped = true;
            GameSounds.thrusterEmitter.Volume = 0f;
            GameSounds.thrusterEmitter.Pause();

            GameSounds.sideThrusterEmitter = GameSounds.sideThrustSound.CreateInstance();
            GameSounds.sideThrusterEmitter.IsLooped = true;
            GameSounds.sideThrusterEmitter.Volume = 0f;
            GameSounds.sideThrusterEmitter.Pause();

            GameSounds.boostEmitter = GameSounds.boostSound.CreateInstance();
            GameSounds.boostEmitter.IsLooped = true;
            GameSounds.boostEmitter.Volume = 0f;
            GameSounds.boostEmitter.Pause();

            GameSounds.boostChargingEmitter = GameSounds.boostChargingSound.CreateInstance();
            GameSounds.boostChargingEmitter.IsLooped = true;
            GameSounds.boostChargingEmitter.Volume = 1.0f;
            GameSounds.boostChargingEmitter.Pause();

            GameSounds.MusicInstance1 = GameSounds.musicIntensity1.CreateInstance();
            GameSounds.MusicInstance2 = GameSounds.musicIntensity2.CreateInstance();
            GameSounds.MusicInstance3 = GameSounds.musicIntensity3.CreateInstance();
            GameSounds.MusicInstance4 = GameSounds.musicIntensity4.CreateInstance();
            GameSounds.MusicInstance5 = GameSounds.musicIntensity5.CreateInstance();
            GameSounds.MusicInstance6 = GameSounds.musicIntensity6.CreateInstance();
            GameSounds.MusicInstance7 = GameSounds.musicIntensity7.CreateInstance();
            GameSounds.MusicInstance8 = GameSounds.musicIntensity8.CreateInstance();
            GameSounds.MusicInstance9 = GameSounds.musicIntensity9.CreateInstance();
            GameSounds.MusicInstance10 = GameSounds.musicIntensity10.CreateInstance();
            GameSounds.MusicInstance11 = GameSounds.musicIntensity11.CreateInstance();
            GameSounds.MusicInstance12 = GameSounds.musicIntensity12.CreateInstance();
            List<SoundEffectInstance> musicList = new List<SoundEffectInstance>
            {
                GameSounds.MusicInstance1,
                GameSounds.MusicInstance2,
                GameSounds.MusicInstance3,
                GameSounds.MusicInstance4,
                GameSounds.MusicInstance5,
                GameSounds.MusicInstance6,
                GameSounds.MusicInstance7,
                GameSounds.MusicInstance8,
                GameSounds.MusicInstance9,
                GameSounds.MusicInstance10,
                GameSounds.MusicInstance11,
                GameSounds.MusicInstance12,
            };
            
            foreach (SoundEffectInstance song in musicList)
            {
                song.IsLooped = true;
                song.Volume = 0f;
                song.Play();
            }
        }

        private void LoadSprites()
        {
            spriteAtlas = new SpriteAtlas<Color>(GraphicsDevice, 2048);

            var white = new Texture2D(GraphicsDevice, 1, 1);
            white.SetData(new Color[] { Color.White });
            GameSprites.NullSprite = spriteAtlas.AddTextureRects(white, new Rectangle(0, 0, 1, 1))[0];
            GameSprites.Circle = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("CircleTex"), new Rectangle(0, 0, 256, 256))[0];
            GameSprites.DiamondStar = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("32x32_Star_4"), new Rectangle(0, 0, 32, 32))[0];
            GameSprites.LightCookie_1 = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("32x32_Arcane_0"), new Rectangle(0, 0, 32, 32))[0];
            GameSprites.LightCookie_2 = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("32x32_Arcane_3"), new Rectangle(0, 0, 32, 32))[0];
            GameSprites.LightCookie_3 = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("32x32_Arcane_4"), new Rectangle(0, 0, 32, 32))[0];
            GameSprites.LightCookie_4 = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("32x32_Arcane_5"), new Rectangle(0, 0, 32, 32))[0];
            GameSprites.LightCookie_5 = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("32x32_Arcane_15"), new Rectangle(0, 0, 32, 32))[0];
            GameSprites.LightCookie_6 = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("32x32_Arcane_16"), new Rectangle(0, 0, 32, 32))[0];
            GameSprites.Planet = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("PlanetTex"), new Rectangle(0, 0, 128, 128))[0];

            var spaceshipTextures = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("SpaceshipTex"),
                new Rectangle(0, 1, 32, 25),
                new Rectangle(37, 36, 22, 22),
                new Rectangle(35, 8, 6, 6),
                new Rectangle(4, 30, 22, 34)
                );

            GameSprites.SpaceshipBody = spaceshipTextures[0];
            GameSprites.SpaceshipMagnet = spaceshipTextures[1];
            GameSprites.SpaceshipThrusterFrames = new Sprite[] { spaceshipTextures[2] };
            GameSprites.SpaceshipMagnetActive = spaceshipTextures[3];

            GameSprites.TurretBase = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("TurretPlatformTex"),
                new Rectangle(0, 0, 48, 48)
                )[0];

            GameSprites.TurretCannon = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("NewTurretMount1CannonStyle"),
                new Rectangle(52, 9, 26, 55),
                new Rectangle(26, 9, 26, 55),
                new Rectangle(0, 9, 26, 55)
                );

            GameSprites.TurretShotgun = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("NewShotgunTurret1"),
                new Rectangle(36, 3, 36, 61),
                new Rectangle(72, 3, 36, 61),
                new Rectangle(0, 3, 36, 61)
                );

            GameSprites.TurretSniper = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("NewSniperTurret2"),
                new Rectangle(45, 4, 45, 60),
                new Rectangle(90, 4, 45, 60),
                new Rectangle(0, 4, 45, 60)
                );

            GameSprites.Laser = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("LaserTex"),
                new Rectangle(11, 1, 10, 30))[0];

            GameSprites.MenuBackground = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("MainMenu512x512"), 
                new Rectangle(0, 0, 512, 512))[0];

            GameSprites.SpaceBackground = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("SpaceBackdrop800x800"),
                new Rectangle(0, 0, 800, 800))[0];

            GameSprites.SpaceBackgroundUpdated = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("spacebackgroundupdated"),
                new Rectangle(0, 0, 1000, 1000))[0];

            GameSprites.Warning = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("IncomingWarning"),
                new Rectangle(65, 8, 32, 32))[0];

            GameSprites.SmallAsteroid = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("Asteroid16x16"),
                new Rectangle(0, 0, 16, 16))[0];

            GameSprites.LargeAsteroid = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("Asteroid32x32"),
                new Rectangle(0, 0, 32, 32))[0];

            GameSprites.SmallExplosion = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("explosion2"),
                new Rectangle(1, 1, 20, 20))[0];

            GameSprites.Health = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("HealthTex"),
                new Rectangle(0, 0, 32, 32))[0];

            GameSprites.CircleOverlay = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("nebulacolor"),
                new Rectangle(0, 0, 726,726))[0];

            GameSprites.RepairCharge = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("ChargeTex"),
                new Rectangle(2, 0, 12, 16))[0];

            var font = new Font();
            font.AddSize(12, Content.Load<SpriteFont>("Fonts/Font12"));
            font.AddSize(24, Content.Load<SpriteFont>("Fonts/Font24"));
            font.AddSize(36, Content.Load<SpriteFont>("Fonts/Font36"));
            font.AddSize(48, Content.Load<SpriteFont>("Fonts/Font48"));
            font.AddSize(56, Content.Load<SpriteFont>("Fonts/Font56"));
            font.AddSize(72, Content.Load<SpriteFont>("Fonts/Font72"));
            font.AddSize(128, Content.Load<SpriteFont>("Fonts/Font128"));
            font.AddSize(80, Content.Load<SpriteFont>("Fonts/ChakraTitle"));
            GameSprites.Font = font;
            
            //Load Sprites Here
            spriteAtlas.Compact();
            renderPipeline.SpriteAtlas = spriteAtlas.AtlasTextures;

            GameSprites.Init();
        }

        private void LoadEffects()
        {
            var custom = Content.Load<Effect>("Custom");
            custom.Parameters["Color"].SetValue(Color.White.ToVector4() * 0.25f);
            GameEffects.Custom = custom;

            custom = custom.Clone();
            custom.Parameters["Color"].SetValue(Color.Red.ToVector4() * 0.25f);
            GameEffects.CustomRed = custom;

            var shield = Content.Load<Effect>("Shield");
            //custom.Parameters["Color"].SetValue(Color.White.ToVector4() * 0.25f);
            GameEffects.Shield = shield;

            var dss = new DepthStencilState();
            GameEffects.DSS = dss;
        } 

        private void CreateScene()
        {
            hierarchyGame = new Hierarchy();

            gameCamera = new Camera() { ViewSize = MapRadius + 4f };
            hierarchyGame.AddObject(gameCamera);

            gameEnded = true;

            //Create initial scene here
            /*DrawableObject backgroundObject = new DrawableObject(new Color(0.1f, 0.1f, 0.3f, 0.25f), -2f);
            hierarchyGame.AddObject(backgroundObject);
            backgroundObject.Sprite = GameSprites.SpaceBackground;
            backgroundObject.Transform.GlobalPosition = new Vector2(0, 0);
            backgroundObject.Transform.LocalScale = new Vector2(150, 150);*/

            var background = new DrawableObject(Color.White * 0.3f, -3f); //new Color(19, 18, 51)
            background.Sprite = GameSprites.SpaceBackgroundUpdated;
            background.Transform.LocalScale = new Vector2(136f * 1.5f);
            //background.Transform.LocalRotation = -1f;
            hierarchyGame.AddObject(background);

            var animatedBackground = new StarryBackground(Color.Transparent, 0.5f, GameSprites.DiamondStar, -2f, 180, 6f, 0.5f);
            hierarchyGame.AddObject(animatedBackground);

            Planet = new PlanetObject(world, renderPipeline);
            hierarchyGame.AddObject(Planet);
            Planet.game = this;
            //AdditionalResettables.Add(Planet);

            Spaceship = new SpaceshipObject(world, 0f);
            Spaceship.ThrustMultiplier = 84f;
            hierarchyGame.AddObject(Spaceship);
            //AdditionalResettables.Add(Spaceship);
    
            asteroidManager = new AsteroidManager(world, hierarchyGame, Planet);
            asteroidManager.game = this;
            AdditionalResettables.Add(asteroidManager);

            musicManager = new MusicManager();
            AdditionalResettables.Add(musicManager);

            cameraManager = new CameraManager(gameCamera);
            AdditionalResettables.Add(cameraManager);

            var turret00 = new TurretStation(world, asteroidManager, renderPipeline);
            turret00.SetSprites(GameSprites.TurretCannon, GameSprites.TurretCannonSizes, new Vector2(0f, 17f) / GameSprites.pixelsPerUnit);
            turret00.Transform.LocalRotation = MathHelper.ToRadians(270f);
            turret00.SetStartingPosition(new Vector2(32f, 0f));
            //AdditionalResettables.Add(turret00);
            hierarchyGame.AddObject(turret00);
            Turrets.Add(turret00);

            var turret10 = new SniperTurret(world, asteroidManager, renderPipeline);
            turret10.SetSprites(GameSprites.TurretSniper, GameSprites.TurretSniperSizes, new Vector2(-6f, 12f) / GameSprites.pixelsPerUnit);
            turret10.SetStartingPosition(new Vector2(0f, 32f));
            //AdditionalResettables.Add(turret10);
            hierarchyGame.AddObject(turret10);
            Turrets.Add(turret10);

            var turret01 = new ShotgunTurret(world, asteroidManager, renderPipeline, 1f);
            turret01.SetSprites(GameSprites.TurretShotgun, GameSprites.TurretShotgunSizes, new Vector2(0f, 12f) / GameSprites.pixelsPerUnit);
            turret01.RangeRadius = 24f;
            turret01.Transform.LocalRotation = MathHelper.ToRadians(90f);
            turret01.SetStartingPosition(new Vector2(-32f, 0f));
            //AdditionalResettables.Add(turret01);
            hierarchyGame.AddObject(turret01);
            Turrets.Add(turret01);
        }

        private void CreateUI()
        {
            hierarchyUI = new Hierarchy();

            uiCamera = new Camera() { ViewSize = MapRadius + 4f };
            hierarchyUI.AddObject(uiCamera);

            var boost = new Bar(() => Spaceship.DisplayedBoost, Color.Green, Color.Red, Color.Transparent);
            boost.Transform.LocalPosition = new Vector2(-50f, 63f);
            boost.Transform.LocalScale = Vector2.One * 5f;
            hierarchyUI.AddObject(boost);

            var boostText = new TextObject();
            boostText.Transform.GlobalPosition = new Vector2(-51f, 58f);
            boostText.Color = Color.White;
            boostText.FontSize = 12;
            boostText.Text = "boost";
            Spaceship.BoostUpdated += (boostLeft) =>
            {
                if (boostLeft > 0)
                {
                    boostText.Transform.GlobalPosition = new Vector2(-51f, 58f);
                    boostText.Text = "boost";
                }
                else
                {
                    boostText.Transform.GlobalPosition = new Vector2(-48f, 58f);
                    boostText.Text = "overloaded!";
                }
            };
            hierarchyUI.AddObject(boostText);

            pointsText = new TextObject();
            pointsText.Transform.GlobalPosition = new Vector2(60f, 60f);
            pointsText.Color = Color.White;
            pointsText.FontSize = 36;
            asteroidManager.PointsUpdated += (pointCount) =>
            {
                if (asteroidManager.WaveNumber > 1)
                    pointsText.Text = $"{pointCount / 100}";

                // Try to align text so that it doesn't overflow off screen
                if (pointsText != null && !string.IsNullOrEmpty(pointsText.Text))
                {
                    pointsText.Transform.GlobalPosition = new Vector2(61f - pointsText.Text.Length, 60f);
                }
            };
            hierarchyUI.AddObject(pointsText);

            wavesText = new TextObject();
            wavesText.Transform.GlobalPosition = new Vector2(56f, -62f);
            wavesText.Color = Color.White;
            wavesText.FontSize = 24;
            asteroidManager.WavesUpdated += (waveCount) =>
            {
                if (waveCount > 1)
                    wavesText.Text = $"Wave {waveCount - 1}";

                // Try to align text so that it doesn't overflow off screen
                if (wavesText != null && !string.IsNullOrEmpty(wavesText.Text))
                {
                    wavesText.Transform.GlobalPosition = new Vector2(62f - wavesText.Text.Length, -62f);
                }
            };
            hierarchyUI.AddObject(wavesText);

            /*var health = new MultiIconDisplay(GameSprites.Health, 5, 0.5f, 4f, 1f);
            health.Transform.LocalPosition = new Vector2(-64f, 64f);
            Planet.HealthChange += health.UpdateCount;
            Planet.ModifyHealth(0);
            hierarchyUI.AddObject(health);
            */

            //DrawableObject overlayObject = new DrawableObject(new Color(0.3f, 0.15f, 0.15f, 1.0f), -1f);
            DrawableObject overlayObject = new DrawableObject(new Color(100, 50, 50) * 0.95f, -1f);
            hierarchyUI.AddObject(overlayObject);
            overlayObject.Sprite = GameSprites.CircleOverlay;
            overlayObject.Transform.GlobalPosition = new Vector2(0, 0);
            overlayObject.Transform.LocalScale = new Vector2(136, 136);
        }

        private void CreateMenuScene()
        {
            hierarchyMenu = new Hierarchy();

            /*
            //obs recording menu
            var background = new DrawableObject(Color.White * 0.3f, -1f); //new Color(19, 18, 51)
            background.Sprite = GameSprites.SpaceBackground;
            background.Transform.LocalScale = new Vector2(136f * 1f);
            //background.Transform.LocalRotation = -1f;
            hierarchyMenu.AddObject(background);

            var starryBackground = new StarryBackground(Color.Transparent, 1f, GameSprites.DiamondStar, 0f, 40, 20f, 1);
            hierarchyMenu.AddObject(starryBackground);

            var gameTitle = new TextObject();
            gameTitle.Transform.GlobalPosition = new Vector2(2, 0);
            gameTitle.Color = Color.LightBlue;
            gameTitle.FontSize = 80;
            gameTitle.Text = "Planet Gloop";
            hierarchyMenu.AddObject(gameTitle);

            hierarchyPressEnter = new Hierarchy();
            hierarchyPaused = new Hierarchy();*/
            
            //original menu
            var background = new DrawableObject(Color.White * 0.3f, -1f); //new Color(19, 18, 51)
            background.Sprite = GameSprites.SpaceBackgroundUpdated;
            background.Transform.LocalScale = new Vector2(136f * 1.5f);
            //background.Transform.LocalRotation = -1f;
            hierarchyMenu.AddObject(background);

            var starryBackground = new StarryBackground(Color.Transparent, 1f, GameSprites.DiamondStar, 0f, 180, 6f, 1);
            hierarchyMenu.AddObject(starryBackground);

            var gameTitle = new TextObject();
            gameTitle.Transform.GlobalPosition = new Vector2(2, 37);
            gameTitle.Color = Color.LightBlue;
            gameTitle.FontSize = 80;
            gameTitle.Text = "Planet Gloop";
            hierarchyMenu.AddObject(gameTitle);

            var controlsText = new TextObject();
            controlsText.Transform.GlobalPosition = new Vector2(-8, -37);
            controlsText.Color = Color.White;
            controlsText.FontSize = 24;
            controlsText.Text = "Controls:\n" +
                "w/s - move spaceship forwards/backwards\n" +
                "a/d - rotate spaceship left/right\n" +
                "shift - boost spaceship\n" +
                "spacebar - drag turret & display turret range\n" +
                "esc - pause game\n" +
                "x - exit game\n\n";
            hierarchyMenu.AddObject(controlsText);

            var footnote = new TextObject();
            footnote.Transform.GlobalPosition = new Vector2(-12, -60);
            footnote.Color = new Color(120, 120, 120);
            footnote.FontSize = 24;
            footnote.Text = "Made with C# and Monogame in 5 days\n" +
                "for Spelkollektivet Halloween Gamejam 2023";
            hierarchyMenu.AddObject(footnote);

            hierarchyPressEnter = new Hierarchy();

            var pressEnterText = new TextObjectTM(Color.White, Color.White * 0.5f, 0.25f);
            pressEnterText.Transform.GlobalPosition = new Vector2(0, 8);
            pressEnterText.Color = Color.White;
            pressEnterText.FontSize = 48;
            pressEnterText.Text = "Press [Enter] to play";
            hierarchyPressEnter.AddObject(pressEnterText);

            hierarchyPaused = new Hierarchy();

            var gamePausedText = new TextObjectTM(Color.White, Color.White * 0.5f, 0.8f);
            gamePausedText.Transform.GlobalPosition = new Vector2(0, 8);
            gamePausedText.Color = Color.White;
            gamePausedText.FontSize = 48;
            gamePausedText.Text = "[Game Paused]";
            hierarchyPaused.AddObject(gamePausedText);
        }

        private void CreateGameOverScene()
        {
            hierarchyGameOver = new Hierarchy();

            var accent = new DrawableObject(Color.Red, 2f);
            accent.Transform.LocalScale = new Vector2(136f, 136f);
            accent.Sprite = GameSprites.NullSprite;
            hierarchyGameOver.AddObject(accent);

            var background = new DrawableObject(Color.Black, 2f);
            background.Transform.LocalScale = new Vector2(128f, 128f);
            background.Sprite = GameSprites.NullSprite;
            hierarchyGameOver.AddObject(background);

            var gameOverText = new TextObject();
            gameOverText.Transform.GlobalPosition = new Vector2(0, 0);
            gameOverText.Color = Color.White;
            gameOverText.FontSize = 72;
            gameOverText.Text = "Game Over";
            hierarchyGameOver.AddObject(gameOverText);

            var resetText = new TextObjectTM(Color.White, Color.White * 0.5f, 0.25f);
            resetText.Transform.GlobalPosition = new Vector2(0, -40);
            resetText.Color = Color.White;
            resetText.FontSize = 36;
            resetText.Text = "Press R to reset";
            hierarchyGameOver.AddObject(resetText);

            var scoreText = new TextObject();
            scoreText.Transform.GlobalPosition = new Vector2(0, -20);
            scoreText.Color = Color.White;
            scoreText.FontSize = 36;
            //scoreText.Text = "Your score is: " + ;
            asteroidManager.PointsUpdated += (pointCount) =>
            {
                scoreText.Text = "Your score is: " + $"{pointCount / 100}";
            };
            hierarchyGameOver.AddObject(scoreText);
        }

        private void CreateWorld()
        {
            world = new World(new Vector2(0f, 0f));
            physicsDebug = new DebugView(world);

            physicsDebug.LoadContent(GraphicsDevice, Content);
        }

        private void CreateBindings()
        {
            accelerate = inputManager.CreateSimpleKeysBinding("accelerate", new Keys[2] { Keys.W, Keys.Up });
            decelerate = inputManager.CreateSimpleKeysBinding("decelerate", new Keys[2] { Keys.S, Keys.Down });
            rotLeft = inputManager.CreateSimpleKeysBinding("rotLeft", new Keys[2] { Keys.A, Keys.Left });
            rotRight = inputManager.CreateSimpleKeysBinding("rotRight", new Keys[2] { Keys.D, Keys.Right });
            
            var toggleDrag = inputManager.CreateSimpleKeysBinding("toggleDrag", new Keys[1] { Keys.Space });
            var restart = inputManager.CreateSimpleKeysBinding("restart", new Keys[1] { Keys.R });
            var boost = inputManager.CreateSimpleKeysBinding("boost", new Keys[2] { Keys.LeftShift, Keys.RightShift });
            var confirm = inputManager.CreateSimpleKeysBinding("confirm", new Keys[1] { Keys.Y });
            var cancel = inputManager.CreateSimpleKeysBinding("cancel", new Keys[1] { Keys.N });
            var pauseGame = inputManager.CreateSimpleKeysBinding("pauseGame", new Keys[1] { Keys.Escape });
            var exitGame = inputManager.CreateSimpleKeysBinding("exitGame", new Keys[1] { Keys.X });
            var enterInput = inputManager.CreateSimpleKeysBinding("confirm", new Keys[1] { Keys.Enter });

            ThrusterBinding(accelerate, 0, 1);
            ThrusterBinding(decelerate, 2, 3);
            ThrusterBinding(rotLeft, 1, 2);
            ThrusterBinding(rotRight, 0, 3);
            ThrusterBoostBinding(boost, 0, 1);

            toggleDrag.Started += (_) =>
            {
                if (menuDisplayed && gameEnded)
                {
                    //StartGame();
                    /*
                    menuDisplayed = !menuDisplayed;

                    asteroidManager.Enabled = !menuDisplayed;
                    */
                }
                else
                {
                    Spaceship.TryInitDragging(10f, 15f);
                }
            };

            restart.Started += (_) =>
            {
                Restart();
            };

            pauseGame.Started += (_) =>
            {
                TogglePause();

                /*
                menuDisplayed = !menuDisplayed;

                asteroidManager.Enabled = !menuDisplayed;

                Console.WriteLine("menuDisplayed is: " + menuDisplayed);
                */
            };

            enterInput.Started += (_) =>
            {
                if (menuDisplayed && gameEnded)
                {
                    StartGame();
                    ++enterKeyEnteredCounter;
                    /*
                    menuDisplayed = !menuDisplayed;

                    asteroidManager.Enabled = !menuDisplayed;
                    */
                }
            };
        }

        private void CreateUpdateables()
        {
            Components.Add(new BoundryFieldComponent(MapRadius - 2f, 32f, PlanetRadius + 6f, 64f, Spaceship,  Turrets[0], Turrets[1], Turrets[2]));
            
            Components.Add(asteroidManager);

            Components.Add(musicManager);

            Components.Add(cameraManager);
        }

        private void ThrusterBinding(IInput input, int one, int two)
        {
            input.Started += (input) =>
            {
                Spaceship.IncrementThruster(one);
                Spaceship.IncrementThruster(two);
            };

            input.Canceled += (input) =>
            {
                Spaceship.DecrementThruster(one);
                Spaceship.DecrementThruster(two);
            };
        }

        private void ThrusterBoostBinding(IInput input, int one, int two)
        {
            input.Started += (input) =>
            {
                Spaceship.BoostThruster(one);
                Spaceship.BoostThruster(two);
            };

            input.Canceled += (input) =>
            {
                Spaceship.DisableBoost(one);
                Spaceship.DisableBoost(two);
            };
        }

        public void StartGame()
        {
            if (!gameEnded)
            {
                return;
            }

            pointsText.Text = "";

            wavesText.Text = "";

            gameEnded = false;

            menuDisplayed = false;

            asteroidManager.Enabled = true;

            Console.WriteLine("Game started!");

            foreach (var resettable in hierarchyGame.AllInstancesOf<IResettable>())
            {
                resettable.Reset();
            }

            foreach (IResettable resettable in AdditionalResettables)
            {
                resettable.Reset();
            }
        }
        
        public void EndGame()
        {
            if (gameEnded)
            {
                return;
            }

            Console.WriteLine("Game ended!");

            foreach (var resettable in hierarchyGame.AllInstancesOf<IResettable>())
            {
                resettable.OnGameEnd();
            }


            foreach (IResettable resettable in AdditionalResettables)
            {
                resettable.OnGameEnd();
            }

            gameEnded = true;
        }

        public void Restart()
        {
            // Don't restart unless the game has ended or is paused
            if (!gameEnded)
                return;

            StartGame();
        }

        public void TogglePause()
        {
            if (!gameEnded)
            {
                menuDisplayed = !menuDisplayed;

                asteroidManager.Enabled = !menuDisplayed;
            }
        }
    }
}