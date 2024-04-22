using System;
using GUI.GameWindow;
using GUI.GameWindow.Popups;
using UnityEngine.Assertions;
using static UnityEngine.Object;

namespace Chess {
    public static class Moves {
        private static Game game => Game.instance;
        private static StateManager stateManager => StateManager.instance;
        
        public static void MovePiece(int from, int to) {
            stateManager.RecordState();
            game.prevMove = new Move(from, to);;

            game._board[to] = game._board[from];
            game._board[from] = null;

            if (LastMoveWasCastle()) {
                CastleRookMove(from);
            }
            else if (LastMoveWasEnPassant()) {
                EnPassant(from);
            }
            else if (LastMoveWasPromotion()) {
                // because PromotePawn() does not modify the argument pawn, but instead directly changes the type
                // of the pawn on the board, we need to call it after the move is made.
                PopupManager.ShowPawnPromotionPopup(to);
                return; // PromotePawn() will conclude the move
            }

            game.OnMoveEnd();
            return;

            void CastleRookMove(int kingIndex) {
                int rookTo = kingIndex + (to - kingIndex) / 2;
                int rookFrom = MoveGenerator.CastleTargetRookPos(kingIndex, to);
                PieceGUI rookGUI = Board.GetPieceGUI(rookFrom)!;

                game._board[rookTo] = game._board[rookFrom];
                game._board[rookFrom] = null;

                rookGUI.transform.parent = Board.GetSquare(rookTo).transform;
                rookGUI.transform.position = rookGUI.transform.parent.position;
            }

            void EnPassant(int pawnIndex) {
                int captureIndex = game.colorToMove == PieceColor.White ? to - 8 : to + 8;

                game._board[captureIndex] = null;
                Destroy(Board.GetPieceGUI(captureIndex).gameObject);
            }
        }

        public static void PromotePawn(int pawnIndex, PieceType type) {
            if (stateManager.last is not null) {
                Assert.AreNotEqual(game._board, stateManager.last.board,
                    "Move is already finished; PromotePawn() should be called to conclude the move.");
            }
            
            Piece pawn = game._board[pawnIndex]!.Value;
            if (pawn.type != PieceType.Pawn) {
                throw new ArgumentException("Piece must be a pawn");
            }

            if (pawnIndex is > 7 and < 56) {
                throw new ArgumentException("Pawn must be on the last rank");
            }

            Piece promoted = new(type, pawn.color);
            game._board[pawnIndex] = promoted;

            game.OnMoveEnd();
        }
        
        public static bool LastMoveWasCapture() {
            if (game.prevMove == null) {
                throw new InvalidOperationException("MoveWasCapture() called before any moves were made");
            }

            Move move = game.prevMove.Value;
            return stateManager.last.board[move.to] != null;
        }

        public static bool LastMoveWasCastle() {
            if (game.prevMove == null) {
                throw new InvalidOperationException("MoveWasCastle() called before any moves were made");
            }

            Move move = game.prevMove.Value;
            return move.piece.type == PieceType.King && Math.Abs(move.from - move.to) == 2;
        }

        public static bool LastMoveWasEnPassant() {
            if (game.prevMove == null) {
                throw new InvalidOperationException("MoveWasEnPassant() called before any moves were made");
            }

            Move move = game.prevMove.Value;
            return move.piece.type == PieceType.Pawn && move.to == game.enPassantIndex;
        }

        public static bool LastMoveWasPromotion() {
            if (game.prevMove == null) {
                throw new InvalidOperationException("MoveWasPromotion() called before any moves were made");
            }

            Move move = game.prevMove.Value;
            return move.piece.type == PieceType.Pawn && (move.to is < 8 or > 55);
        }
    }
}