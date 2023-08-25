/*
 * This class is responsible for generating and making legal moves.
 */

using System;
using System.Collections.Generic;
using GUI.GameWindow;
using static UnityEngine.Object;

namespace Chess {
    public struct Move {
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
                return _game.board[index - 1] == null && _game.board[index - 2] == null && _game.board[index - 3] == null;
            }
        }
        
        public static void MovePiece(ref Piece piece, int to) {
            _game.moveHistory.Add(new Move(piece, to)); // move needs to be added before piece's index gets changed
            _game.board[piece.index] = null;

            if (MoveIsCastle(piece, to)) {
                Castle(piece.index);
            }
            else if (MoveIsEnPassant(piece, to)) {
                EnPassant(piece.index);
            }
            else if (_game.board[to] != null) {
                _game.pieces.Remove(_game.board[to].Value);
            }

            _game.pieces.Remove(piece);
            piece.index = to;
            _game.board[to] = piece;
            _game.pieces.Add(piece);

            _game.IncrementTurn();
            return;

            void Castle(int kingIndex) {
                int rookTo = kingIndex + (to - kingIndex) / 2;
                int rookPos = CastleTargetRookPos(kingIndex, to);
                Piece rook = _game.board[rookPos]!.Value;
                PieceGUI rookGUI = Board.GetPieceGUI(rookPos)!;

                _game.pieces.Remove(rook);
                rook.index = rookTo;
                _game.board[rookTo] = rook;
                _game.pieces.Add(rook);
                
                rookGUI.GetComponent<MoveHandler>().piece = rook;
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
        
        private static bool MoveIsCastle(Piece piece, int toIndex) {
            return piece.type == PieceType.King && Math.Abs(piece.index - toIndex) == 2;
        }
        
        private static int CastleTargetRookPos(int kingIndex, int toIndex) {
            return toIndex > kingIndex ? kingIndex + 3 : kingIndex - 4;
        }
        
        private static bool MoveIsEnPassant(Piece piece, int toIndex) {
            return piece.type == PieceType.Pawn && toIndex == _game.enPassantIndex;
        }
    }
}