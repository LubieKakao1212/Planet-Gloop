using GlobalLoopGame.Spaceship;
using GlobalLoopGame.Spaceship.Dragging;
using GlobalLoopGame.Updaters;
using GlobalLoopGame.Asteroid;
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

namespace GlobalLoopGame
{
    public class GlobalLoopGame : Game
    {
        const float MapSize = 64f;

        private GraphicsDeviceManager _graphics;

        private RenderPipeline renderPipeline;
        private World world;
        private Hierarchy hierarchy;
        private InputManager inputManager;
        private Camera camera;
        private SpriteAtlas<Color> spriteAtlas;
        private Sprite NullSprite;

        private GameTime GameTime;
        public AsteroidManager asteroidManager { get; private set; }
        public SpaceshipObject Spaceship { get; private set; }

        public GlobalLoopGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 800;
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

            foreach (var updatable in hierarchy.OrderedInstancesOf<IUpdatable>())
            {
                updatable.Update(gameTime);
            }
            
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
        }

        private void LoadSprites()
        {
            spriteAtlas = new SpriteAtlas<Color>(GraphicsDevice, 2048);

            var white = new Texture2D(GraphicsDevice, 1, 1);
            white.SetData(new Color[] { Color.White });
            NullSprite = spriteAtlas.AddTextureRects(white, new Rectangle(0, 0, 1, 1))[0];

            //Load Sprites Here

            spriteAtlas.Compact();
            renderPipeline.SpriteAtlas = spriteAtlas.AtlasTextures;
        }

        private void CreateScene()
        {
            hierarchy = new Hierarchy();
            camera = new Camera() { ViewSize = MapSize + 4f };
            hierarchy.AddObject(camera);

            //Create initial scene here
            Spaceship = new SpaceshipObject(world, 0f);
            Spaceship.ThrustMultiplier = 64f;
            hierarchy.AddObject(Spaceship);
    
            asteroidManager = new AsteroidManager(world, hierarchy);

            var turret00 = new TurretStation(world, asteroidManager);
            turret00.Transform.LocalPosition = new Vector2(-10f, -10f);
            
            var turret10 = new TurretStation(world, asteroidManager);
            turret10.Transform.LocalPosition = new Vector2(10f, -10f);
            
            var turret01 = new TurretStation(world, asteroidManager);
            turret01.Transform.LocalPosition = new Vector2(-10f, 10f);
            
            var turret11 = new TurretStation(world, asteroidManager);
            turret11.Transform.LocalPosition = new Vector2(10f, 10f);
            hierarchy.AddObject(turret00);
            hierarchy.AddObject(turret10);
            hierarchy.AddObject(turret01);
            hierarchy.AddObject(turret11);
        }

        private void CreateWorld()
        {
            world = new World(new Vector2(0f, 0f));
        }

        private void CreateBindings()
        {
            var accelerate = inputManager.GetKey(Keys.W);
            var decelerate = inputManager.GetKey(Keys.S);
            var rotLeft = inputManager.GetKey(Keys.A);
            var rotRight = inputManager.GetKey(Keys.D);

            var toggleDrag = inputManager.GetKey(Keys.Space);

            ThrusterBinding(accelerate, 0, 1);
            ThrusterBinding(decelerate, 2, 3);
            ThrusterBinding(rotLeft, 1, 2);
            ThrusterBinding(rotRight, 0, 3);

            toggleDrag.Started += (_) =>
            {
                Spaceship.TryInitDragging(5f, 10f);
            };
        }

        private void CreateUpdateables()
        {
            Components.Add(new BoundryFieldComponent(MapSize, 16f, Spaceship));
            Components.Add(asteroidManager);
            asteroidManager.components = Components;
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
    }

}