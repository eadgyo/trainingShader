using Game2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TrainingShader
{
    public class Water : IRenderable
    {
        private Model model;
        private GraphicsDevice GraphicsDevice;
        public float Scale = 10000.0f;
        public Vector3 Position = new Vector3(0, 350, 0);
        private Texture2D waterNormalTex;
        public Effect effect;
        private RenderTarget2D renderTarget2D;

        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;

        private Texture textureTest;
        private float cellSize;
        private int nRows, nCols;
        private Texture2D disTex0, disTex1;
        private float texScale;
        private int nVerticies, nIndices;

        private Matrix[] modelTransforms;
        public Water(Vector3 SunDirection, float CellSize, int MAP_SIZE, ContentManager content, GraphicsDevice graphicsDevice)
        {
            model = content.Load<Model>("water_mesh");
            modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            waterNormalTex = content.Load<Texture2D>("water_normal_texture");
            effect = content.Load<Effect>("water_effect_displacement");
            textureTest = content.Load<Texture2D>("grass");

            cellSize = CellSize;
            nRows = MAP_SIZE;
            nCols = MAP_SIZE;

            disTex0 = content.Load<Texture2D>("disTex0");
            disTex1 = content.Load<Texture2D>("disTex1");
            effect.Parameters["gDisTex0"]?.SetValue(disTex0);
            effect.Parameters["gDisTex1"]?.SetValue(disTex1);
            effect.Parameters["DMAP_SIZE"]?.SetValue((float)disTex0.Width);
            effect.Parameters["gScaleHeights"]?.SetValue(new Vector2(50, 200));
            effect.Parameters["gCellSize"]?.SetValue(cellSize);

            texScale = 2f;

            nVerticies = nRows * nCols;
            nIndices = (nRows - 1) * (nCols - 1) * 6;

            effect.Parameters["gViewportWidth"]?.SetValue((float)graphicsDevice.Viewport.Width);
            effect.Parameters["gViewportHeight"]?.SetValue((float) graphicsDevice.Viewport.Height);
            effect.Parameters["gBaseColor"]?.SetValue(new Vector3(0.2f, 0.2f, 0.2f));
            effect.Parameters["gBaseColorAmount"]?.SetValue(0.5f);
            effect.Parameters["gTime"]?.SetValue(0.0f);
            effect.Parameters["gWaterNormalTexture"]?.SetValue(waterNormalTex);
            effect.Parameters["gSunDirection"]?.SetValue(SunDirection);


            GraphicsDevice = graphicsDevice;
            renderTarget2D = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);

            SetModelEffect(effect);

            CreateVertexGrid();
            CreateIndicesGrid();
        }

        public void CreateVertexGrid()
        {
            VertexPositionTexture[] waterVertices = new VertexPositionTexture[nVerticies];
            
            // Calculate the origin
            Vector3 offsetToCenter = -new Vector3((float)(cellSize * nRows / 2.0f), 0, (float)(cellSize * nCols / 2.0f));

            // For each pixel in the image
            for (int z = 0; z < nCols; z++)
            {
                for (int x = 0; x < nRows; x++)
                {
                    // Find Position based on grid coordinates and height in heightmap  
                    Vector3 position = new Vector3((float)(cellSize * x), Position.Y, (float)(cellSize * z));

                    // Center to origin
                    position += offsetToCenter;

                    // Compute UV texture coordinates
                    Vector2 UV = new Vector2((float)x / nRows, (float)z / nCols);

                    waterVertices[z * nRows + x] = new VertexPositionTexture(position, UV);
                }
            }
            vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionTexture.VertexDeclaration, nVerticies, BufferUsage.WriteOnly);
            vertexBuffer.SetData(waterVertices);
        }

        public void CreateIndicesGrid()
        {
            short[] indices = new short[nIndices];
            int i = 0;

            for (int x = 0; x < nRows - 1; x++)
            {
                for (int z = 0; z < nCols - 1; z++)
                {
                    // Find the indices of the corners
                    short upperLeft = (short)(z * nRows + x);
                    short upperRight = (short)(upperLeft + 1);
                    short lowerLeft = (short)(upperLeft + nRows);
                    short lowerRight = (short)(lowerLeft + 1);

                    // Specify upper triangle
                    indices[i++] = upperLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerLeft;

                    // Specify lower triangle
                    indices[i++] = lowerLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerRight;
                }
            }
            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), nIndices, BufferUsage.WriteOnly);
            indexBuffer.SetData<short>(indices);
        }


        public void CreateVertex()
        {
            VertexPositionTexture[] waterVertices = new VertexPositionTexture[6];

            waterVertices[0] = new VertexPositionTexture(new Vector3(-Scale, Position.Y, Scale), new Vector2(0, 1));
            waterVertices[2] = new VertexPositionTexture(new Vector3(Scale, Position.Y, -Scale), new Vector2(1, 0));
            waterVertices[1] = new VertexPositionTexture(new Vector3(-Scale, Position.Y, -Scale), new Vector2(0, 0));

            waterVertices[3] = new VertexPositionTexture(new Vector3(-Scale, Position.Y, Scale), new Vector2(0, 1));
            waterVertices[5] = new VertexPositionTexture(new Vector3(Scale, Position.Y, Scale), new Vector2(1, 1));
            waterVertices[4] = new VertexPositionTexture(new Vector3(Scale, Position.Y, -Scale), new Vector2(1, 0));

            vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionTexture.VertexDeclaration, waterVertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(waterVertices);
        }

        public void CreateIndices()
        {
            short[] indicies = new short[6];

            indicies[0] = 0;
            indicies[1] = 1;
            indicies[2] = 2;

            indicies[3] = 3;
            indicies[4] = 4;
            indicies[5] = 5;

            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), indicies.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indicies);
        }


        public void SetModelEffect(Effect effect)
        {

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }
        }

        public void PreDraw(Camera camera, GameTime gameTime, List<IRenderable> renderables)
        {
            RenderReflection(camera, renderables);
        }
        
        public void Update(GameTime gameTime)
        {
            effect.Parameters["gTime"]?.SetValue((float)gameTime.TotalGameTime.TotalMilliseconds / 10000);
        }

        public void RenderReflection(Camera camera, List<IRenderable> renderables)
        {
           
            // Reflect camera
            camera.Update();
            //camera.CreateProjectionReflected(reflection);
            
            Vector3 reflectedCameraPosition = ((FreeCamera)camera).Origin;
            reflectedCameraPosition.Y = -reflectedCameraPosition.Y + Position.Y * 2;

            Vector3 reflectedCameraTarget = ((FreeCamera)camera).Target;
            reflectedCameraTarget.Y = -reflectedCameraTarget.Y + Position.Y * 2;

            Matrix rotation = Matrix.CreateFromYawPitchRoll(((FreeCamera)camera).Yaw, ((FreeCamera)camera).Pitch, 0);

            Vector3 cameraRight = Vector3.Transform(new Vector3(1, 0, 0), rotation);
            Vector3 invUpVector = Vector3.Cross(cameraRight, reflectedCameraPosition- reflectedCameraTarget);

            Matrix reflectionViewMatrix = Matrix.CreateLookAt(reflectedCameraPosition, reflectedCameraTarget, invUpVector);

            effect.Parameters["gReflectionView"].SetValue(reflectionViewMatrix);

            Vector4 clipPlane = new Vector4(0, 1, 0, -Position.Y + 5);
           
            // Set the render target
            GraphicsDevice.SetRenderTarget(renderTarget2D);
            
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);


            foreach (IRenderable renderable in renderables)
            {
                if (renderable is Terrain)
                {
                    ((Terrain)renderable).boundingFrustum = null;
                }

                renderable.SetClipSpace(clipPlane);
                renderable.Draw(reflectionViewMatrix, camera.Projection, camera.Position);
                renderable.SetClipSpace(null);
            }

            GraphicsDevice.SetRenderTarget(null);
            effect.Parameters["gReflectionMap"]?.SetValue(renderTarget2D);
        }

        public void DrawWithVertexGrid(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None };
            
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            effect.Parameters["World"]?.SetValue(Matrix.Identity);
            effect.Parameters["View"]?.SetValue(View);
            effect.Parameters["Projection"]?.SetValue(Projection);
            effect.Parameters["gCameraPosition"].SetValue(CameraPosition);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
            }
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {

            DrawWithVertexGrid(View, Projection, CameraPosition);
            /*
            // Calculate the base transformtion by combining translation, rotation and scaling
            GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullClockwiseFace };

            Matrix baseWorld =  Matrix.CreateRotationX((float)Math.PI/2) * Matrix.CreateScale(new Vector3(Scale,Scale, Scale)) * Matrix.CreateTranslation(Position);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                    effect.Parameters["World"]?.SetValue(baseWorld);
                    effect.Parameters["View"]?.SetValue(View);
                    effect.Parameters["Projection"]?.SetValue(Projection);
                    effect.Parameters["gCameraPosition"]?.SetValue(CameraPosition);
                }
                mesh.Draw();
            }*/
        }

        void setEffectParameter(Effect effect, string paramName, object val)
        {
            if (val is Vector3)
                effect.Parameters[paramName]?.SetValue((Vector3)val);
            else if (val is Vector4)
                effect.Parameters[paramName]?.SetValue((Vector4)val);
            else if (val is bool)
                effect.Parameters[paramName]?.SetValue((bool)val);
            else if (val is Matrix)
                effect.Parameters[paramName]?.SetValue((Matrix)val);
            else if (val is Texture2D)
                effect.Parameters[paramName]?.SetValue((Texture2D)val);
            else if (val is float)
                effect.Parameters[paramName]?.SetValue((float)val);
        }

        public void SetClipSpace(Vector4? plane)
        {
        }
    }
}
