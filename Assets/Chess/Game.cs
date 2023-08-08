using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utility.Notation;

namespace Chess {
    public class Game {
        public Piece?[] board { get; } = new Piece?[64];

        // TODO: Revise get & set accessors visibility
        public List<Piece> pieces { get; private set; } = new(32);
        public Side sideToMove { get; set; }
        public CastlingRights castlingRights { get; set; }
        public SquarePos? enPassantPos { get; set; }
        public int halfMoveClock { get; set; }
        public int fullMoveClock { get; set; }

        public Game(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") {
            LoadFromFEN(fen);
            Debug.Log("Game created!");
        }

        public void LoadFromFEN(string fen) {
            string[] fields = fen.Split(' ');

            // TODO: Very basic FEN validation, replace with IsValidFEN() once it's complete
            if (fields.Length != 6) {
                throw new ArgumentException("Invalid FEN string!");
            }

            Array.Clear(board, 0, board.Length);
            pieces.Clear();
            string[] ranks = fields[0].Split('/');
            for (int i = (int)SquarePos.a8, j = 0; j < ranks.Length; i -= 8, j++) {
                for (int file = 0; file < 8;) {
                    char c = ranks[j][file];
                    if (Char.IsDigit(c)) {
                        file += c - '0';
                    }
                    else {
                        pieces.Add(new Piece(CharToPieceType[c], (SquarePos)(i + file)));
                        board[i + file] = pieces.Last();
                        file++;
                    }
                }
            }

            sideToMove = fields[1] == "w" ? Side.White : Side.Black;

            castlingRights = CastlingRights.None;
            if (fields[2] != "-") {
                foreach (char c in fields[2]) {
                    castlingRights |= CharToCastlingRights[c];
                }
            }

            enPassantPos = fields[3] == "-" ? null : Enum.Parse<SquarePos>(fields[3]);
            halfMoveClock = Int32.Parse(fields[4]);
            fullMoveClock = Int32.Parse(fields[5]);
        }
    }

    public enum Side {
        White,
        Black
    }

    [Flags]
    public enum CastlingRights {
        None = 0,
        WhiteKingSide = 1,
        WhiteQueenSide = 2,
        BlackKingSide = 4,
        BlackQueenSide = 8
    }
}