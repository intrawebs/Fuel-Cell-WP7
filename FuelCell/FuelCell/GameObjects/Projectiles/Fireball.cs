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
    class Fireball : Projectile
    {
        public static Texture2D Texture { get; set; }
        static BasicEffect effect;
        public static float Scale = GameConstants.FireballScale;
        public Pawn ShotBy { get; set; }

        public static Matrix World 
        {
            get { return effect.World; }
        }

        public Fireball(GameState state, Vector3 position, Quaternion rotation, Pawn parent)
            : base(state, position, rotation)
        {
            ShotBy = parent;
            GameState.Projectiles.Add(this);
        }

        public static void DrawAll(GameState state, SpriteBatch spriteBatch, Camera gameCamera)
        {
            BasicEffect effect = Fireball.GetEffect(state.GraphicsDevice);
            effect.Projection = gameCamera.ProjectionMatrix;
            spriteBatch.Begin(0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, effect);

            foreach (Fireball ball in state.Projectiles)
            {
                Vector3 viewSpacePosition = Vector3.Transform(ball.Position, gameCamera.ViewMatrix * Fireball.World);
                Vector2 locationOffset = new Vector2(viewSpacePosition.X - (Fireball.Texture.Width / 2) * Fireball.Scale, viewSpacePosition.Y - Fireball.Texture.Height * Fireball.Scale);
                spriteBatch.Draw(Fireball.Texture, locationOffset, null, Color.White, 0, Vector2.Zero, Fireball.Scale, 0, viewSpacePosition.Z);
            }
            spriteBatch.End();
        }

        public static BasicEffect GetEffect(GraphicsDevice device)
        {
            if (effect == null)
            {
                effect = new BasicEffect(device)
                {
                    TextureEnabled = true,
                    VertexColorEnabled = true,
                    View = Matrix.Identity,
                    Texture = Texture,
                    World = Matrix.CreateScale(1, -1, 1)
                };
            }
            return effect;
        }
    }
}
