using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace PipeDreamClone
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D gamePieces;
        Texture2D bg;
        Texture2D title;

        GameBoard gameBoard;

        Vector2 gameBoardDisplayOffset = new Vector2(70, 89);
        Vector2 scoreDisplayOffset = new Vector2(605, 215);
        Vector2 gameOverDisplayOffset = new Vector2(200, 260);

        int playerScore = 0;

        enum GameStates { Title, Playing, GameOver };
        GameStates gameState = GameStates.Title;

        Rectangle emptyTilePointer = new Rectangle(1, 247, 40, 40);

        const float MinTimeBetweenInputs = 0.25f;
        float timeSinceLastInput = 0.0f;
        
        SpriteFont pericles36Font;

        float gameOverTimer;

        // Variables for handling the flooding and loosing conditions
        const float MaxFloodCounter = 100.0f;
        float floodAmount = 0.0f;
        float timeSinceLastFloodIncrease = 0.0f;
        float timeBetweenFloodTicks = 1.0f;
        float floodIncreaseAmount = 0.5f;

        // Variables for displaying the total flooding in the water tank
        const int MaxWaterHeight = 244;
        const int WaterWidth = 297; 
        Vector2 waterOverlayOffset = new Vector2(85, 245);
        Vector2 waterPositionOffset = new Vector2(478, 338);

        // Variables for handling difficult levels
        int curLevel = 0;
        int linesCompThisLvl = 0;
        const float floodIncreasePerLvl = 0.5f;
        Vector2 levelOffset = new Vector2(512, 215);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();
            gameBoard = new GameBoard();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            gamePieces = Content.Load<Texture2D>(@"Textures\Tile_Sheet");
            bg = Content.Load<Texture2D>(@"Textures\Background");
            title = Content.Load<Texture2D>(@"Textures\TitleScreen");
            pericles36Font = Content.Load<SpriteFont>(@"Fonts\Pericles36");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            switch (gameState)
            {
             
                case GameStates.Title:
                    if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    {
                        gameBoard.Clear();
                        gameBoard.GeneratePieces(false);
                        playerScore = 0;
                        floodAmount = 0;
                        curLevel = 0;
                        floodIncreaseAmount = 0.0f;
                        StartNewLevel();
                        gameState = GameStates.Playing;
                    }
                    break;

                case GameStates.Playing:
                    timeSinceLastInput += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    timeSinceLastFloodIncrease += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (timeSinceLastFloodIncrease >= timeBetweenFloodTicks)
                    {
                        floodAmount += floodIncreaseAmount;
                        timeSinceLastFloodIncrease = 0.0f;
                        
                        if (floodAmount >= MaxFloodCounter)
                        {
                            gameOverTimer = 8.0f;
                            gameState = GameStates.GameOver;
                        }
                    }

                    if (gameBoard.PiecesAreAnimating())
                    {
                        gameBoard.UpdateAnimatingPieces();
                    }
                    else
                    {
                        gameBoard.ResetFlooding();

                        for (int r = 0; r < GameBoard.height; r++)
                            CheckForScoringConnections(gameBoard.GetFloodedChain(r));

                        gameBoard.GeneratePieces(true);

                        if (timeSinceLastInput >= MinTimeBetweenInputs)
                            HandleMouseInput(Mouse.GetState());

                    }
                    break;

                case GameStates.GameOver:
                    gameOverTimer -= (float) gameTime.ElapsedGameTime.TotalSeconds;

                    if (gameOverTimer <= 0)
                        gameState = GameStates.Title;
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (gameState == GameStates.Title)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(title, new Rectangle(0, 0,
                    this.Window.ClientBounds.Width,
                    this.Window.ClientBounds.Height),
                    Color.White);
                spriteBatch.End();
            }

            if (gameState == GameStates.Playing || gameState == GameStates.GameOver)
            {
                spriteBatch.Begin();

                spriteBatch.Draw(bg, new Rectangle(0, 0,
                    this.Window.ClientBounds.Width,
                    this.Window.ClientBounds.Height),
                    Color.White);

                for (int c = 0; c < GameBoard.width; c++)
                {
                    for (int r = 0; r < GameBoard.height; r++)
                    {
                        int x = (int)gameBoardDisplayOffset.X + (c * Piece.width);
                        int y = (int)gameBoardDisplayOffset.Y + (r * Piece.height);
                        //string pos = x.ToString() + "_" + y.ToString();
                        string pos = c.ToString() + "_" + r.ToString();
                        bool isNormal = true;

                        DrawEmptyPiece(x, y);

                        if (gameBoard.rotatingPieces.ContainsKey(pos))
                        {
                            DrawRotatingPiece(x, y, pos);
                            isNormal = false;
                        }

                        if (gameBoard.fadingPieces.ContainsKey(pos))
                        {
                            DrawFadingPiece(x, y, pos);
                            isNormal = false;
                        }

                        if (gameBoard.fallingPieces.ContainsKey(pos))
                        {
                            DrawFallingPiece(x, y, pos);
                            isNormal = false;
                        }

                        if (isNormal)
                            DrawNormalPiece(x, y, c, r);


                    }
                }

                spriteBatch.DrawString(
                        pericles36Font,
                        playerScore.ToString(),
                        scoreDisplayOffset,
                        Color.Black);

                spriteBatch.DrawString(
                    pericles36Font,
                    curLevel.ToString(),
                    levelOffset,
                    Color.Black);
                    

                int waterHeight = (int)(MaxWaterHeight * (floodAmount / 100));
                spriteBatch.Draw(
                        bg,
                        new Rectangle(
                            (int)waterPositionOffset.X,
                            (int)waterPositionOffset.Y + (MaxWaterHeight - waterHeight),
                            WaterWidth,
                            waterHeight),
                        new Rectangle(
                            (int)waterOverlayOffset.X,
                            (int)waterOverlayOffset.Y + (MaxWaterHeight - waterHeight),
                            WaterWidth,
                            waterHeight),
                        new Color(255, 255, 255, 180));


                spriteBatch.End();
            }

            if (gameState == GameStates.GameOver)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(
                        pericles36Font,
                        "G A M E  O V E R!",
                        gameOverDisplayOffset,
                        Color.Yellow);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private int CalcScore(int numTiles)
        {
            return (int)((Math.Pow((numTiles / 5), 2) + numTiles) * 10);
        }

        private void CheckForScoringConnections(List<Vector2> PipeChain)
        {
            if (PipeChain.Count > 0)
            {
                Vector2 lastPipe = PipeChain[PipeChain.Count - 1];

                if (lastPipe.X == GameBoard.width - 1)
                {
                    if (gameBoard.ConnectedOnSide(
                        (int)lastPipe.X,
                        (int)lastPipe.Y,
                        Piece.sides.RIGHT))
                    {
                        playerScore += CalcScore(PipeChain.Count);
                        linesCompThisLvl++;
                        floodAmount = MathHelper.Clamp(floodAmount - CalcScore(PipeChain.Count) / 10, 0.0f, 100.0f);

                        foreach (Vector2 piece in PipeChain)
                        {
                            gameBoard.AddFadingPiece(
                                (int)piece.X,
                                (int)piece.Y,
                                gameBoard.GetPieceType(
                                (int)piece.X,
                                (int)piece.Y));

                            gameBoard.SetPieceType(
                                (int)piece.X,
                                (int)piece.Y,
                                Piece.pieceTypes.EMPTY);
                        }

                        if (linesCompThisLvl >= 10)
                            StartNewLevel();
                    }
                }
            }
        }

        private void HandleMouseInput(MouseState mouseState)
        {
            int c = ((mouseState.X - (int)gameBoardDisplayOffset.X) / Piece.width);
            int r = ((mouseState.Y - (int)gameBoardDisplayOffset.Y) / Piece.height);

            if ((c >= 0) && (c < GameBoard.width) &&
                (r >= 0) && (r < GameBoard.height))
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    gameBoard.AddRotatingPiece(c, r, gameBoard.GetPieceType(c, r), false);
                    gameBoard.RotatePiece(c, r, false);
                    timeSinceLastInput = 0.0f;
                }

                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    gameBoard.AddRotatingPiece(c, r, gameBoard.GetPieceType(c, r), true);
                    gameBoard.RotatePiece(c, r, true);                  
                    timeSinceLastInput = 0.0f;
                }
            }
        }

        private void DrawEmptyPiece(int x, int y)
        {
            spriteBatch.Draw(
                gamePieces,
                new Rectangle(x, y, Piece.width, Piece.height),
                emptyTilePointer,
                Color.White);
        }

        private void DrawNormalPiece(int x, int y, int boardCol, int boardRow)
        {
            spriteBatch.Draw(
                gamePieces,
                new Rectangle(x, y, Piece.width, Piece.height),
                gameBoard.GetBlitRect(boardCol, boardRow),
                Color.White);
        }

        private void DrawFallingPiece(int x, int y, string dictKey)
        {
            spriteBatch.Draw(
                gamePieces,
                new Rectangle(
                    x,
                    y - gameBoard.fallingPieces[dictKey].vertOffset,
                    Piece.width,
                    Piece.height),
                gameBoard.fallingPieces[dictKey].GetBlitRect(),
                Color.White);
        }

        private void DrawFadingPiece(int x, int y, string dictKey)
        {
            spriteBatch.Draw(
                gamePieces,
                new Rectangle(x, y, Piece.width, Piece.height),
                gameBoard.fadingPieces[dictKey].GetBlitRect(),
                Color.White * gameBoard.fadingPieces[dictKey].alphaLevel);
        }

        private void DrawRotatingPiece(int x, int y, string dictKey)
        {
            spriteBatch.Draw(
                gamePieces,
                new Rectangle(
                    x + (Piece.width / 2),
                    y + (Piece.height / 2),
                    Piece.width,
                    Piece.height),
                gameBoard.rotatingPieces[dictKey].GetBlitRect(),
                Color.White,
                gameBoard.rotatingPieces[dictKey].RotAmount,
                new Vector2(
                    Piece.width / 2,
                    Piece.height / 2),
                SpriteEffects.None,
                0.0f);
        }

        private void StartNewLevel()
        {
            curLevel++;
            floodAmount = 0.0f;
            linesCompThisLvl = 0;
            floodIncreaseAmount += floodIncreasePerLvl;
            gameBoard.Clear();
            gameBoard.GeneratePieces(false);
        }
    }
}
