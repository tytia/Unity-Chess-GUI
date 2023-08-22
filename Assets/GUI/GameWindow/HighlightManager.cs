using System.Collections.ObjectModel;
using Chess;
using UnityEngine;
using static Utility.Notation;

namespace GUI.GameWindow {
    public static class HighlightManager {
        private static (int from, int to)? _prevMove;
        public static Piece? highlightedPiece { get; set; }
        public static Square[] highlights { get; } = new Square[64];
        
        public static void InitHighlights(Square highlight) {
            Transform parent = GameObject.FindWithTag("Highlights").transform;
            for (var pos = SquarePos.a1; pos <= SquarePos.h8; pos++) {
                var point = pos.ToVector2();
                Square highlightSquare = Object.Instantiate(highlight, point, Quaternion.identity, parent);
                highlightSquare.name = pos.ToString();

                highlights[(int)pos] = highlightSquare;
            }
        }
        
        public static void HighlightMove(int from, int to) {
            Square fromSq = highlights[from];
            Square toSq = highlights[to];

            if (_prevMove != null) {
                highlights[_prevMove.Value.from].gameObject.SetActive(false);
                highlights[_prevMove.Value.to].gameObject.SetActive(false);
            }

            fromSq.gameObject.SetActive(true);
            toSq.gameObject.SetActive(true);
            fromSq.color = toSq.color = Square.prevMoveCol;

            _prevMove = (from, to);
        }

        public static void HighlightLegalMoves(Piece piece) {
            if (highlightedPiece.Equals(piece)) {
                return;
            }
            
            UnHighlightLegalMoves();
            highlights[piece.index].gameObject.SetActive(true);
            highlights[piece.index].color = Square.legalMovesCol;

            foreach (int index in piece.GetLegalSquares()) {
                highlights[index].gameObject.SetActive(true);
                highlights[index].color = Square.legalMovesCol;
            }
        }

        public static void UnHighlightLegalMoves() {
            ClearHighlights();
            if (_prevMove != null) { // reapply previous move highlight if it existed
                HighlightMove(_prevMove.Value.from, _prevMove.Value.to);
            }

            highlightedPiece = null;
        }

        public static void ClearHighlights() {
            foreach (Square sq in highlights) {
                sq.gameObject.SetActive(false);
            }
        }
    }
}