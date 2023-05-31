using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoEngine.Math;
using MonoEngine.Rendering;
using MonoEngine.Scenes;
using MonoEngine.Tilemap;
using System;
using System.Collections.Generic;

namespace EngineTest
{
    public class TestGame : Game
    {
        private GraphicsDeviceManager graphics;

        private RenderPipeline renderer;

        private Hierarchy scene;

        private List<HierarchyObject> Tips = new List<HierarchyObject>();

        private Camera Camera;

        private float CameraRotationSpeed = MathHelper.PiOver4;
        private float CameraZoomOutSpeed = 4f;
        private float TipRotationSpeed = MathHelper.PiOver2;

        private float bladeLength = 32f;

        private const int WindmillCount = 4096;

        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 512;
            graphics.PreferredBackBufferHeight = 512;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            renderer = new RenderPipeline();
        }

        protected override void Initialize()
        {
            renderer.Init(GraphicsDevice);

            Camera = new Camera() { ViewSize = 5 };
            scene = new Hierarchy();

            for (int i = 0; i < WindmillCount; i++) 
            {
                var rot = MathHelper.Pi * WindmillCount / i;
                var mat = Matrix2x2.Rotation(rot);
                Tips.Add(CreateWindmill(scene, rot, Vector2.UnitY * 64f * mat));
            }

            var grid = new Grid();

            scene.AddObject(grid);

            var tilemap = new Tilemap();

            FIllTilemap(tilemap, new Rectangle(-128, -128, 256, 256));
            //FIllTilemap(tilemap, new Rectangle(-2, -2, 4, 4));

            var mapRenderer = new TilemapRenderer(tilemap, grid, renderer, Color.White, -1f);

            mapRenderer.Parent = grid;

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

            //RotationRoot.Transform.LocalRotation += RootRotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //RotationRoot.Transform.LocalPosition = Vector2.UnitY * MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds);

            foreach (var windmill in Tips)
            {
                windmill.Transform.LocalRotation += TipRotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            Camera.Transform.LocalRotation += CameraRotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Camera.ViewSize += CameraZoomOutSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            renderer.RenderScene(scene, Camera);

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

            blade1.Transform.LocalScale = new Vector2(0.25f, bladeLength);
            blade2.Transform.LocalScale = new Vector2(bladeLength, 0.25f);
            tip.Transform.LocalScale = new Vector2(0.5f, 0.5f);
            origin.Transform.LocalScale = new Vector2(0.1f, 0.1f);

            hierarchy.AddObject(root);

            return tip;
        }

        public void FIllTilemap(Tilemap tilemap, Rectangle bounds)
        {
            var bucket = new TileInstance[]
            {
                new TileInstance(Tiles.Green, Tiles.GreenTransform),
                new TileInstance(Tiles.OversizedRed, Tiles.OversizedRedTransform),
                new TileInstance(Tiles.OversizedTransparentYellow, Tiles.OversizedTransparentYellowTransform),
            };

            for(int x = 0; x < bounds.Width; x++)
                for(int y = 0; y < bounds.Height; y++)
                {
                    tilemap.SetTile(new Point(bounds.X + x, bounds.Y + y), bucket[Random.Shared.Next(bucket.Length)]);
                }
        }
    }
}