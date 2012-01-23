using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PipeDreamClone
{
    class RotatingPiece : Piece
    {
        public bool clkwise;
        public static float rotRate = (MathHelper.PiOver2 / 10);
        public int rotTicksRemaining = 10;

        private float rotAmount = 0;

        public RotatingPiece(Piece.pieceTypes pieceType, bool clkwise) :
            base(pieceType)
        {
            this.clkwise = clkwise;
        }

        public float RotAmount
        {
            get
            {
                if (clkwise)
                    return rotAmount;
                else
                    return (MathHelper.Pi * 2) - rotAmount;
            }
        }

        public void UpdatePiece()
        {
            rotAmount += rotRate;
            rotTicksRemaining = (int)MathHelper.Max(0, rotTicksRemaining - 1);
        }
    }
}
