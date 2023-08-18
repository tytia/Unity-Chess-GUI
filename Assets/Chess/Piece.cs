namespace Chess {
    public enum PieceType : byte {
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King,
    }
    
    public enum Side : byte {
        White,
        Black
    }
    
    public struct Piece {
        public Piece(PieceType type, Side side, int index) {
            this.type = type;
            this.side = side;
            this.index = index;
        }

        public Piece((PieceType type, Side side) pieceInfo, int index) {
            type = pieceInfo.type;
            side = pieceInfo.side;
            this.index = index;
        }

        public PieceType type { get; }
        public Side side { get; }
        public int index { get; set; }
    }
}