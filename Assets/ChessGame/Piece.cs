using System;
using static Utility.Notation;

namespace ChessGame
{
    public struct Piece
    {
        public Piece(PieceType type, SquarePos pos)
        {
            this.type = type;
            this.pos = pos;
        }
        
        public PieceType type { get; set; }
        public SquarePos pos { get; set; }
    }
    
    public enum PieceType : byte
    {
        Pawn = 1,
        Knight = 2,
        Bishop = 3,
        Rook = 4,
        Queen = 5,
        King = 6,
        White = 8,
        Black = 16
    }
}