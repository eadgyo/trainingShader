using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game2
{
    class TriGen
    {
        public VertexBuffer vertexBufferGrid;
        public IndexBuffer indexBufferGrid;

        public Effect waveEffect;
        GraphicsDevice graphicsDevice;


        public List<Vector3> verts = new List<Vector3>();
        public List<short> indices = new List<short>();

        public TriGen(GraphicsDevice graphicsDevice, Effect waveEffect)
        {
            this.graphicsDevice = graphicsDevice;

            this.waveEffect = waveEffect;


            GenTriGrid(100, 100, 1.0f, 1.0f, new Vector3(0.0f, 0.0f, 0.0f), verts, indices);
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
                    indices[k] = (short)(i * numVertCols + j);
                    indices[k + 1] = (short)(i * numVertCols + j + 1);
                    indices[k + 2] = (short)((i + 1) * numVertCols + j);

                    indices[k + 3] = (short)((i + 1) * numVertCols + j);
                    indices[k + 4] = (short)(i * numVertCols + j + 1);
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
