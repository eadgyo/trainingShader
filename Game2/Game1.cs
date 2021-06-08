using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Game2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Camera camera;

        MouseState lastMouseState;
        private CubeDemo cubeDemo;
        private int LastScrollWheel = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            camera = new FreeCamera(new Vector3(1000, 500, -2000), MathHelper.ToRadians(153), MathHelper.ToRadians(5), GraphicsDevice);
            lastMouseState = Mouse.GetState();

            Effect simpleEffect = Content.Load<Effect>("SimpleEffect");
            Effect waveEffect = Content.Load<Effect>("WaveEffect");
            Effect diffuseEffect = Content.Load<Effect>("Diffuse");

            Model model = Content.Load<Model>("monkey");

            cubeDemo = new CubeDemo(GraphicsDevice, model, simpleEffect, waveEffect, diffuseEffect);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            updateCamera(gameTime);
            base.Update(gameTime);
            updateModel(gameTime);
            cubeDemo.update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            cubeDemo.draw(gameTime);

            base.Draw(gameTime);
        }

        void updateCamera(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            // Determine how much the camera should turn
            float deltaX = (float)lastMouseState.X - (float)mouseState.X;
            float deltaY = (float)lastMouseState.Y - (float)mouseState.Y;

            // Rotate Camera
            //(camera).Rotate(deltaX * .005f, deltaY * .005f);

            Vector3 translation = Vector3.Zero;

            // Determine in which direction to move the camera
            if (keyState.IsKeyDown(Keys.Z)) translation += Vector3.Forward;
            if (keyState.IsKeyDown(Keys.S)) translation += Vector3.Backward;
            if (keyState.IsKeyDown(Keys.Q)) translation += Vector3.Left;
            if (keyState.IsKeyDown(Keys.D)) translation += Vector3.Right;



            // Move 3 units per millisecond, independant of frame rate
            translation *= 4 * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            // Move the camera
            camera.Update();

            lastMouseState = mouseState;
        }

        void updateModel(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (keyState.IsKeyDown(Keys.Q))
                cubeDemo.updateCameraRot((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.0030f);
            if (keyState.IsKeyDown(Keys.D))
                cubeDemo.updateCameraRot(-(float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.0030f);

            int wheelValue = LastScrollWheel - mouseState.ScrollWheelValue; ;
            if (wheelValue != 0)
            {
                cubeDemo.updateScaling(wheelValue);
            }

            LastScrollWheel = mouseState.ScrollWheelValue;
        }
    }
}
