using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingShader
{
    public struct VertexBlend : IVertexType
    {
        public Vector3 Position;
        public Byte4 BlendIndices;
        public Vector4 BlendWeight;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;

        public static readonly VertexDeclaration MyVertexDeclaration = new VertexDeclaration(new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0),
                new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
                new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(44, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            });


        public VertexBlend(Vector3 Position, Byte4 BlendIndices, Vector4 BlendWeight, Vector3 Normal, Vector2 TextureCoordinate)
        {
            this.Position = Position;
            this.BlendIndices = BlendIndices;
            this.BlendWeight = BlendWeight;
            this.Normal = Normal;
            this.TextureCoordinate = TextureCoordinate;
        }

        public VertexDeclaration VertexDeclaration
        {
            get { return MyVertexDeclaration; }
        }

    }
}
