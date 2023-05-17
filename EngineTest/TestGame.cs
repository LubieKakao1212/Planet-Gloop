using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoEngine.Rendering;
using MonoEngine.Scenes;
using System;

namespace EngineTest
{
    public class TestGame : Game
    {
        private GraphicsDeviceManager graphics;

        private RenderPipeline renderer;

        private Hierarchy scene;
        private HierarchyObject RotationRoot;
        private DrawableObject Tip;

        private Camera Camera;

        private float RootRotationSpeed = MathHelper.PiOver2;
        private float TipRotationSpeed = MathHelper.PiOver2;

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
            Camera = new Camera() { ViewSize = 5 };
            scene = new Hierarchy();
            RotationRoot = new HierarchyObject();

            var bar = new DrawableObject(Color.AliceBlue, -1f);
            var bar2 = new DrawableObject(Color.GreenYellow, 0f);
            var origin = new DrawableObject(Color.Black, 0f);
            var blade1 = new DrawableObject(Color.BurlyWood, -0.5f);
            var blade2 = new DrawableObject(Color.BurlyWood, 0f);

            Tip = new DrawableObject(Color.Red, 0f);

            //Hierarchy RotationRoot -> bar -> bar2 -> tip -> blade1, blade2
            blade1.Parent = Tip;
            blade2.Parent = Tip;
            Tip.Parent = bar2;
            bar2.Parent = bar;
            bar.Parent = RotationRoot;

            bar.Transform.LocalPosition = new Vector2(0f, 0.5f);
            bar2.Transform.LocalPosition = new Vector2(0f, 1f);

            blade1.Transform.LocalScale = new Vector2(0.25f, 4f);
            blade2.Transform.LocalScale = new Vector2(4f, 0.25f);
            Tip.Transform.LocalScale = new Vector2(0.5f, 0.5f);
            origin.Transform.LocalScale = new Vector2(0.1f, 0.1f);

            //camera.Parent = Tip;

            //RotationRoot.Transform.LocalScale = new Vector2(1f, 2f);

            /*scene.RegisterDrawable(bar);
            scene.RegisterDrawable(bar2);
            scene.RegisterDrawable(Tip);
            scene.RegisterDrawable(origin);
            scene.RegisterDrawable(blade1);
            scene.RegisterDrawable(blade2);*/


            scene.AddObject(RotationRoot);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            renderer.Init(GraphicsDevice, Content);
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //RotationRoot.Transform.LocalRotation += RootRotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //RotationRoot.Transform.LocalPosition = Vector2.UnitY * MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds);
            Tip.Transform.LocalRotation += TipRotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);
            renderer.RenderScene(GraphicsDevice, scene, Camera);

            base.Draw(gameTime);
        }
    }
}