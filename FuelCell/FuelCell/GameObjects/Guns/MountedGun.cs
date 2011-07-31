#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
#endregion

namespace FuelCell
{
    //in the future this probably should have a model and inherit from Pawn instead
    class MountedGun : Actor 
    {
        double lastFired = 0;
        double rateOfFire = 100;

        public MountedGun(GameState state, double rateOfFire)
            : base(state)

        {
            this.rateOfFire = rateOfFire;
        }

        //This shoots fireballs only right now!
        public void Shoot(Vector3 fromPosition, Quaternion fromRotation, Pawn shotBy, GameTime gameTime)
        {
            Fireball newBullet = new Fireball(GameState, fromPosition, fromRotation, shotBy);
            lastFired = gameTime.TotalGameTime.TotalMilliseconds; 
        }

        public bool CanShoot(GameTime gameTime)
        {
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            if (currentTime - lastFired > rateOfFire)
            {
                return true;
            }
            return false;
        }
    }
}
