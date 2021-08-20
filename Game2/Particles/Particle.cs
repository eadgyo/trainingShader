using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TrainingShader.Particles
{
    public class Particle
    {
        public ParticleVertex[] particles;
        public short[] indices;
        public int index;
        public Particle()
        {
            particles = new ParticleVertex[4];
            particles[0].tex = new Vector2(0.0f, 1.0f);
            particles[1].tex = new Vector2(0.0f, 0.0f);
            particles[2].tex = new Vector2(1.0f, 0.0f);
            particles[3].tex = new Vector2(1.0f, 1.0f);

            indices = new short[6];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;
        }


        public Vector3 initialPos
        {
            get
            {
                return particles[0].initialPos;
            }
            set
            {
                particles[0].initialPos = value;
                particles[1].initialPos = value;
                particles[2].initialPos = value;
                particles[3].initialPos = value;

            }
        }

        public Vector3 initialVelocity
        {
            get
            {
                return particles[0].initialVelocity;
            }
            set
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].initialVelocity = value;
                }
            }
        }

        public float initialSize
        {
            get
            {
                return particles[0].initialSize;
            }
            set
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].initialSize = value;
                }
            }
        }

        public float initialTime
        {
            get
            {
                return particles[0].initialTime;
            }
            set
            {
                for( int i = 0; i < particles.Length; i++)
                {
                    particles[i].initialTime = value;
                }
            }
        }

        public float lifeTime
        {
            get
            {
                return particles[0].lifeTime;
            }
            set
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].lifeTime = value;
                }
            }
        }

        public float mass
        {
            get
            {
                return particles[0].mass;
            }
            set
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].mass = value;
                }
            }
        }

        public Vector4 initialColor
        {
            get
            {
                return particles[0].initialColor;
            }
            set
            {
                for(int i = 0; i < particles.Length; i++)
                {
                    particles[i].initialColor = value;
                }
            }
        }

        public override string ToString()
        {
            string a =  "initialPos" + initialPos + " initialVelocity " + initialVelocity + " initialSize " + initialSize + " initialTime " + initialTime + " lifeTime " + lifeTime + " mass " + mass + " initialColor " + initialColor;

            for (int i = 0; i < 4; i++)
            {
                a += "\n";
                a += "[" + i + "] initialPos" + particles[i].initialPos + " tex " + particles[i].tex + " initialVelocity " + particles[i].initialVelocity + " initialSize " + particles[i].initialSize + " initialTime " + particles[i].initialTime + " lifeTime " + particles[i].lifeTime + " mass " + particles[i].mass + " initialColor " + particles[i].initialColor;
            }
            return a;
        }

    }
}
