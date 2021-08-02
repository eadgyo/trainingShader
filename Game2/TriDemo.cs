using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Game2
{
    class TriDemo
    {
        TriGen triGen;

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

        private GraphicsDevice graphicsDevice;
        public TriDemo(GraphicsDevice _graphicsDevice, Texture2D texture, Texture2D texture2, Texture2D blendMap, Texture2D normalMap, Effect textureEffect, Effect multiTextureEffect, Material lightingMat, float texScale)
        {
            this.graphicsDevice = _graphicsDevice;
            this.material = lightingMat;
            triGen = new TriGen(_graphicsDevice, textureEffect, multiTextureEffect, texture, texture2, blendMap, normalMap, texScale);
        }


        internal void draw(GameTime gameTime)
        {

            gTime += (gameTime.ElapsedGameTime.TotalMilliseconds / 5000) * 4;
            updateWVP();

            drawMultiTextured(gameTime);
        }

        internal void drawMultiTextured(GameTime gameTime)
        {
            graphicsDevice.SetVertexBuffer(triGen.vertexBufferGrid);
            graphicsDevice.Indices = triGen.indexBufferGrid;

            material.SetEffectParameters(triGen.multiTextureEffect);
            triGen.multiTextureEffect.Parameters["gTex0"]?.SetValue(triGen.texture);
            triGen.multiTextureEffect.Parameters["gTex1"]?.SetValue(triGen.texture2);
            triGen.multiTextureEffect.Parameters["blendMap"]?.SetValue(triGen.blendMap);


            triGen.multiTextureEffect.Parameters["gWVP"]?.SetValue(gWVP);

            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in triGen.multiTextureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triGen.indices.Count / 3);
            }
        }

        internal void drawTriTextured(GameTime gameTime)
        {
            graphicsDevice.SetVertexBuffer(triGen.vertexBufferGrid);
            graphicsDevice.Indices = triGen.indexBufferGrid;

            material.SetEffectParameters(triGen.textureEffect);
            triGen.textureEffect.Parameters["gTex0"]?.SetValue(triGen.texture);
            triGen.textureEffect.Parameters["gNormal0"]?.SetValue(triGen.normal);

            triGen.textureEffect.Parameters["gWVP"]?.SetValue(gWVP);
            triGen.textureEffect.Parameters["World"]?.SetValue(world);
            triGen.textureEffect.Parameters["View"]?.SetValue(view);
            triGen.textureEffect.Parameters["Projection"]?.SetValue(projection);
            triGen.textureEffect.Parameters["gWorldInvTrans"]?.SetValue(Matrix.Invert(Matrix.Transpose(gWVP)));
            triGen.textureEffect.Parameters["gEyePos"]?.SetValue(eyePosition);
            triGen.textureEffect.Parameters["gLightVecW"]?.SetValue(triGen.LightVecW);

            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in triGen.textureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triGen.indices.Count / 3);
            }
        }

        private void drawTri(GameTime gameTime)
        {
            graphicsDevice.SetVertexBuffer(triGen.vertexBufferGrid);
            graphicsDevice.Indices = triGen.indexBufferGrid;

            triGen.waveEffect.Parameters["gWVP"].SetValue(gWVP);
            triGen.waveEffect.Parameters["gTime"].SetValue((float)gTime);


            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in triGen.waveEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triGen.indices.Count / 3);
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
            
            
            triGen.LightVecW.Z = (float)Math.Cos(totalTime);
            if (triGen.LightVecW.Z < 0)
            {
                triGen.LightVecW.Z = 0;
                triGen.LightVecW.Y = (float)Math.Abs(Math.Sin(totalTime));
            }
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
