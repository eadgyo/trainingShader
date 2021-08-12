using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingShader
{
    public class MathUtils
    {
        public static Random rand = new Random();

        public static float GetRandomFloat(float a, float b)
        {
            if (a >= b)
                return a;
            float f = ((float)rand.NextDouble());

            return (f * (b - a)) + a;
        }

        public static Vector3 GenRandomNormalizedVec()
        {
            float x = GetRandomFloat(-1.0f, 1.0f);
            float y = GetRandomFloat(-1.0f, 1.0f);
            float z = GetRandomFloat(-1.0f, 1.0f);

            Vector3 vec = new Vector3(x, y, z);
            vec.Normalize();
            return vec;
        }

    }
}
