using System;
using System.Collections.Generic;

namespace Chess {
    public static class Moves {
        private static readonly Game _game = Game.GetInstance();
        private static readonly Dictionary<int, int>[] _distanceToEdge = new Dictionary<int, int>[64];

        static Moves() {
            for (var i = 0; i < 64; i++) {
                _distanceToEdge[i] = new Dictionary<int, int>(8) {
                    { -1, i % 8 }, // left
                    { 1, 7 - i % 8 }, // right
                    { -8, i / 8 }, // down
                    { 8, 7 - i / 8 }, // up
                    { 7, Math.Min(_distanceToEdge[i][8], _distanceToEdge[i][-1]) }, // up-left
                    { 9, Math.Min(_distanceToEdge[i][8], _distanceToEdge[i][1]) }, // up-right
                    { -9, Math.Min(_distanceToEdge[i][-8], _distanceToEdge[i][-1]) }, // down-left
                    { -7, Math.Min(_distanceToEdge[i][-8], _distanceToEdge[i][1]) } // down-right
                };
            }
        }

        public static List<int> GetLegalSquares(this Piece piece) {
            if (_game.playerColor != _game.colorToMove && !_game.analysisMode) {
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
                if (_distanceToEdge[index][offset] < 1) {
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
                    continue;
                }

                moves.Add(toIndex);
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
                if (_game.castlingRights.HasFlag(CastlingRights.WhiteKingSide) && CanCastleKingSide(index)) {
                    moves.Add(index + 2);
                }
                if (_game.castlingRights.HasFlag(CastlingRights.WhiteQueenSide) && CanCastleQueenSide(index)) {
                    moves.Add(index - 2);
                }
            }
            else {
                if (_game.castlingRights.HasFlag(CastlingRights.BlackKingSide) && CanCastleKingSide(index)) {
                    moves.Add(index + 2);
                }
                if (_game.castlingRights.HasFlag(CastlingRights.BlackQueenSide) && CanCastleQueenSide(index)) {
                    moves.Add(index - 2);
                }
            }

            return moves;

            bool CanCastleKingSide(int i) {
                return _game.board[i + 1] == null && _game.board[i + 2] == null;
            }
            
            bool CanCastleQueenSide(int i) {
                return _game.board[i - 1] == null && _game.board[i - 2] == null && _game.board[i - 3] == null;
            }
        }
    }
}