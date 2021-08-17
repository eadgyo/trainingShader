using Game2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TrainingShader.Particles;

namespace TrainingShader
{
    public abstract class PSystem
    {
        protected string name;
        protected Effect effect;
        protected Texture2D tex;
        protected VertexBuffer vertexBuffer;
        protected IndexBuffer indexBuffer;

        // Conversion from local to world
        public Matrix World { get; set; }
        // Conversion from world to local
        public Matrix InvWorld { get; set; }

        protected float time;
        protected Vector3 accel;
        protected BoundingBox box;
        protected int maxNumParticles;
        protected float timePerParticle;

        protected List<Particle> particles;
        protected List<Particle> aliveParticles;
        protected List<Particle> deadParticles;

        protected GraphicsDevice graphicsDevice;

        public PSystem(string fxName, string techName, string texName, Vector3 accel, ContentManager contentManger, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.name = techName;
            particles = new List<Particle>(maxNumParticles);
            aliveParticles = new List<Particle>(maxNumParticles);
            deadParticles = new List<Particle>(maxNumParticles);

            effect = contentManger.Load<Effect>(fxName);
            tex = contentManger.Load<Texture2D>(texName);

            this.accel = accel;

            for (int i = 0; i <maxNumParticles; i++)
            {
                particles[i] = new Particle
                {
                    lifeTime = -1.0f,
                    initialSize = 0.0f
                };
                
            }
        }

        public abstract void InitParticle(Particle p);

        public void SetTime(float t)
        {
            time = t;
        }

        public void SetWorldMatrix(Matrix world)
        {
            this.World = world;
            this.InvWorld = Matrix.Invert(world);
        }


        public void AddParticle()
        {
            // If any dead particles avalaible
            if (deadParticles.Count > 0)
            {
                Particle p = deadParticles[^1];
                InitParticle(p);

                deadParticles.RemoveAt(deadParticles.Count - 1);
                aliveParticles.Add(p);
            }
        }

        public void Update(GameTime gameTime)
        {
            deadParticles.Clear();
            aliveParticles.Clear();

            for (int i = 0; i < maxNumParticles; i++)
            {
                // Is the particle dead
                if ((time - particles[i].initialTime) > particles[i].lifeTime)
                {
                    deadParticles.Add(particles[i]);
                }
                else
                {
                    aliveParticles.Add(particles[i]);
                }
            }

            if (timePerParticle > 0.0f)
            {
                // Emit particle
                float timeAccum = 0.0f;
                timeAccum += (float) gameTime.ElapsedGameTime.TotalMilliseconds;
                while ( timeAccum >= timePerParticle)
                {
                    AddParticle();
                    timeAccum -= timePerParticle;
                }
            }
        }

        public void Draw(int vpHeight, Camera camera)
        {
            Vector3 eyePosL = Vector3.Transform(camera.Position, InvWorld);
            Matrix WVP = World * camera.View * camera.Projection; 

            effect.Parameters["gEyePosL"]?.SetValue(eyePosL);
            effect.Parameters["gTime"]?.SetValue(time);
            effect.Parameters["gWVP"]?.SetValue(WVP);

            effect.Parameters["gViewportHeight"]?.SetValue(vpHeight);

            Vector3 minW = Vector3.Transform(box.Min, World);
            Vector3 maxW = Vector3.Transform(box.Max, World);
            BoundingBox boxWorld = new BoundingBox(minW, maxW);
            
            if (camera.BoundingVolumeIsInView(boxWorld))
            {
                Particle[] p = new Particle[aliveParticles.Count];
                int vbIndex = 0;

                for (int i = 0; i < aliveParticles.Count; i++)
                {
                    p[i] = aliveParticles[i];
                    vbIndex++;
                }

                vertexBuffer.SetData(p);

                if (vbIndex > 0)
                {

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, indexBuffer.IndexCount / 3);
                    }
                }
            }


        }

    }
}
