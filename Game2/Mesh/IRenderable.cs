using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingShader
{
    public interface IRenderable
    {
        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition);

        public void SetClipSpace(Vector4? plane);

    }
}
