namespace Chess {
    public enum PieceType : byte {
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King,
    }
    
    public enum PieceColor : byte {
        White,
        Black
    }
    
    public readonly struct Piece {
        public PieceType type { get; }
        public PieceColor color { get; }
        
        public Piece(PieceType type, PieceColor color) {
            this.type = type;
            this.color = color;
        }

        public readonly override string ToString() {
            return $"{color} {type}";
        }
    }
}