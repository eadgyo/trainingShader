using Game2.Vertex;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game2
{
    public class Mesh
    {
        public VertexBuffer vertexBuffer;
        public IndexBuffer indexBuffer;

        public Effect diffuseEffect;

        public Vector4 AmbientMtrl = new Vector4(0.3f, 0.2f, 0.3f, 1.0f);
        public Vector4 AmbientLight = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);
        public Vector4 DiffuseMtrl = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
        public Vector4 DiffuseLight = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
        public Vector3 LightVecW = new Vector3(1.0f, 0.0f, 0.0f);

        public Model model;

        public Mesh(Model model, Effect diffuseEffect)
        {
            this.model = model;
            this.diffuseEffect = diffuseEffect;
        }

        public void buildVertexBuffer()
        {
            /*ModelMeshPart meshPart = model.Meshes[0].MeshParts[0];
            VertexPN[] vertices = new VertexPN[meshPart.NumVertices];

            for (int i = 0; i < meshPart.NumVertices; i++)
            {
                vertices[i] = new VertexPN(meshPart.VertexBuffer[])
            }*/

        }

        public void buildIndexBuffer()
        {


        }
    }
}
