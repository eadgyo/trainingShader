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


        private GraphicsDevice graphicsDevice;
        public SphericalDemo(GraphicsDevice _graphicsDevice, Texture2D texture, Effect textureEffect, Material lightingMat)
        {
            this.graphicsDevice = _graphicsDevice;
            this.material = lightingMat;
            sphericalMesh = new SphericalMesh(_graphicsDevice, textureEffect, texture);
            cylinderMesh = new CylinderMesh(_graphicsDevice, textureEffect, texture);
        }


        internal void draw(GameTime gameTime)
        {

            gTime += (gameTime.ElapsedGameTime.TotalMilliseconds / 5000) * 4;
            updateWVP();

            drawTexturedCylinder(gameTime);
            drawTexturedSphere(gameTime);
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

            //graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            BlendState blendState = new BlendState();
            //blendState.

            /*
            //graphicsDevice.BlendState = BlendState.Opaque;
            DepthStencilState depthStencilState = new DepthStencilState();
            depthStencilState.DepthBufferEnable = true;
            depthStencilState.DepthBufferWriteEnable = true;
            depthStencilState.DepthBufferFunction = CompareFunction.LessEqual;*/

            //graphicsDevice.DepthStencilState = depthStencilState;
            //graphicsDevice.SamplerStates[0] = samplerState;
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

            //graphicsDevice.

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in cylinderMesh.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, cylinderMesh.indexBuffer.IndexCount / 3);
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
