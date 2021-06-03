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
        VertexBuffer vertexBufferGrid;
        IndexBuffer indexBufferGrid;

        Effect cubeEffect;
        BasicEffect basicEffect2;
        Effect waveEffect;


        List<Vector3> verts = new List<Vector3>();
        List<short> indices = new List<short>();

        Matrix world =Matrix.CreateTranslation(0, 0, 0);
        Matrix view = Matrix.CreateLookAt(new Vector3(2, 3, -5), new Vector3(0, 0, 0), new Vector3(1, 0, 0));
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);

        double angle = 0;
        double gTime = 0.0;

        private GraphicsDevice graphicsDevice;
        public CubeDemo(GraphicsDevice _graphicsDevice, Effect effect, Effect waveEffect)
        {
            this.graphicsDevice = _graphicsDevice;
            this.cubeEffect = effect;
            this.basicEffect2 = new BasicEffect(graphicsDevice);
            this.waveEffect = waveEffect;

            BuildVertexBuffer();
            BuildIndicesBuffer();

            GenTriGrid(100, 100, 1.0f, 1.0f, new Vector3(0.0f, 0.0f, 0.0f), verts, indices);
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
            gTime +=( gameTime.ElapsedGameTime.TotalMilliseconds / 5000 )* 4;

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            view = Matrix.CreateLookAt(new Vector3(25, 50*(float)Math.Sin(angle), -50*(float)Math.Cos(angle)), new Vector3(0, 0, 0), new Vector3(1, 0, 0));

            Matrix gWVP = world * view * projection;

            basicEffect2.World = world;
            basicEffect2.View = view;
            basicEffect2.Projection = projection;

            EffectParameter effectParameter = cubeEffect.Parameters["gWVP"];
            effectParameter.SetValue(gWVP);

            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                //FillMode = FillMode.WireFrame
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in cubeEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
            }

            DrawTri();
        }
        
        private void DrawTri()
        {
            graphicsDevice.SetVertexBuffer(vertexBufferGrid);
            graphicsDevice.Indices = indexBufferGrid;

            Matrix gWVP = world * view * projection;
            waveEffect.Parameters["gWVP"].SetValue(gWVP);
            waveEffect.Parameters["gTime"].SetValue((float)gTime);


            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame
            };

            graphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in waveEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Count / 3);
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
            float dz,
            Vector3 center,
            List<Vector3> verts,
            List<short> indices)
        {
            int numVertices = numVertRows * numVertCols;
            int numCellRows = numVertRows - 1;
            int numCellCols = numVertCols - 1;

            int numTris = numCellRows * numCellCols * 2;


            float width = (float)numCellCols * dx;
            float depth = (float)numCellRows * dz;

            // --------- Build Vertices --------- 
            for (uint i = 0; i < numVertices; i++)
                verts.Add(Vector3.Zero);

            float xOffset = -width * 0.5f;
            float zOffset = depth * 0.5f;

            int k = 0;
            for (float i = 0; i < numVertRows; i++)
            {
                for (float j = 0; j < numVertCols; j++)
                {
                    // Negate the depth coordiante to put in
                    // quadrant four. Then offset to center about
                    // coordinate system.
                    Vector3 v = verts[k];
                    v.X = -2.0f;
                    v.Y = -i * dz + zOffset;
                    v.Z = j * dx + xOffset;

                    // Translate so that the center of the grid is at the 
                    // specified 'center' parameter
                    Matrix T = Matrix.CreateTranslation(center);
                    verts[k] = Vector3.Transform(v, T);

                    k++;
                }
            }
            VertexPosition[] vertsColors = new VertexPosition[verts.Count];
            for (int i = 0; i < verts.Count; i++)
            {
                vertsColors[i] = new VertexPosition(verts[i]);
            }
            vertexBufferGrid = new VertexBuffer(graphicsDevice, typeof(VertexPosition), verts.Count, BufferUsage.WriteOnly);
            vertexBufferGrid.SetData<VertexPosition>(vertsColors);

            // --------- Build indices ---------
            for (uint i = 0; i < numTris * 3; i++)
                indices.Add(0);

            k = 0;
            for (short i = 0; i < (short)numCellRows; i++)
            {
                for (short j = 0; j < (short)numCellCols; j++)
                {
                    indices[k] =     (short) (     i * numVertCols + j);
                    indices[k + 1] = (short) (     i * numVertCols + j + 1);
                    indices[k + 2] = (short)((i + 1) * numVertCols + j);

                    indices[k + 3] = (short)((i + 1) * numVertCols + j);
                    indices[k + 4] = (short)(      i * numVertCols + j + 1);
                    indices[k + 5] = (short)((i + 1) * numVertCols + j + 1);

                    // next Quad
                    k += 6;
                }
            }

            indexBufferGrid = new IndexBuffer(graphicsDevice, typeof(short), indices.Count, BufferUsage.WriteOnly);
            indexBufferGrid.SetData<short>(indices.ToArray());

        }

    }
}
