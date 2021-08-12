﻿using Microsoft.Xna.Framework;
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

        GrassVertex[] grassVertices;
        short[] grassIndices;
        int totalGrass = 1000;


        Terrain terrain;
        CubeDemo cubeDemo;
        TriDemo triDemo;
        SphericalDemo sphericalDemo;
        Effect grassEffect;
        VertexBuffer grassVertexBuffer;
        IndexBuffer grassIndexBuffer;

        Vector3 sunDirection = new Vector3(5, -1, 0);
        List<Mesh> meshes = new List<Mesh>();
        Texture2D grass;

        MouseState lastMouseState;
        //private CubeDemo cubeDemo;
        private int LastScrollWheel = 0;
        private bool isReleasedW = false;
        private bool isReleasedX = false;
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
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.IsFullScreen = false;
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

            terrain = new Terrain(heightMap, 20, 1500, textureMap, 128, sunDirection, GraphicsDevice, Content);


            /*meshes.Add(new Mesh(Content.Load<Model>("monkey_rigging"),
                new Vector3(0f, -100f, 0f),
                new Vector3(0.5f, 3.14f, 3.14f),
                new Vector3(15.0f, 15.0f, 15.0f),
                GraphicsDevice));
            */
            skinnedModel.Player.StartClip("Armature|ArmatureAction", false);

            camera = new FreeCamera(new Vector3(0, 0f, 0), MathHelper.ToRadians(260), MathHelper.ToRadians(0), GraphicsDevice);
            LoadContentTree();
            LoadContentGrass();
            lastMouseState = Mouse.GetState();
        }

        protected void LoadContentTree()
        {
            Model model = Content.Load<Model>("tree");

            for (int i = 0; i < 100; i++)
            {
                Vector3 vec = MathUtils.GenRandomNormalizedVec();

                vec.X *= terrain.Width * 0.49f;
                vec.Z *= terrain.Height * 0.49f;
                vec.Y = terrain.GetHeight(vec.X, vec.Z);
                float deg = MathUtils.GetRandomFloat(0, 360);
                float deg2 = MathUtils.GetRandomFloat(-10f, 10f);
                meshes.Add(new Mesh(model, vec, new Vector3(MathHelper.ToRadians(deg), MathHelper.ToRadians(deg2), 0), new Vector3(0.2f, 0.2f, 0.2f), GraphicsDevice));
            }
        }

        protected Vector3 GetPositionOnTerrain(Terrain terrain)
        {
            Vector3 vec = MathUtils.GenRandomNormalizedVec();
            vec.X *= terrain.Width * 0.49f;
            vec.Z *= terrain.Height * 0.49f;
            vec.Y = terrain.GetHeight(vec.X, vec.Z);
            return vec;
        }

        protected void LoadContentGrass()
        {
            // Load effect and associate texture
            grass = Content.Load<Texture2D>("grass");
            grassEffect = Content.Load<Effect>("GrassEffect");
            grassEffect.Parameters["gTex"].SetValue(grass);
            
            // Create grass vertices and indices
            grassVertices = new GrassVertex[4*totalGrass];
            grassIndices = new short[6* totalGrass];

            for (int idGrass = 0; idGrass < totalGrass; idGrass++)
            {
                Vector3 scale = new Vector3(100.0f, 100.0f, 1.0f);
                Vector3 worldPos = GetPositionOnTerrain(terrain);
                CreateGrass(idGrass, worldPos, scale, ref grassVertices, ref grassIndices);
            }

            grassVertexBuffer = new VertexBuffer(GraphicsDevice, GrassVertex.MyVertexDeclaration, grassVertices.Length, BufferUsage.WriteOnly);
            grassIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), grassIndices.Length, BufferUsage.WriteOnly);

            grassVertexBuffer.SetData(grassVertices);
            grassIndexBuffer.SetData<short>(grassIndices);
        }

        protected void CreateGrass(int numGrasses, Vector3 worldPos, Vector3 scale, ref GrassVertex[] gv, ref short[] k)
        {
            int offsetGV = numGrasses * 4;
            int offsetIndices = numGrasses * 6;

            float amp = MathUtils.GetRandomFloat(0.5f, 1.0f);
            gv[offsetGV + 0] = new GrassVertex(new Vector3(-1.0f, -0.5f, 0.0f),
                                    new Vector2(0.0f, 1.0f), 0.0f);
            gv[offsetGV + 1] = new GrassVertex(new Vector3(-1.0f, 0.5f, 0.0f),
                                    new Vector2(0.0f, 0.0f), amp);
            gv[offsetGV + 2] = new GrassVertex(new Vector3(1.0f, 0.5f, 0.0f),
                                    new Vector2(1.0f, 0.0f), amp);
            gv[offsetGV + 3] = new GrassVertex(new Vector3(1.0f, -0.5f, 0.0f),
                                    new Vector2(1.0f, 1.0f), 0.0f);

            // The pointer k specifies the position in the index buffer
            // where to write the new fin indices
            k[0 + offsetIndices] = (short)(0 + offsetGV);
            k[1 + offsetIndices] = (short)(1 + offsetGV);
            k[2 + offsetIndices] = (short)(2 + offsetGV);
            k[3 + offsetIndices] = (short)(0 + offsetGV);
            k[4 + offsetIndices] = (short)(2 + offsetGV);
            k[5 + offsetIndices] = (short)(3 + offsetGV);

            for (int i = offsetGV; i < offsetGV + 4; i++)
            {
                gv[i].Position.X *= scale.X;
                gv[i].Position.Y *= scale.Y;
                gv[i].Position.Z *= scale.Z;

                // Generate random offset color (mostly green)
                gv[i].ColorOffset = new Vector4(
                    MathUtils.GetRandomFloat(0.0f, 0.1f),
                    MathUtils.GetRandomFloat(0.0f, 0.2f),
                    MathUtils.GetRandomFloat(0.0f, 0.1f),
                    0.0f
                    );
            }

            // Add offset so that the bottom of fin touches the ground
            // when placed on terrain. Otherwise, the fi's center point
            // will touch the ground and only half of the fin will show.
            float heightOver2 = (gv[1 + offsetGV].Position.Y - gv[0 + offsetGV].Position.Y) / 2;
            worldPos.Y += heightOver2;

            // Set world center position for the quad
            gv[0 + offsetGV].QuadPos = worldPos;
            gv[1 + offsetGV].QuadPos = worldPos;
            gv[2 + offsetGV].QuadPos = worldPos;
            gv[3 + offsetGV].QuadPos = worldPos;

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

        protected void DrawGrass(GameTime gameTime)
        {

            GraphicsDevice.SetVertexBuffer(grassVertexBuffer);
            GraphicsDevice.Indices = grassIndexBuffer;

            Matrix viewProj = Matrix.Multiply(camera.View, camera.Projection);
            grassEffect.Parameters["gViewProj"].SetValue(viewProj);
            grassEffect.Parameters["gTime"]?.SetValue(0f);// (float)gameTime.TotalGameTime.TotalMilliseconds);
            grassEffect.Parameters["gDirToSunW"]?.SetValue(sunDirection);
            grassEffect.Parameters["gEyePosW"]?.SetValue(((FreeCamera)camera).Origin);
            grassEffect.Parameters["gUseAlpha"].SetValue(true);
            grassEffect.Parameters["gReferenceAlpha"].SetValue(0.5f);
            grassEffect.Parameters["gAlphaTestGreater"].SetValue(true);


            // --> FIRST PASS
            // Drow only the opaque pixel
            /*
            GraphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
            };*/
           
            foreach (EffectPass pass in grassEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, grassIndexBuffer.IndexCount / 3);
            }


            // --> SECOND PASS
            // Draw only transparent
            grassEffect.Parameters["gUseAlpha"].SetValue(true);
            grassEffect.Parameters["gReferenceAlpha"].SetValue(0.5f);
            grassEffect.Parameters["gAlphaTestGreater"].SetValue(false);
            /*
            GraphicsDevice.BlendState = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.SourceAlpha,
                ColorDestinationBlend = Blend.InverseSourceAlpha
                
            };
            

            GraphicsDevice.DepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = false,
                StencilEnable = true,
                StencilFunction = CompareFunction.Less,
                StencilPass = StencilOperation.Keep,
                ReferenceStencil = 200
            };*/
            /*
            foreach (EffectPass pass in grassEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, grassIndexBuffer.IndexCount / 3);
            }
            */
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DimGray);

            //skinnedModel.Draw(camera.View, camera.Projection, ((FreeCamera) camera).Origin);
            
            terrain.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Origin, camera.Frustum);
            
            foreach (Mesh mesh in meshes)
            {
                mesh.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Origin);
            }

            DrawGrass(gameTime);

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
            //skinnedModel.Player.StartClip("Armature|ArmatureAction", false);
            if (keyState.IsKeyDown(Keys.W))// && isReleasedW)
            {
                terrain.TestDecrement();
                isReleasedW = false;
            }
            if (keyState.IsKeyDown(Keys.X))// && isReleasedX)
            {
                terrain.TestIncrement();
                isReleasedX = false;
            }
            if (keyState.IsKeyUp(Keys.W))
            {
                isReleasedW = true;
            }
            if (keyState.IsKeyUp(Keys.X))
            {
                isReleasedX = true;
            }

            // Move 3 units per millisecond, independant of frame rate
            translation *= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            
            if (camera is WalkingCamera)
            {
                ((WalkingCamera)camera).Move(translation);

            }
            else
            {
                ((FreeCamera)camera).Origin += translation;
            }


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
