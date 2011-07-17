#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace FuelCell
{
    class Actor
    {
        #region Fields
        public Vector3 Position { get; set; }
        public Quaternion Rotation;
        public bool IsActive { get; set; }
        public bool IsMarkedRemoved { get; set; }
        private GameState gameState;
        public GameState GameState
        {
            get
            {
                return gameState;
            }
        }
        #endregion

        public Actor(GameState state) 
        {
            gameState = state;
        }

        /// <summary>
        /// Load graphics content for the screen.
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// Unload content for the screen.
        /// </summary>
        public virtual void UnloadContent() { }

        /// <summary>
        /// Update the state of this object
        /// </summary>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        public virtual void Draw(GameTime gameTime, Matrix view, Matrix projection) { }

        public virtual void ProcessCollisions() { }
    }
}
