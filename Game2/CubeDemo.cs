using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Game2
{
    class CubeDemo
    {
        Cube cube;
        TriGen triGen;

        protected Matrix world = Matrix.CreateTranslation(0, 0, 0);
        protected Matrix view = Matrix.CreateLookAt(new Vector3(2, 3, -5), new Vector3(0, 0, 0), new Vector3(1, 0, 0));
        protected Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);
        protected Matrix gWVP = Matrix.Identity;

        double scale = 1.0;
        double angle = 0;
        double gTime = 0.0;

        private GraphicsDevice graphicsDevice;
        public CubeDemo(GraphicsDevice _graphicsDevice, Model model, Effect effect, Effect waveEffect, Effect diffuseEffect)
        {
            this.graphicsDevice = _graphicsDevice;

            cube = new Cube(_graphicsDevice, effect, diffuseEffect);
            triGen = new TriGen(_graphicsDevice, waveEffect);
        }


        internal void draw(GameTime gameTime)
        {

            gTime += (gameTime.ElapsedGameTime.TotalMilliseconds / 5000) * 4;
            updateWVP();
            drawCubeLight(gameTime);
            drawTri(gameTime);
        }
        /*
        internal void drawMesh(GameTime gameTime)
        {
            //graphicsDevice.SetVertexBuffer()
            ModelMeshPart meshPart = mesh.Model.Meshes[0].MeshParts[0];
            graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
            graphicsDevice.Indices = meshPart.IndexBuffer;


            mesh.diffuseEffect.Parameters["gWorldInverseTranspose"]?.SetValue(Matrix.Transpose(Matrix.Invert(world)));
            mesh.diffuseEffect.Parameters["gWVP"]?.SetValue(gWVP);


            mesh.diffuseEffect.Parameters["gAmbientMtrl"]?.SetValue(mesh.AmbientMtrl);
            mesh.diffuseEffect.Parameters["gAmbientLight"]?.SetValue(mesh.AmbientLight);
            mesh.diffuseEffect.Parameters["gDiffuseMtrl"]?.SetValue(mesh.DiffuseMtrl);
            mesh.diffuseEffect.Parameters["gDiffuseLight"]?.SetValue(mesh.DiffuseLight);
            mesh.diffuseEffect.Parameters["gLightVecW"]?.SetValue(mesh.LightVecW);

            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in mesh.diffuseEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.PrimitiveCount);
            }
        }*/

        internal void drawCubeLight(GameTime gameTime)
        {
            graphicsDevice.SetVertexBuffer(cube.vertexBuffer);
            graphicsDevice.Indices = cube.indexBuffer;

            cube.diffuseEffect.Parameters["gWorldInverseTranspose"]?.SetValue(Matrix.Transpose(Matrix.Invert(world)));
            cube.diffuseEffect.Parameters["gWVP"]?.SetValue(gWVP);


            cube.diffuseEffect.Parameters["gAmbientMtrl"]?.SetValue(cube.AmbientMtrl);
            cube.diffuseEffect.Parameters["gAmbientLight"]?.SetValue(cube.AmbientLight);
            cube.diffuseEffect.Parameters["gDiffuseMtrl"]?.SetValue(cube.DiffuseMtrl);
            cube.diffuseEffect.Parameters["gDiffuseLight"]?.SetValue(cube.DiffuseLight);
            cube.diffuseEffect.Parameters["gLightVecW"]?.SetValue(cube.LightVecW);



            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in cube.diffuseEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
            }
        }

        internal void drawCubeColorEffect(GameTime gameTime)
        {
            graphicsDevice.SetVertexBuffer(cube.vertexBuffer);
            graphicsDevice.Indices = cube.indexBuffer;

            EffectParameter effectParameter = cube.cubeEffect.Parameters["gWVP"];
            effectParameter.SetValue(gWVP);

            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in cube.cubeEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
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

        }

        public void updateWVP()
        {
            view = Matrix.CreateLookAt(new Vector3((float)scale*12, (float)scale * 25 * (float)Math.Sin(angle), (float)scale * 25 * (float)Math.Cos(angle)), new Vector3(0, 0, 0), new Vector3(1, 0, 0));
            gWVP = world * view * projection;
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
