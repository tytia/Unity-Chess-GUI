using Chess;
using UnityEngine;

namespace GUI.GameWindow {
    public static class PointConversion {
        public static Vector2 ToSquarePosVector2(this int index, PieceColor orientation) {
            return orientation == PieceColor.White
                ? new Vector2(index % 8, index / 8)
                : new Vector2(7 - index % 8, 7 - index / 8);
        }
        
        public static int ToBoardIndex(this Vector3 vec, PieceColor orientation) {
            return orientation == PieceColor.White ? (int)(vec.x + vec.y * 8) : (int)((7 - vec.x) + (7 - vec.y) * 8);
        }
    }
}