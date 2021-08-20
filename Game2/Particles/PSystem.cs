using Game2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        protected short[] myIndices;
        protected ParticleVertex[] myParticles;

        // Conversion from local to world
        public Matrix World { get; set; }
        // Conversion from world to local
        public Matrix InvWorld { get; set; }

        protected float time;
        protected Vector3 accel;
        protected BoundingBox box;
        protected int maxNumParticles = 1000;
        protected float timePerParticle = 0.02f;

        protected List<Particle> particles;
        protected List<Particle> aliveParticles;
        protected List<Particle> deadParticles;

        protected GraphicsDevice graphicsDevice;
        private float timeAccum = 0.0f;

        public float TimePerParticle 
        {
            get
            {
                return timePerParticle;
            }
            set
            {
                timePerParticle = value;
            }
        }

        public PSystem(string fxName, string techName, string texName, Vector3 accel, ContentManager contentManger, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.name = techName;
            particles = new List<Particle>(maxNumParticles);
            aliveParticles = new List<Particle>(maxNumParticles);
            deadParticles = new List<Particle>(maxNumParticles);
            World = Matrix.Identity;

            effect = contentManger.Load<Effect>(fxName);
            tex = contentManger.Load<Texture2D>(texName);
            effect.Parameters["gTex"]?.SetValue(tex);

            this.accel = accel;

            myIndices = new short[6*maxNumParticles];
            myParticles = new ParticleVertex[4* maxNumParticles];
            for (int i = 0; i < maxNumParticles; i++)
            {
                Particle particle = new Particle
                {
                    lifeTime = -1.0f,
                    initialSize = 0.0f,
                    index = i
                };
                particles.Add(particle);
                deadParticles.Add(particle);
                
                for (int y = 0; y < 4; y++)
                {
                    ParticleVertex pv = particles[i].particles[y];
                    myParticles[y + i * 4] = pv;
                }
                
                for (int y = 0; y < 6; y++)
                {
                    myIndices[y + i * 6] = (short) (particles[i].indices[y] + i * 4);
                }

            }

            vertexBuffer = new VertexBuffer(graphicsDevice, ParticleVertex.vertexDeclaration, myParticles.Length, BufferUsage.WriteOnly);
            indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), myIndices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(myParticles);
            indexBuffer.SetData<short>(myIndices);
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
                int y = 0;
                for (int i = p.index * 4; i < p.index * 4 + 4; i++, y++)
                {
                    myParticles[i] = p.particles[y];
                }

                deadParticles.RemoveAt(deadParticles.Count - 1);
                aliveParticles.Add(p);
            }
        }

        public void Update(GameTime gameTime)
        {
            deadParticles.Clear();
            aliveParticles.Clear();
            SetTime(time + (float)gameTime.ElapsedGameTime.TotalSeconds);

            for (int i = 0; i < maxNumParticles; i++)
            {
                // Is the particle dead
                Particle p = particles[i];
               
                
                if ((time - particles[i].initialTime) > particles[i].lifeTime)
                {
                    deadParticles.Add(particles[i]);
                    for (int y = i*4; y < i*4 + 4; y++)
                    {
                        myParticles[y].lifeTime = -1.0f;
                    }

                    //Debug.WriteLine("lifeTime");
                }
                else
                {
                    aliveParticles.Add(particles[i]);
                }
            }
            
            if (timePerParticle > 0.0f)
            {
                // Emit particle
                timeAccum += (float) gameTime.ElapsedGameTime.TotalSeconds;
                while ( timeAccum >= timePerParticle)
                {
                    AddParticle();
                    timeAccum -= timePerParticle;
                }
            }
            else
            {
                while (deadParticles.Count != 0)
                {
                    AddParticle();
                }
            }

            vertexBuffer.SetData(myParticles);

        }

        public virtual void Draw(GameTime gametime, int vpHeight, Camera camera)
        {

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            Vector3 eyePosL = Vector3.Transform(camera.Position, InvWorld);
            Matrix WVP = camera.View * camera.Projection; 

            effect.Parameters["gEyePosL"]?.SetValue(eyePosL);
            effect.Parameters["gTime"].SetValue(time);
            effect.Parameters["gWVP"]?.SetValue(WVP);
            effect.Parameters["gTex"]?.SetValue(tex);
            effect.Parameters["gAccel"]?.SetValue(accel);

            effect.Parameters["gViewportHeight"]?.SetValue((float)vpHeight);

            Vector3 minW = Vector3.Transform(box.Min, World);
            Vector3 maxW = Vector3.Transform(box.Max, World);
            BoundingBox boxWorld = new BoundingBox(minW, maxW);

            if (camera.BoundingVolumeIsInView(boxWorld) || true)
            {
                Particle[] p = new Particle[aliveParticles.Count];
                int vbIndex = 0;

                for (int i = 0; i < aliveParticles.Count; i++)
                {
                    p[i] = aliveParticles[i];
                    vbIndex++;
                }



                if (vbIndex > 0 || true)
                {

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
                    }
                }
            }


        }

    }
}
