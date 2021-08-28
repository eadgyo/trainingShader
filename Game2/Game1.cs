using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TrainingShader;
using TrainingShader.Particles;

namespace Game2
{
    public class Game1 : Game
    {
        #region variables
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

        Vector3 sunPosition = new Vector3(25175, 16081, 721);
        Vector3 sunDirection = new Vector3(-5000, -3000, 0);
        Mesh monkeyProjection;
        List<Mesh> meshes = new List<Mesh>();
        Texture2D grass;
        FiringRing firingRing;
        RainSystem rainSystem;

        SkySphere skySphere;
        Water water;

        Texture2D ombre_text;
        
        RenderTarget2D shadowMappingTarget2D;
        Effect shadowEffect;
        Effect projectionEffect;

        RenderTarget2D radaraRenderTarget2D;
        VertexBuffer radarVertexBuffer;
        IndexBuffer radarIndexBuffer;
        Effect radarEffect;
        bool useShadowLerp = true;
        Matrix shadowView, shadowProjection;
        float shadowFarPlane = 100000;

        SpriteFont font;
        
        List<IRenderable> renderables = new List<IRenderable>();

        MouseState lastMouseState;
        //private CubeDemo cubeDemo;
        private int LastScrollWheel = 0;
        private bool isReleasedW = false;
        private bool isReleasedX = false;
        private bool isReleaseC = false;

        private bool drawMesh = true;

        #endregion variables
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
            };
            Content.RootDirectory = "Content";
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 1600;
            _graphics.PreferredBackBufferHeight = 900;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            
            base.Initialize();

            //SDL_SetWindowGrab(base.Window.Handle, true);
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

        [System.Runtime.InteropServices.DllImport("SDL2.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowGrab")]
        public static extern int SDL_SetWindowGrab(IntPtr window, bool grabbed);

        private void LoadParticles()
        {
            firingRing = new FiringRing("FiringEffect", "FireRingTech", "torch", new Vector3(0.0f, 0.9f, 0.0f), Content, GraphicsDevice);
            for (int i = 0; i < 20; i++)
            {
                firingRing.AddParticle();
            }

            rainSystem = new RainSystem(camera, "RainEffect", "RainTech", "rain", new Vector3(-1.0f, -9.8f, 0.0f), Content, GraphicsDevice);
            rainSystem.TimePerParticle = -1.0f;
            for (int i = 0; i < 500; i++)
            {
                rainSystem.AddParticle();
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            skinnedModel = new SkinnedModel(Content.Load<Model>("monkey_rigging"), new Vector3(10000f, 0f, 0f),
                new Vector3(0, 3.14f/4, 0),
                new Vector3(0.5f, 0.5f, 0.5f),
                GraphicsDevice,
                Content);

            font = Content.Load<SpriteFont>("text2");

            Texture2D heightMap = Content.Load<Texture2D>("heightMap");
            Texture2D textureMap = Content.Load<Texture2D>("terrain");
            Texture2D noise = Content.Load<Texture2D>("noise");

            camera = new FreeCamera(new Vector3(0, 0f, 0), MathHelper.ToRadians(260), MathHelper.ToRadians(0), GraphicsDevice);
            skySphere = new SkySphere("StandardCubeMap", Content, GraphicsDevice);
            renderables.Add(skySphere);

            terrain = new Terrain(heightMap, 20, 1500, textureMap, 128, sunDirection, GraphicsDevice, Content);
            renderables.Add(terrain);

            skinnedModel.Player.StartClip("Armature|ArmatureAction", false);

            /*meshes.Add(new Mesh(Content.Load<Model>("monkey_rigging"),
                new Vector3(0f, -100f, 0f),
                new Vector3(0.5f, 3.14f, 3.14f),
                new Vector3(15.0f, 15.0f, 15.0f),
                GraphicsDevice));
            */

            LoadContentTree();
            LoadContentGrass();
            LoadParticles();
            LoadProjectedMonkey();
            //LoadReflectiveMonkey();
            LoadWater();
            LoadRadar();
            LoadShadow();

            lastMouseState = Mouse.GetState();
        }

        protected void LoadRadar()
        {
            radaraRenderTarget2D = new RenderTarget2D(GraphicsDevice, 128, 128, false, SurfaceFormat.Color, DepthFormat.Depth24);

            VertexPositionTexture[] vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(GraphicsDevice.Viewport.Width - radaraRenderTarget2D.Width, 0, GraphicsDevice.Viewport.Height - radaraRenderTarget2D.Height), new Vector2(0, 0));
            vertices[1] = new VertexPositionTexture(new Vector3(GraphicsDevice.Viewport.Width - radaraRenderTarget2D.Width, 0, GraphicsDevice.Viewport.Height), new Vector2(0, 1));
            vertices[2] = new VertexPositionTexture(new Vector3(GraphicsDevice.Viewport.Width, 0, GraphicsDevice.Viewport.Height), new Vector2(1, 1));
            vertices[3] = new VertexPositionTexture(new Vector3(GraphicsDevice.Viewport.Width, 0, GraphicsDevice.Viewport.Height - radaraRenderTarget2D.Height), new Vector2(1, 0));

            radarVertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            radarVertexBuffer.SetData(vertices);

            short[] indices = new short[6];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 3;

            indices[3] = 1;
            indices[4] = 3;
            indices[5] = 2;

            radarIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), 6, BufferUsage.WriteOnly);
            radarIndexBuffer.SetData(indices);

            radarEffect = Content.Load<Effect>("RadarEffect");
        }

        public void LoadShadow()
        {
            shadowMappingTarget2D = new RenderTarget2D(GraphicsDevice, 8192, 8192, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            shadowEffect = Content.Load<Effect>("BuildShadowMap");
        }

        protected void LoadWater()
        {
            water = new Water(sunDirection, 400, 256, Content, GraphicsDevice);
        }

        protected void LoadProjectedMonkey()
        {
            monkeyProjection = new Mesh(Content.Load<Model>("monkey"),
                new Vector3(700f, 500f, 0f),
                new Vector3(0, 0, 0),
                new Vector3(1f, 1f, 1f),
                GraphicsDevice);


            ombre_text = Content.Load<Texture2D>("ombre_text");

            float width = 512;
            float height = 512;
            float Scale = 1.0f;


            Matrix orthoProjection = Matrix.CreateOrthographicOffCenter(-Scale * width / 2, Scale * width / 2, -Scale * height / 2, Scale * height / 2, -1000000, 1000000);

            Vector3 projectorDirection = new Vector3(100, 300, 0f);
            Vector3 projectorTarget = new Vector3(0, 300, 0f);
            Matrix view = Matrix.CreateLookAt(projectorDirection, projectorTarget, Vector3.Up);

            projectionEffect = Content.Load<Effect>("ProjectionEffect");
            projectionEffect.Parameters["gSunDirection"]?.SetValue(projectorDirection);
            projectionEffect.Parameters["gTex"].SetValue(ombre_text);
            projectionEffect.Parameters["gLightWVP"].SetValue(view * orthoProjection);

            monkeyProjection.SetModelEffect(projectionEffect, false);

            meshes.Add(monkeyProjection);

        }

        protected void LoadReflectiveMonkey()
        {
            Mesh monkey = new Mesh(Content.Load<Model>("monkey"),
                new Vector3(250f, 400f, 3000f),
                new Vector3(0, 0, 0),
                new Vector3(1f, 1f, 1f),
                GraphicsDevice);

            Effect effect = Content.Load<Effect>("DiffuseFog");
            Effect cubeMapReflectiveEffect = Content.Load<Effect>("reflectionEnv_effect");
            ReflectiveMaterial reflectiveMaterial = new ReflectiveMaterial(Content.Load<TextureCube>("StandardCubeMap"));
            reflectiveMaterial.SetEffectParameters(cubeMapReflectiveEffect);
            monkey.SetModelEffect(cubeMapReflectiveEffect, false);
            monkey.Material = reflectiveMaterial;

            //meshes.Add(monkey);
            //meshes.Add(monkey2);
           // renderables.Add(monkey);
        }

        protected void LoadContentTree()
        {
            Model model = Content.Load<Model>("tree");
            Effect effect = Content.Load<Effect>("DiffuseFog");
            Material lightingMat = new LightingMaterial();
            lightingMat.SetEffectParameters(effect);
            for (int i = 0; i < 200; i++)
            {
                Vector3 vec = MathUtils.GenRandomNormalizedVec();

                vec.X *= terrain.Width * 0.5f;
                vec.Z *= terrain.Height * 0.5f;
                vec.Y = terrain.GetHeight(vec.X, vec.Z);
                float deg = MathUtils.GetRandomFloat(0, 360);
                float deg2 = MathUtils.GetRandomFloat(-10f, 10f);
                meshes.Add(new Mesh(model, vec, new Vector3(MathHelper.ToRadians(deg), MathHelper.ToRadians(deg2), 0), new Vector3(0.5f, 0.5f, 0.5f), GraphicsDevice));
                meshes[i].SetModelEffect(effect, false);
                renderables.Add(meshes[i]);
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

            float amp = MathUtils.GetRandomFloat(20.5f, 40.0f);
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
            water.Update(gameTime);
            //monkeyProjection.Position = monkeyProjection.Position + new Vector3(0, (float)( 50 * gameTime.ElapsedGameTime.TotalSeconds), 0);
            //skinnedModel.Update(gameTime);
            //firingRing.Update(gameTime);
            //rainSystem.Update(gameTime);
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

        protected void PreDrawRadar(GameTime gameTime)
        {
            Camera topCamera = new TopCamera(new Vector3(0, 10000, 0), new Vector3(0, 0, 0), GraphicsDevice);
            topCamera.Update();
            // Render to texture
            GraphicsDevice.SetRenderTarget(radaraRenderTarget2D);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            terrain.boundingFrustum = null;
            terrain.SetFog(false);
            terrain.Draw(topCamera.View, topCamera.Projection, ((TopCamera)topCamera).Origin);
            GraphicsDevice.SetRenderTarget(null);
        }

        protected void DrawRadar(GameTime gameTime)
        {
            Vector3 translate = new Vector3(350, 0, 800 + 256 / 2);

            // Draw texture
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(radaraRenderTarget2D, new Vector2(GraphicsDevice.Viewport.Width- radaraRenderTarget2D.Width, GraphicsDevice.Viewport.Height - radaraRenderTarget2D.Height), Color.White); 
            spriteBatch.End();
        }

        protected void DrawShadow(GameTime gameTime)
        {
            // Draw texture
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(shadowMappingTarget2D, new Vector2(0, 0), Color.White);
            spriteBatch.End();
        }


        protected void DrawGrass(GameTime gameTime)
        {

            GraphicsDevice.SetVertexBuffer(grassVertexBuffer);
            GraphicsDevice.Indices = grassIndexBuffer;

            Matrix viewProj = Matrix.Multiply(camera.View, camera.Projection);
            grassEffect.Parameters["gViewProj"].SetValue(viewProj);
            grassEffect.Parameters["gTime"]?.SetValue((float)gameTime.TotalGameTime.TotalMilliseconds/10000);// (float)gameTime.TotalGameTime.TotalMilliseconds);
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

        protected void DrawText(string text, float x, float y, Color color)
        {
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin();
            spriteBatch.DrawString(font, text, new Vector2(x, y), color);
            spriteBatch.End();
        }

        protected override void Draw(GameTime gameTime)
        {
            water.PreDraw(camera, gameTime, renderables);
            PreDrawRadar(gameTime);
            PreDrawShadow();

            GraphicsDevice.Clear(Color.DimGray);
            
            skySphere.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Origin);
            //skinnedModel.Draw(camera.View, camera.Projection, ((FreeCamera) camera).Origin);

            //firingRing.Draw(gameTime, GraphicsDevice.Viewport.Height, camera);
            //rainSystem.Draw(gameTime, GraphicsDevice.Viewport.Height, camera);
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            DrawTerrainWithShadow();

            //terrain.boundingFrustum = camera.Frustum;
            //terrain.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Origin);
            
            if (drawMesh)
            {
                foreach (Mesh mesh in meshes)
                {
                    mesh.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Origin);
                }
            }

            water.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Origin);
            DrawGrass(gameTime);
            //monkeyProjection.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Origin);
            //DrawRadar(gameTime);
            //DrawShadow(gameTime);

            DrawText("UsingShadowLerp = " + useShadowLerp.ToString(), 10, 10, Color.Black);
            DrawText("DrawMesh = " + drawMesh.ToString(), 10, 30, Color.Black);
            DrawText("DrawSpecular = " + water.effect.Parameters["gUseSpecular"].GetValueBoolean(), 10, 50, Color.Black);

            base.Draw(gameTime);
        }


        public void PreDrawShadow()
        {
            GraphicsDevice.SetRenderTarget(shadowMappingTarget2D);
            GraphicsDevice.Clear(Color.White);

            uint numPasses = 0;
            // Draw scene mesh
            Vector3 position = ((FreeCamera)camera).Position + sunPosition;
            Vector3 target = ((FreeCamera)camera).Target;
            target = sunDirection;
            Vector3 vec = target - position;
            vec.Normalize();

            target = sunDirection;
            shadowView = Matrix.CreateLookAt(sunPosition, sunPosition+ sunDirection, Vector3.Up);
            shadowProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1, 1, shadowFarPlane);

            shadowEffect.Parameters["gLightWVP"].SetValue(shadowView * shadowProjection);
            shadowEffect.Parameters["gFarPlane"]?.SetValue(shadowFarPlane);

            //terrain.Draw(ligthView.View, ligthView.Projection, ((TargetCamera)ligthView).Origin);
            foreach (Mesh mesh in meshes)
            {
                mesh.CacheEffects();
                mesh.SetModelEffect(shadowEffect, false);
                mesh.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Origin);
                mesh.RestoreEffects();
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        public void DrawTerrainWithShadow()
        {
            uint numPasses = 0;
            terrain.SetFog(true);
            terrain.SetClipSpace(null);
            terrain.boundingFrustum = ((FreeCamera)camera).Frustum;

            terrain.effect.Parameters["gShadowMap"]?.SetValue(shadowMappingTarget2D);
            terrain.effect.Parameters["gShadowView"].SetValue(shadowView);
            terrain.effect.Parameters["gShadowProjection"].SetValue(shadowProjection);
            terrain.effect.Parameters["gShadowFarPlane"]?.SetValue(shadowFarPlane);
            terrain.effect.Parameters["SMAP_SIZE"]?.SetValue((float)shadowMappingTarget2D.Width);
            terrain.effect.Parameters["gUseShadowLerp"]?.SetValue(useShadowLerp);
            terrain.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Origin);
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
            if (keyState.IsKeyDown(Keys.W) && isReleasedW)
            {
                drawMesh = !drawMesh;
                isReleasedW = false;
            }
            if (keyState.IsKeyUp(Keys.W))// && isReleasedX)
            {
                isReleasedW = true;
            }
            if (keyState.IsKeyDown(Keys.X) && isReleasedX)
            {
                useShadowLerp = !useShadowLerp;
                isReleasedX = false;
            }
            if (keyState.IsKeyUp(Keys.X))
            {
                isReleasedX = true;
            }
            if (keyState.IsKeyDown(Keys.C) && isReleaseC)
            {
                bool useSpec = !water.effect.Parameters["gUseSpecular"].GetValueBoolean();
                water.effect.Parameters["gUseSpecular"].SetValue(useSpec);
                isReleaseC = false;
            }

            if (keyState.IsKeyUp(Keys.C))
            {
                isReleaseC = true;
            }

            if (keyState.IsKeyDown(Keys.B))
            {
                Debug.WriteLine(((FreeCamera)camera).Origin + sunPosition);
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

            if (mouseState.LeftButton == ButtonState.Pressed)
            {

                Ray ray = GetWorldPickingRay(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

                for (int i = 0; i < meshes.Count; i++)
                {
                    if (meshes[i].BoundingSphere.Intersects(ray) != null)
                    {
                        meshes[i].Scale = new Vector3(0.2f, 0.2f, 0.2f);
                    }
                }
            }

            // Move the camera
            camera.Update();

            lastMouseState = mouseState;
        }

        public Ray GetWorldPickingRay(float sx, float sy)
        {

            float w = (float)GraphicsDevice.Viewport.Width;
            float h = (float)GraphicsDevice.Viewport.Height;

            Matrix proj = camera.Projection;

            float x = (2.0f * sx / w - 1.0f) / proj[0, 0];
            float y = (-2.0f * sy / h + 1.0f) / proj[1, 1];

            Vector3 origin = new Vector3();
            Vector3 dir = new Vector3(x, y, 1.0f);

            Matrix invView = Matrix.Invert(camera.View);
            Vector3 originW = Vector3.Transform(origin, invView);
            Vector3 dirW = -Vector3.TransformNormal(dir, invView);
            dirW.Normalize();

            Ray ray = new Ray(originW, dirW);
            return ray;
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
