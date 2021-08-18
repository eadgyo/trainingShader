using Game2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TrainingShader.Particles
{
    public class FiringRing : PSystem
    {
        public FiringRing(string fxName, string techName, string texName, Vector3 accel, ContentManager contentManger, GraphicsDevice graphicsDevice) : base(fxName, techName, texName, accel, contentManger, graphicsDevice)
        {

        }

        public override void Draw(int vpHeight, Camera camera)
        {

            base.Draw(vpHeight, camera);
        }

        public override void InitParticle(Particle p)
        {
            // Time particle is created relative to the global running time of the particle system
            p.initialTime = time;

            // Flare lives for 2-4 secoinds
            p.lifeTime = MathUtils.GetRandomFloat(2.0f, 4.0f);

            // Initial size in pixels
            p.initialSize = MathUtils.GetRandomFloat(10.0f, 15.0f);

            // Give a very small initial velocity to give the flares some randomness
            p.initialVelocity = MathUtils.GenRandomNormalizedVec();

            // Scalar value used in vertex shader as an amplitude factor
            p.mass = MathUtils.GetRandomFloat(1.0f, 2.0f);

            // Start color at 50-100% intensity when born for variation
            p.initialColor = MathUtils.GetRandomFloat(0.5f, 1.0f) * Color.White.ToVector4();

            // Generate random particles on the ring in polar coordinates
            // random radius and random angle.
            float r = MathUtils.GetRandomFloat(50.0f, 55.0f);
            float t = MathUtils.GetRandomFloat(0, (float)(2.0f * Math.PI));

            // Convert to Cartesian coordinates.
            Vector3 initialPos = new Vector3();
            initialPos.X = (float)(r * Math.Cos(t));
            initialPos.Y = (float)(r * Math.Sin(t));

            // Random depth value in [-1, 1] (depth of the ring)
            initialPos.Z = MathUtils.GetRandomFloat(-1.0f, 1.0f);

            p.initialPos = initialPos;
 
            //Debug.WriteLine(p);
        }
    }
}
