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
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        Effect basicEffect;
        BasicEffect basicEffect2;

        //Camera camera;

        Matrix world =Matrix.CreateTranslation(0, 0, 0);
        Matrix view = Matrix.CreateLookAt(new Vector3(2, 3, -5), new Vector3(0, 0, 0), new Vector3(1, 0, 0));
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);

        double angle = 0;

        private GraphicsDevice graphicsDevice;
        public CubeDemo(GraphicsDevice _graphicsDevice, Effect effect)
        {
            this.graphicsDevice = _graphicsDevice;
            this.basicEffect = effect;
            this.basicEffect2 = new BasicEffect(graphicsDevice);

            BuildVertexBuffer();
            BuildIndicesBuffer();
        }


        public void BuildVertexBuffer()
        {
            VertexPositionColor[] vertices = new VertexPositionColor[8];

            vertices[0] = new VertexPositionColor(new Vector3(-1.0f, -1.0f, -1.0f), Color.Red) ;
            vertices[1] = new VertexPositionColor(new Vector3(-1.0f, 1.0f, -1.0f), Color.Blue);
            vertices[2] = new VertexPositionColor(new Vector3(1.0f, 1.0f, -1.0f), Color.Green);
            vertices[3] = new VertexPositionColor(new Vector3(1.0f, -1.0f, -1.0f), Color.Red);
            vertices[4] = new VertexPositionColor(new Vector3(-1.0f, -1.0f, 1.0f), Color.Blue);
            vertices[5] = new VertexPositionColor(new Vector3(-1.0f, 1.0f, 1.0f), Color.Green);
            vertices[6] = new VertexPositionColor(new Vector3(1.0f, 1.0f, 1.0f), Color.Red);
            vertices[7] = new VertexPositionColor(new Vector3(1.0f, -1.0f, 1.0f), Color.Blue);

            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), 8, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColor>(vertices);
        }


        public void BuildIndicesBuffer()
        {
            short[] k = new short[36];

            // Front face
            k[0] = 0; k[1] = 1; k[2] = 2;
            k[3] = 0; k[4] = 2; k[5] = 3;

            // Back face
            k[6] = 4; k[7] = 6; k[8] = 5;
            k[9] = 4; k[10] = 7; k[11] = 6;

            // Left face
            k[12] = 4; k[13] = 5; k[14] = 1;
            k[15] = 4; k[16] = 1; k[17] = 0;

            // Right face
            k[18] = 3; k[19] = 2; k[20] = 6;
            k[21] = 3; k[22] = 6; k[23] = 7;

            // Top face
            k[24] = 1; k[25] = 5; k[26] = 6;
            k[27] = 1; k[28] = 6; k[29] = 2;

            // Bottom face
            k[30] = 4; k[31] = 0; k[32] = 3;
            k[33] = 4; k[34] = 3; k[35] = 7;

            indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), k.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(k);
        }

        internal void draw(GameTime gameTime)
        {
            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            view = Matrix.CreateLookAt(new Vector3(5, 10*(float)Math.Sin(angle), -10*(float)Math.Cos(angle)), new Vector3(0, 0, 0), new Vector3(1, 0, 0));

            Matrix gWVP = world * view * projection;

            basicEffect2.World = world;
            basicEffect2.View = view;
            basicEffect2.Projection = projection;

            EffectParameter effectParameter = basicEffect.Parameters["gWVP"];
            effectParameter.SetValue(gWVP);

            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                //FillMode = FillMode.WireFrame
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
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

        public void BuildProjectionMatrix()
        {
         

        }

        public void GenTriGrid(int numVertRows, 
            int numVertCols,
            float dx,
            float dy,
            Vector3 center,
            List<Vector3> verts,
            List<Vector3> indices)
        {
            int numVertices = numVertRows * numVertCols;
            int numCellRows = numVertRows - 1;
            int numCellCols = numVertCols - 1;


            int numTris = numCellRows * numCellCols



        }
    }
}
