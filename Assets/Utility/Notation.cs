using System.Collections.Generic;
using Chess;
using UnityEngine;

namespace Utility
{
    public static class Notation
    {
        public enum SquarePos : byte
        {
            a1, b1, c1, d1, e1, f1, g1, h1,
            a2, b2, c2, d2, e2, f2, g2, h2,
            a3, b3, c3, d3, e3, f3, g3, h3,
            a4, b4, c4, d4, e4, f4, g4, h4,
            a5, b5, c5, d5, e5, f5, g5, h5,
            a6, b6, c6, d6, e6, f6, g6, h6,
            a7, b7, c7, d7, e7, f7, g7, h7,
            a8, b8, c8, d8, e8, f8, g8, h8
        }

        public static Vector2 ToVector2(this SquarePos pos)
        {
            return new Vector2((int)pos % 8, (int)pos / 8);
        }
        
        public static readonly Dictionary<char, PieceType> CharToPieceType = new()
        {
            {'P', PieceType.Pawn | PieceType.White},
            {'N', PieceType.Knight | PieceType.White},
            {'B', PieceType.Bishop | PieceType.White},
            {'R', PieceType.Rook | PieceType.White},
            {'Q', PieceType.Queen | PieceType.White},
            {'K', PieceType.King | PieceType.White},
            {'p', PieceType.Pawn | PieceType.Black},
            {'n', PieceType.Knight | PieceType.Black},
            {'b', PieceType.Bishop | PieceType.Black},
            {'r', PieceType.Rook | PieceType.Black},
            {'q', PieceType.Queen | PieceType.Black},
            {'k', PieceType.King | PieceType.Black}
        };
    }
}