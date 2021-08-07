using Game2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingShader
{
    public class WalkingCamera : FreeCamera
    {
        Terrain terrain;
        public float PersonHeight { get; set; }

        public bool ActiveCorrection { get; set; }

        public WalkingCamera(Vector3 Position, float Yaw, float Pitch, Terrain Terrain, GraphicsDevice graphicsDevice) : base(Position, Yaw, Pitch, graphicsDevice)
        {
            this.terrain = Terrain;
            this.PersonHeight = 40f;
            this.ActiveCorrection = true;
        }

        public void Move(Vector3 translation)
        {
            var distance = translation.Length();
            var vec = translation;
            var computedOrigin = Origin + vec;
            computedOrigin.Y = terrain.GetHeight(computedOrigin.X, computedOrigin.Z) + PersonHeight;

            if (ActiveCorrection == true && computedOrigin - Origin != Vector3.Zero)
            {
                // Correct speed
                var diff = computedOrigin - Origin;
                diff.Normalize();
                computedOrigin = Origin + diff * distance;

                // Correct new position
                computedOrigin.Y = terrain.GetHeight(computedOrigin.X, computedOrigin.Z) + PersonHeight;
            }

            Origin = computedOrigin;
        }
    }
}
