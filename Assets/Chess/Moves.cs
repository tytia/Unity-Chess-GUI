/*
 * This class is responsible for generating and making legal moves.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using GUI.GameWindow;
using UnityEngine.Assertions;
using static UnityEngine.Object;

namespace Chess {
    public readonly struct Move {
        public Piece piece { get; }
        public int to { get; }

        public Move(Piece piece, int to) {
            this.piece = piece;
            this.to = to;
        }
    }

    public static class Moves {
        private static readonly Game _game = Game.GetInstance();
        private static readonly Dictionary<int, int>[] _distanceToEdge = new Dictionary<int, int>[64];
        public static event EventHandler MoveEnd;

        static Moves() {
            InitDistanceToEdge();
        }

        private static void InitDistanceToEdge() {
            for (var i = 0; i < 64; i++) {
                _distanceToEdge[i] = new Dictionary<int, int>(8) {
                    { -1, i % 8 }, // left
                    { 1, 7 - i % 8 }, // right
                    { -8, i / 8 }, // down
                    { 8, 7 - i / 8 } // up
                };
                // We're self-referencing cardinal directions to calculate diagonals,
                // therefore can't use collection initializer to initialize them all at once
                _distanceToEdge[i][7] = Math.Min(_distanceToEdge[i][8], _distanceToEdge[i][-1]); // up-left
                _distanceToEdge[i][9] = Math.Min(_distanceToEdge[i][8], _distanceToEdge[i][1]); // up-right
                _distanceToEdge[i][-9] = Math.Min(_distanceToEdge[i][-8], _distanceToEdge[i][-1]); // down-left
                _distanceToEdge[i][-7] = Math.Min(_distanceToEdge[i][-8], _distanceToEdge[i][1]); // down-right
            }
        }

        public static List<int> GetLegalSquares(this Piece piece) {
            if (piece.color != _game.colorToMove) {
                return new List<int>();
            }

            return piece.type switch {
                PieceType.Pawn => GetPawnMoves(piece.index),
                PieceType.Knight => GetKnightMoves(piece.index),
                PieceType.Bishop or PieceType.Rook or PieceType.Queen => GetSlidingMoves(piece),
                PieceType.King => GetKingMoves(piece.index),
                _ => throw new ArgumentOutOfRangeException(nameof(piece.type), "Piece type not recognized")
            };
        }

        private static List<int> GetPawnMoves(int index) {
            List<int> moves = new();
            var offsets =
                Array.AsReadOnly(_game.colorToMove == PieceColor.White
                    ? new[] { 8, 16, 7, 9 }
                    : new[] { -8, -16, -7, -9 });

            foreach (int offset in offsets) {
                if (Math.Abs(offset) != 16 && _distanceToEdge[index][offset] < 1) {
                    continue;
                }

                if (Math.Abs(offset) == 16 && !OnStartingRank(index)) {
                    continue;
                }

                int toIndex = index + offset;
                bool existingPiece = _game.board[toIndex] != null;
                switch (Math.Abs(offset)) {
                    case 8 when !existingPiece:
                        moves.Add(toIndex);
                        break;
                    case 16 when !existingPiece && _game.board[toIndex - (offset / 2)] == null:
                        moves.Add(toIndex);
                        break;
                    case 7 or 9 when (existingPiece && _game.board[toIndex].Value.color != _game.colorToMove) ||
                                     toIndex == _game.enPassantIndex:
                        moves.Add(toIndex);
                        break;
                }
            }

            return moves;

            bool OnStartingRank(int i) {
                return _game.colorToMove == PieceColor.White ? i is >= 8 and <= 15 : i is >= 48 and <= 55;
            }
        }

        private static List<int> GetKnightMoves(int index) {
            List<int> moves = new();
            var offsets = Array.AsReadOnly(new[] { -17, -15, -10, -6, 6, 10, 15, 17 });
            Dictionary<int, int> dist = _distanceToEdge[index];
            var isValidOffset = new Dictionary<int, bool>(8) {
                { -17, dist[-1] >= 1 && dist[-8] >= 2 },
                { -15, dist[1] >= 1 && dist[-8] >= 2 },
                { -10, dist[-1] >= 2 && dist[-8] >= 1 },
                { -6, dist[1] >= 2 && dist[-8] >= 1 },
                { 6, dist[-1] >= 2 && dist[8] >= 1 },
                { 10, dist[1] >= 2 && dist[8] >= 1 },
                { 15, dist[-1] >= 1 && dist[8] >= 2 },
                { 17, dist[1] >= 1 && dist[8] >= 2 }
            };

            foreach (int offset in offsets) {
                if (!isValidOffset[offset]) {
                    continue;
                }

                int toIndex = index + offset;
                if (_game.board[toIndex] == null || _game.board[toIndex].Value.color != _game.colorToMove) {
                    moves.Add(toIndex);
                }
            }

            return moves;
        }

        private static List<int> GetSlidingMoves(Piece piece) {
            List<int> moves = new();
            var directions = piece.type switch {
                PieceType.Bishop => Array.AsReadOnly(new[] { -9, -7, 7, 9 }),
                PieceType.Rook => Array.AsReadOnly(new[] { -8, -1, 1, 8 }),
                PieceType.Queen => Array.AsReadOnly(new[] { -9, -8, -7, -1, 1, 7, 8, 9 }),
                _ => throw new ArgumentOutOfRangeException(nameof(piece.type), "Not a sliding piece")
            };

            foreach (int dir in directions) {
                int toIndex = piece.index;
                for (var _ = 0; _ < _distanceToEdge[piece.index][dir]; _++) {
                    toIndex += dir;
                    if (_game.board[toIndex] != null) {
                        // blocked by a piece
                        if (_game.board[toIndex].Value.color != _game.colorToMove) {
                            // can capture
                            moves.Add(toIndex);
                        }

                        break;
                    }

                    moves.Add(toIndex);
                }
            }

            return moves;
        }

        private static List<int> GetKingMoves(int index) {
            List<int> moves = new();
            var offsets = Array.AsReadOnly(new[] { -9, -8, -7, -1, 1, 7, 8, 9 });

            foreach (int offset in offsets) {
                if (_distanceToEdge[index][offset] < 1) {
                    continue;
                }

                int toIndex = index + offset;
                if (_game.board[toIndex] == null || _game.board[toIndex].Value.color != _game.colorToMove) {
                    moves.Add(toIndex);
                }
            }

            if (_game.colorToMove == PieceColor.White) {
                if (_game.castlingRights.HasFlag(CastlingRights.WhiteKingSide) && CanCastleKingSide()) {
                    moves.Add(index + 2);
                }

                if (_game.castlingRights.HasFlag(CastlingRights.WhiteQueenSide) && CanCastleQueenSide()) {
                    moves.Add(index - 2);
                }
            }
            else {
                if (_game.castlingRights.HasFlag(CastlingRights.BlackKingSide) && CanCastleKingSide()) {
                    moves.Add(index + 2);
                }

                if (_game.castlingRights.HasFlag(CastlingRights.BlackQueenSide) && CanCastleQueenSide()) {
                    moves.Add(index - 2);
                }
            }

            return moves;

            bool CanCastleKingSide() {
                return _game.board[index + 1] == null && _game.board[index + 2] == null;
            }

            bool CanCastleQueenSide() {
                return _game.board[index - 1] == null && _game.board[index - 2] == null &&
                       _game.board[index - 3] == null;
            }
        }

        public static void MovePiece(Piece piece, int to) {
            var move = new Move(piece, to);

            _game.prevMove = move;
            _game.board[piece.index] = null;
            if (_game.board[to] != null) {
                _game.pieces.Remove(_game.board[to].Value);
            }

            _game.pieces.Remove(piece);
            piece.index = to;
            _game.board[to] = piece;
            _game.pieces.Add(piece);

            if (MoveWasCastle()) {
                CastleRookMove(move.piece.index);
            }
            else if (MoveWasEnPassant()) {
                EnPassant(move.piece.index);
            }
            else if (MoveWasPromotion()) {
                // because PromotePawn() does not modify the argument pawn, but instead directly changes the type
                // of the pawn on the board, we need to call it after the move is made.
                PopupManager.ShowPawnPromotionPopup(piece);
                return; // PromotePawn() will conclude the move
            }

            _game.IncrementTurn();
            OnMoveEnd();
            return;

            void CastleRookMove(int kingIndex) {
                int rookTo = kingIndex + (to - kingIndex) / 2;
                int rookPos = CastleTargetRookPos(kingIndex, to);
                Piece rook = _game.board[rookPos]!.Value;
                PieceGUI rookGUI = Board.GetPieceGUI(rookPos)!;

                _game.pieces.Remove(rook);
                rook.index = rookTo;
                _game.board[rookTo] = rook;
                _game.pieces.Add(rook);

                rookGUI.piece = rook;
                rookGUI.transform.parent = Board.GetSquare(rookTo).transform;
                rookGUI.transform.position = rookGUI.transform.parent.position;
            }

            void EnPassant(int pawnIndex) {
                int captureIndex = to - pawnIndex > 0 ? to - 8 : to + 8;

                _game.pieces.Remove(_game.board[captureIndex]!.Value);
                _game.board[captureIndex] = null;
                Destroy(Board.GetPieceGUI(captureIndex).gameObject);
            }
        }
        
        public static void PromotePawn(Piece pawn, PieceType type) {
            Assert.AreNotEqual(_game.board, _game.history.Last().board,
                "Move is already finished; PromotePawn() should be called to conclude the move.");
            if (pawn.type != PieceType.Pawn) {
                throw new ArgumentException("Piece must be a pawn");
            }

            if (pawn.index is > 7 and < 56) {
                throw new ArgumentException("Pawn must be on the last rank");
            }

            Piece promoted = new(type, pawn.color, pawn.index);
            _game.board[pawn.index] = promoted;
            _game.pieces.Remove(pawn);
            _game.pieces.Add(promoted);
            
            _game.IncrementTurn();
            OnMoveEnd();
        }
        
        public static void UndoMove(bool fullmove = false) {
            if (_game.history.Count == 0) {
                throw new InvalidOperationException("UndoMove() called before any moves were made");
            }
            
            if (fullmove && _game.colorToMove == _game.playerColor) {
                _game.historyIndex -= 2;
            }
            else {
                _game.historyIndex -= 1;
            }
            
            _game.ApplyState(_game.history[_game.historyIndex]);
        }
        
        public static void RedoMove() {
            if (_game.historyIndex == _game.history.Count - 1) {
                throw new InvalidOperationException("There are no more moves to redo");
            }

            _game.historyIndex += 1;
            _game.ApplyState(_game.history[_game.historyIndex]);
        }

        private static int CastleTargetRookPos(int kingIndex, int toIndex) {
            return toIndex > kingIndex ? kingIndex + 3 : kingIndex - 4;
        }

        public static bool MoveWasCapture() {
            if (_game.prevMove == null) {
                throw new InvalidOperationException("MoveWasCapture() called before any moves were made");
            }
            
            Move move = _game.prevMove.Value;
            return _game.history[_game.historyIndex - 1].board[move.to] != null;
        }

        private static bool MoveWasCastle() {
            if (_game.prevMove == null) {
                throw new InvalidOperationException("MoveWasCastle() called before any moves were made");
            }
            
            Move move = _game.prevMove.Value;
            return move.piece.type == PieceType.King && Math.Abs(move.piece.index - move.to) == 2;
        }

        private static bool MoveWasEnPassant() {
            if (_game.prevMove == null) {
                throw new InvalidOperationException("MoveWasEnPassant() called before any moves were made");
            }
            
            Move move = _game.prevMove.Value;
            return move.piece.type == PieceType.Pawn && move.to == _game.enPassantIndex;
        }

        public static bool MoveWasPromotion() {
            if (_game.prevMove == null) {
                throw new InvalidOperationException("MoveWasPromotion() called before any moves were made");
            }
            
            Move move = _game.prevMove.Value;
            return move.piece.type == PieceType.Pawn && (move.to is < 8 or > 55);
        }

        private static void OnMoveEnd() {
            MoveEnd?.Invoke(typeof(Moves), EventArgs.Empty);
        }
    }
}