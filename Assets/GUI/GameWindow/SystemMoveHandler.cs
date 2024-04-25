using Chess;
using UnityEngine;
using static GUI.GameWindow.HighlightManager;

namespace GUI.GameWindow {
    public static class SystemMoveHandler {
        private static readonly Game _game = Game.instance;
        
        public static void Initialise() {
            Moves.Castle += CastleRook;
            Moves.EnPassant += EnPassantCapture;
        }
        
        private static void MovePieceGUI(PieceGUI pieceGUI, int to) {
            // it is important to note that this method only affects the GUI and updates
            // which piece the specified PieceGUI is referring to
            if (to == pieceGUI.index) {
                throw new System.ArgumentException("Piece's index should not be the same as the index it's moving to");
            }

            Object.Destroy(Board.GetPieceGUI(to)?.gameObject);

            pieceGUI.transform.parent = Board.GetSquare(to).transform;
            pieceGUI.transform.position = pieceGUI.transform.parent.position;
            pieceGUI.index = to;
        }

        public static void MovePieceGUI(int from, int to) {
            PieceGUI pieceGUI = Board.GetPieceGUI(from);
            if (pieceGUI is null) {
                throw new System.ArgumentException("No piece at index " + from);
            }
            
            MovePieceGUI(pieceGUI, to);
        }

        private static void CastleRook(object sender, CastleEventArgs e) {
            MovePieceGUI(e.rookFrom, e.rookTo);
        }
        
        private static void EnPassantCapture(object sender, EnPassantEventArgs e) {
            Object.Destroy(Board.GetPieceGUI(e.captureIndex).gameObject);
        }
        
        public static void UndoMove() {
            Move move = _game.prevMove!.Value;
            PieceGUI pieceGUI = Board.GetPieceGUI(move.to);

            MovePieceGUI(pieceGUI, move.from);

            if (Moves.LastMoveWasPromotion()) {
                // cancel promotion - undo move without saving current state to redo list
                _game.stateManager.ApplyState(_game.stateManager.last);
            }
            else {
                if (Moves.LastMoveWasPromotion()) {
                    pieceGUI.SetSprite(PieceManager.PieceToSprite(move.piece));
                    pieceGUI.name = move.piece.ToString();
                }
                
                _game.stateManager.Undo();
            }
            
            HighlightPrevMove();

            if (_game.board[move.to] != null) {
                // instantiate captured piece
                PieceGUI revivedPieceGUI = Object.Instantiate(pieceGUI, move.to.ToSquarePosVector2(_game.playerColor), Quaternion.identity,
                    Board.GetSquare(move.to).transform);
                revivedPieceGUI.index = move.to;
                revivedPieceGUI.name = revivedPieceGUI.piece.ToString();
                revivedPieceGUI.SetSprite(PieceManager.PieceToSprite(revivedPieceGUI.piece));
            }
        }
    }
}