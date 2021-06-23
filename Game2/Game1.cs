using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;

namespace Game2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Camera camera;
        bool isX = false;

        CubeDemo cubeDemo;
        TriDemo triDemo;

        List<Mesh> meshes = new List<Mesh>();

        MouseState lastMouseState;
        //private CubeDemo cubeDemo;
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
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            meshes.Add(new Mesh(Content.Load<Model>("pinata"),
                            new Vector3(0f,-100f,0f),
                            new Vector3(0.5f, 3.14f, 3.14f),
                            new Vector3(15.0f, 15.0f, 15.0f),
                            GraphicsDevice)); ;

            camera = new FreeCamera(new Vector3(-3.004f, -0.18f, -2.7f), MathHelper.ToRadians(230), MathHelper.ToRadians(0), GraphicsDevice);
            lastMouseState = Mouse.GetState();

            //Effect simpleEffect = Content.Load<Effect>("SimpleEffect");
            //Effect waveEffect = Content.Load<Effect>("WaveEffect");
            //Effect diffuseEffect = Content.Load<Effect>("Diffuse");
            //Effect spotLightEffect = Content.Load<Effect>("Spotlight");
            //Effect Phong = Content.Load<Effect>("Phong");
            //Effect basicEffect = new BasicEffect(GraphicsDevice);
            Effect textureEffect = Content.Load<Effect>("TextureLighting");
            Effect textureNormalEffect = Content.Load<Effect>("TextureLightingNormal");

            Texture2D texture = Content.Load<Texture2D>("box1");
            Material lightingMat = new LightingMaterial();

            Texture2D textureTri = Content.Load<Texture2D>("textureTri");
            Texture2D textureTri2 = Content.Load<Texture2D>("textureTri2");
            Texture2D normalTri = Content.Load<Texture2D>("normalTri");

            Effect multiTextureEffect = Content.Load<Effect>("multiTexture");

            /*meshes[0].SetModelEffect(spotLightEffect, false);
            meshes[0].Material = lightingMat;
            lightingMat.SetEffectParameters(spotLightEffect);*/

            lastMouseState = Mouse.GetState();

            cubeDemo = new CubeDemo(GraphicsDevice, texture, textureEffect, lightingMat);
            triDemo = new TriDemo(GraphicsDevice, textureTri, textureTri2, normalTri, textureNormalEffect, multiTextureEffect, lightingMat, 1);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            updateCamera(gameTime);
            Vector3 result = meshes[0].LightPos;
            if (isX == false)
            {
                result.X -= 0.005f * gameTime.ElapsedGameTime.Milliseconds;
                if (result.X < -200)
                {
                    isX = true;
                }
            }
            else
            {
                result.X = MathHelper.Min(result.X + 0.05f * gameTime.ElapsedGameTime.Milliseconds, 0);
                result.Y += 0.005f * gameTime.ElapsedGameTime.Milliseconds;

                if (result.X == 0)
                {
                    result = new Vector3(10, 0, -100);
                    isX = false;
                }
            }
            meshes[0].LightPos = result;

            base.Update(gameTime);
            //updateModel(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Debug.WriteLine(((FreeCamera)camera).Position);
            Matrix posMatrix = Matrix.CreateTranslation(((FreeCamera)camera).Position);
            Matrix gWVP =  camera.View * camera.Projection;

            cubeDemo.SetWVP(gWVP, ((FreeCamera)camera).Position, posMatrix, camera.View, camera.Projection);
            //cubeDemo.draw(gameTime);
            triDemo.SetWVP(gWVP, ((FreeCamera)camera).Position, posMatrix, camera.View, camera.Projection);
            triDemo.draw(gameTime);

            /*
            foreach (Mesh mesh in meshes)
            {
                mesh.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
            }
            */

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
            ((FreeCamera)camera).Rotate(deltaX * .005f, deltaY * .005f);

            Vector3 translation = Vector3.Zero;

            // Determine in which direction to move the camera
            if (keyState.IsKeyDown(Keys.Z))
            {
                translation += ((FreeCamera)camera).TransformVector(Vector3.Forward) / 1000;
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                translation += ((FreeCamera)camera).TransformVector(Vector3.Backward) / 1000;
            }
            if (keyState.IsKeyDown(Keys.Q))
            {
                translation += ((FreeCamera)camera).TransformVector(Vector3.Left) / 1000;
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                translation += ((FreeCamera)camera).TransformVector(Vector3.Right) / 1000;
            }
            // Move 3 units per millisecond, independant of frame rate
            translation *= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            ((FreeCamera)camera).Position += translation;
            Vector3 Rotation = meshes[0].Rotation;
            Rotation.X += 0.001f;
            //meshes[0].Rotation = Rotation;
            cubeDemo.update(gameTime);
            triDemo.update(gameTime);

            // Move the camera
            camera.Update();

            lastMouseState = mouseState;
        }

        void updateModel(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            Vector3 rotChange = new Vector3(0, 0, 0);

            // Determine on which axes
            if (keyState.IsKeyDown(Keys.W))
                rotChange += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.S))
                rotChange += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.A))
                rotChange += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.D))
                rotChange += new Vector3(0, -1, 0);

            meshes[0].Rotation += rotChange * .025f;

            // if space isn't down, the ship shouldn't move
            if (!keyState.IsKeyDown(Keys.Space))
                return;

            // Determine what direction to move in
            Matrix rotation = Matrix.CreateFromYawPitchRoll(meshes[0].Rotation.Y, meshes[0].Rotation.X, meshes[0].Rotation.Z);

            // Move in the direction dictated by our rotation matrix
            meshes[0].Position += Vector3.Transform(Vector3.Forward, rotation) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
        }
    }
}
