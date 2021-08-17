using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TrainingShader;

namespace Game2
{
    public class SkinnedModel
    {
        private Model model;
        private GraphicsDevice graphicsDevice;
        private ContentManager content;

        private Matrix[] modelTransforms;

        private SkinningData skinningData;

        public Vector3 Position, Rotation, Scale;

        public Model Model { get { return model; } }
        private Effect skinningEffect;

        public SkinnedAnimationPlayer Player { get; private set; }


        public SkinnedModel(Model Model, Vector3 Position, Vector3 Rotation, Vector3 Scale,
            GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            this.model = Model;
            this.graphicsDevice = GraphicsDevice;
            this.content = Content;
            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;

            modelTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            this.skinningData = model.Tag as SkinningData;
            this.Player = new SkinnedAnimationPlayer(this.skinningData);

            setNewEffect();
        }

        private void setNewEffect()
        {
            skinningEffect = content.Load<Effect>("SkinnedShader");

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach(ModelMeshPart part in mesh.MeshParts)
                {
                    LightingMaterial lightingMaterial = new LightingMaterial();
                    lightingMaterial.SetEffectParameters(skinningEffect);
                    part.Effect = skinningEffect;
                }
            }
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            Matrix baseWorld = Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index] * baseWorld;

                Matrix gWVP = localWorld * View * Projection;

                foreach (Effect effect in mesh.Effects)
                {

                    effect.Parameters["gWVP"]?.SetValue(gWVP);
                    effect.Parameters["gWorldInvTrans"]?.SetValue(Matrix.Transpose(Matrix.Invert(localWorld)));

                    effect.Parameters["gFinalXForms"]?.SetValue(Player.SkinTransforms);

                    setEffectParameter(effect, "World", localWorld);
                    setEffectParameter(effect, "View", View);
                    setEffectParameter(effect, "Projection", Projection);
                    setEffectParameter(effect, "gEyePos", CameraPosition);
                    setEffectParameter(effect, "gWorldInverseTranspose", Matrix.Transpose(Matrix.Invert(localWorld)));
                    setEffectParameter(effect, "gWVP", gWVP);

                    // Diffuse
                    setEffectParameter(effect, "gLightVecW", new Vector3(-100, 0, 10));

                }
                mesh.Draw();
                
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    var buffer = meshPart.VertexBuffer;
                    var count = buffer.VertexCount;
                    var vertexDeclaration = buffer.VertexDeclaration;

                    var vertices = new VertexBlend[count];
                    buffer.GetData<VertexBlend>(vertices); 
                    Debug.WriteLine("VertexBuffer");
                }
            }
        }

        void setEffectParameter(Effect effect, string paramName, object val)
        {
            /*if (effect.Parameters[paramName] == null)
            {
                Debug.WriteLine("Mesh::SetEffectParameter() -> Not found " + paramName);
                return;
            }*/

            if (val is Vector3)
                effect.Parameters[paramName]?.SetValue((Vector3)val);
            else if (val is Vector4)
                effect.Parameters[paramName]?.SetValue((Vector4)val);
            else if (val is bool)
                effect.Parameters[paramName]?.SetValue((bool)val);
            else if (val is Matrix)
                effect.Parameters[paramName]?.SetValue((Matrix)val);
            else if (val is Texture2D)
                effect.Parameters[paramName]?.SetValue((Texture2D)val);
            else if (val is float)
                effect.Parameters[paramName]?.SetValue((float)val);
        }


        public void Update(GameTime gameTime)
        {
            Matrix world = Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) * Matrix.CreateTranslation(Position);

            Player.Update(gameTime.ElapsedGameTime, world);
        }

    }
}
