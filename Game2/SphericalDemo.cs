using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Game2
{
    class SphericalDemo
    {
        SphericalMesh sphericalMesh;
        CylinderMesh cylinderMesh;
        PlainMesh mirrorMesh;
        PlainMesh plainMesh;

        RenderTarget2D renderTarget;

        protected Matrix world = Matrix.CreateTranslation(0, 0, 0);
        protected Matrix view = Matrix.CreateLookAt(new Vector3(2, 3, -5), new Vector3(0, 0, 0), new Vector3(1, 0, 0));
        protected Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);
        protected Matrix gWVP = Matrix.Identity;
        protected Vector3 eyePosition = new Vector3();


        double totalTime = 0;
        protected Material material;

        double scale = 1.0;
        double angle = 0;
        double gTime = 0.0;
        double offsetU = 0.0f;
        float pos = -2.00f;

        private GraphicsDevice graphicsDevice;
        public SphericalDemo(GraphicsDevice _graphicsDevice, Texture2D texture, Effect textureEffect, Effect blackEffect, Material lightingMat)
        {
            this.graphicsDevice = _graphicsDevice;
            this.material = lightingMat;
            sphericalMesh = new SphericalMesh(_graphicsDevice, textureEffect, texture);
            cylinderMesh = new CylinderMesh(_graphicsDevice, textureEffect, texture);
            mirrorMesh = new PlainMesh(_graphicsDevice, blackEffect, new Vector3(5.0f, 0, 0), new Vector3(1.0f, 0, 0), 3, 2);
            plainMesh = new PlainMesh(_graphicsDevice, blackEffect, new Vector3(0.0f, -2.0f, 0), new Vector3(0.0f, 1.0f, 0.0f), 1.5f, 0.5f);

            renderTarget = new RenderTarget2D(graphicsDevice, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        }


        internal void draw(GameTime gameTime)
        {

            gTime += (gameTime.ElapsedGameTime.TotalMilliseconds / 5000) * 4;
            updateWVP();

            //drawTexturedSphere(gameTime);

            drawReflection(gameTime);
            //drawPlane();
            drawShadowCylinder(gameTime);
            drawTexturedCylinder(gameTime);

        }

        internal void drawTexturedSphere(GameTime gameTime)
        {
            graphicsDevice.SetVertexBuffer(sphericalMesh.vertexBuffer);
            graphicsDevice.Indices = sphericalMesh.indexBuffer;

            material.SetEffectParameters(sphericalMesh.effect);
            sphericalMesh.effect.Parameters["gTex0"]?.SetValue(sphericalMesh.texture);
            sphericalMesh.effect.Parameters["offset"]?.SetValue(new Vector2(0, (float)offsetU));

            //Matrix gWVP = Matrix.CreateTranslation(10.0f, 0.0f, 0.0f);

            sphericalMesh.effect.Parameters["gWVP"]?.SetValue(gWVP);
            sphericalMesh.effect.Parameters["alpha"]?.SetValue(0.5f);
            
            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                //FillMode = FillMode.WireFrame
            };

            graphicsDevice.RasterizerState = rasterizerState;
            SamplerState samplerState = new SamplerState
            {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Wrap
            };


            foreach (EffectPass pass in sphericalMesh.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, sphericalMesh.indexBuffer.IndexCount / 3);
            }

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

        }


        internal void drawTexturedCylinder(GameTime gameTime)
        {
            graphicsDevice.SetVertexBuffer(cylinderMesh.vertexBuffer);
            graphicsDevice.Indices = cylinderMesh.indexBuffer;

            material.SetEffectParameters(cylinderMesh.effect);
            cylinderMesh.effect.Parameters["gTex0"]?.SetValue(cylinderMesh.texture);

            cylinderMesh.effect.Parameters["gWVP"]?.SetValue(gWVP);
            cylinderMesh.effect.Parameters["offset"]?.SetValue(new Vector2((float)offsetU, 0));

            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                //FillMode = FillMode.WireFrame
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in cylinderMesh.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, cylinderMesh.indexBuffer.IndexCount / 3);
            }
        }

        internal void drawReflection(GameTime gameTime)
        {
            graphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0f, 0);

            // Part 1 
            // Enabling the stencil buffer and setting render states
            var stencilCreator = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 1,
                DepthBufferEnable = false
            };
            graphicsDevice.DepthStencilState = stencilCreator;

            // Part 2
            // Only renders to stencil buffer the mirror, not that the depth buffer was deactivated 
            var oneBlendState =  new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.Zero,
            };
            graphicsDevice.BlendState = oneBlendState;

            drawMirrorPlane();

            // Part 3 
            // Prepare for the second pass
            graphicsDevice.DepthStencilState = new DepthStencilState
            {
                //DepthBufferWriteEnable = true,
                StencilEnable = true,
                StencilFunction = CompareFunction.Equal, // ======== EQUAL
                ReferenceStencil = 1,
                StencilPass = StencilOperation.Keep,
                DepthBufferEnable = false
            };

            graphicsDevice.BlendState = BlendState.Opaque;

            // Part 4
            // Compute reflection of object
            Matrix matrixReflection = Matrix.CreateReflection(mirrorMesh.GetPlane());
            CylinderMesh cylinderMeshReflection = new CylinderMesh(graphicsDevice, cylinderMesh.effect, cylinderMesh.texture);
            cylinderMeshReflection.Transform(matrixReflection);

            // Part 5: Draw
            drawCylinder(cylinderMeshReflection);

            //graphicsDevice.Clear(ClearOptions.Stencil, Color.White, 0, 0);
            // RESET
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        internal void drawCylinder(CylinderMesh cylinderMesh)
        {
            graphicsDevice.SetVertexBuffer(cylinderMesh.vertexBuffer);
            graphicsDevice.Indices = cylinderMesh.indexBuffer;

            material.SetEffectParameters(cylinderMesh.effect);
            cylinderMesh.effect.Parameters["gTex0"]?.SetValue(cylinderMesh.texture);
            cylinderMesh.effect.Parameters["offset"]?.SetValue(new Vector2((float)offsetU, 0));

            cylinderMesh.effect.Parameters["gWVP"]?.SetValue(gWVP);
            cylinderMesh.effect.Parameters["alpha"]?.SetValue(0.5f);

            graphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.CullClockwiseFace,
                //FillMode = FillMode.WireFrame
            };

            foreach (EffectPass pass in cylinderMesh.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, cylinderMesh.indexBuffer.IndexCount / 3);
            }

        }

        internal void drawCylinderBlack(CylinderMesh cylinderMesh)
        {
            graphicsDevice.SetVertexBuffer(cylinderMesh.vertexBuffer);
            graphicsDevice.Indices = cylinderMesh.indexBuffer;

            plainMesh.effect.Parameters["gWVP"]?.SetValue(gWVP);
            plainMesh.effect.Parameters["myColor"]?.SetValue(new Vector4(0.0f, 0.0f, 0.0f, 0.8f));

            graphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                //FillMode = FillMode.WireFrame
            };

            foreach (EffectPass pass in plainMesh.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, cylinderMesh.indexBuffer.IndexCount / 3);
            }

        }

        internal void drawShadowCylinder(GameTime gameTime)
        {
            int stencil = 2;

            graphicsDevice.Clear(ClearOptions.Stencil, Color.Transparent, 0f, stencil-1);
            // Part 1 
            // Enabling the stencil buffer and setting render states
            var stencilCreator = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = stencil,
                DepthBufferEnable = false
            };
            graphicsDevice.DepthStencilState = stencilCreator;

            // Part 2
            // Only renders to stencil buffer the mirror, not that the depth buffer was deactivated 
            var oneBlendStateShadow = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.Zero,
            };
            graphicsDevice.BlendState = oneBlendStateShadow;

            drawPlane();

            Vector3 lightVec = new Vector3(-0.577f, -10, 0.577f);

            lightVec.Normalize();

            // Part 3 : Prepare shadow
            var shadowCreator = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Equal,
                StencilPass = StencilOperation.Increment,
                ReferenceStencil = stencil,
                DepthBufferEnable = false
            };
            graphicsDevice.DepthStencilState = shadowCreator;

            var oneBlendState = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.SourceAlpha,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
            };
            graphicsDevice.BlendState = oneBlendState;

            // Part 2: project object
            Matrix matrixShadow = Matrix.CreateShadow(lightVec, new Plane(new Vector3(0.00f, 1.0f, 0), -2.0f));

            Matrix matrixTranlsation = Matrix.CreateTranslation(new Vector3(0.0f, 0.001f, 0.00f)); // Translate to make the shasow appear above the ground
            CylinderMesh cylinderMeshShadow = new CylinderMesh(graphicsDevice, cylinderMesh.effect, cylinderMesh.texture);
            cylinderMeshShadow.Transform(Matrix.Multiply(matrixShadow, matrixTranlsation));

            //drawPlane();
            drawCylinderBlack(cylinderMeshShadow);

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        internal void drawPlane()
        {
            graphicsDevice.SetVertexBuffer(plainMesh.vertexBuffer);
            graphicsDevice.Indices = plainMesh.indexBuffer;

            plainMesh.effect.Parameters["gWVP"]?.SetValue(gWVP);
            plainMesh.effect.Parameters["myColor"]?.SetValue(new Vector4(0.8f,0.8f,0.8f,1.0f));
            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in plainMesh.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, plainMesh.indexBuffer.IndexCount / 3);
            }
        }

        internal void drawMirrorPlane()
        {
            graphicsDevice.SetVertexBuffer(mirrorMesh.vertexBuffer);
            graphicsDevice.Indices = mirrorMesh.indexBuffer;

            mirrorMesh.effect.Parameters["gWVP"]?.SetValue(gWVP);
            mirrorMesh.effect.Parameters["myColor"]?.SetValue(new Vector4(0.5f, 0.5f, 0.7f, 1.0f));
            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.CullClockwiseFace,
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in mirrorMesh.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mirrorMesh.indexBuffer.IndexCount / 3);
            }
        }


        internal void updateCameraRot(float p)
        {
            angle += p;
            angle %= 2 * Math.PI;
            //Debug.WriteLine(angle);
        }

        internal void update(GameTime gameTime)
        {
            totalTime = (totalTime + gameTime.ElapsedGameTime.TotalMilliseconds / 600) % (2*Math.PI);

            offsetU += (gameTime.ElapsedGameTime.TotalMilliseconds / 3000) % 1.0f;
            pos = (float)(pos + gameTime.ElapsedGameTime.TotalMilliseconds / 3000) % 10.0f;

            

            plainMesh = new PlainMesh(graphicsDevice, plainMesh.effect, new Vector3(0.0f, -2.0f, 0), new Vector3(0.0f, 1.0f, 0.0f), pos, pos);
        }

        public void updateWVP()
        {
            view = Matrix.CreateLookAt(new Vector3((float)scale*12, (float)scale * 25 * (float)Math.Sin(angle), (float)scale * 25 * (float)Math.Cos(angle)), new Vector3(0, 0, 0), new Vector3(1, 0, 0));
            //gWVP = world * view * projection;
        }

        public void SetWVP(Matrix matrix, Vector3 eyePos, Matrix world, Matrix view, Matrix projection)
        {
            this.eyePosition = eyePos;
            this.world = world;
            this.view = view;
            this.projection = projection;

            gWVP = matrix;
        }

        internal void updateScaling(int wheelValue)
        {
            if ( wheelValue < 0)
            {
                scale *= -90.0 / wheelValue;
            }
            else if (wheelValue > 0)
            {
                scale *= wheelValue / 90.0;
            }
        }
    }
}
