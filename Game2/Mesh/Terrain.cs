using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TrainingShader
{
    public class Terrain
    {
        private VertexPositionNormalTexture[] vertices;
        private int[] indices;

        private float[,] heights;

        private float height;
        private float cellSize;
        public int nRows { get; }
        public int nCols { get; }

        private int nVertices, nIndices;
        private Effect effect;
        private GraphicsDevice graphicsDevice;
        private Texture2D heightMap;
        private ContentManager content;

        private Texture2D tex0;
        private float texScale;
        private Vector3 sunDirection;
        private float noiseDistance = 200f;
        private int useNoise = 1;

        private int ActualDisplayer = 0;

        private float Radius = 0.05f;

        public int SubGridSize { get; set; }
        public int NRowsSubGrid { get; set; }
        public int NColsSubGrid { get; set; }
        public int RowOffsetSubGrid { get; set; }

        private BoundingFrustum boundingFrustum = null;

        private bool testBounding = true;

        private bool isUsingFustrum = true;
        private List<SubGrid> SubGrids { get; set; }

        public Terrain(Texture2D HeightMap, float CellSize, float Height, Texture2D Tex0, float TexScale, Vector3 SunDirection, GraphicsDevice graphicsDevice, ContentManager Content)
        {
            this.heightMap = HeightMap;
            this.cellSize = CellSize;
            this.nRows = HeightMap.Width;
            this.nCols = HeightMap.Height;
            this.height = Height;
            this.tex0 = Tex0;
            this.texScale = TexScale;
            this.sunDirection = SunDirection;

            this.graphicsDevice = graphicsDevice;
            this.content = Content;

            this.SubGridSize = 16;
            this.NRowsSubGrid = 0;
            this.NColsSubGrid = 0;

            // 1 vertex per pixel
            nVertices = nRows * nCols;

            // (Width-1) * (Length-1) cells, 2 triangles per cell
            nIndices = (nRows - 1) * (nCols - 1) * 6;
            SubGrids = new List<SubGrid>();

            LoadHeightMap();
            GenGeometry();
            SetEffect();
        }

        public void TestIncrement()
        {
            ActualDisplayer += 5;
            if (ActualDisplayer + 5 > NRowsSubGrid * NColsSubGrid)
            {
                ActualDisplayer = NRowsSubGrid * NColsSubGrid - 5;
            }
            Radius += 10f;
            testBounding = true;

            isUsingFustrum = true;
            Debug.WriteLine(Radius);
        }

        public void TestDecrement()
        {
            ActualDisplayer -= 5;
            if (ActualDisplayer < 0)
                ActualDisplayer = 0;

            Radius -= 10f;
            if (Radius <= 0.01f)
            {
                Radius = 10f;
            }
            isUsingFustrum = false;
            Debug.WriteLine(Radius);
        }

        private void SetEffect()
        {
            effect = content.Load<Effect>("TerrainEffect");
            Texture2D noise = content.Load<Texture2D>("noise");
            float noiseScale = 20.0f;
            float noiseDistance = 2500.0f;

            effect.Parameters["gTex0"]?.SetValue(tex0);
            effect.Parameters["gTexScale"]?.SetValue(texScale);
            effect.Parameters["gDirToSunW"]?.SetValue(sunDirection);
            effect.Parameters["gNoiseScale"]?.SetValue(noiseScale);
            effect.Parameters["gNoiseDistance"]?.SetValue(noiseDistance);
            effect.Parameters["gNoise"]?.SetValue(noise);
            effect.Parameters["gUseNoise"]?.SetValue(useNoise);
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 cameraPosition, BoundingFrustum BoundingFrustum)
        {
            if (testBounding == true)
            {
                this.boundingFrustum = BoundingFrustum;
                testBounding = false;
            }

            Matrix ViewProj = Matrix.Multiply(View, Projection);
            List<int> SubGridIndices = GetSubGridInBound(this.boundingFrustum);

            // Only draw if intersects bounding furstrum
            for (int i = 0; i < SubGridIndices.Count; i++)
            {
                Draw(SubGrids[SubGridIndices[i]], ViewProj, cameraPosition);
            }
        }

        public List<int> GetSubGridInBound(BoundingFrustum BoundingFrustum)
        {
            BoundingSphere boundingSphere = new BoundingSphere(Vector3.Zero, Radius);
            List<int> subGridsInBound = new List<int>();
            for (int i = 0; i < SubGrids.Count; i++)
            {
                if (isUsingFustrum)
                {
                    if (BoundingFrustum.Contains(SubGrids[i].BoundingBox) != ContainmentType.Disjoint)
                    {
                        subGridsInBound.Add(i);
                    }
                }
                else
                {
                    if (boundingSphere.Contains(SubGrids[i].BoundingBox) != ContainmentType.Disjoint)
                    {
                        subGridsInBound.Add(i);
                    }
                }

                /*
                if ( i >= this.ActualDisplayer && i < ActualDisplayer + 5 && i < NRowsSubGrid * NColsSubGrid)
                {
                    subGridsInBound.Add(i);
                }*/
            }
            return subGridsInBound;
        }

        private void Draw(SubGrid subGrid, Matrix ViewProj, Vector3 cameraPosition)
        {
            graphicsDevice.SetVertexBuffer(subGrid.VertexBuffer);
            graphicsDevice.Indices = subGrid.IndexBuffer;

            effect.Parameters["gViewProj"]?.SetValue(ViewProj);
            effect.Parameters["gPosCamera"]?.SetValue(cameraPosition);

            graphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid
            };
        
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, subGrid.IndexBuffer.IndexCount / 3);
            }
        }

        public void ChangeDistance(int useNoise)
        {
            effect.Parameters["gUseNoise"]?.SetValue(useNoise);
        }

        private void LoadHeightMap()
        {
            // Retrieve content color
            Color[] heightMapData = new Color[nRows * nCols];
            heightMap.GetData<Color>(heightMapData);

            // Create heights array
            heights = new float[nRows, nCols];
            // Get each value

            for (int z = 0; z < nCols; z++)
            {
                for (int x = 0; x < nRows; x++)
                {
                    // Get color
                    float amt = heightMapData[z * nRows + x].R;

                    // Scale to (0 - 1)
                    amt /= 255.0f;

                    // Scale to max height
                    heights[x, z] = amt * height;
                }
            }
            Filter3x3();
        }

        public void GenGeometry()
        {
            CreateVertices();
            CreateIndices();
            GenNormals();
            GenSubGrids();
        }

        public void GenSubGrids()
        {
            float nRowsSubGridF = (nRows - 1)/ (float)SubGridSize;
            NRowsSubGrid = (int) Math.Ceiling(nRowsSubGridF);
            float nColsSubGridF = (nCols - 1)/ (float)SubGridSize;
            NColsSubGrid = (int) Math.Ceiling(nColsSubGridF);
            RowOffsetSubGrid = nRows - 1 - NRowsSubGrid * SubGridSize;
            Debug.Assert(RowOffsetSubGrid == 0, "Cannot treat subGrid with not divisable width height"); 

            for (int subRow = 0; subRow < NRowsSubGrid; subRow++)
            {
                for (int subCol = 0; subCol < NColsSubGrid; subCol++)
                {
                    SubGrid subGrid = new SubGrid();

                    // Retrieve the vertices and triangle indices
                    List<int> l_vertices = new List<int>();

                    for (int r = 0; r < SubGridSize + 1; r++)
                    {
                        for (int c = 0; c < SubGridSize + 1; c++)
                        {
                            int realRow = subRow * SubGridSize + r;
                            int realCol = subCol * SubGridSize + c;
                            int indexVertices = realRow + realCol * nRows;

                            if (indexVertices < nVertices)
                            {
                                l_vertices.Add(indexVertices);
                            }
                        }
                    }
                    
                    List<int> l_indices = new List<int>();
                    for (int r = 0; r < SubGridSize; r++)
                    {
                        for (int c = 0; c < SubGridSize; c++)
                        {
                            int upperLeft = r * (SubGridSize + 1) + c;
                            int upperRight = upperLeft + 1;
                            int lowerLeft = upperLeft + (SubGridSize + 1);
                            int lowerRight = lowerLeft + 1;

                            l_indices.Add(upperLeft);
                            l_indices.Add(upperRight);
                            l_indices.Add(lowerLeft);

                            l_indices.Add(lowerLeft);
                            l_indices.Add(upperRight);
                            l_indices.Add(lowerRight);
                        }
                    }

                    // Transforms indices to vertex and indices buffer
                    subGrid.VertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, l_vertices.Count, BufferUsage.WriteOnly);
                    subGrid.IndexBuffer = new IndexBuffer(graphicsDevice, typeof(short), l_indices.Count, BufferUsage.WriteOnly);

                    VertexPositionNormalTexture[] vx = new VertexPositionNormalTexture[l_vertices.Count];
                    for (int i = 0; i < vx.Length; i++)
                    {
                        vx[i] = vertices[l_vertices[i]];
                    }
                    short[] idx = new short[l_indices.Count];
                    for (int i = 0; i < idx.Length; i++)
                    {
                        idx[i] = (short) l_indices[i];
                    }

                    subGrid.VertexBuffer.SetData(vx);
                    subGrid.IndexBuffer.SetData<short>(idx);
                    Vector3 v1= new Vector3(vx[0].Position.X, -5, vx[0].Position.Z);
                    Vector3 v2 = new Vector3(vx[^1].Position.X, height, vx[^1].Position.Z);
                    Vector3 min = new Vector3(Math.Min(v1.X, v2.X), Math.Min(v1.Y, v2.Y), Math.Min(v1.Z, v2.Z));
                    Vector3 max = new Vector3(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y), Math.Max(v1.Z, v2.Z));

                    subGrid.BoundingBox = new BoundingBox(min, max);
                    SubGrids.Add(subGrid);
                }
            }
        }


        private void CreateVertices()
        {
            vertices = new VertexPositionNormalTexture[nVertices];

            // Calculate the origin
            Vector3 offsetToCenter = -new Vector3((float)(cellSize * nRows / 2.0f), 0, (float)(cellSize * nCols / 2.0f));

            // For each pixel in the image
            for (int z = 0; z < nCols; z++)
            {
                for (int x = 0; x < nRows; x++)
                {
                    // Find Position based on grid coordinates and height in heightmap  
                    Vector3 position = new Vector3((float)(cellSize * x), heights[x, z], (float)(cellSize * z));

                    // Center to origin
                    position += offsetToCenter;

                    // Compute UV texture coordinates
                    Vector2 UV = new Vector2((float)x / nRows, (float)z / nCols);

                    vertices[z * nRows + x] = new VertexPositionNormalTexture(position, Vector3.Zero, UV);
                }
            }
        }

        private void CreateIndices()
        {
            indices = new int[nIndices];
            int i = 0;

            for (int x = 0; x < nRows-1; x++)
            {
                for (int z = 0; z < nCols-1; z++)
                {
                    // Find the indices of the corners
                    int upperLeft = z * nRows + x;
                    int upperRight = upperLeft + 1;
                    int lowerLeft = upperLeft + nRows;
                    int lowerRight = lowerLeft + 1;

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
        }

        public bool InBoundsArray(int x, int z)
        {
            return (x >= 0 && x < nRows && z < nCols && z >= 0);
        }

        public float GetHeight(float x, float z)
        {
            // Get cell
            float r = (x / cellSize + 0.5f * nRows);
            float c = (z / cellSize + 0.5f * nCols);

            int row = (int)Math.Floor(r);
            int col = (int)Math.Floor(c);

            // Test inbounds
            if (row >= 0 && row + 1 < nRows && col >= 0 && col + 1 < nCols)
            {
                float A = GetHeightArray(row, col);
                float B = GetHeightArray(row, col + 1);
                float C = GetHeightArray(row + 1, col);
                float D = GetHeightArray(row + 1, col + 1);

                float s = r - (float)row;
                float t = c - (float)col;

                // If upper triangle
                if (t < 1.0f - s)
                {
                    float uy = B - A;
                    float vy = C - A;

                    float b = A + t * uy + s * vy;
                    return b;
                }
                else // Lower triangle DCB
                {
                    float uy = C - D;
                    float vy = B - D;

                    float a =  D + (1.0f - t) * uy + (1.0f - s) * vy;
                    return a;
                }
            }
            return height;   
        }

        public float GetHeightArray(int x, int z)
        {
            if (InBoundsArray(x, z))
            {
                return heights[x, z];
            }

            return 0.0f;
        }

        public void Filter3x3()
        {
            float[,] tempHeights = new float[nRows, nCols];
            for (int x = 0; x < nRows; x++)
            {
                for (int z = 0; z < nCols; z++)
                {
                    tempHeights[x, z] = SampleHeight3x3(x, z);
                }
            }
            heights = tempHeights;
        }

        public float SampleHeight3x3(int x, int z)
        {
            float avg = 0.0f;
            float num = 0.0f;

            for (int m = x - 1; m <= x + 1; ++m)
            {
                for (int n = z - 1; n <= z + 1; ++n)
                {
                    if (InBoundsArray(n, m))
                    {
                        avg += GetHeightArray(m, n);
                        num += 1.0f;
                    }
                }
            }

            return avg / num;
        }
        
        public void GenNormals()
        {
            for (int i = 0; i < nIndices; i += 3)
            {
                Vector3 v1 = vertices[indices[i]].Position;
                Vector3 v2 = vertices[indices[i + 1]].Position;
                Vector3 v3 = vertices[indices[i + 2]].Position;

                //Cross the vectors between the corners to get the normal
                Vector3 normal = Vector3.Cross(v1 - v2, v1 - v3);
                normal.Normalize();

                vertices[indices[i]].Normal += normal;
                vertices[indices[i + 1]].Normal += normal;
                vertices[indices[i + 2]].Normal += normal;
            }
        }



    }
}
