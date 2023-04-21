using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoEngine.Rendering;
using MonoEngine.Scenes;

namespace EngineTest
{
    public class TestGame : Game
    {
        private GraphicsDeviceManager graphics;

        private RenderPipeline renderer;

        private Scene scene;
        private SceneObject RotationRoot;
        private DrawableObject Tip;

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
            var camera = new Camera() { ViewSize = 5 };
            scene = new Scene(camera);
            RotationRoot = new SceneObject();

            var bar = new DrawableObject(Color.AliceBlue);
            var bar2 = new DrawableObject(Color.GreenYellow);
            Tip = new DrawableObject(Color.Red);

            //Hierarchy RotationRoot -> bar -> bar2 -> tip
            Tip.Parent = bar2;
            bar2.Parent = bar;
            bar.Parent = RotationRoot;

            bar.Transform.LocalPosition = new Vector2(0f, 0.5f);
            Tip.Transform.LocalScale = new Vector2(2f, 2f);

            scene.RegisterDrawable(bar);
            scene.RegisterDrawable(bar2);
            scene.RegisterDrawable(Tip);

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

            RotationRoot.Transform.LocalRotation += RootRotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Tip.Transform.LocalRotation += TipRotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            renderer.RenderScene(GraphicsDevice, scene);

            base.Draw(gameTime);
        }
    }
}