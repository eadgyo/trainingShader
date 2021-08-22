using Game2;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingShader
{
    public class ReflectiveMaterial : Material
    {
        public TextureCube CubeMap { get; set; }

        public ReflectiveMaterial(TextureCube textureCube)
        {
            CubeMap = textureCube;
        }

        public override void SetEffectParameters(Effect effect)
        {
            effect.Parameters["gCubeMap"]?.SetValue(CubeMap);
        }
    }
}
