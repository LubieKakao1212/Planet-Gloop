using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoEngine.Math;
using MonoEngine.Rendering;
using MonoEngine.Scenes;
using MonoEngine.Tilemap;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EngineTest
{
    public class TestGame : Game
    {
        private GraphicsDeviceManager graphics;

        private RenderPipeline renderer;

        private Hierarchy scene;

        private List<HierarchyObject> Tips = new List<HierarchyObject>();

        private Camera Camera;

        private float CameraSpeed = 1f;
        private float CameraZoomSpeed = 1f;
        private float CameraRotSpeed = MathHelper.ToRadians(90f);

        private float TipRotationSpeed = MathHelper.PiOver2;

        private float bladeLength = 1f;

        private const int WindmillCount = 1;

        private Tilemap tilemap;
        private Grid grid;

        private Stopwatch timer = new Stopwatch();
        private TimeSpan lastFrameStamp;

        private double smoothDelta;

        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 512;
            graphics.PreferredBackBufferHeight = 512;

            /*graphics.PreferredBackBufferWidth = 512;
            graphics.PreferredBackBufferHeight = 512;*/

            //graphics.ToggleFullScreen();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            renderer = new RenderPipeline();

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            renderer.Init(GraphicsDevice);

            Camera = new Camera() { ViewSize = 4 };
            scene = new Hierarchy();

            for (int i = 0; i < WindmillCount; i++) 
            {
                var rot = i / MathHelper.Pi * WindmillCount;
                var mat = Matrix2x2.Rotation(rot);
                Tips.Add(CreateWindmill(scene, rot, Vector2.UnitY * 0f * mat));
            }

            grid = new Grid(Vector2.One);

            scene.AddObject(grid);

            tilemap = new Tilemap();

            //FIllTilemap(tilemap, new Rectangle(-256, -256, 512, 512));
            //FIllTilemap(tilemap, new Rectangle(-128, -128, 256, 256));
            //FIllTilemap(tilemap, new Rectangle(-2, -2, 4, 4));

            //var mapRenderer = new TilemapRenderer(tilemap, grid, renderer, Color.White, -10f);

            //mapRenderer.Parent = grid;

            timer.Start();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Effects.Init(Content);
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            foreach (var windmill in Tips)
            {
                windmill.Transform.LocalRotation += TipRotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            HandleCameraControls(gameTime);
            HandlePlaceControls();

            var t = (float)gameTime.TotalGameTime.TotalSeconds;

            Tiles.OversizedRed.Transform = TransformMatrix.TranslationRotationScale(
                Vector2.Zero, 0f,
                new Vector2(MathF.Cos(t) + 1f, MathF.Sin(t) / 2f + 1f));

            //grid.Transform.LocalScale = new Vector2(
            //    MathF.Cos((float)gameTime.TotalGameTime.TotalSeconds) / 2f + 0.5f,
            //    MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds) / 2f + 0.5f);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            renderer.RenderScene(scene, Camera);

            //Console.WriteLine($"Fps: {1.0 / gameTime.ElapsedGameTime.TotalSeconds}");

            var newStamp = timer.Elapsed;

            var delta = newStamp - lastFrameStamp;

            lastFrameStamp = newStamp;

            smoothDelta = smoothDelta * 0.95 + delta.TotalSeconds * 0.05;

            Console.WriteLine($"Smooth Fps: {1.0 / smoothDelta}");
            Console.WriteLine($"Fps: {1.0 / delta.TotalSeconds}");

            base.Draw(gameTime);
        }

        private HierarchyObject CreateWindmill(Hierarchy hierarchy, float rotation, Vector2 position)
        {
            var root = new HierarchyObject();

            root.Transform.LocalRotation = rotation;
            root.Transform.LocalPosition = position;

            var bar = new DrawableObject(Color.AliceBlue, -1f);
            var bar2 = new DrawableObject(Color.GreenYellow, 0f);
            var origin = new DrawableObject(Color.Black, 0f);
            var blade1 = new DrawableObject(Color.BurlyWood, -0.5f);
            var blade2 = new DrawableObject(Color.BurlyWood, 0f);

            var tip = new DrawableObject(Color.Red, 0f);

            //Hierarchy RotationRoot -> bar -> bar2 -> tip -> blade1, blade2
            blade1.Parent = tip;
            blade2.Parent = tip;
            tip.Parent = bar2;
            bar2.Parent = bar;
            bar.Parent = root;

            bar.Transform.LocalPosition = new Vector2(0f, 0.5f);
            bar2.Transform.LocalPosition = new Vector2(0f, 1f);
            bar2.Transform.LocalScale = new Vector2(1f, 2f);

            blade1.Transform.LocalScale = new Vector2(0.25f, bladeLength);
            blade2.Transform.LocalScale = new Vector2(bladeLength, 0.25f);

            blade1.Transform.LocalPosition = new Vector2(2f, 2f);

            tip.Transform.LocalScale = new Vector2(0.5f, 0.5f);
            origin.Transform.LocalScale = new Vector2(0.1f, 0.1f);

            hierarchy.AddObject(root);

            return tip;
        }

        public void FIllTilemap(Tilemap tilemap, Rectangle bounds)
        {
            for(int x = 0; x < bounds.Width; x++)
                for(int y = 0; y < bounds.Height; y++)
                {
                    tilemap.SetTile(new Point(bounds.X + x, bounds.Y + y), Tiles.bucket[Random.Shared.Next(Tiles.bucket.Length)]);
                }
        }

        private void HandleCameraControls(GameTime gameTime)
        {
            KeyAction(Keys.Up, 
                () => Camera.ViewSize *= (1 + CameraZoomSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds));

            KeyAction(Keys.Down,
                () => Camera.ViewSize *= (1 - CameraZoomSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds));

            var t = Camera.Transform;

            KeyAction(Keys.W,
                () => t.GlobalPosition += t.Up * CamMoveSpeed(gameTime));

            KeyAction(Keys.S,
                () => t.GlobalPosition += -t.Up * CamMoveSpeed(gameTime));

            KeyAction(Keys.A,
                () => t.GlobalPosition += -t.Right * CamMoveSpeed(gameTime));

            KeyAction(Keys.D,
                () => t.GlobalPosition += t.Right * CamMoveSpeed(gameTime));

            KeyAction(Keys.Q,
                () => t.LocalRotation += -CameraRotSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            
            KeyAction(Keys.E,
                () => t.LocalRotation += CameraRotSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void HandlePlaceControls()
        {
            var targetGridPos = grid.WorldToCell(MousePosWorld());

            if (Mouse.GetState(Window).LeftButton == ButtonState.Pressed)
            {
                tilemap.SetTile(targetGridPos, Tiles.bucket[0]);//Tiles.bucket[Random.Shared.Next(Tiles.bucket.Length)]);
            }
            else if(Mouse.GetState(Window).RightButton == ButtonState.Pressed)
            {
                tilemap.SetTile(targetGridPos, new TileInstance(null, new Matrix2x2(1f)));
            }
        }

        private Vector2 MousePosView()
        {
            var m = Mouse.GetState(Window);
            return new Vector2((m.X / (float)Window.ClientBounds.Width) * 2f - 1f, -((m.Y / (float) Window.ClientBounds.Height) * 2f - 1f));
        }

        private Vector2 MousePosWorld()
        {
            return Camera.ViewToWorldPos(MousePosView());
        }

        private float CamMoveSpeed(GameTime time)
        {
            return Camera.ViewSize * CameraSpeed * (float)time.ElapsedGameTime.TotalSeconds; 
        }

        private void KeyAction(Keys key, Action action)
        {
            if (Keyboard.GetState().IsKeyDown(key))
            {
                action();
            }
        }
    }
}