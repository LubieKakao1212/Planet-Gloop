﻿using GlobalLoopGame.Spaceship;
using GlobalLoopGame.Spaceship.Dragging;
using GlobalLoopGame.Updaters;
using GlobalLoopGame.Asteroid;
using GlobalLoopGame.Planet;
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
using static System.Formats.Asn1.AsnWriter;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace GlobalLoopGame
{
    public class GlobalLoopGame : Game
    {
        const float MapRadius = 64f;
        const float PlanetRadius = 12f;

        private GraphicsDeviceManager _graphics;

        private RenderPipeline renderPipeline;
        private World world;
        private Hierarchy hierarchy;
        private InputManager inputManager;
        private Camera camera;
        private SpriteAtlas<Color> spriteAtlas;

        private GameTime GameTime;

        private bool gameEnded = true;

        public AsteroidManager asteroidManager { get; private set; }

        public SpaceshipObject Spaceship { get; private set; }
        public PlanetObject Planet { get; private set; }

        public List<IResettable> Resettables { get; private set; } = new List<IResettable>();

        public GlobalLoopGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1000;
            _graphics.PreferredBackBufferHeight = 1000;
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

            LoadSounds();

            LoadSprites();

            CreateWorld();

            CreateScene();

            CreateBindings();

            CreateUpdateables();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            GameTime = gameTime;

            inputManager.UpdateState();

            world.Step(gameTime.ElapsedGameTime);

            hierarchy.BeginUpdate();
            foreach (var updatable in hierarchy.OrderedInstancesOf<IUpdatable>())
            {
                updatable.Update(gameTime);
            }
            hierarchy.EndUpdate();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.MidnightBlue);

            renderPipeline.RenderScene(hierarchy, camera);

            base.Draw(gameTime);
        }

        private void LoadSounds()
        {
            //Load sounds and songs here

            GameSounds.asteroidDeathSound = Content.Load<SoundEffect>("Sounds/AsteroidHurt");
            GameSounds.dropTurretSound = Content.Load<SoundEffect>("Sounds/DropTurret");
            GameSounds.magnetSound = Content.Load<SoundEffect>("Sounds/Magnet");
            GameSounds.pickupTurretSound = Content.Load<SoundEffect>("Sounds/PickupTurret");
            GameSounds.planetHurtSound = Content.Load<SoundEffect>("Sounds/PlanetHurt");
            GameSounds.playerHurtSound = Content.Load<SoundEffect>("Sounds/PlayerHurt");
            GameSounds.sideThrustSound = Content.Load<SoundEffect>("Sounds/SideThrust");
            GameSounds.thrusterSound = Content.Load<SoundEffect>("Sounds/Thruster");
            GameSounds.warningSound = Content.Load<SoundEffect>("Sounds/Warning");

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
        }

        private void LoadSprites()
        {
            spriteAtlas = new SpriteAtlas<Color>(GraphicsDevice, 2048);

            var white = new Texture2D(GraphicsDevice, 1, 1);
            white.SetData(new Color[] { Color.White });
            GameSprites.NullSprite = spriteAtlas.AddTextureRects(white, new Rectangle(0, 0, 1, 1))[0];
            GameSprites.Planet = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("PlanetTex"), new Rectangle(0, 0, 128, 128))[0];

            var spaceshipTextures = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("SpaceshipTex"),
                new Rectangle(0, 6, 32, 20),
                new Rectangle(36, 17, 12, 18),
                new Rectangle(35, 8, 6, 6)
                );

            GameSprites.SpaceshipBody = spaceshipTextures[0];
            GameSprites.SpaceshipMagnet = spaceshipTextures[1];
            GameSprites.SpaceshipThrusterFrames = new Sprite[] { spaceshipTextures[2] };

            GameSprites.TurretBase = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("TurretPlatformTex"),
                new Rectangle(0, 0, 48, 48)
                )[0];

            GameSprites.TurretCannon = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("CannonTex"),
                new Rectangle(3, 4, 26, 54),
                new Rectangle(32, 20, 26, 38)
                );

            GameSprites.Laser = spriteAtlas.AddTextureRects(Content.Load<Texture2D>("LaserTex"),
                new Rectangle(1, 0, 4, 27))[0];

            //Load Sprites Here
            spriteAtlas.Compact();
            renderPipeline.SpriteAtlas = spriteAtlas.AtlasTextures;

            GameSprites.Init();
        }

        private void CreateScene()
        {
            hierarchy = new Hierarchy();
            camera = new Camera() { ViewSize = MapRadius + 4f };
            hierarchy.AddObject(camera);

            //Create initial scene here
            Planet = new PlanetObject(world);
            hierarchy.AddObject(Planet);
            Planet.game = this;
            Resettables.Add(Planet);

            Spaceship = new SpaceshipObject(world, 0f);
            Spaceship.ThrustMultiplier = 96f;
            hierarchy.AddObject(Spaceship);
            Resettables.Add(Spaceship);
    
            asteroidManager = new AsteroidManager(world, hierarchy);
            asteroidManager.game = this;
            Resettables.Add(asteroidManager);

            var turret00 = new TurretStation(world, asteroidManager);
            turret00.SetStartingPosition(new Vector2(0f, 25f));
            Resettables.Add(turret00);
            hierarchy.AddObject(turret00);

            var turret10 = new TurretStation(world, asteroidManager);
            turret10.SetStartingPosition(new Vector2(22f, -22f));
            Resettables.Add(turret10);
            hierarchy.AddObject(turret10);

            var turret01 = new TurretStation(world, asteroidManager);
            turret01.SetStartingPosition(new Vector2(-23f, -23f));
            Resettables.Add(turret01);
            hierarchy.AddObject(turret01);

            StartGame();
        }

        private void CreateWorld()
        {
            world = new World(new Vector2(0f, 0f));
        }

        private void CreateBindings()
        {
            var accelerate = inputManager.CreateSimpleKeysBinding("accelerate", new Keys[2] { Keys.W, Keys.Up });
            var decelerate = inputManager.CreateSimpleKeysBinding("decelerate", new Keys[2] { Keys.S, Keys.Down });
            var rotLeft = inputManager.CreateSimpleKeysBinding("rotLeft", new Keys[2] { Keys.A, Keys.Left });
            var rotRight = inputManager.CreateSimpleKeysBinding("rotRight", new Keys[2] { Keys.D, Keys.Right });
            
            var toggleDrag = inputManager.CreateSimpleKeysBinding("toggleDrag", new Keys[1] { Keys.Space });
            var restart = inputManager.CreateSimpleKeysBinding("restart", new Keys[1] { Keys.R });
            var boost = inputManager.CreateSimpleKeysBinding("boost", new Keys[2] { Keys.LeftShift, Keys.RightShift });
            var confirm = inputManager.CreateSimpleKeysBinding("confirm", new Keys[1] { Keys.Enter });
            var cancel = inputManager.CreateSimpleKeysBinding("cancel", new Keys[1] { Keys.Escape });

            ThrusterBinding(accelerate, 0, 1);
            ThrusterBinding(decelerate, 2, 3);
            ThrusterBinding(rotLeft, 1, 2);
            ThrusterBinding(rotRight, 0, 3);

            toggleDrag.Started += (_) =>
            {
                Spaceship.TryInitDragging(5f, 10f);
            };

            restart.Started += (_) =>
            {
                Restart();
            };
        }

        private void CreateUpdateables()
        {
            Components.Add(new BoundryFieldComponent(MapRadius, 16f, PlanetRadius + 6f, 32f, Spaceship));
            Components.Add(asteroidManager);
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
            if (!gameEnded)
            {
                EndGame();
            }

            StartGame();
        }
    }
}