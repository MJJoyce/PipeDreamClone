using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PipeDreamClone
{
    class GameBoard
    {
        Random rand = new Random();

        public const int width = 8;
        public const int height = 10;

        private Piece[,] board = new Piece[width, height];

        private List<Vector2> FloodTracker = new List<Vector2>();

        public Dictionary<string, FallingPiece> fallingPieces =
            new Dictionary<string, FallingPiece>();
        public Dictionary<string, RotatingPiece> rotatingPieces =
            new Dictionary<string, RotatingPiece>();
        public Dictionary<string, FadingPiece> fadingPieces =
            new Dictionary<string, FadingPiece>();

        public GameBoard()
        {
            Clear();
        }

        public void Clear()
        {
            for (int c = 0; c < width; c++)
                for (int r = 0; r < height; r++)
                    board[c, r] = new Piece(Piece.pieceTypes.EMPTY);
        }

        public void RotatePiece(int c, int r, bool clkwise)
        {
            board[c, r].Rotate(clkwise);
        }

        public Rectangle GetBlitRect(int c, int r)
        {
            return board[c, r].GetBlitRect();
        }

        public Piece.pieceTypes GetPieceType(int c, int r)
        {
            return board[c, r].PieceType;
        }

        public void SetPieceType(int c, int r, Piece.pieceTypes type)
        {
            board[c, r].SetPiece(type);
        }

        public bool ConnectedOnSide(int c, int r, Piece.sides side)
        {
            return board[c, r].ConnectedOnSide(side);
        }

        public void SetRandomPiece(int c, int r)
        {
            board[c, r].SetPiece((Piece.pieceTypes)rand.Next(0, Piece.numPieces));
        }

        public void FillSpotFromAbove(int c, int r)
        {
            int upOneRow = r - 1;

            while (upOneRow >= 0)
            {
                if (GetPieceType(c, upOneRow) != Piece.pieceTypes.EMPTY)
                {
                    SetPieceType(c, r, GetPieceType(c, upOneRow));
                    SetPieceType(c, upOneRow, Piece.pieceTypes.EMPTY);
                    AddFallingPiece(c, r, GetPieceType(c, r), Piece.height * (r - upOneRow));
                    upOneRow = -1;
                }

                upOneRow--;
            }
        }

        public void GeneratePieces(bool dropPieces)
        {
            if (dropPieces)
            {
                for (int c = 0; c < GameBoard.width; c++)
                {
                    for (int r = GameBoard.height - 1; r >= 0; r--)
                    {
                        if (GetPieceType(c, r) == Piece.pieceTypes.EMPTY)
                            FillSpotFromAbove(c, r);
                    }
                }
            }

            for (int c = 0; c < GameBoard.width; c++)
            {
                for (int r = 0; r < GameBoard.height; r++)
                {
                    if (GetPieceType(c, r) == Piece.pieceTypes.EMPTY)
                    {
                        SetRandomPiece(c, r);
                        // Piece.height is the height of an individual pice.
                        // height is the total size of the game board in terms of piece count
                        AddFallingPiece(c, r, GetPieceType(c, r), Piece.height * height);
                    }
                }
            }
        }

        public void ResetFlooding()
        {
            for (int c = 0; c < GameBoard.width; c++)
                for (int r = 0; r < GameBoard.height; r++)
                    board[c, r].Flooded = false;
        }

        public void FloodPiece(int c, int r)
        {
            board[c, r].Flooded = true;
        }

        public void PropogateFlooding(int c, int r, Piece.sides dir)
        {
            if ((c >= 0) && (c < GameBoard.width) && (r >= 0) && (r < GameBoard.height))
            {
                if (board[c, r].ConnectedOnSide(dir) && !board[c, r].Flooded)
                {
                    board[c, r].Flooded = true;
                    FloodTracker.Add(new Vector2(c, r));

                    foreach (Piece.sides side in board[c, r].GetOtherConnections(dir))
                    {
                        switch (side)
                        {
                            case Piece.sides.LEFT:
                                PropogateFlooding(c - 1, r, Piece.sides.RIGHT);
                                break;

                            case Piece.sides.TOP:
                                PropogateFlooding(c, r - 1, Piece.sides.BOTTOM);
                                break;

                            case Piece.sides.RIGHT:
                                PropogateFlooding(c + 1, r, Piece.sides.LEFT);
                                break;

                            case Piece.sides.BOTTOM:
                                PropogateFlooding(c, r + 1, Piece.sides.TOP);
                                break;
                        }
                    }
                }
            }
        }

        public List<Vector2> GetFloodedChain(int r)
        {
            FloodTracker.Clear();
            PropogateFlooding(0, r, Piece.sides.LEFT);
            return FloodTracker;
        }

        public void AddFallingPiece(int c, int r, Piece.pieceTypes type, int vertOffset)
        {
            fallingPieces[c.ToString() + "_" + r.ToString()] = new FallingPiece(type, vertOffset);
        }

        public void AddRotatingPiece(int c, int r, Piece.pieceTypes type, bool clkwise)
        {
            rotatingPieces[c.ToString() + "_" + r.ToString()] = new RotatingPiece(type, clkwise);
        }

        public void AddFadingPiece(int c, int r, Piece.pieceTypes type)
        {
            fadingPieces[c.ToString() + "_" + r.ToString()] = new FadingPiece(type, true);
        }

        public bool PiecesAreAnimating()
        {
            if ((fallingPieces.Count == 0) &&
                (rotatingPieces.Count == 0) &&
                (fadingPieces.Count == 0))
                return false;
            else
                return true;
        }

        public void UpdateFadingPieces()
        {
            Queue<string> toRemove = new Queue<string>();

            foreach (string key in fadingPieces.Keys)
            {
                fadingPieces[key].UpdatePiece();

                if (fadingPieces[key].alphaLevel == 0.0f)
                    toRemove.Enqueue(key.ToString());
            }

            while (toRemove.Count > 0)
                fadingPieces.Remove(toRemove.Dequeue());
        }

        public void UpdateFallingPieces()
        {
            Queue<string> toRemove = new Queue<string>();

            foreach (string key in fallingPieces.Keys)
            {
                fallingPieces[key].UpdatePiece();

                if (fallingPieces[key].vertOffset == 0)
                    toRemove.Enqueue(key.ToString());
            }

            while (toRemove.Count > 0)
                fallingPieces.Remove(toRemove.Dequeue());
        }

        public void UpdateRotatingPieces()
        {
            Queue<string> toRemove = new Queue<string>();

            foreach (string key in rotatingPieces.Keys)
            {
                rotatingPieces[key].UpdatePiece();

                if (rotatingPieces[key].rotTicksRemaining == 0)
                    toRemove.Enqueue(key.ToString());
            }

            while (toRemove.Count > 0)
                rotatingPieces.Remove(toRemove.Dequeue());
        }

        public void UpdateAnimatingPieces()
        {
            if (fadingPieces.Count == 0)
            {
                UpdateFallingPieces();
                UpdateRotatingPieces();
            }
            else
            {
                UpdateFadingPieces();
            }
        }
    }
}
