using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess {
    public static class Notation {
        public const string StartingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
        
        public enum SquarePos : byte {
            a1, b1, c1, d1, e1, f1, g1, h1,
            a2, b2, c2, d2, e2, f2, g2, h2,
            a3, b3, c3, d3, e3, f3, g3, h3,
            a4, b4, c4, d4, e4, f4, g4, h4,
            a5, b5, c5, d5, e5, f5, g5, h5,
            a6, b6, c6, d6, e6, f6, g6, h6,
            a7, b7, c7, d7, e7, f7, g7, h7,
            a8, b8, c8, d8, e8, f8, g8, h8
        }

        public static readonly Dictionary<char, (PieceType type, PieceColor color)> charToPieceInfo = new() {
            { 'P', (PieceType.Pawn, PieceColor.White) },
            { 'N', (PieceType.Knight, PieceColor.White) },
            { 'B', (PieceType.Bishop, PieceColor.White) },
            { 'R', (PieceType.Rook, PieceColor.White) },
            { 'Q', (PieceType.Queen, PieceColor.White) },
            { 'K', (PieceType.King, PieceColor.White) },
            { 'p', (PieceType.Pawn, PieceColor.Black) },
            { 'n', (PieceType.Knight, PieceColor.Black) },
            { 'b', (PieceType.Bishop, PieceColor.Black) },
            { 'r', (PieceType.Rook, PieceColor.Black) },
            { 'q', (PieceType.Queen, PieceColor.Black) },
            { 'k', (PieceType.King, PieceColor.Black) }
        };

        public static readonly Dictionary<char, CastlingRights> charToCastlingRights = new() {
            { 'K', CastlingRights.WhiteKingSide },
            { 'Q', CastlingRights.WhiteQueenSide },
            { 'k', CastlingRights.BlackKingSide },
            { 'q', CastlingRights.BlackQueenSide }
        };

        public static bool IsValidFEN(in string fen) {
            // TODO: Validation is incomplete; it does not account for correct game logic.
            // See: https://chess.stackexchange.com/questions/1482/how-do-you-know-when-a-fen-position-is-legal
            string[] fields = fen.Split(' ');

            if (fields.Length is < 1 or > 6) {
                return false;
            }

            string[] ranks = fields[0].Split('/');
            if (ranks.Length != 8) {
                return false;
            }

            foreach (string rank in ranks) {
                var files = 0;
                foreach (char c in rank) {
                    if (!charToPieceInfo.ContainsKey(c) && !Char.IsDigit(c)) {
                        return false;
                    }

                    files += (Char.IsDigit(c)) ? c - '0' : 1;
                }

                if (files != 8) {
                    return false;
                }
            }

            if (fields.Length < 2) return true;
            if (fields[1] != "w" && fields[1] != "b") {
                return false;
            }

            if (fields.Length < 3) return true;
            bool areValidCastlingRights = fields[2].All(c => charToCastlingRights.ContainsKey(c));
            bool isDistinct = fields[2].Distinct().Count() == fields[2].Length;
            if ((!areValidCastlingRights || !isDistinct) && fields[2] != "-") {
                return false;
            }
            
            if (fields.Length < 4) return true;
            if (!Enum.TryParse<SquarePos>(fields[3], out _) && fields[3] != "-") {
                return false;
            }

            if (fields.Length < 5) return true;
            if (!Int32.TryParse(fields[4], out int num) || num >= 150) {
                return false;
            }

            if (fields.Length < 6) return true;
            if (!Int32.TryParse(fields[5], out int _)) {
                return false;
            }

            return true;
        }
    }
}