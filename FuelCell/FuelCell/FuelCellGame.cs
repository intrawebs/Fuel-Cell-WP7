#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
#endregion

namespace FuelCell
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FuelCellGame : Microsoft.Xna.Framework.Game
    {
        #region Fields
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Terrain ground;
        Camera gameCamera;
        Random random;
        Pawn boundingSphere;
        FrameRateCounter frameRate;
        private Texture2D thumbstick;
        SpriteFont spriteFont;
        GameState gameState;
        Texture2D fireballTexture;
        BasicEffect fireballEffect;
        #endregion

        #region Init

        public FuelCellGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            random = new Random(DateTime.Now.Millisecond);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            TouchPanel.EnabledGestures = GestureType.Tap;
            gameCamera = new Camera();
            frameRate = new FrameRateCounter(this, new Vector2(0, 0), Color.Wheat, Color.Wheat);
            gameState = new GameState(Content, GraphicsDevice);

            base.Initialize();
        }

        #endregion

        #region Load and Unload

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //Init some misc stuff
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Fonts/SmallText");
            thumbstick = Content.Load<Texture2D>("Textures/thumbstick");
            ground = new Terrain("Models/ground", gameState);
            boundingSphere = new Pawn("Models/sphere1uR", gameState);
            frameRate.LoadContent(spriteBatch);

            //Init fuel cells

            gameState.FuelCells = new FuelCell[GameConstants.NumFuelCells];
            for (int index = 0; index < gameState.FuelCells.Length; index++)
                gameState.FuelCells[index] = new FuelCell("Models/fuelcell", gameState);

            //Init barriers
            gameState.Barriers = new Barrier[GameConstants.NumBarriers];
            int randomBarrier = random.Next(3);
            string barrierName = null;

            for (int index = 0; index < gameState.Barriers.Length; index++)
            {
                switch (randomBarrier)
                {
                    case 0:
                        barrierName = "Models/cube10uR";
                        break;
                    case 1:
                        barrierName = "Models/cylinder10uR";
                        break;
                    case 2:
                        barrierName = "Models/pyramid10uR";
                        break;
                }
                gameState.Barriers[index] = new Barrier(barrierName, gameState);
                randomBarrier = random.Next(3);
            }
            PlaceFuelCellsAndBarriers();

            //Init fuel carrier
            gameState.Avatar = new FuelCarrier("Models/fuelcarrier", gameState);

            //bullet
            fireballTexture = Content.Load<Texture2D>("Textures/bullet");

            fireballEffect = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
            };

            gameState.WireFrameModel = boundingSphere;
            gameState.Particles = new List<Particle>();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            frameRate.UnloadContent();
        }

        #endregion

        #region Update

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            VirtualThumbsticks.Update();
            frameRate.Update(gameTime);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            foreach (FuelCell cell in gameState.FuelCells)
                cell.Update(gameTime);

            gameState.Avatar.Update(gameTime);
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            gameCamera.Update(gameState.Avatar.ForwardDirection, gameState.Avatar.Position, aspectRatio);

            //Since we dont have a real particle engine...
            foreach (Particle sprite in gameState.Particles)
            {
                sprite.Update(gameTime);

                //Check barriers, ideally these would be generic types instead and would have methods returing true/false for collision
                //these in the future would have events/recievers etc to let downstream objects know a collision was made
                foreach (Barrier barrier in gameState.Barriers)
                {
                    if (barrier.BoundingSphere.Intersects(new BoundingBox(sprite.Position, sprite.Position)))
                        sprite.IsMarkedRemoved = true;
                }
            }
            for (int x = 0; x < gameState.Particles.Count; x++)
                if (gameState.Particles[x].IsMarkedRemoved)
                    gameState.Particles.Remove(gameState.Particles[x]);


            base.Update(gameTime);
        }

        #endregion
        
        #region Draw

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            //For better perf, draw whats closest to the camera first, XNA likes that doesnt spend time drawing pixels covered by something else
            //however, they are still sent to the GPU for processing, so futuer culling can help if perf gets bad
            gameState.Avatar.Draw(gameTime, gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);

            foreach (Barrier barrier in gameState.Barriers)
                barrier.Draw(gameTime, gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);

            foreach (FuelCell fuelCell in gameState.FuelCells)
                if (!fuelCell.Retrieved)
                    fuelCell.Draw(gameTime, gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);            

            ground.Draw(gameTime, gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);

            DrawParticles();



            // Draw 2D HUD
            spriteBatch.Begin();

            // if the user is touching the screen and the thumbsticks have positions,
            // draw our thumbstick sprite so the user knows where the centers are
            if (VirtualThumbsticks.LeftThumbstickCenter.HasValue)
            {
                spriteBatch.Draw(
                    thumbstick,
                    VirtualThumbsticks.LeftThumbstickCenter.Value - new Vector2(thumbstick.Width / 2f, thumbstick.Height / 2f),
                    Color.Green);
            }

            if (VirtualThumbsticks.RightThumbstickCenter.HasValue)
            {
                spriteBatch.Draw(
                    thumbstick,
                    VirtualThumbsticks.RightThumbstickCenter.Value - new Vector2(thumbstick.Width / 2f, thumbstick.Height / 2f),
                    Color.Blue);
            }

            frameRate.Draw(gameTime);

            spriteBatch.End();



            //reset the render back to something usable for 3d
            //see http://blogs.msdn.com/b/shawnhar/archive/2010/06/18/spritebatch-and-renderstates-in-xna-game-studio-4-0.aspx
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            base.Draw(gameTime);
        }

        private void DrawParticles()
        {
            if (gameState.Particles.Count > 0)
            {
                VertexPositionTexture[] bulletVertices = new VertexPositionTexture[gameState.Particles.Count * 6];
                int i = 0;
                foreach (Fireball currentBullet in gameState.Particles)
                {
                    Vector3 center = currentBullet.Position;

                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 0));

                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 1));
                    bulletVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                }

                //Billboarding in XNA 4...text and textures
                //http://blogs.msdn.com/b/shawnhar/archive/2011/01/12/spritebatch-billboards-in-a-3d-world.aspx

                #region Draw Text Sprites
                //Matrix invertY = Matrix.CreateScale(1, -1, 1);
                //basicEffect.World = invertY;
                //basicEffect.View = Matrix.Identity;
                //basicEffect.Projection = gameCamera.ProjectionMatrix;

                //spriteBatch.Begin(0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, basicEffect);

                //foreach (Bullet currentBullet in bulletList)
                //{
                //    Vector3 textPosition = currentBullet.position;

                //    Vector3 viewSpaceTextPosition = Vector3.Transform(textPosition, gameCamera.ViewMatrix * invertY);

                //    const string message = "o";
                //    Vector2 textOrigin = spriteFont.MeasureString(message) / 2;
                //    const float textSize = 0.25f;

                //    spriteBatch.DrawString(spriteFont, message, new Vector2(viewSpaceTextPosition.X, viewSpaceTextPosition.Y), Color.White, 0, textOrigin, textSize, 0, viewSpaceTextPosition.Z);
                //}
                #endregion

                #region Draw Texture Sprites

                Matrix invertY = Matrix.CreateScale(1, -1, 1);
                fireballEffect.World = invertY;
                fireballEffect.View = Matrix.Identity;
                fireballEffect.Projection = gameCamera.ProjectionMatrix;
                fireballEffect.Texture = fireballTexture;
                fireballEffect.TextureEnabled = true;

                spriteBatch.Begin(0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, fireballEffect);

                foreach (Fireball currentBullet in gameState.Particles)
                {
                    const float size = 0.03f;
                    Vector3 textPosition = currentBullet.Position;
                    Vector3 viewSpaceTextPosition = Vector3.Transform(textPosition, gameCamera.ViewMatrix * invertY);
                    Vector2 locationOffset = new Vector2(viewSpaceTextPosition.X - (fireballTexture.Width / 2) * size, viewSpaceTextPosition.Y - fireballTexture.Height * size);

                    spriteBatch.Draw(fireballTexture, locationOffset, null, Color.White, 0, Vector2.Zero, size, 0, viewSpaceTextPosition.Z);
                }
                spriteBatch.End();

                #endregion
            }
        }

        #endregion

        #region Helpers

        private void PlaceFuelCellsAndBarriers()
        {
            int min = GameConstants.MinDistance;
            int max = GameConstants.MaxDistance;
            Vector3 tempCenter;

            //place fuel cells
            foreach (FuelCell cell in gameState.FuelCells)
            {
                cell.Position = GenerateRandomPosition(min, max);
                tempCenter = cell.BoundingSphere.Center;
                tempCenter.X = cell.Position.X;
                tempCenter.Z = cell.Position.Z;
                cell.BoundingSphere = new BoundingSphere(tempCenter, cell.BoundingSphere.Radius);
                cell.Retrieved = false;
            }

            //place barriers
            foreach (Barrier barrier in gameState.Barriers)
            {
                barrier.Position = GenerateRandomPosition(min, max);
                tempCenter = barrier.BoundingSphere.Center;
                tempCenter.X = barrier.Position.X;
                tempCenter.Z = barrier.Position.Z;
                barrier.BoundingSphere = new BoundingSphere(tempCenter, barrier.BoundingSphere.Radius);
            }
        }

        private Vector3 GenerateRandomPosition(int min, int max)
        {
            int xValue, zValue;
            do
            {
                xValue = random.Next(min, max);
                zValue = random.Next(min, max);
                if (random.Next(100) % 2 == 0)
                    xValue *= -1;
                if (random.Next(100) % 2 == 0)
                    zValue *= -1;

            } while (IsOccupied(xValue, zValue));

            return new Vector3(xValue, 0, zValue);
        }

        private bool IsOccupied(int xValue, int zValue)
        {
            foreach (Pawn currentObj in gameState.FuelCells)
            {
                if (((int)(MathHelper.Distance(
                    xValue, currentObj.Position.X)) < 15) &&
                    ((int)(MathHelper.Distance(
                    zValue, currentObj.Position.Z)) < 15))
                    return true;
            }

            foreach (Pawn currentObj in gameState.Barriers)
            {
                if (((int)(MathHelper.Distance(
                    xValue, currentObj.Position.X)) < 15) &&
                    ((int)(MathHelper.Distance(
                    zValue, currentObj.Position.Z)) < 15))
                    return true;
            }
            return false;
        }

        #endregion
    }
}
