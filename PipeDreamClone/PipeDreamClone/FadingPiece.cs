using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PipeDreamClone
{
    class FadingPiece : Piece
    {
        public float alphaLevel = 1.0f;
        public static float alphaChangeRate = 0.02f; // Adjustment per tick

        public FadingPiece(Piece.pieceTypes pieceType, bool flooded) :
            base(pieceType, flooded)
        {
        }

        public void UpdatePiece()
        {
            alphaLevel = MathHelper.Max(0, alphaLevel - alphaChangeRate);
        }
    }
}
