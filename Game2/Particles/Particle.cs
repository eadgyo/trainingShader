using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TrainingShader.Particles
{
    public struct Particle : IVertexType
    {
        [DataMember]
        public Vector3 initialPos;
        [DataMember]
        public Vector3 initialVelocity;
        [DataMember]
        public float initialSize;
        [DataMember]
        public float initialTime;
        [DataMember]
        public float lifeTime;
        [DataMember]
        public float mass;
        [DataMember]
        public Vector4 initialColor;

 
        public static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(new VertexElement[]
        {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(24, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(26, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(30, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3),
            new VertexElement(34, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4),
            new VertexElement(38, VertexElementFormat.Vector4, VertexElementUsage.Color, 0)
        });

        public VertexDeclaration VertexDeclaration => vertexDeclaration;
    }
}
