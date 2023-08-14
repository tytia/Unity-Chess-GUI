namespace GUI.GameWindow {
    public static class SquareHighlighter {
        private static Board _board;
        private static Move _prevMoveHighlight;

        public static void Init(Board board) {
            _board = board;
        }

        private static Square GetSquareFrom(this Move move) {
            return _board.GetHighlight(move.from);
        }

        private static Square GetSquareTo(this Move move) {
            return _board.GetHighlight(move.to);
        }

        public static void HighlightMove(Move move) {
            Square fromSq = move.GetSquareFrom();
            Square toSq = move.GetSquareTo();
            
            _prevMoveHighlight.GetSquareFrom().gameObject.SetActive(false);
            _prevMoveHighlight.GetSquareTo().gameObject.SetActive(false);
            
            fromSq.gameObject.SetActive(true);
            toSq.gameObject.SetActive(true);
            fromSq.color = toSq.color = Square.prevMoveCol;
            
            _prevMoveHighlight = move;
        }
    }
}