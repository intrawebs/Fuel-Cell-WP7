#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FuelCell
{
    class Terrain : Pawn
    {
        public Terrain(string modelName, GameState state)
            : base(modelName, state)
        {

        }

        public override void LoadContent()
        {
            base.LoadContent();
            BoundingSphere = CalculateBoundingSphere();
            Position = Vector3.Down;
            ScaleBoundingSphere(GameConstants.BarrierBoundingSphereFactor);
        }

        public override void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = false;
                    effect.LightingEnabled = true;
                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight1.Enabled = false;
                    effect.DirectionalLight2.Enabled = false;
                    effect.World = Matrix.Identity;

                    // Use the matrices provided by the game camera
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }
    }
}
