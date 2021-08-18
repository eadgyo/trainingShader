using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TrainingShader.Particles
{
    public struct ParticleVertex : IVertexType
    {
        public Vector3 initialPos;
        public Vector2 tex;
        public Vector3 initialVelocity;
        public float initialSize;
        public float initialTime;
        public float lifeTime;
        public float mass;
        public Vector4 initialColor;


        public static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(new VertexElement[]
        {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(32, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(36, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3),
            new VertexElement(40, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4),
            new VertexElement(44, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 5),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.Color, 0)
        });

        public VertexDeclaration VertexDeclaration => vertexDeclaration;
    }
}
