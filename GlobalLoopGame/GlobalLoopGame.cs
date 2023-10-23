using GlobalLoopGame.Spaceship;
using GlobalLoopGame.Asteroid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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
        private GraphicsDeviceManager _graphics;

        private RenderPipeline renderPipeline;
        private World world;
        private Hierarchy hierarchy;
        private InputManager inputManager;
        private Camera camera;
        private SpriteAtlas<Color> spriteAtlas;
        private Sprite NullSprite;

        private GameTime GameTime;

        private SpaceshipObject Spaceship;

        private float MapSize = 64f;

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
            camera = new Camera() { ViewSize = MapSize };
            hierarchy.AddObject(camera);

            //Create initial scene here
            Spaceship = new SpaceshipObject(world, 0f);
            Spaceship.ThrustMultiplier = 32f;
            hierarchy.AddObject(Spaceship);

            AsteroidObject firstAsteroid = new AsteroidObject(world, 0f);
            firstAsteroid.InitializeAsteroid(new Vector2(64f, 64f), new Vector2(-8f, -8f), 2f);
            hierarchy.AddObject(firstAsteroid);
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

            ThrusterBinding(accelerate, 0, 1);
            ThrusterBinding(decelerate, 2, 3);
            ThrusterBinding(rotLeft, 1, 2);
            ThrusterBinding(rotRight, 0, 3);
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