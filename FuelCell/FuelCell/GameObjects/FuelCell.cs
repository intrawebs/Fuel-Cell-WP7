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
    class FuelCell : GameObject
    {
        public bool Retrieved { get; set; }

        public FuelCell()
            : base()
        {
            Retrieved = false;
        }

        public void LoadContent(ContentManager content, string modelName)
        {
            Model = content.Load<Model>(modelName);
            BoundingSphere = CalculateBoundingSphere();
            Position = Vector3.Down;
            ScaleBoundingSphere(GameConstants.FuelCellBoundingSphereFactor);
        }

        public void Draw(Matrix view, Matrix projection)
        {
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
                        effect.PreferPerPixelLighting = true;
                    }
                    mesh.Draw();
                }
            }
        }

        internal void Update(BoundingSphere vehicleBoundingSphere)
        {
            if (vehicleBoundingSphere.Intersects(this.BoundingSphere))
                this.Retrieved = true;
        }
    }
}
