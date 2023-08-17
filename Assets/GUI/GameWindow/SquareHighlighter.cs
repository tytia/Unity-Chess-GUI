namespace GUI.GameWindow {
    public static class SquareHighlighter {
        private static Move _prevMove;
        public static Square[] highlights { get; set; }

        private static Square GetSquareFrom(this Move move) {
            return highlights[move.from];
        }

        private static Square GetSquareTo(this Move move) {
            return highlights[move.to];
        }

        public static void HighlightMove(Move move) {
            Square fromSq = move.GetSquareFrom();
            Square toSq = move.GetSquareTo();
            
            _prevMove.GetSquareFrom().gameObject.SetActive(false);
            _prevMove.GetSquareTo().gameObject.SetActive(false);
            
            fromSq.gameObject.SetActive(true);
            toSq.gameObject.SetActive(true);
            fromSq.color = toSq.color = Square.prevMoveCol;
            
            _prevMove = move;
        }

        public static void ClearHighlights() {
            foreach (Square sq in highlights) {
                sq.gameObject.SetActive(false);
            }
        }
        
        public static void ClearPrevMoveHighlight() {
            _prevMove.GetSquareFrom().gameObject.SetActive(false);
            _prevMove.GetSquareTo().gameObject.SetActive(false);
        }
    }
}