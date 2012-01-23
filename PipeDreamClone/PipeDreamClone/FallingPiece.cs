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
        public static int fallRate = 5; // How many pixels to fall each update

        public FallingPiece(Piece.pieceTypes pieceType, int vertOffset) :
            base(pieceType)
        {
            this.vertOffset = vertOffset;
        }

        public void UpdatePiece()
        {
            vertOffset = (int)MathHelper.Max(0, vertOffset - fallRate);
        }
    }
}
