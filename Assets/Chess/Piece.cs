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
    
    public struct Piece {
        public PieceType type { get; }
        public PieceColor color { get; }
        public int index { get; set; }
        
        public Piece(PieceType type, PieceColor color, int index) {
            this.type = type;
            this.color = color;
            this.index = index;
        }
        
        public Piece(PieceType type, PieceColor color) {
            this.type = type;
            this.color = color;
            index = -1;
        }
    }
}