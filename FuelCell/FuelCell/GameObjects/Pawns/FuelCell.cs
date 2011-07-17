using System;
using System.Collections.Generic;
using System.Linq;
#region Using Statements
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace FuelCell
{
    class FuelCell : Pawn
    {
        public bool Retrieved { get; set; }

        public FuelCell(string modelName, GameState state)
            : base(modelName, state)
        {
            Retrieved = false;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            BoundingSphere = CalculateBoundingSphere();
            Position = Vector3.Down;
            ScaleBoundingSphere(GameConstants.FuelCellBoundingSphereFactor);
        }

        public override void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            base.Draw(gameTime, view, projection);

            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix translateMatrix = Matrix.CreateTranslation(Position);
            Matrix worldMatrix = translateMatrix;

            if (!Retrieved)
            {
                foreach (ModelMesh mesh in Model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World =
                            worldMatrix * transforms[mesh.ParentBone.Index];
                        effect.View = view;
                        effect.Projection = projection;

                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = false;
                        effect.LightingEnabled = true;
                        effect.DirectionalLight0.Enabled = true;
                        effect.DirectionalLight1.Enabled = false;
                        effect.DirectionalLight2.Enabled = false;
                    }
                    mesh.Draw();
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Retrieved)
            {
                if (GameState.Avatar.BoundingSphere.Intersects(BoundingSphere))
                    Retrieved = true;
            }
        }

    }
}
