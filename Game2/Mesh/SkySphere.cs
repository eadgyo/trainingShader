using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingShader
{
    public class SkySphere : IRenderable
    {
        public Model model;

        public Effect effect;

        public GraphicsDevice graphicsDevice;

        public TextureCube texture;

        public Vector3 position = new Vector3(0, 500, 0);

        public float Scale = 10000.0f;

        public SkySphere(string textureName, ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            model = contentManager.Load<Model>("skysphere");
            texture = contentManager.Load<TextureCube>(textureName);
            effect = contentManager.Load<Effect>("skysphere_effect");

            this.graphicsDevice = graphicsDevice;

            effect.Parameters["gCubeMap"].SetValue(texture);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
            }
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            Matrix localWorld =  Matrix.CreateScale(Scale) * Matrix.CreateTranslation(CameraPosition + position);

            graphicsDevice.RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };

            graphicsDevice.DepthStencilState = DepthStencilState.None;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    Matrix gWVP = localWorld * Matrix.Multiply(View, Projection);

                    meshPart.Effect.Parameters["gWVP"]?.SetValue(gWVP);
                }
                mesh.Draw();
            }

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public void SetClipSpace(Vector4? plane)
        {
            effect.Parameters["gClipSpaceEnabled"]?.SetValue(plane.HasValue);

            if (plane.HasValue)
            {
                effect.Parameters["gClipePlane"]?.SetValue(plane.Value);
            }
        }
    }
}
