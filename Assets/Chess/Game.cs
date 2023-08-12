/*
 * This class is responsible for managing the state of the game.
 *
 * Regarding draws, this implementation will use:
 * 1. The threefold repetition rule
 * 2. The 75-move rule (instant draw)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Utility;
using static Utility.Notation;

namespace Chess {
    public class Game {
        public Piece?[] board { get; } = new Piece?[64];

        // TODO: Revise get & set accessors visibility
        public List<Piece> pieces { get; private set; } = new(32);
        public Side sideToMove { get; set; }
        public CastlingRights castlingRights { get; set; }
        public int? enPassantIndex { get; set; }
        public int halfMoveClock { get; set; }
        public int fullMoveClock { get; set; }

        public void CapturePiece(Piece piece) {
            pieces.Remove(piece);
            board[piece.index] = null;
        }

        public Game(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") {
            LoadFromFEN(fen);
        }

        private void LoadFromFEN(in string fen) {
            // GUI input field will be validated before calling this method
            string[] fields = fen.Split(' ');

            Array.Clear(board, 0, board.Length);
            pieces.Clear();
            string[] ranks = fields[0].Split('/');
            for (int i = (int)SquarePos.a8, j = 0; j < 8; i -= 8, j++) {
                for (int fileIndex = 0, file = 0; fileIndex < ranks[j].Length && file < 8; fileIndex++) {
                    char c = ranks[j][fileIndex];
                    if (Char.IsDigit(c)) {
                        file += c - '0';
                    }
                    else {
                        pieces.Add(new Piece(Notation.charToPieceType[c], i + file));
                        board[i + file] = pieces.Last();
                        file++;
                    }
                }
            }

            sideToMove = fields[1] == "w" ? Side.White : Side.Black;

            castlingRights = CastlingRights.None;
            if (fields[2] != "-") {
                foreach (char c in fields[2]) {
                    castlingRights |= Notation.charToCastlingRights[c];
                }
            }

            enPassantIndex = fields[3] == "-" ? null : (int)Enum.Parse<SquarePos>(fields[3]);
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