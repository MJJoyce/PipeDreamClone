using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PipeDreamClone
{
    class FallingPiece : Piece
    {
        public int vertOffset;
        public static float fallRate = 1; // How many pixels to fall each update
        private float fallRateIncrease = 0.004f;
        private float maxFallRate = 10.0f;

        public FallingPiece(Piece.pieceTypes pieceType, int vertOffset) :
            base(pieceType)
        {
            this.vertOffset = vertOffset;
        }

        public void UpdatePiece()
        {
            fallRate = MathHelper.Min(maxFallRate, fallRate + fallRateIncrease);
            vertOffset = (int)MathHelper.Max(0, vertOffset - (int)fallRate);
        }
    }
}
