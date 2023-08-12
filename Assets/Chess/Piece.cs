using static Utility.Notation;

namespace Chess {
    public struct Piece {
        public Piece(PieceType type, int index) {
            this.type = type;
            this.index = index;
        }

        public PieceType type { get; set; }
        public int index { get; set; }
    }

    public enum PieceType : byte {
        Pawn = 0,
        Knight = 1,
        Bishop = 2,
        Rook = 3,
        Queen = 4,
        King = 5,
        White = 8,
        Black = 16
    }
}