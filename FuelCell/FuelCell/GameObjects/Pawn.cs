#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace FuelCell
{
    class Pawn : Actor
    {
        #region Fields
        public Model Model { get; set; }
        public BoundingSphere BoundingSphere { get; set; }
        public string ModelName { get; set; }
        #endregion

        public Pawn(string modelName, GameState state)
            : base(state)
        {
            Model = null;
            Position = Vector3.Zero;
            IsActive = false;
            ModelName = modelName;
            LoadContent();
        }

        #region Load

        public override void LoadContent()
        {
            base.LoadContent();
            Model = GameState.Content.Load<Model>(ModelName);
        }

        #endregion

        #region Draw

        public override void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            base.Draw(gameTime, view, projection);
            DrawWireFrame(view, projection);
        }

        private void DrawWireFrame(Matrix view, Matrix projection)
        {
            if (GameConstants.DrawCollisionSpheres)
            {
                RasterizerState rsC = new RasterizerState();
                rsC.FillMode = FillMode.WireFrame;
                GameState.GraphicsDevice.RasterizerState = rsC;

                Matrix scaleMatrix = Matrix.CreateScale(BoundingSphere.Radius);
                Matrix translateMatrix = Matrix.CreateTranslation(BoundingSphere.Center);
                Matrix worldMatrix = scaleMatrix * translateMatrix;

                foreach (ModelMesh mesh in GameState.WireFrameModel.Model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = worldMatrix;
                        effect.View = view;
                        effect.Projection = projection;
                    }
                    mesh.Draw();
                }

                rsC = new RasterizerState();
                rsC.FillMode = FillMode.Solid;
                GameState.GraphicsDevice.RasterizerState = rsC;
            }
        }

        #endregion

        protected void ScaleBoundingSphere(float scale)
        {
            //Not necessary, but this tightens up the bounding spheres closer to the models edges
            //Keep this seperate from CalculateBoundingSphere() as that is unscaled
            BoundingSphere scaledSphere;
            scaledSphere = BoundingSphere;
            scaledSphere.Radius *= scale;
            BoundingSphere = new BoundingSphere(scaledSphere.Center, scaledSphere.Radius);
        }

        protected BoundingSphere CalculateBoundingSphere()
        {
            BoundingSphere mergedSphere = new BoundingSphere();
            BoundingSphere[] boundingSpheres;
            int index = 0;
            int meshCount = Model.Meshes.Count;

            boundingSpheres = new BoundingSphere[meshCount];
            foreach (ModelMesh mesh in Model.Meshes)
            {
                boundingSpheres[index++] = mesh.BoundingSphere;
            }

            mergedSphere = boundingSpheres[0];
            if ((Model.Meshes.Count) > 1)
            {
                index = 1;
                do
                {
                    mergedSphere = BoundingSphere.CreateMerged(mergedSphere, boundingSpheres[index]);
                    index++;
                } while (index < Model.Meshes.Count);
            }
            mergedSphere.Center.Y = 0;
            return mergedSphere;
        }
    }
}
