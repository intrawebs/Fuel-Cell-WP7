#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace FuelCell
{
    class GameState
    {
        public FuelCell[] FuelCells { get; set; }
        public Barrier[] Barriers { get; set; }
        public FuelCarrier Avatar { get; set; }
        public List<Projectile> Projectiles { get; set; }
        public ContentManager Content { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        public Pawn WireFrameModel { get; set; }

        public GameState(ContentManager content, GraphicsDevice graphicsDevice)
        {
            Content = content;
            GraphicsDevice = graphicsDevice;
        }
    }
}
