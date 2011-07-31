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
    class FuelCarrier : Pawn
    {
        //TO DO: player input is a bit to wired into this class, however, it's the only Pawn right now where that can be true
        //so, if we add any other player controllable pawns lets remove the input stuff?
        #region Fields
        public float ForwardDirection { get; set; }
        public int MaxRange { get; set; }
        public Vector3 Speed { get; set; }
        public MountedGun PrimaryCannon { get; set; }        
        #endregion

        public FuelCarrier(string modelName, GameState state)
            : base(modelName, state)
        {
            ForwardDirection = 0.0f;
            MaxRange = GameConstants.MaxRange;
            PrimaryCannon = new MountedGun(state, 100);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            BoundingSphere = CalculateBoundingSphere();
            ScaleBoundingSphere(GameConstants.FuelCarrierBoundingSphereFactor);
        }

        public override void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            base.Draw(gameTime, view, projection);
            Matrix worldMatrix = Matrix.Identity;
            Matrix rotationYMatrix = Matrix.CreateRotationY(ForwardDirection);
            Matrix translateMatrix = Matrix.CreateTranslation(Position);
            Rotation = Quaternion.CreateFromRotationMatrix(rotationYMatrix);

            worldMatrix = rotationYMatrix * translateMatrix;

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = worldMatrix * BoneTransforms[mesh.ParentBone.Index];
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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            ManageMovement();

            ManageWeapons(gameTime);
        }

        private void ManageMovement()
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


            Speed = Vector3.Transform(movement, orientationMatrix);
            Speed *= GameConstants.Velocity;
            futurePosition = Position + Speed;

            if (ValidateMovement(futurePosition))
            {
                Position = futurePosition;
                BoundingSphere updatedSphere;
                updatedSphere = BoundingSphere;
                updatedSphere.Center.X = Position.X;
                updatedSphere.Center.Z = Position.Z;
                BoundingSphere = new BoundingSphere(updatedSphere.Center, updatedSphere.Radius);
            }
        }

        private void ManageWeapons(GameTime gameTime)
        {
            if (VirtualThumbsticks.RightThumbstick.Length() > .2f)
            {
                //can we shoot? dont want to be shooting too fast
                if (PrimaryCannon.CanShoot(gameTime))
                {
                    PrimaryCannon.Shoot(Position, Rotation, this, gameTime);
                }
            }
        }

        private bool ValidateMovement(Vector3 futurePosition)
        {
            BoundingSphere futureBoundingSphere = BoundingSphere;
            futureBoundingSphere.Center.X = futurePosition.X;
            futureBoundingSphere.Center.Z = futurePosition.Z;

            //Don't allow off-terrain driving
            if ((Math.Abs(futurePosition.X) > MaxRange) || (Math.Abs(futurePosition.Z) > MaxRange))
                return false;

            //Don't allow driving through a barrier
            if (CheckForBarrierCollision(futureBoundingSphere))
                return false;

            return true;
        }

        private bool CheckForBarrierCollision(BoundingSphere vehicleBoundingSphere)
        {
            for (int curBarrier = 0; curBarrier < GameState.Barriers.Length; curBarrier++)
            {
                if (vehicleBoundingSphere.Intersects(
                    GameState.Barriers[curBarrier].BoundingSphere))
                    return true;
            }
            return false;
        }
    }
}
