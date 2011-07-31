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
        #endregion

        #region Init

        public FuelCellGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;

            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

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
            thumbstick = Content.Load<Texture2D>(GameConstants.TxtThumbsticks);
            ground = new Terrain(GameConstants.MdlGround, gameState);
            boundingSphere = new Pawn(GameConstants.MdlSphere, gameState);
            Fireball.Texture = Content.Load<Texture2D>(GameConstants.TxtFireball); 
            frameRate.LoadContent(spriteBatch);

            //quad = new Quad(Vector3.Zero, Vector3.Backward, Vector3.Up, 1, 1);
            //quadTexture = Content.Load<Texture2D>("Textures/Glass");
            //quadEffect = new BasicEffect(graphics.GraphicsDevice);
            //quadEffect.EnableDefaultLighting();

            //quadEffect.World = Matrix.Identity;
            //quadEffect.View = gameCamera.ViewMatrix;
            //quadEffect.Projection = gameCamera.ProjectionMatrix;
            //quadEffect.TextureEnabled = true;
            //quadEffect.Texture = quadTexture;

            //myBoundingBox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(8, 4, 15));

            //Init fuel cells
            gameState.FuelCells = new FuelCell[GameConstants.NumFuelCells];
            for (int index = 0; index < gameState.FuelCells.Length; index++)
                gameState.FuelCells[index] = new FuelCell(GameConstants.MdlFuelcell, gameState);

            //Init barriers
            gameState.Barriers = new Barrier[GameConstants.NumBarriers];
            int randomBarrier = random.Next(3);

            for (int index = 0; index < GameConstants.NumBarriers; index++)
            {
                BarrierType type = BarrierType.Cube;
                switch (randomBarrier)
                {
                    case 0:
                        type = BarrierType.Cube;
                        break;
                    case 1:
                        type = BarrierType.Cylinder;
                        break;
                    case 2:
                        type = BarrierType.Pryamid;
                        break;
                }
                gameState.Barriers[index] = Barrier.BarrierFromType(type, gameState);
                randomBarrier = random.Next(3);
            }
            PlaceFuelCellsAndBarriers();

            //Init fuel carrier
            gameState.Avatar = new FuelCarrier(GameConstants.MdlAvatar, gameState);
            gameState.WireFrameModel = boundingSphere;
            gameState.Projectiles = new List<Projectile>();
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

            foreach (Barrier barrier in gameState.Barriers)
                barrier.Update(gameTime);

            gameState.Avatar.Update(gameTime);
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            gameCamera.Update(gameState.Avatar.ForwardDirection, gameState.Avatar.Position, aspectRatio);

            //Since we dont have a real particle engine...
            foreach (Fireball sprite in gameState.Projectiles)
            {
                sprite.Update(gameTime);

                //Check barriers, ideally these would be generic types instead and would have methods returing true/false for collision
                //these in the future would have events/recievers etc to let downstream objects know a collision was made
                foreach (Barrier barrier in gameState.Barriers)
                {
                    if (sprite.ShotBy != barrier && barrier.BoundingSphere.Intersects(new BoundingBox(sprite.Position, sprite.Position)))
                        sprite.IsMarkedRemoved = true;
                }

                //remove them if they traveld too far
                if (sprite.Position.Length() > GameConstants.MaxDistance*2)
                    sprite.IsMarkedRemoved = true;
            }
            for (int x = 0; x < gameState.Projectiles.Count; x++)
                if (gameState.Projectiles[x].IsMarkedRemoved)
                    gameState.Projectiles.Remove(gameState.Projectiles[x]);


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

            ////draw wall
            //foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
            //{
            //    pass.Apply();

            //    GraphicsDevice.DrawUserIndexedPrimitives
            //        <VertexPositionNormalTexture>(
            //        PrimitiveType.TriangleList,
            //        quad.Vertices, 0, 4,
            //        quad.Indexes, 0, 2);
            //}
            //BoundingBoxRenderer.Render(myBoundingBox, GraphicsDevice, gameCamera.ViewMatrix, gameCamera.ProjectionMatrix, Color.SaddleBrown);

            ground.Draw(gameTime, gameCamera.ViewMatrix, gameCamera.ProjectionMatrix);

            DrawProjectiles();



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

        private void DrawProjectiles()
        {
            if (gameState.Projectiles.Count > 0)
            {
                VertexPositionTexture[] projectileVertices = new VertexPositionTexture[gameState.Projectiles.Count * 6];
                int i = 0;
                foreach (Fireball fireball in gameState.Projectiles)
                {
                    Vector3 center = fireball.Position;

                    projectileVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    projectileVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                    projectileVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 0));

                    projectileVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                    projectileVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 1));
                    projectileVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                }

                //Billboarding in XNA 4...text and textures
                //http://blogs.msdn.com/b/shawnhar/archive/2011/01/12/spritebatch-billboards-in-a-3d-world.aspx
                Fireball.DrawAll(gameState, spriteBatch, gameCamera);

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
