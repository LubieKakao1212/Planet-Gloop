﻿using GlobalLoopGame.Spaceship;
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
        private Hierarchy hierarchyMenu;
        private Hierarchy hierarchyGame;
        private Hierarchy hierarchyUI;
        private Hierarchy hierarchyGameOver;
        private Hierarchy hierarchyModalBox;
        private InputManager inputManager;
        private Camera camera;
        private SpriteAtlas<Color> spriteAtlas;

        private SpriteBatch textRenderer;

        private GameTime GameTime;

        private bool gameEnded = true;
        private bool menuDisplayed = true;
        private bool modalBoxActive = false;
        public AsteroidManager asteroidManager { get; private set; }
        public MusicManager musicManager { get; private set; }

        public SpaceshipObject Spaceship { get; private set; }
        public PlanetObject Planet { get; private set; }
        public List<TurretStation> Turrets { get; private set; } = new List<TurretStation>();

        public List<IResettable> Resettables { get; private set; } = new List<IResettable>();

        CompoundAxixBindingInput accelerate, decelerate, rotLeft, rotRight;

        public GlobalLoopGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = WindowSize;
            _graphics.PreferredBackBufferHeight = WindowSize;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
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
            CreateModalBox();

            CreateBindings();

            CreateUpdateables();

            CreateUI();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            GameTime = gameTime;

            inputManager.UpdateState();

            if (!menuDisplayed)
            {
                world.Step(gameTime.ElapsedGameTime);
                hierarchyGame.BeginUpdate();
                foreach (var updatable in hierarchyGame.OrderedInstancesOf<IUpdatable>())
                {
                    updatable.Update(gameTime);
                }
                hierarchyGame.EndUpdate();

                hierarchyUI.BeginUpdate();
                foreach (var updatable in hierarchyUI.OrderedInstancesOf<IUpdatable>())
                {
                    updatable.Update(gameTime);
                }
                hierarchyUI.EndUpdate();

            }
            else
            {
                hierarchyMenu.BeginUpdate();
                foreach (var updatable in hierarchyMenu.OrderedInstancesOf<IUpdatable>())
                {
                    updatable.Update(gameTime);
                }
                hierarchyMenu.EndUpdate();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!menuDisplayed)
            {
                GraphicsDevice.Clear(new Color(0.1f, 0.1f, 0.1f, 1.0f));
                renderPipeline.RenderScene(hierarchyGame, camera);
                renderPipeline.RenderScene(hierarchyUI, camera);

                textRenderer.DrawAllText(hierarchyGame, GameSprites.Font, camera);
                textRenderer.DrawAllText(hierarchyUI, GameSprites.Font, camera);

                if (gameEnded)
                {
                    GraphicsDevice.Clear(Color.DarkRed);
                    renderPipeline.RenderScene(hierarchyGameOver, camera);
                    textRenderer.DrawAllText(hierarchyGameOver, GameSprites.Font, camera);
                }
            }
            else
            {
                GraphicsDevice.Clear(Color.DarkGoldenrod);
                renderPipeline.RenderScene(hierarchyMenu, camera);
            }


            if (modalBoxActive)
            {
                renderPipeline.RenderScene(hierarchyModalBox, camera);
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

            GameSounds.musicIntensityOne = Content.Load<SoundEffect>("Sounds/Music/Song0");
            GameSounds.musicIntensityTwo = Content.Load<SoundEffect>("Sounds/Music/Song1");
            GameSounds.musicIntensityThree = Content.Load<SoundEffect>("Sounds/Music/Song2");

            GameSounds.magnetEmitter = GameSounds.magnetSound.CreateInstance();
            GameSounds.magnetEmitter.IsLooped = true;
            GameSounds.magnetEmitter.Pause();

            GameSounds.thrusterEmitter = GameSounds.thrusterSound.CreateInstance();
            GameSounds.thrusterEmitter.IsLooped = true;
            GameSounds.thrusterEmitter.Volume = 0.1f;
            GameSounds.thrusterEmitter.Pause();

            GameSounds.sideThrusterEmitter = GameSounds.sideThrustSound.CreateInstance();
            GameSounds.sideThrusterEmitter.IsLooped = true;
            GameSounds.sideThrusterEmitter.Volume = 0.1f;
            GameSounds.sideThrusterEmitter.Pause();

            GameSounds.boostEmitter = GameSounds.boostSound.CreateInstance();
            GameSounds.boostEmitter.IsLooped = true;
            GameSounds.boostEmitter.Volume = 0.1f;
            GameSounds.boostEmitter.Pause();

            GameSounds.boostChargingEmitter = GameSounds.boostChargingSound.CreateInstance();
            GameSounds.boostChargingEmitter.IsLooped = true;
            GameSounds.boostChargingEmitter.Volume = 1.0f;
            GameSounds.boostChargingEmitter.Pause();

            GameSounds.firstMusicInstance = GameSounds.musicIntensityOne.CreateInstance();
            GameSounds.firstMusicInstance.IsLooped = true;
            GameSounds.firstMusicInstance.Volume = 0f;
            GameSounds.firstMusicInstance.Play();

            GameSounds.secondMusicInstance = GameSounds.musicIntensityTwo.CreateInstance();
            GameSounds.secondMusicInstance.IsLooped = true;
            GameSounds.secondMusicInstance.Volume = 0f;
            GameSounds.secondMusicInstance.Play();

            GameSounds.thirdMusicInstance = GameSounds.musicIntensityThree.CreateInstance();
            GameSounds.thirdMusicInstance.IsLooped = true;
            GameSounds.thirdMusicInstance.Volume = 0f;
            GameSounds.thirdMusicInstance.Play();
        }

        private void LoadSprites()
        {
            spriteAtlas = new SpriteAtlas<Color>(GraphicsDevice, 2048);

            var white = new Texture2D(GraphicsDevice, 1, 1);
            white.SetData(new Color[] { Color.White });
            GameSprites.NullSprite = spriteAtlas.AddTextureRects(white, new Rectangle(0, 0, 1, 1))[0];
            GameSprites.Circle = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("CircleTex"), new Rectangle(0, 0, 256, 256))[0];

            GameSprites.Planet = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("PlanetTex"), new Rectangle(0, 0, 128, 128))[0];

            var spaceshipTextures = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("SpaceshipTex"),
                new Rectangle(0, 6, 32, 20),
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

            GameSprites.TurretCannon = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("CannonTex"),
                new Rectangle(3, 4, 26, 54),
                new Rectangle(32, 20, 26, 38)
                );

            GameSprites.TurretShotgun = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("ShotgunTex"),
                new Rectangle(15, 1, 36, 62),
                new Rectangle(15, 1, 36, 62)
                );

            GameSprites.TurretSniper = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("SniperTex"),
                new Rectangle(9, 1, 45, 60),
                new Rectangle(9, 1, 45, 60)
                );

            GameSprites.Laser = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("LaserTex"),
                new Rectangle(11, 1, 10, 30))[0];

            GameSprites.MenuBackground = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("MainMenu512x512"), 
                new Rectangle(0, 0, 512, 512))[0];

            GameSprites.SpaceBackground = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("SpaceBackdrop800x800"),
                new Rectangle(0, 0, 800, 800))[0];

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

            GameSprites.CircleOverlay = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("circleoverlay"),
                new Rectangle(0, 0, 726,726))[0];

            var font = new Font();
            font.AddSize(12, Content.Load<SpriteFont>("Fonts/Font12"));
            font.AddSize(36, Content.Load<SpriteFont>("Fonts/Font36"));
            font.AddSize(72, Content.Load<SpriteFont>("Fonts/Font72"));
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

            var dss = new DepthStencilState();
            GameEffects.DSS = dss;
        } 

        private void CreateScene()
        {
            hierarchyGame = new Hierarchy();
            camera = new Camera() { ViewSize = MapRadius + 4f };
            hierarchyGame.AddObject(camera);

            //Create initial scene here
            DrawableObject backgroundObject = new DrawableObject(new Color(0.1f, 0.1f, 0.3f, 0.25f), -2f);
            hierarchyGame.AddObject(backgroundObject);
            backgroundObject.Sprite = GameSprites.SpaceBackground;
            backgroundObject.Transform.GlobalPosition = new Vector2(0, 0);
            backgroundObject.Transform.LocalScale = new Vector2(150, 150);

            Planet = new PlanetObject(world);
            hierarchyGame.AddObject(Planet);
            Planet.game = this;
            Resettables.Add(Planet);

            Spaceship = new SpaceshipObject(world, 0f);
            Spaceship.ThrustMultiplier = 84f;
            hierarchyGame.AddObject(Spaceship);
            Resettables.Add(Spaceship);
    
            asteroidManager = new AsteroidManager(world, hierarchyGame);
            asteroidManager.game = this;
            Resettables.Add(asteroidManager);

            musicManager = new MusicManager();
            Resettables.Add(musicManager);

            var turret00 = new TurretStation(world, asteroidManager, renderPipeline);
            turret00.SetSprites(GameSprites.TurretCannon, GameSprites.TurretCannonSizes, new Vector2(0f, 17f) / GameSprites.pixelsPerUnit);
            turret00.SetStartingPosition(new Vector2(32f, 0f));
            turret00.Transform.LocalRotation = MathHelper.ToRadians(270f);
            Resettables.Add(turret00);
            hierarchyGame.AddObject(turret00);
            Turrets.Add(turret00);

            var turret10 = new SniperTurret(world, asteroidManager, renderPipeline);
            turret10.SetSprites(GameSprites.TurretSniper, GameSprites.TurretSniperSizes, new Vector2(-6f, 12f) / GameSprites.pixelsPerUnit);
            turret10.SetStartingPosition(new Vector2(0f, 32f));
            Resettables.Add(turret10);
            hierarchyGame.AddObject(turret10);
            Turrets.Add(turret10);

            var turret01 = new ShotgunTurret(world, asteroidManager, renderPipeline, 1f);
            turret01.SetSprites(GameSprites.TurretShotgun, GameSprites.TurretShotgunSizes, new Vector2(0f, 12f) / GameSprites.pixelsPerUnit);
            turret01.RangeRadius = 24f;
            turret01.SetStartingPosition(new Vector2(-32f, 0f));
            turret01.Transform.LocalRotation = MathHelper.ToRadians(90f);
            Resettables.Add(turret01);
            hierarchyGame.AddObject(turret01);
            Turrets.Add(turret01);

            StartGame();
        }

        private void CreateUI()
        {
            hierarchyUI = new Hierarchy();
            var boost = new Bar(() => Spaceship.BoostLeft, Color.Green, Color.Red);
            boost.Transform.LocalPosition = new Vector2(-64f, 60f);
            hierarchyUI.AddObject(boost);

            var health = new MultiIconDisplay(GameSprites.Health, 5, 0.5f, 4f, 1f);
            health.Transform.LocalPosition = new Vector2(-64f, 64f);
            Planet.HealthChange += health.UpdateCount;
            Planet.ModifyHealth(0);
            hierarchyUI.AddObject(health);

            DrawableObject overlayObject = new DrawableObject(Color.DarkSlateGray, -1f);
            hierarchyUI.AddObject(overlayObject);
            overlayObject.Sprite = GameSprites.CircleOverlay;
            overlayObject.Transform.GlobalPosition = new Vector2(0, 0);
            overlayObject.Transform.LocalScale = new Vector2(136, 136);
        }

        private void CreateMenuScene()
        {
            hierarchyMenu = new Hierarchy();

            var background = new DrawableObject(Color.White, 1f);
            background.Sprite = GameSprites.MenuBackground;
            background.Transform.LocalScale = new Vector2(128f, 128f);
            hierarchyMenu.AddObject(background);
        }

        private void CreateGameOverScene()
        {
            hierarchyGameOver = new Hierarchy();

            var background = new DrawableObject(Color.Red, 1f);
            background.Transform.LocalScale = new Vector2(128f, 128f);
            hierarchyGameOver.AddObject(background);

            var gameOverMessage = new TextObject();
            gameOverMessage.Transform.LocalPosition = new Vector2(0, 0);
            gameOverMessage.Color = Color.White;
            gameOverMessage.Text = "Game Over";
            gameOverMessage.FontSize = 72;
            hierarchyGameOver.AddObject(gameOverMessage);
        }

        private void CreateModalBox()
        {
            hierarchyModalBox = new Hierarchy();

            var accent = new DrawableObject(Color.White, 3f);
            accent.Transform.LocalScale = new Vector2(120f, 60f);
            hierarchyModalBox.AddObject(accent);
            var background = new DrawableObject(Color.DarkOliveGreen, 2f);
            background.Transform.LocalScale = new Vector2(110f, 55f);
            hierarchyModalBox.AddObject(background);
        }

        private void CreateWorld()
        {
            world = new World(new Vector2(0f, 0f));
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
            var confirm = inputManager.CreateSimpleKeysBinding("confirm", new Keys[1] { Keys.Enter });
            var cancel = inputManager.CreateSimpleKeysBinding("cancel", new Keys[1] { Keys.Escape });

            var playGame = inputManager.CreateSimpleKeysBinding("playGame", new Keys[1] { Keys.P });
            var exitGame = inputManager.CreateSimpleKeysBinding("exitGame", new Keys[1] { Keys.X });
            var debugKey = inputManager.CreateSimpleKeysBinding("debugKey", new Keys[1] { Keys.N });

            ThrusterBinding(accelerate, 0, 1);
            ThrusterBinding(decelerate, 2, 3);
            ThrusterBinding(rotLeft, 1, 2);
            ThrusterBinding(rotRight, 0, 3);
            ThrusterBoostBinding(boost, 0, 1);

            toggleDrag.Started += (_) =>
            {
                Spaceship.TryInitDragging(10f, 15f);
            };

            restart.Started += (_) =>
            {
                Restart();
            };

            playGame.Started += (_) =>
            {
                menuDisplayed = !menuDisplayed;
                asteroidManager.Enabled = !menuDisplayed;
                //Console.WriteLine("menuDisplayed is: " + menuDisplayed);
            };

            exitGame.Started += (_) =>
            {

            };

            debugKey.Started += (_) =>
            {
                modalBoxActive = !modalBoxActive;
            };
        }

        private void CreateUpdateables()
        {
            Components.Add(new BoundryFieldComponent(MapRadius - 2f, 32f, PlanetRadius + 6f, 64f, Spaceship,  Turrets[0], Turrets[1], Turrets[2]));
            
            Components.Add(asteroidManager);

            Components.Add(musicManager);
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

        private void DisplayModalBox()
        {

        }

        public void StartGame()
        {
            if (!gameEnded)
            {
                return;
            }

            gameEnded = false;

            Console.WriteLine("Game started!");

            foreach (IResettable resettable in Resettables)
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

            foreach (IResettable resettable in Resettables)
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
    }
}