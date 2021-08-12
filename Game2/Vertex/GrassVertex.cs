using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TrainingShader
{
    public struct GrassVertex : IVertexType
    {
        [DataMember]
        public Vector3 Position;
        [DataMember]
        public Vector3 QuadPos;
        [DataMember]
        public Vector2 Tex0;
        [DataMember]
        public float Amplitude;
        [DataMember]
        public Vector4 ColorOffset;


        public GrassVertex(Vector3 position, Vector2 tex0, float amplitude)
        {
            this.Position = position;
            this.QuadPos = new Vector3();
            this.Tex0 = tex0;
            this.Amplitude = amplitude;
            this.ColorOffset = new Vector4(0.0f,0.0f,0.0f,0.0f);
        }

        public static readonly VertexDeclaration MyVertexDeclaration = new VertexDeclaration(new VertexElement[]
        {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(32, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(36, VertexElementFormat.Vector4, VertexElementUsage.Color, 0)
        });

        public VertexDeclaration VertexDeclaration
        {
            get { return MyVertexDeclaration;  }
        }

        public override string ToString()
        {
            return "Position " + this.Position + "   QuadPos " + this.QuadPos + "   tex0 " + Tex0 + "   amplitude " + Amplitude + "    Color " + ColorOffset + " ";
        }
    }
}
