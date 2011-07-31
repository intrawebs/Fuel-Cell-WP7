#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace FuelCell
{
    class Barrier : Pawn
    {
        #region Fields
        public BarrierType BarrierType { get; set; }
        private MountedGun Cannon;
        TimeSpan rotationCheck = new TimeSpan();
        float lastRot = 5;
        #endregion

        public Barrier(string modelName, GameState state, BarrierType type)
            : base(modelName, state)
        {
            
            BarrierType = type;
            double rateOfFire = 0;
            switch (BarrierType)
            {
                case BarrierType.Cube:
                    rateOfFire = 650;
                    break;
                case BarrierType.Cylinder:
                    rateOfFire = 100;
                    break;
                case BarrierType.Pryamid:
                    rateOfFire = 650;
                    break;
                default:
                    break;
            }
            Cannon = new MountedGun(state, rateOfFire);
        }

        public static Barrier BarrierFromType(BarrierType type, GameState state)
        {
            string modelName = string.Empty;
            switch (type)
            {
                case BarrierType.Cube:
                    modelName = GameConstants.MdlCube;
                    break;
                case BarrierType.Cylinder:
                    modelName = GameConstants.MdlCylinder;
                    break;
                case BarrierType.Pryamid:
                    modelName = GameConstants.MdlPryamid;
                    break;
                default:
                    break;
            }

            return new Barrier(modelName, state, type);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            BoundingSphere = CalculateBoundingSphere();
            Position = Vector3.Down;
            ScaleBoundingSphere(GameConstants.BarrierBoundingSphereFactor);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var distToAvatar = (Position - GameState.Avatar.Position).Length();
            rotationCheck += gameTime.ElapsedGameTime;

            if (distToAvatar < 30)
            {
                //they shoot out from one side, but quickly and rotate quickly
                if (BarrierType == BarrierType.Cylinder)
                {
                    //rotate
                    if (rotationCheck.TotalMilliseconds > 25)
                    {
                        #region Notes about Coordinates
                        /*
                        In a right-handed coordinate system, the rotations are as follows:

                        90 degrees CW about x-axis: (x, y, z) -> (x, -z, y)
                        90 degrees CCW about x-axis: (x, y, z) -> (x, z, -y)

                        90 degrees CW about y-axis: (x, y, z) -> (-z, y, x)
                        90 degrees CCW about y-axis: (x, y, z) -> (z, y, -x)

                        90 degrees CW about z-axis: (x, y, z) -> (y, -x, z)
                        90 degrees CCW about z-axis: (x, y, z) -> (-y, x, z)

                        If you're using a left-handed coordinate system, simply switch 'CW' with 'CCW' above. 
                            */
                        
                        //Euler to Quaternion
                        //http://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/steps/index.htm
                        //shoot out from the four sides of the cube
                        //http://social.msdn.microsoft.com/Forums/en-US/xnaframework/thread/35090fe1-468b-4449-ba29-b08aee98359d

                        //Use these for fixed (something that doesnt rotate) 90 degree angles as its faster
                        //Cannon.Shoot(Position, Rotation, this);
                        //Cannon.Shoot(Position, new Quaternion(0, .7071f, 0, .7071f), this);
                        //Cannon.Shoot(Position, new Quaternion(0,1,0,0), this);
                        //Cannon.Shoot(Position, new Quaternion(0,0,1,0), this);
                        //Cannon.Shoot(Position, new Quaternion(0,.7071f,0,-.7071f), this);
                        #endregion

                        Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(lastRot));                        
                        lastRot += 5;
                        rotationCheck = new TimeSpan();                       
                    }
                    if (Cannon.CanShoot(gameTime))
                    {
                        Cannon.Shoot(Position, Rotation, this, gameTime);
                    }
                }

                //they shoot out from all sides
                else if (BarrierType == BarrierType.Cube)
                {
                    //rotate
                    if (rotationCheck.TotalMilliseconds > 25)
                    {
                        Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(lastRot));
                        lastRot += 5;
                        rotationCheck = new TimeSpan();
                    }

                    if (Cannon.CanShoot(gameTime))
                    {
                        //Euler to Quaternion
                        //shoot out from the four sides of the cube
                        //http://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/steps/index.htm                    
                        //http://social.msdn.microsoft.com/Forums/en-US/xnaframework/thread/35090fe1-468b-4449-ba29-b08aee98359d

                        var rotY1 = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(lastRot + 90));
                        var rotY2 = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(lastRot + 180));
                        var rotY3 = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(lastRot + 270));

                        Cannon.Shoot(Position, Rotation, this, gameTime);
                        Cannon.Shoot(Position, rotY1, this, gameTime);
                        Cannon.Shoot(Position, rotY2, this, gameTime);
                        Cannon.Shoot(Position, rotY3, this, gameTime);
                    }
                }

                //they track you and shot right at you
                else if (BarrierType == BarrierType.Pryamid)
                {
                    //rotate
                    if (rotationCheck.TotalMilliseconds > 25)
                    {
                        //THIS WORKS FOR ROTATING SOMETHING TO THE SAME ROTATION AS SOMETHING ELSE
                        //Quaternion destQuat = new Quaternion(GameState.Avatar.Rotation.X, GameState.Avatar.Rotation.Y, GameState.Avatar.Rotation.Z, GameState.Avatar.Rotation.W);
                        //while (!Helpers.autoRotateDone(ref destQuat, ref Rotation, .5f))
                        //{
                        //}
                        //Cannon.Shoot(Position, Rotation, this);

                        // the new forward vector, so the target faces the avatar
                        Vector3 newForward = Vector3.Normalize(Position - GameState.Avatar.Position);
                        // calc the rotation so the target faces the avatar
                        Rotation = Helpers.GetRotation(Vector3.Forward, newForward, Vector3.Up);

                        rotationCheck = new TimeSpan();
                    }
                    if (Cannon.CanShoot(gameTime))
                    {
                        Cannon.Shoot(Position, Rotation, this, gameTime);
                    }
                }
            }
        }      

        public override void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            base.Draw(gameTime, view, projection);
            
            Matrix translateMatrix = Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
            Matrix worldMatrix = translateMatrix;

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
    }
}
