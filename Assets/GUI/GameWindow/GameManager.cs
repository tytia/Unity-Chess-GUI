using System.Collections.ObjectModel;
using Chess;

namespace GUI.GameWindow {
    public static class GameManager {
        private static Game _game = new();
        public static PieceManager pieceManager { get; set; }

        public static void StartNewGame(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") {
            _game = new Game(fen);
            pieceManager.RemovePieces();
            pieceManager.InitPieces();
        }
        
        public static ReadOnlyCollection<Piece> GetPieces() {
            return _game.pieces;
        }

        public static Move MovePiece(PieceGUI pieceGUI, int toIndex) {
            var move = new Move(pieceGUI.piece.index, toIndex);
            _game.MovePiece(ref pieceGUI.piece, toIndex); // this changes the index of pieceGUI.piece because ref
            return move;
        }

        public static void CapturePiece(PieceGUI pieceGUI) {
            _game.CapturePiece(pieceGUI.piece);
        }
    }
}