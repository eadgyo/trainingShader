using Game2.Vertex;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game2
{
    public class PlainMesh
    {
        public VertexBuffer vertexBuffer;
        public IndexBuffer indexBuffer;
        public VertexPNT[] vertices = new VertexPNT[4];
        public Vector3 normal = new Vector3(1.0f, 0, 0);
        public Vector3 position = new Vector3(5.0f, 0, 0);
        public float ScaleY = 3;
        public float ScaleZ = 2;

        public Effect effect;

        public PlainMesh(GraphicsDevice graphicsDevice, Effect effect)
        {
            this.effect = effect;

            BuildVertexBuffer(graphicsDevice);
            BuildIndicesBuffer(graphicsDevice);
        }

        public void BuildVertexBuffer(GraphicsDevice graphicsDevice)
        {
            normal.Normalize();
            Vector3 left = Vector3.Cross(Vector3.Up, normal);

            vertices[0] = new VertexPNT(position  - left * ScaleZ - ScaleY * Vector3.Up, normal, new Vector2(0, 0));
            vertices[1] = new VertexPNT(position  - left * ScaleZ + ScaleY * Vector3.Up, normal, new Vector2(0, 1));
            vertices[2] = new VertexPNT(position + left * ScaleZ + ScaleY * Vector3.Up, normal, new Vector2(1, 1));
            vertices[3] = new VertexPNT(position + left * ScaleZ - ScaleY * Vector3.Up, normal, new Vector2(1, 0));


            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPNT), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPNT>(vertices);
        }

        public void BuildIndicesBuffer(GraphicsDevice graphicsDevice)
        {
            short[] k = new short[6];

            // Front face
            k[0] = 0; k[1] = 1; k[2] = 2;
            k[3] = 0; k[4] = 2; k[5] = 3;

            indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), k.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(k);
        }
    }
}
