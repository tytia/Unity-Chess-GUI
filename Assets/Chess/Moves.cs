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
        public int from => piece.index;

        public Move(Piece piece, int to) {
            this.piece = piece;
            this.to = to;
        }
    }

    public static class Moves {
        private static readonly Game _game = Game.GetInstance();

        private static readonly Dictionary<int, int>[] _distanceToEdge = new Dictionary<int, int>[64];

        // we can use the index as a key because there can only be one piece on a square at a time
        private static readonly Dictionary<int, HashSet<int>> _legalMoves = new();
        private static readonly HashSet<int> _attackedSquares = new();
        private static readonly int[] _kingIndexes = new int[2];
        private static readonly Dictionary<int, HashSet<int>>[] _kingRays = new Dictionary<int, HashSet<int>>[2];

        private static readonly Dictionary<PieceType, int[]> _slidingPieceDirections = new() {
            { PieceType.Bishop, new[] { -9, -7, 7, 9 } },
            { PieceType.Rook, new[] { -8, -1, 1, 8 } },
            { PieceType.Queen, new[] { -9, -8, -7, -1, 1, 7, 8, 9 } }
        };

        public static List<Piece> checkedBy { get; } = new(2);
        public static event EventHandler MoveEnd;

        static Moves() {
            InitDistanceToEdge();
            InitKingData();
            UpdateMoveData();
        }

        public static void RefreshData() {
            InitKingData();
            UpdateMoveData();
        }

        private static void OnMoveEnd() {
            _game.IncrementTurn();
            UpdateKingData();
            UpdateMoveData();
            MoveEnd?.Invoke(typeof(Moves), EventArgs.Empty);
        }

        public static HashSet<int> GetLegalSquares(this Piece piece) {
            return piece.color == _game.colorToMove ? _legalMoves[piece.index] : new HashSet<int>();
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

        private static void InitKingData() {
            _kingRays[0] = new Dictionary<int, HashSet<int>>();
            _kingRays[1] = new Dictionary<int, HashSet<int>>();
            foreach (Piece piece in _game.pieces) {
                if (piece.type == PieceType.King) {
                    _kingIndexes[(int)piece.color] = piece.index;
                    UpdateKingRays(piece);
                }
            }
        }

        private static void UpdateKingData() {
            if (_game.prevMove == null) {
                throw new InvalidOperationException("UpdateKingData() called before any moves were made");
            }

            Move prevMove = _game.prevMove.Value;
            if (prevMove.piece.type == PieceType.King) {
                _kingIndexes[(int)prevMove.piece.color] = prevMove.to;
                UpdateKingRays(prevMove.piece);
            }
        }

        private static void UpdateKingRays(Piece king) {
            var directions = Array.AsReadOnly(new[] { -9, -8, -7, -1, 1, 7, 8, 9 });

            foreach (int dir in directions) {
                _kingRays[(int)king.color][dir] = new HashSet<int>();
                int square = king.index;
                for (var _ = 0; _ < _distanceToEdge[king.index][dir]; _++) {
                    square += dir;
                    _kingRays[(int)king.color][dir].Add(square);
                }
            }
        }

        private static void UpdateMoveData() {
            _legalMoves.Clear();
            _attackedSquares.Clear();
            checkedBy.Clear();

            // we need to check for checks before, in order to generate correct legal moves
            foreach (Piece piece in _game.pieces) {
                if (piece.color != _game.colorToMove) {
                    HashSet<int> moves = GetMoves(piece, getProtects: true);
                    if (moves.Contains(_kingIndexes[(int)_game.colorToMove])) {
                        checkedBy.Add(piece);
                    }

                    _attackedSquares.UnionWith(moves);
                }
            }

            foreach (Piece piece in _game.pieces) {
                if (piece.color == _game.colorToMove) {
                    _legalMoves[piece.index] = GetMoves(piece);
                }
            }
        }

        private static HashSet<int> GetMoves(Piece piece, bool getProtects = false) {
            return piece.type switch {
                PieceType.Pawn => GetPawnMoves(piece.index, piece.color, getProtects),
                PieceType.Knight => GetKnightMoves(piece.index, getProtects),
                PieceType.Bishop or PieceType.Rook or PieceType.Queen => GetSlidingMoves(piece, getProtects),
                PieceType.King => GetKingMoves(piece.index, getProtects),
                _ => throw new ArgumentOutOfRangeException(nameof(piece.type), "Invalid piece type")
            };
        }

        private static int GetRayDirFromKing(int index) {
            foreach ((int dir, HashSet<int> ray) in _kingRays[(int)_game.colorToMove]) {
                if (ray.Contains(index)) {
                    return dir;
                }
            }

            return 0; // not a ray direction
        }

        private static bool PieceIsPinned(int index) {
            int dir = GetRayDirFromKing(index);
            if (dir != 0) {
                int square = index;
                for (var _ = 0; _ < _distanceToEdge[index][dir]; _++) {
                    square += dir;
                    if (_game.board[square] != null) {
                        Piece piece = _game.board[square]!.Value;
                        if (piece.color == _game.colorToMove) {
                            // friendly piece blocking the ray
                            break;
                        }

                        if (_slidingPieceDirections.ContainsKey(piece.type) &&
                            _slidingPieceDirections[piece.type].Contains(dir)) {
                            // enemy sliding piece pinning the piece
                            return true;
                        }

                        break;
                    }
                }
            }

            return false;
        }

        private static bool MoveIsCheckLegal(int from, int to) {
            if (checkedBy.Count > 2) {
                throw new InvalidOperationException("There are more than 2 pieces checking the king");
            }

            if (checkedBy.Count == 1) {
                // if piece is pinned, return false
                if (PieceIsPinned(from)) {
                    return false;
                }

                // otherwise, piece can take the checking piece or block the check (if sliding piece)
                return to == checkedBy[0].index ||
                       (_slidingPieceDirections.ContainsKey(_game.board[checkedBy[0].index]!.Value.type) &&
                        _kingRays[(int)_game.colorToMove][GetRayDirFromKing(checkedBy[0].index)].Contains(to) &&
                        to > _kingIndexes[(int)_game.colorToMove] == to < checkedBy[0].index);
            }

            // if piece is pinned, it can take the checking piece or move along the king ray
            if (PieceIsPinned(from)) {
                return _kingRays[(int)_game.colorToMove][GetRayDirFromKing(from)].Contains(to);
            }
            
            // if move was en passant, we need to make sure that the taken pawn was not pinned
            if (_game.board[from]!.Value.type == PieceType.Pawn && to == _game.enPassantIndex) {
                Piece temp = _game.board[from].Value;
                int targetPawnIndex = to - from > 0 ? _game.enPassantIndex.Value - 8 : _game.enPassantIndex.Value + 8;
                
                _game.board[from] = null;
                bool isLegal = !PieceIsPinned(targetPawnIndex);
                _game.board[from] = temp;
                
                return isLegal;
            }

            // otherwise, piece can move anywhere
            return true;
        }

        private static void AddMoveIfLegal(this HashSet<int> moves, int from, int to) {
            if (MoveIsCheckLegal(from, to)) {
                moves.Add(to);
            }
        }

        private static HashSet<int> GetPawnMoves(int index, PieceColor color, bool getProtects = false) {
            if (checkedBy.Count == 2) {
                return new HashSet<int>();
            }

            HashSet<int> moves = new();
            var offsets =
                Array.AsReadOnly(color == PieceColor.White
                    ? new[] { 8, 16, 7, 9 }
                    : new[] { -8, -16, -7, -9 });

            foreach (int offset in offsets) {
                if (Math.Abs(offset) != 16 && _distanceToEdge[index][offset] < 1) {
                    continue;
                }

                if (Math.Abs(offset) == 16 && !OnStartingRank(index)) {
                    continue;
                }

                int to = index + offset;
                bool existingPiece = _game.board[to] != null;
                switch (Math.Abs(offset)) {
                    case 8 when !existingPiece:
                        moves.AddMoveIfLegal(index, to);
                        break;
                    case 16 when !existingPiece && _game.board[to - (offset / 2)] == null:
                        moves.AddMoveIfLegal(index, to);
                        break;
                    case 7 or 9 when (existingPiece && _game.board[to].Value.color != color) ||
                                     to == _game.enPassantIndex || getProtects:
                        moves.AddMoveIfLegal(index, to);
                        break;
                }
            }

            return moves;

            bool OnStartingRank(int i) {
                return color == PieceColor.White ? i is >= 8 and <= 15 : i is >= 48 and <= 55;
            }
        }

        private static HashSet<int> GetKnightMoves(int index, bool getProtects = false) {
            if (checkedBy.Count == 2) {
                return new HashSet<int>();
            }

            HashSet<int> moves = new();
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

                int to = index + offset;
                if (_game.board[to] == null || _game.board[to].Value.color != _game.colorToMove || getProtects) {
                    moves.AddMoveIfLegal(index, to);
                }
            }

            return moves;
        }

        private static HashSet<int> GetSlidingMoves(Piece piece, bool getProtects = false) {
            if (checkedBy.Count == 2) {
                return new HashSet<int>();
            }

            if (!_slidingPieceDirections.ContainsKey(piece.type)) {
                throw new ArgumentOutOfRangeException(nameof(piece.type), "Not a sliding piece");
            }

            HashSet<int> moves = new();
            int[] directions = _slidingPieceDirections[piece.type];

            foreach (int dir in directions) {
                int to = piece.index;
                for (var _ = 0; _ < _distanceToEdge[piece.index][dir]; _++) {
                    to += dir;
                    if (_game.board[to] != null) {
                        // blocked by a piece
                        if (_game.board[to].Value.color != _game.colorToMove || getProtects) {
                            // can capture
                            moves.AddMoveIfLegal(piece.index, to);
                        }

                        break;
                    }

                    moves.AddMoveIfLegal(piece.index, to);
                }
            }

            return moves;
        }

        private static HashSet<int> GetKingMoves(int index, bool getProtects = false) {
            HashSet<int> moves = new();
            var offsets = Array.AsReadOnly(new[] { -9, -8, -7, -1, 1, 7, 8, 9 });

            foreach (int offset in offsets) {
                if (_distanceToEdge[index][offset] < 1) {
                    continue;
                }

                int to = index + offset;
                if (getProtects) {
                    moves.Add(to);
                }
                else if ((_game.board[to] == null || _game.board[to].Value.color != _game.colorToMove) &&
                         !_attackedSquares.Contains(to)) {
                    moves.Add(to);
                }
            }

            // castling
            if (!getProtects) {
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
            }

            return moves;

            bool CanCastleKingSide() {
                return _game.board[index + 1] == null && _game.board[index + 2] == null && checkedBy.Count == 0 &&
                       !_attackedSquares.Overlaps(new[] { index + 1, index + 2 });
            }

            bool CanCastleQueenSide() {
                return _game.board[index - 1] == null && _game.board[index - 2] == null && checkedBy.Count == 0 &&
                       _game.board[index - 3] == null &&
                       !_attackedSquares.Overlaps(new[] { index - 1, index - 2, index - 3 });
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
                CastleRookMove(move.from);
            }
            else if (MoveWasEnPassant()) {
                EnPassant(move.from);
            }
            else if (MoveWasPromotion()) {
                // because PromotePawn() does not modify the argument pawn, but instead directly changes the type
                // of the pawn on the board, we need to call it after the move is made.
                PopupManager.ShowPawnPromotionPopup(piece);
                return; // PromotePawn() will conclude the move
            }

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

        private static int CastleTargetRookPos(int kingIndex, int to) {
            return to > kingIndex ? kingIndex + 3 : kingIndex - 4;
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
            return move.piece.type == PieceType.King && Math.Abs(move.from - move.to) == 2;
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
    }
}