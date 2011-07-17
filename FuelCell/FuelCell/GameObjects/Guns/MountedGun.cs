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
        public MountedGun(GameState state)
            : base(state)

        { }

        //This shoots fireballs only right now!
        public void Shoot(Vector3 fromPosition, Quaternion fromRotation)
        {
            Fireball newBullet = new Fireball(GameState, fromPosition, fromRotation);         
        }
    }
}
