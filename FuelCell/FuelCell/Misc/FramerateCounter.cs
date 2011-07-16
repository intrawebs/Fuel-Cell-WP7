#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace FuelCell
{
    public class FrameRateCounter
    {
        #region Fields and Properties

        public Vector2 Position { get; set; }
        public Vector2 Position2 { get; set; }
        public Color Color { get; set; }
        public Color Color2 { get; set; }

        private ContentManager content;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;

        private int frameRate = 0;
        private int frameCounter = 0;
        private TimeSpan elapsedTime = TimeSpan.Zero;

        bool loaded = false;
        #endregion

        #region Init

        public FrameRateCounter(Game game, Vector2 position, Color color, Color color2)
        {
            content = game.Content;
            Position = position;
            Position2 = new Vector2(Position.X + 1, Position.Y + 1);
            Color = color;
            Color2 = color2;
        }

        public void LoadContent(SpriteBatch batch)
        {
            spriteBatch = batch;
            spriteFont = content.Load<SpriteFont>("Fonts/SmallText");
        }

        public void UnloadContent()
        {
            content.Unload();
        }

        #endregion

        #region Update

        public void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }

        public void Draw(GameTime gameTime)
        {
            frameCounter++;

            string text = "FPS: " + frameRate;

            spriteBatch.DrawString(spriteFont, text, Position2, Color2);
            spriteBatch.DrawString(spriteFont, text, Position, Color);
        }

        #endregion
    }
}