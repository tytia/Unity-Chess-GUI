using System.Collections.Generic;
using System.Linq;
using Chess;
using UnityEngine;
using static Utility.Notation;

namespace GUI.GameWindow {
    public static class HighlightManager {
        private static (int from, int to)? _prevMoveHighlight;
        public static Piece? selectedPiece { get; set; }
        public static Square[] highlights { get; } = new Square[64];
        
        private static Move? prevMove => Game.GetInstance().prevMove;
        
        public static void InitHighlights(Square highlight) {
            Transform parent = GameObject.FindWithTag("Highlights").transform;
            for (var i = 0; i < 64; i++) {
                var point = i.ToSquarePosVector2();
                Square highlightSquare = Object.Instantiate(highlight, point, Quaternion.identity, parent);
                highlightSquare.name = i.ToString();

                highlights[i] = highlightSquare;
            }
        }

        public static void HighlightPrevMove() {
            if (_prevMoveHighlight != null) {
                highlights[_prevMoveHighlight.Value.from].gameObject.SetActive(false);
                highlights[_prevMoveHighlight.Value.to].gameObject.SetActive(false);
            }

            if (prevMove != null) {
                int from = prevMove.Value.from, to = prevMove.Value.to;
                Square fromSq = highlights[from];
                Square toSq = highlights[to];

                fromSq.gameObject.SetActive(true);
                toSq.gameObject.SetActive(true);
                fromSq.color = toSq.color = Square.prevMoveColor;

                _prevMoveHighlight = (from, to);
            }
        }

        public static void HighlightLegalMoves(Piece piece) {
            if (selectedPiece.Equals(piece)) {
                return;
            }
            
            UnHighlightLegalMoves();
            highlights[piece.index].gameObject.SetActive(true);
            highlights[piece.index].color = Square.legalMovesColor;

            foreach (int index in piece.GetLegalSquares()) {
                highlights[index].gameObject.SetActive(true);
                highlights[index].color = Square.legalMovesColor;
            }
        }

        public static void UnHighlightLegalMoves() {
            ClearHighlights();
            HighlightPrevMove();
            
            selectedPiece = null;
        }

        public static void ClearHighlights() {
            foreach (Square sq in highlights) {
                sq.gameObject.SetActive(false);
            }
        }
    }
}