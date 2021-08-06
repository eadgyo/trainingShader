using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using TrainingShader;

namespace Game2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Camera camera;
        bool isX = false;
        private SkinnedModel skinnedModel;

        Terrain terrain;
        CubeDemo cubeDemo;
        TriDemo triDemo;
        SphericalDemo sphericalDemo;

        List<Mesh> meshes = new List<Mesh>();

        MouseState lastMouseState;
        //private CubeDemo cubeDemo;
        private int LastScrollWheel = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
            
            base.Initialize();
        }

        protected void LoadContentSceneCylinder()
        {
            meshes.Add(new Mesh(Content.Load<Model>("pinata"),
                new Vector3(0f, -100f, 0f),
                new Vector3(0.5f, 3.14f, 3.14f),
                new Vector3(1.0f, 1.0f, 1f),
                GraphicsDevice));

            //Effect simpleEffect = Content.Load<Effect>("SimpleEffect");
            //Effect waveEffect = Content.Load<Effect>("WaveEffect");
            //Effect diffuseEffect = Content.Load<Effect>("Diffuse");
            //Effect spotLightEffect = Content.Load<Effect>("Spotlight");
            //Effect Phong = Content.Load<Effect>("Phong");
            //Effect basicEffect = new BasicEffect(GraphicsDevice);
            Effect simpleEffect = Content.Load<Effect>("Texture");
            Effect textureEffect = Content.Load<Effect>("TextureLighting");
            Effect textureNormalEffect = Content.Load<Effect>("TextureLightingNormal");
            Effect blackEffect = Content.Load<Effect>("BlackEffect");

            Texture2D texture = Content.Load<Texture2D>("box1");
            Material lightingMat = new LightingMaterial();

            Texture2D textureTri = Content.Load<Texture2D>("textureTri");
            Texture2D textureTri2 = Content.Load<Texture2D>("textureTri2");
            Texture2D normalTri = Content.Load<Texture2D>("normalTri");
            Texture2D blendMap = Content.Load<Texture2D>("blendMap");
            Texture2D sphereTexture = Content.Load<Texture2D>("Cylindre");


            Effect multiTextureEffect = Content.Load<Effect>("multiTexture");

            cubeDemo = new CubeDemo(GraphicsDevice, texture, textureEffect, lightingMat);
            triDemo = new TriDemo(GraphicsDevice, textureTri, textureTri2, blendMap, normalTri, textureNormalEffect, multiTextureEffect, lightingMat, 1);
            sphericalDemo = new SphericalDemo(GraphicsDevice, sphereTexture, simpleEffect, blackEffect, lightingMat);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            skinnedModel = new SkinnedModel(Content.Load<Model>("monkey_rigging"), new Vector3(10000f, 0f, 0f),
                new Vector3(0, 3.14f/4, 0),
                new Vector3(0.5f, 0.5f, 0.5f),
                GraphicsDevice,
                Content);


            Texture2D heightMap = Content.Load<Texture2D>("heightMap");
            Texture2D textureMap = Content.Load<Texture2D>("terrain");
            Texture2D noise = Content.Load<Texture2D>("noise");

            terrain = new Terrain(heightMap, 20, 1500, textureMap, 128, new Vector3(5, -1, 0), GraphicsDevice, Content);


            /*meshes.Add(new Mesh(Content.Load<Model>("monkey_rigging"),
                new Vector3(0f, -100f, 0f),
                new Vector3(0.5f, 3.14f, 3.14f),
                new Vector3(15.0f, 15.0f, 15.0f),
                GraphicsDevice));
            */
            skinnedModel.Player.StartClip("Armature|ArmatureAction", false);

            camera = new FreeCamera(new Vector3(-1, -0.18f, -2.7f), MathHelper.ToRadians(260), MathHelper.ToRadians(0), GraphicsDevice);
            lastMouseState = Mouse.GetState();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            updateCamera(gameTime);
            skinnedModel.Update(gameTime);
            //skinnedModel.Rotation.Y -= 0.00005f*gameTime.ElapsedGameTime.Milliseconds;
            //skinnedModel.Rotation.Y = skinnedModel.Rotation.Y % 2 * 3.14f;

            
            base.Update(gameTime);
        }

        private void OldDraw(GameTime gameTime)
        {
            Matrix posMatrix = Matrix.CreateTranslation(((FreeCamera)camera).Origin);
            Matrix gWVP = camera.View * camera.Projection * posMatrix;
            //cubeDemo.SetWVP(gWVP, ((FreeCamera)camera).Position, posMatrix, camera.View, camera.Projection);
            //cubeDemo.draw(gameTime);
            //triDemo.SetWVP(gWVP, ((FreeCamera)camera).Position, posMatrix, camera.View, camera.Projection);
            //triDemo.draw(gameTime);

            sphericalDemo.SetWVP(gWVP, ((FreeCamera)camera).Origin, posMatrix, camera.View, camera.Projection);
            sphericalDemo.draw(gameTime);
            /*
            foreach (Mesh mesh in meshes)
            {
                mesh.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
            }
            */
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DimGray);

            //skinnedModel.Draw(camera.View, camera.Projection, ((FreeCamera) camera).Origin);
            terrain.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Origin);
            
            foreach (Mesh mesh in meshes)
            {
                mesh.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Origin);
            }

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

            float factor = 1.0f;
            if (keyState.IsKeyDown(Keys.LeftShift))
            {
                factor *= 5.0f;
            }


            // Determine in which direction to move the camera
            if (keyState.IsKeyDown(Keys.Z))
            {
                translation += factor * ((FreeCamera)camera).TransformVector(Vector3.Forward);
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                translation += factor * ((FreeCamera)camera).TransformVector(Vector3.Backward)  ;
            }
            if (keyState.IsKeyDown(Keys.Q))
            {
                translation += factor * ((FreeCamera)camera).TransformVector(Vector3.Left) ;
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                translation += factor * ((FreeCamera)camera).TransformVector(Vector3.Right);
            }
            if (keyState.IsKeyDown(Keys.C))
            {
                //skinnedModel.Player.StartClip("Armature|ArmatureAction", false);
                terrain.ChangeDistance(0);
            }
            if (keyState.IsKeyDown(Keys.W))
            {
                //skinnedModel.Player.StartClip("Armature|ArmatureAction", false);
                terrain.ChangeDistance(1);
            }
            if (keyState.IsKeyDown(Keys.X))
            {
                terrain.ChangeDistance(2);
            }
            

            // Move 3 units per millisecond, independant of frame rate
            translation *= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            ((FreeCamera)camera).Origin += translation;


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
