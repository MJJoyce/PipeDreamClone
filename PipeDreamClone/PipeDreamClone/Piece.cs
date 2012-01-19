using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace PipeDreamClone
{
    class Piece
    {
        public enum pieceTypes {
            LINE = 0,
            BAR,
            LEFT_TOP,
            TOP_RIGHT,
            RIGHT_BOTTOM,
            BOTTOM_LEFT,
            EMPTY,
        };

        public enum sides {
            TOP = 0,
            RIGHT = 1,
            BOTTOM = 2,
            LEFT = 3
        };

        public const int height = 40;
        public const int width = 40;

        public const int numPieces = 6;

        private int textureOffsetX = 1;
        private int textureOffsetY = 1;
        private int texturePaddingX = 1;
        private int texturePaddingY = 1;

        private pieceTypes pieceType;
        private bool flooded;
        private bool[] connectedSides;

        public Piece(pieceTypes type, bool flooded)
        {
            pieceType = type;
            this.flooded = flooded;
            connectedSides = new bool[4];
            SetConnSides();

        }

        public Piece(pieceTypes type) : this(type, false)
        {
        }

        public pieceTypes PieceType
        {
            get { return pieceType; }
        }

        public bool Flooded
        {
            get { return flooded; }
            set { flooded = value; }
        }

        public void SetPiece(pieceTypes type, bool isFlooded)
        {
            pieceType = type;
            flooded = isFlooded;
            SetConnSides();
        }

        public void SetPiece(pieceTypes type)
        {
            SetPiece(type, false);
        }

        public void Rotate(bool clkwise)
        {
            switch (pieceType)
            {
                case pieceTypes.BAR:
                    pieceType = pieceTypes.LINE;
                    break;

                case pieceTypes.LINE:
                    pieceType = pieceTypes.BAR;
                    break;

                case pieceTypes.LEFT_TOP:
                    if (clkwise)
                        pieceType = pieceTypes.TOP_RIGHT;
                    else
                        pieceType = pieceTypes.BOTTOM_LEFT;
                    break;

                case pieceTypes.TOP_RIGHT:
                    if (clkwise)
                        pieceType = pieceTypes.RIGHT_BOTTOM;
                    else
                        pieceType = pieceTypes.LEFT_TOP;
                    break;

                case pieceTypes.RIGHT_BOTTOM:
                    if (clkwise)
                        pieceType = pieceTypes.BOTTOM_LEFT;
                    else
                        pieceType = pieceTypes.TOP_RIGHT;
                    break;

                case pieceTypes.BOTTOM_LEFT:
                    if (clkwise)
                        pieceType = pieceTypes.LEFT_TOP;
                    else
                        pieceType = pieceTypes.RIGHT_BOTTOM;
                    break;

                case pieceTypes.EMPTY:
                    break;
            }

            SetConnSides();
        }

        public List<sides> GetOtherConnections(sides start)
        {
            List<sides> otherSides = new List<sides>();

            for (int i = 0; i < connectedSides.Length; i++)
            {
                if (connectedSides[i])
                    if (i != (int)start)
                        otherSides.Add((sides)i);
            }

            return otherSides;
        }

        public bool ConnectedOnSide(sides side)
        {
            return connectedSides[(int)side];
        }

        public Rectangle GetBlitRect()
        {
            int x = textureOffsetX;
            int y = textureOffsetY;

            if (flooded)
                x += width + texturePaddingX;

            y += (int)pieceType * (height + texturePaddingY);

            return new Rectangle(x, y, width, height);
        }

        private void SetConnSides()
        {
            switch(this.pieceType)
            {
                case pieceTypes.BAR:
                    connectedSides[(int)sides.TOP] = true;
                    connectedSides[(int)sides.RIGHT] = false;
                    connectedSides[(int)sides.BOTTOM] = true;
                    connectedSides[(int)sides.LEFT] = false;
                    break;

                case pieceTypes.LINE:
                    connectedSides[(int)sides.TOP] = false;
                    connectedSides[(int)sides.RIGHT] = true;
                    connectedSides[(int)sides.BOTTOM] = false;
                    connectedSides[(int)sides.LEFT] = true;
                    break;

                case pieceTypes.LEFT_TOP:
                    connectedSides[(int)sides.TOP] = true;
                    connectedSides[(int)sides.RIGHT] = false;
                    connectedSides[(int)sides.BOTTOM] = false;
                    connectedSides[(int)sides.LEFT] = true;
                    break;

                case pieceTypes.TOP_RIGHT:
                    connectedSides[(int)sides.TOP] = true;
                    connectedSides[(int)sides.RIGHT] = true;
                    connectedSides[(int)sides.BOTTOM] = false;
                    connectedSides[(int)sides.LEFT] = false;
                    break;

                case pieceTypes.RIGHT_BOTTOM:
                    connectedSides[(int)sides.TOP] = false;
                    connectedSides[(int)sides.RIGHT] = true;
                    connectedSides[(int)sides.BOTTOM] = true;
                    connectedSides[(int)sides.LEFT] = false;
                    break;

                case pieceTypes.BOTTOM_LEFT:
                    connectedSides[(int)sides.TOP] = false;
                    connectedSides[(int)sides.RIGHT] = false;
                    connectedSides[(int)sides.BOTTOM] = true;
                    connectedSides[(int)sides.LEFT] = true;
                    break;

                case pieceTypes.EMPTY:
                    connectedSides[(int)sides.TOP] = false;
                    connectedSides[(int)sides.RIGHT] = false;
                    connectedSides[(int)sides.BOTTOM] = false;
                    connectedSides[(int)sides.LEFT] = false;
                    break;
            }
        }

            
    }
}
