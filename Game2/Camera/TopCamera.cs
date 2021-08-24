using Game2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingShader
{
    public class TopCamera : Camera
    {
        public Vector3 Origin { get; set; }
        public Vector3 Target { get; set; }
    
        public TopCamera(Vector3 Position, Vector3 Target, GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            this.Origin = Position;
            this.Target = Target;
        }

        public override void Update()
        {
            this.View = Matrix.CreateLookAt(Origin, Target, Vector3.Left);
        }

    }
}
