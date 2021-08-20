using Game2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingShader.Particles
{
    public class RainSystem : PSystem
    {
        private Camera camera;

        public RainSystem(Camera camera, string fxName, string techName, string texName, Vector3 accel, ContentManager contentManger, GraphicsDevice graphicsDevice) : base(fxName, techName, texName, accel, contentManger, graphicsDevice)
        {
            this.camera = camera;
        }

        public override void InitParticle(Particle p)
        {
            // Generate about the camera
            Vector3 initialPos = new Vector3() + ((FreeCamera) camera).Origin;

            // Spread the particles out on xz-plane
            initialPos.X += MathUtils.GetRandomFloat(-100.0f, 100.0f);
            initialPos.Z += MathUtils.GetRandomFloat(-100.0f, 100.0f);

            // Generate above the camera
            initialPos.Y += MathUtils.GetRandomFloat(50.0f, 55.0f);

            p.initialPos = initialPos;

            p.initialTime = time;
            p.lifeTime = MathUtils.GetRandomFloat(2.0f, 5.0f);
            p.initialColor = Color.White.ToVector4();
            p.initialSize = MathUtils.GetRandomFloat(0.5f, 1.0f);

            Vector3 initialVelocity = new Vector3();
            initialVelocity.X = MathUtils.GetRandomFloat(-1.5f, 0.0f);
            initialVelocity.Y = MathUtils.GetRandomFloat(-50.0f, -45.0f);
            initialVelocity.Z = MathUtils.GetRandomFloat(-0.2f, 0.2f);
            p.initialVelocity = initialVelocity;
        }
    }
}
