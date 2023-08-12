using System.Collections.ObjectModel;
using UnityEngine;
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
        
        public static ReadOnlyCollection<Chess.Piece> GetPieces() {
            return _game.pieces;
        }

        public static void CapturePiece(Piece pieceGUI) {
            _game.CapturePiece(pieceGUI.piece);
            Object.Destroy(pieceGUI.gameObject);
        }
    }
}