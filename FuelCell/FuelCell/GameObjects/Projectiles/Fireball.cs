#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
#endregion

namespace FuelCell
{
    class Fireball : Particle
    {
        public Fireball(GameState state, Vector3 position, Quaternion rotation)
            : base(state, position, rotation)
        {
            GameState.Particles.Add(this);
        }
    }
}
