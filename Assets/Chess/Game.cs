using System;
using System.Linq;
using UnityEngine;
using static Utility.Notation;

namespace Chess
{
    public class Game
    {
        public Piece?[] board { get; } = new Piece?[64];
        public SquarePos? enPassantPos { get; set; } = null;
        public int halfMoveClock { get; set; } = 0;
        public int fullMoveClock { get; set; } = 0;

        public Game(string fen="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            LoadFromFEN(fen);
            Debug.Log("Game created!");
        }

        public Piece[] GetPieces()
        {
            return board.Where(piece => piece.HasValue).Select(piece => piece.Value).ToArray();
        }
        
        public void LoadFromFEN(string fen)
        {
            string[] fields = fen.Split(' ');
            string[] ranks = fields[0].Split('/');

            Array.Clear(board, 0, board.Length);
            Array.Reverse(ranks);
            for (int r = 0; r < 8; r++)
            {
                for (int f = 0; f < 8;)
                {
                    if (Char.IsDigit(ranks[r][f]))
                    {
                        f += ranks[r][f] - '0';
                    }
                    else
                    {
                        var pos = (SquarePos)(8*r + f);
                        board[(int)pos] = new Piece(CharToPieceType[ranks[r][f]], pos);
                        f++;
                    }
                }
            }
        }
    }
}