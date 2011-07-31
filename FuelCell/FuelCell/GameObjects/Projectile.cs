#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FuelCell
{
    class Projectile : Actor
    {
        public Projectile(GameState state, Vector3 position, Quaternion rotation)
            : base(state)
        {
            Position = position;
            Rotation = rotation;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            MoveForward(GameConstants.Velocity * 2.0f);
        }

        private void MoveForward(float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, 1), Rotation);
            Position += addVector * speed;
        }
    }
}
