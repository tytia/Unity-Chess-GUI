using System;
using UnityEngine.Assertions;

namespace Chess {
    public static class Moves {
        private static readonly Game _game = Game.instance;
        private static readonly StateManager _stateManager = StateManager.instance;
        
        public static event EventHandler MoveEnd;
        public static event EventHandler<CastleEventArgs> Castle;
        public static event EventHandler<EnPassantEventArgs> EnPassant;
        public static event EventHandler<PromotionEventArgs> Promotion;
        
        internal static void OnMoveEnd() {
            _game.IncrementTurn();
            MoveGenerator.UpdateData();
            _game.CheckGameEnd();
            MoveEnd?.Invoke(null, EventArgs.Empty);
        }
        
        public static void MovePiece(int from, int to) {
            _stateManager.RecordState();
            _game.prevMove = new Move(from, to);;

            _game._board[to] = _game._board[from];
            _game._board[from] = null;

            if (LastMoveWasCastle()) {
                OnCastle();
            }
            else if (LastMoveWasEnPassant()) {
                OnEnPassant();
            }
            else if (LastMoveWasPromotion()) {
                // because PromotePawn() does not modify the argument pawn, but instead directly changes the type
                // of the pawn on the board, we need to call it after the move is made.
                Promotion?.Invoke(null, new PromotionEventArgs(to));
                return; // PromotePawn() will conclude the move
            }

            OnMoveEnd();
            return;

            void OnCastle() {
                int rookTo = from + (to - from) / 2;
                int rookFrom = MoveGenerator.GetCastleTargetRookPos(from, to);

                _game._board[rookTo] = _game._board[rookFrom];
                _game._board[rookFrom] = null;

                Castle?.Invoke(null, new CastleEventArgs(rookFrom, rookTo));
            }

            void OnEnPassant() {
                int captureIndex = _game.colorToMove == PieceColor.White ? to - 8 : to + 8;

                _game._board[captureIndex] = null;
                EnPassant?.Invoke(null, new EnPassantEventArgs(captureIndex));
            }
        }

        public static void PromotePawn(int pawnIndex, PieceType type) {
            if (_stateManager.last is not null) {
                Assert.AreNotEqual(_game._board, _stateManager.last.board,
                    "Move is already finished; PromotePawn() should be called to conclude the move.");
            }
            
            Piece pawn = _game._board[pawnIndex]!.Value;
            if (pawn.type != PieceType.Pawn) {
                throw new ArgumentException("Piece must be a pawn");
            }

            if (pawnIndex is > 7 and < 56) {
                throw new ArgumentException("Pawn must be on the last rank");
            }

            Piece promoted = new(type, pawn.color);
            _game._board[pawnIndex] = promoted;

            OnMoveEnd();
        }
        
        public static bool LastMoveWasCapture() {
            if (_game.prevMove == null) {
                throw new InvalidOperationException("MoveWasCapture() called before any moves were made");
            }

            Move move = _game.prevMove.Value;
            return _stateManager.last.board[move.to] != null;
        }

        public static bool LastMoveWasCastle() {
            if (_game.prevMove == null) {
                throw new InvalidOperationException("MoveWasCastle() called before any moves were made");
            }

            Move move = _game.prevMove.Value;
            return move.piece.type == PieceType.King && Math.Abs(move.from - move.to) == 2;
        }

        public static bool LastMoveWasEnPassant() {
            if (_game.prevMove == null) {
                throw new InvalidOperationException("MoveWasEnPassant() called before any moves were made");
            }

            Move move = _game.prevMove.Value;
            return move.piece.type == PieceType.Pawn && move.to == _game.enPassantIndex;
        }

        public static bool LastMoveWasPromotion() {
            if (_game.prevMove == null) {
                throw new InvalidOperationException("MoveWasPromotion() called before any moves were made");
            }

            Move move = _game.prevMove.Value;
            return move.piece.type == PieceType.Pawn && (move.to is < 8 or > 55);
        }
    }

    public class CastleEventArgs : EventArgs {
        public int rookFrom { get; }
        public int rookTo { get; }
        
        public CastleEventArgs(int rookFrom, int rookTo) {
            this.rookFrom = rookFrom;
            this.rookTo = rookTo;
        }
    }

    public class EnPassantEventArgs : EventArgs {
        public int captureIndex { get; }

        public EnPassantEventArgs(int captureIndex) {
            this.captureIndex = captureIndex;
        }
    }

    public class PromotionEventArgs : EventArgs {
        public int pawnIndex { get; }
        
        public PromotionEventArgs(int pawnIndex) {
            this.pawnIndex = pawnIndex;
        }
    }
}