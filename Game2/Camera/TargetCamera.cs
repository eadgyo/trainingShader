using Game2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingShader
{
    public class TargetCamera : Camera
    {
        public Vector3 Origin { get; set; }
        public Vector3 Target { get; set; }
    
        public TargetCamera(Vector3 Position, Vector3 Target, GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            this.Origin = Position;
            this.Target = Target;
        }

        public override void Update()
        {
            Vector3 forward = Target - Position;
            Vector3 side = Vector3.Cross(forward, Vector3.Up);
            Vector3 up = Vector3.Cross(forward, side);
            this.View = Matrix.CreateLookAt(Position, Target, up);
        }

    }
}
