#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace FuelCell
{
    class FuelCarrier : GameObject
    {
        #region Fields
        public float ForwardDirection { get; set; }
        public int MaxRange { get; set; }
        #endregion

        public FuelCarrier()
            : base()
        {
            ForwardDirection = 0.0f;
            MaxRange = GameConstants.MaxRange;
        }

        public void LoadContent(ContentManager content, string modelName)
        {
            Model = content.Load<Model>(modelName);
            BoundingSphere = CalculateBoundingSphere();
            ScaleBoundingSphere(GameConstants.FuelCarrierBoundingSphereFactor);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix worldMatrix = Matrix.Identity;
            Matrix rotationYMatrix = Matrix.CreateRotationY(ForwardDirection);
            Matrix translateMatrix = Matrix.CreateTranslation(Position);

            worldMatrix = rotationYMatrix * translateMatrix;

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World =
                        worldMatrix * transforms[mesh.ParentBone.Index]; ;
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

        public void Update(Barrier[] barriers)
        {
            Vector3 futurePosition = Position;
            float turnAmount = 0;

            Vector3 movement = Vector3.Zero;
            if (VirtualThumbsticks.LeftThumbstick.Length() > .2f)
			{
                //Rotation = -(float)Math.Atan2(
                //    -VirtualThumbsticks.LeftThumbstick.Y, 
                //    VirtualThumbsticks.LeftThumbstick.X);
                if (VirtualThumbsticks.LeftThumbstick.Y > 0)
                    movement.Z = -1;
                else if (VirtualThumbsticks.LeftThumbstick.Y < 0)
                    movement.Z = 1;
                else if (VirtualThumbsticks.LeftThumbstick.Y == 0)
                    movement.Z = 0;

                if (VirtualThumbsticks.LeftThumbstick.X > GameConstants.TurnAmountGive)
                    turnAmount = -1;
                else if (VirtualThumbsticks.LeftThumbstick.X < -GameConstants.TurnAmountGive)
                    turnAmount = 1;
                else 
                    turnAmount = 0;
			}

            ForwardDirection += turnAmount * GameConstants.TurnSpeed;
            Matrix orientationMatrix = Matrix.CreateRotationY(ForwardDirection);


            Vector3 speed = Vector3.Transform(movement, orientationMatrix);
            speed *= GameConstants.Velocity;
            futurePosition = Position + speed;

            if (ValidateMovement(futurePosition, barriers))
            {
                Position = futurePosition;
                BoundingSphere updatedSphere;
                updatedSphere = BoundingSphere;
                updatedSphere.Center.X = Position.X;
                updatedSphere.Center.Z = Position.Z;
                BoundingSphere = new BoundingSphere(updatedSphere.Center, updatedSphere.Radius);
            }
        }

        private bool ValidateMovement(Vector3 futurePosition, Barrier[] barriers)
        {
            BoundingSphere futureBoundingSphere = BoundingSphere;
            futureBoundingSphere.Center.X = futurePosition.X;
            futureBoundingSphere.Center.Z = futurePosition.Z;

            //Don't allow off-terrain driving
            if ((Math.Abs(futurePosition.X) > MaxRange) || (Math.Abs(futurePosition.Z) > MaxRange))
                return false;

            //Don't allow driving through a barrier
            if (CheckForBarrierCollision(futureBoundingSphere, barriers))
                return false;

            return true;
        }

        private bool CheckForBarrierCollision(BoundingSphere vehicleBoundingSphere, Barrier[] barriers)
        {
            for (int curBarrier = 0; curBarrier < barriers.Length; curBarrier++)
            {
                if (vehicleBoundingSphere.Intersects(
                    barriers[curBarrier].BoundingSphere))
                    return true;
            }
            return false;
        }
    }
}
