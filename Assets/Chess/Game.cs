using static Utility.Notation;

namespace Chess
{
    public class Game
    {
        public Piece?[] board { get; set; } = new Piece?[64];
        public SquarePos? enPassantPos { get; set; } = null;
        public int halfMoveClock { get; set; } = 0;
        public int fullMoveClock { get; set; } = 0;

        public Game(string fen="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR")
        {
            LoadFEN(this, fen);
        }
    }
}