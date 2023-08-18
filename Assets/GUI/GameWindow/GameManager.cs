/*
 * An intermediary class between the Game class and the GUI, helping to keep Game and GUI logic separate.
 */

using System.Collections.ObjectModel;
using Chess;

namespace GUI.GameWindow {
    public static class GameManager {
        private static readonly Game _game = Game.GetInstance();
        public static PieceManager pieceManager { get; set; }

        public static void StartNewGame(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") {
            _game.LoadFromFEN(fen);
            pieceManager.RemovePieces();
            pieceManager.InitPieces();
            SquareHighlighter.ClearHighlights();
        }
        
        public static ReadOnlyCollection<Piece> GetPieces() {
            return _game.pieces;
        }

        public static Move MovePiece(PieceGUI pieceGUI, int toIndex) {
            var move = new Move(pieceGUI.piece.index, toIndex);
            _game.MovePiece(ref pieceGUI.piece, toIndex); // this changes the index of pieceGUI.piece because ref
            return move;
        }
    }
}