using System;
using System.Collections.Generic;
using System.Linq;

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

    public static class MoveGenerator {
        private static readonly Dictionary<int, int>[] _distanceToEdge = new Dictionary<int, int>[64];
        private static readonly Dictionary<PieceType, int[]> _slidingPieceDirections = new() {
            { PieceType.Bishop, new[] { -9, -7, 7, 9 } },
            { PieceType.Rook, new[] { -8, -1, 1, 8 } },
            { PieceType.Queen, new[] { -9, -8, -7, -1, 1, 7, 8, 9 } }
        };

        // we can use the index as a key because there can only be one piece on a square at a time
        private static Dictionary<int, HashSet<int>> _legalMoves = new();
        private static HashSet<int> _attackedSquares = new();
        private static int[] _kingIndexes = new int[2];
        private static Dictionary<int, HashSet<int>>[] _kingRays = new Dictionary<int, HashSet<int>>[2];
        private static List<Piece> _checkedBy = new(2);

        private static Game game => Game.instance;
        // the setters are for StateManager only
        internal static Dictionary<int, HashSet<int>> legalMoves {
            get => _legalMoves;
            set => _legalMoves = value;
        }

        internal static HashSet<int> attackedSquares {
            get => _attackedSquares;
            set => _attackedSquares = value;
        }

        internal static int[] kingIndexes {
            get => _kingIndexes;
            set => _kingIndexes = value;
        }

        internal static Dictionary<int, HashSet<int>>[] kingRays {
            get => _kingRays;
            set => _kingRays = value;
        }

        public static List<Piece> checkedBy {
            get => new(_checkedBy);
            internal set => _checkedBy = value;
        }

        public static HashSet<int> GetLegalSquares(this Piece piece) {
            return piece.color == game.colorToMove
                ? new HashSet<int>(_legalMoves[piece.index])
                : new HashSet<int>();
        }

        private static void AddMoveIfLegal(this HashSet<int> moves, int from, int to) {
            if (MoveIsCheckLegal(from, to)) {
                moves.Add(to);
            }
        }

        static MoveGenerator() {
            InitDistanceToEdge();
            InitKingData();
            UpdateMoveData();
        }

        internal static void UpdateData() {
            UpdateKingData();
            UpdateMoveData();
        }

        public static void RefreshData() {
            InitKingData();
            UpdateMoveData();
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
            foreach (Piece piece in game.pieces) {
                if (piece.type == PieceType.King) {
                    _kingIndexes[(int)piece.color] = piece.index;
                    UpdateKingRays(piece);
                }
            }
        }

        private static void UpdateKingData() {
            if (game.prevMove == null) {
                throw new InvalidOperationException("UpdateKingData() called before any moves were made");
            }

            Move prevMove = game.prevMove.Value;
            if (prevMove.piece.type == PieceType.King) {
                _kingIndexes[(int)prevMove.piece.color] = prevMove.to;
                UpdateKingRays(game.board[prevMove.to]!.Value);
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
            _checkedBy.Clear();

            // we need to check for checks before, in order to generate correct legal moves
            foreach (Piece piece in game.pieces) {
                if (piece.color != game.colorToMove) {
                    HashSet<int> moves = GetMoves(piece, getProtects: true);
                    if (moves.Contains(_kingIndexes[(int)game.colorToMove])) {
                        _checkedBy.Add(piece);
                    }

                    _attackedSquares.UnionWith(moves);
                }
            }

            foreach (Piece piece in game.pieces) {
                if (piece.color == game.colorToMove) {
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
            foreach ((int dir, HashSet<int> ray) in _kingRays[(int)game.colorToMove]) {
                if (ray.Contains(index)) {
                    return dir;
                }
            }

            return 0; // not a ray direction
        }

        private static bool PieceIsPinned(int index) {
            int dir = GetRayDirFromKing(index);
            if (dir != 0) {
                int square = _kingIndexes[(int)game.colorToMove] + dir;
                
                // check if there is any other piece between the king and the piece we are testing
                while (square != index) {
                    // if there is a piece between ourselves and the king, then we are not pinned
                    try {
                        if (game.board[square] != null) {
                            return false;
                        }
                    }
                    catch (IndexOutOfRangeException) {
                        throw new InvalidOperationException($"out of range index: {square} from {_kingIndexes[(int)game.colorToMove]} to {index} with dir {dir}");
                    }
                    
                    square += dir;
                }
                
                for (var _ = 0; _ < _distanceToEdge[index][dir]; _++) {
                    square += dir;
                    if (game.board[square] != null) {
                        Piece piece = game.board[square]!.Value;
                        if (piece.color == game.colorToMove) {
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
            if (_checkedBy.Count > 2) {
                throw new InvalidOperationException("There are more than 2 pieces checking the king");
            }

            if (_checkedBy.Count == 1) {
                // if piece is pinned, return false
                if (PieceIsPinned(from)) {
                    return false;
                }

                // otherwise, piece can take the checking piece or block the check (if sliding piece)
                int rayDir = GetRayDirFromKing(_checkedBy[0].index);
                return to == _checkedBy[0].index ||
                       (_slidingPieceDirections.ContainsKey(_checkedBy[0].type) && rayDir != 0 &&
                        _kingRays[(int)game.colorToMove][rayDir].Contains(to) &&
                        (to > _kingIndexes[(int)game.colorToMove]) == (to < _checkedBy[0].index));
            }

            // if piece is pinned, it can take the checking piece or move along the king ray
            if (PieceIsPinned(from)) {
                return _kingRays[(int)game.colorToMove][GetRayDirFromKing(from)].Contains(to);
            }

            // if move was en passant, we need to make sure that the taken pawn was not pinned
            if (game.board[from]!.Value.type == PieceType.Pawn && to == game.enPassantIndex) {
                Piece temp = game.board[from].Value;
                int targetPawnIndex =
                    to - from > 0 ? game.enPassantIndex.Value - 8 : game.enPassantIndex.Value + 8;

                game.board[from] = null;
                bool isLegal = !PieceIsPinned(targetPawnIndex);
                game.board[from] = temp;

                return isLegal;
            }

            // otherwise, piece can move anywhere
            return true;
        }

        private static HashSet<int> GetPawnMoves(int index, PieceColor color, bool getProtects = false) {
            if (_checkedBy.Count == 2) {
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
                bool existingPiece = game.board[to] != null;
                switch (Math.Abs(offset)) {
                    case 8 when !existingPiece && !getProtects:
                        moves.AddMoveIfLegal(index, to);
                        break;
                    case 16 when !existingPiece && game.board[to - (offset / 2)] == null && !getProtects:
                        moves.AddMoveIfLegal(index, to);
                        break;
                    case 7 or 9 when getProtects:
                        moves.Add(to);
                        break;
                    case 7 or 9 when (existingPiece && game.board[to].Value.color != color) ||
                                     to == game.enPassantIndex:
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
            if (_checkedBy.Count == 2) {
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
                if (getProtects) {
                    moves.Add(to);
                }
                else if (game.board[to] == null || game.board[to].Value.color != game.colorToMove) {
                    moves.AddMoveIfLegal(index, to);
                }
            }

            return moves;
        }

        private static HashSet<int> GetSlidingMoves(Piece piece, bool getProtects = false) {
            if (_checkedBy.Count == 2) {
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
                    if (game.board[to] != null) {
                        // blocked by a piece
                        if (getProtects) {
                            moves.Add(to);
                        }
                        else if (game.board[to].Value.color != game.colorToMove) {
                            // can capture
                            moves.AddMoveIfLegal(piece.index, to);
                        }

                        break;
                    }

                    if (getProtects) {
                        moves.Add(to);
                    }
                    else {
                        moves.AddMoveIfLegal(piece.index, to);
                    }
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
                else if ((game.board[to] == null || game.board[to].Value.color != game.colorToMove) &&
                         !_attackedSquares.Contains(to)) {
                    moves.Add(to);
                }
            }

            // castling
            if (!getProtects) {
                if (game.colorToMove == PieceColor.White) {
                    if (game.castlingRights.HasFlag(CastlingRights.WhiteKingSide) && CanCastleKingSide()) {
                        moves.Add(index + 2);
                    }

                    if (game.castlingRights.HasFlag(CastlingRights.WhiteQueenSide) && CanCastleQueenSide()) {
                        moves.Add(index - 2);
                    }
                }
                else {
                    if (game.castlingRights.HasFlag(CastlingRights.BlackKingSide) && CanCastleKingSide()) {
                        moves.Add(index + 2);
                    }

                    if (game.castlingRights.HasFlag(CastlingRights.BlackQueenSide) && CanCastleQueenSide()) {
                        moves.Add(index - 2);
                    }
                }
            }

            return moves;

            bool CanCastleKingSide() {
                return game.board[index + 1] == null && game.board[index + 2] == null && _checkedBy.Count == 0 &&
                       !_attackedSquares.Overlaps(new[] { index + 1, index + 2 });
            }

            bool CanCastleQueenSide() {
                return game.board[index - 1] == null && game.board[index - 2] == null && _checkedBy.Count == 0 &&
                       game.board[index - 3] == null &&
                       !_attackedSquares.Overlaps(new[] { index - 1, index - 2});
            }
        }
        
        public static int CastleTargetRookPos(int kingIndex, int to) {
            return to > kingIndex ? kingIndex + 3 : kingIndex - 4;
        }
    }
}