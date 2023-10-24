using GlobalLoopGame.Spaceship;
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
using GlobalLoopGame.UI;

namespace GlobalLoopGame
{
    public class GlobalLoopGame : Game
    {
        const float MapRadius = 64f;
        const float PlanetRadius = 12f;

        private GraphicsDeviceManager _graphics;

        private RenderPipeline renderPipeline;
        private World world;
        private Hierarchy hierarchyGame;
        private Hierarchy hierarchyUI;
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

            CreateUI();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            GameTime = gameTime;

            inputManager.UpdateState();

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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.MidnightBlue);

            renderPipeline.RenderScene(hierarchyGame, camera);
            renderPipeline.RenderScene(hierarchyUI, camera);

            base.Draw(gameTime);
        }

        private void LoadSounds()
        {
            //Load sounds and songs here
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
            hierarchyGame = new Hierarchy();
            camera = new Camera() { ViewSize = MapRadius + 4f };
            hierarchyGame.AddObject(camera);

            //Create initial scene here
            Planet = new PlanetObject(world);
            hierarchyGame.AddObject(Planet);
            Planet.game = this;
            Resettables.Add(Planet);

            Spaceship = new SpaceshipObject(world, 0f);
            Spaceship.ThrustMultiplier = 64f;
            hierarchyGame.AddObject(Spaceship);
            Resettables.Add(Spaceship);
    
            asteroidManager = new AsteroidManager(world, hierarchyGame);
            Resettables.Add(asteroidManager);

            var turret00 = new TurretStation(world, asteroidManager);
            turret00.SetStartingPosition(new Vector2(0f, 25f));
            Resettables.Add(turret00);
            hierarchyGame.AddObject(turret00);
            //turret00.Transform.LocalPosition = new Vector2(0f, 25f);

            var turret10 = new TurretStation(world, asteroidManager);
            turret10.SetStartingPosition(new Vector2(22f, -22f));
            Resettables.Add(turret10);
            hierarchyGame.AddObject(turret10);
            //turret10.Transform.LocalPosition = new Vector2(22f, -22f);

            var turret01 = new TurretStation(world, asteroidManager);
            turret01.SetStartingPosition(new Vector2(-23f, -23f));
            Resettables.Add(turret01);
            hierarchyGame.AddObject(turret01);

            //turret01.Transform.LocalPosition = new Vector2(-23f, -23f);

            //var turret11 = new TurretStation(world, asteroidManager);
            //turret11.Transform.LocalPosition = new Vector2(20f, 20f);

            //hierarchy.AddObject(turret11);

            StartGame();
        }

        private void CreateUI()
        {
            hierarchyUI = new Hierarchy();
            var boost = new Bar(() => Spaceship.BoostLeft, Color.Green, Color.Red);
            boost.Transform.LocalPosition = new Vector2(-64f, 64f);

            hierarchyUI.AddObject(boost);
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
            ThrusterBoostBinding(boost, 0, 1);

            toggleDrag.Started += (_) =>
            {
                Spaceship.TryInitDragging(10f, 15f);
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