using Chess;
using UnityEngine;
using static Utility.Notation;

namespace GUI.GameWindow {
    public static class HighlightManager {
        private static (int from, int to)? _prevMoveHighlight;
        public static int selectedIndex { get; set; } = -1;
        public static Square[] highlights { get; } = new Square[64];
        private static readonly Game _game = Game.instance;
        private static Move? prevMove => _game.prevMove;
        
        public static void InitHighlights(Square highlight) {
            Transform parent = GameObject.FindWithTag("Highlights").transform;
            for (var i = 0; i < 64; i++) {
                var point = i.ToSquarePosVector2(_game.playerColor);
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

        public static void HighlightLegalMoves(int pieceIndex) {
            if (selectedIndex.Equals(pieceIndex)) {
                return;
            }
            
            UnHighlightLegalMoves();
            highlights[pieceIndex].gameObject.SetActive(true);
            highlights[pieceIndex].color = Square.legalMovesColor;

            foreach (int index in MoveGenerator.GetLegalSquares(pieceIndex)) {
                highlights[index].gameObject.SetActive(true);
                highlights[index].color = Square.legalMovesColor;
            }
        }

        public static void UnHighlightLegalMoves() {
            ClearHighlights();
            HighlightPrevMove();
            
            selectedIndex = -1;
        }

        public static void ClearHighlights() {
            foreach (Square sq in highlights) {
                sq.gameObject.SetActive(false);
            }
        }
    }
}