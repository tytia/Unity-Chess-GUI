using System;
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
            LoadFromFEN(fen);
        }
        
        public void LoadFromFEN(string fen)
        {
            string[] fields = fen.Split(' ');
            string[] ranks = fields[0].Split('/');

            Array.Clear(board, 0, board.Length);
            for (int r = 7; r >= 0; r--)
            {
                int f = 0;
                while (f < 8)
                {
                    if (Char.IsDigit(ranks[r][f]))
                    {
                        f += ranks[r][f] - '0';
                    }
                    else
                    {
                        var pos = (SquarePos)(8*r + f);
                        board[(int)pos] = new Piece(CharToPieceType[ranks[r][f]], pos);
                    }
                }
            }
        }
    }
}