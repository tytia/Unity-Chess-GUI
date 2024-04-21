using System;
using System.Collections.Generic;

namespace Chess {
    public readonly struct Move {
        public int to { get; }
        public int from { get; }
        public Piece piece { get; }

        public Move(int from, int to) {
            this.from = from;
            this.to = to;
            piece = Game.instance.board[from]!.Value;
        }
    }

    internal static class MoveGenerator {
        private static readonly Dictionary<int, int>[] _distanceToEdge = new Dictionary<int, int>[64];
        private static readonly int[] _rayDirection = new int[64 * 64];

        private static readonly Dictionary<PieceType, int[]> _slidingPieceDirections = new() {
            { PieceType.Bishop, new[] { -9, -7, 7, 9 } },
            { PieceType.Rook, new[] { -8, -1, 1, 8 } },
            { PieceType.Queen, new[] { -9, -8, -7, -1, 1, 7, 8, 9 } }
        };

        // we can use the index as a key because there can only be one piece on a square at a time
        internal static Dictionary<int, HashSet<int>> legalMoves { get; set; } = new();
        internal static Dictionary<int, HashSet<int>> attackedSquaresByPiece { get; set; } = new();
        private static readonly HashSet<int> _attackedSquares = new();
        internal static int[] kingIndexes { get; set; } = new int[2];

        private static readonly Game _game = Game.instance;

        static MoveGenerator() {
            InitDistanceToEdge();
            InitRayDirections();
            AssignKingIndexes();
            UpdateData();
        }

        public static void Reset() {
            AssignKingIndexes();
            UpdateData();
        }
        
        public static HashSet<int> GetLegalSquares(int pieceIndex) {
            return legalMoves.TryGetValue(pieceIndex, out var move) ? move : new HashSet<int>();
        }

        private static void AddMoveIfLegal(this HashSet<int> moves, int from, int to, bool kingMove = false) {
            if (IsMoveLegal(from, to, kingMove)) {
                moves.Add(to);
            }
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

        private static void InitRayDirections() {
            for (var i = 0; i < 64; i++) {
                foreach (int dir in _distanceToEdge[i].Keys) {
                    int to = i;
                    for (var _ = 0; _ < _distanceToEdge[i][dir]; _++) {
                        to += dir;
                        _rayDirection[i | (to << 6)] = dir;
                    }
                }
            }
        }

        private static void AssignKingIndexes() {
            for (var i = 0; i < _game.board.Count; i++) {
                var piece = _game.board[i];
                if (piece is { type: PieceType.King }) {
                    kingIndexes[(int)piece.Value.color] = i;
                }
            }
        }

        internal static void UpdateData() {
            legalMoves.Clear();
            attackedSquaresByPiece.Clear();
            _attackedSquares.Clear();

            // update king index
            if (_game.prevMove is { piece: { type: PieceType.King } }) {
                kingIndexes[(int)_game.prevMove.Value.piece.color] = _game.prevMove.Value.to;
            }

            // we need to calculate the attacked squares first in order to generate correct legal moves
            for (var i = 0; i < _game.board.Count; i++) {
                var piece = _game.board[i];
                if (piece is not null && piece.Value.color != _game.colorToMove) {
                    HashSet<int> moves = GetMoves(i, getProtects: true);
                    attackedSquaresByPiece[i] = moves;
                    _attackedSquares.UnionWith(moves);
                }
            }

            // update check status
            _game.inCheck = _attackedSquares.Contains(kingIndexes[(int)_game.colorToMove]);

            // generate legal moves
            for (var i = 0; i < _game.board.Count; i++) {
                var piece = _game.board[i];
                if (piece is not null && piece.Value.color == _game.colorToMove) {
                    var moves = GetMoves(i);
                    if (moves.Count != 0) legalMoves[i] = moves;
                }
            }
        }

        private static bool IsMoveLegal(int from, int to, bool kingMove) {
            var result = true;
            int kingIndex = kingMove ? to : kingIndexes[(int)_game.colorToMove];
            List<(int key, HashSet<int> value)> modifiedEntries = new();

            // try move in order to update attacked squares
            Piece fromPiece = _game.board[from]!.Value;
            Piece? toPiece = _game.board[to];
            int capturedPawnIndex = -1; // for possible en passant capture
            _game._board[from] = null;
            _game._board[to] = fromPiece;

            // if an enemy piece is captured, temporarily remove it's attacking squares
            if (toPiece is not null) {
                modifiedEntries.Add((to, attackedSquaresByPiece[to]));
                attackedSquaresByPiece.Remove(to);
            }
            else if (fromPiece.type == PieceType.Pawn && to == _game.enPassantIndex) {
                capturedPawnIndex = _game.colorToMove == PieceColor.White ? to - 8 : to + 8;
                _game._board[capturedPawnIndex] = null;
                modifiedEntries.Add((capturedPawnIndex, attackedSquaresByPiece[capturedPawnIndex]));
                attackedSquaresByPiece.Remove(capturedPawnIndex);
            }

            // update sliding piece attacked squares if move reveals or blocks a check
            foreach ((int pieceIndex, HashSet<int> attackedSquares) in attackedSquaresByPiece) {
                if (_game.board[pieceIndex] is not null &&
                    _slidingPieceDirections.ContainsKey(_game.board[pieceIndex].Value.type)) {
                    // does it reveal a check?
                    if (attackedSquares.Contains(from) || attackedSquares.Contains(capturedPawnIndex)) {
                        int attack = pieceIndex;
                        int direction = attackedSquares.Contains(from)
                            ? _rayDirection[pieceIndex | (from << 6)]
                            : _rayDirection[pieceIndex | (capturedPawnIndex << 6)];
                        for (var _ = 0; _ < _distanceToEdge[pieceIndex][direction]; _++) {
                            attack += direction;
                            if (_game.board[attack] is not null) {
                                if (attack == kingIndex) {
                                    result = false;
                                }

                                break;
                            }
                        }
                    }
                    // does it block a check?
                    if (attackedSquares.Contains(to)) {
                        int attack = to;
                        int direction = _rayDirection[pieceIndex | (to << 6)];
                        modifiedEntries.Add((pieceIndex, new HashSet<int>(attackedSquaresByPiece[pieceIndex])));
                        for (var _ = 0; _ < _distanceToEdge[to][direction]; _++) {
                            attack += direction;
                            attackedSquaresByPiece[pieceIndex].Remove(attack);
                        }
                    }

                    if (result == false) {
                        break;
                    }
                }
            }

            foreach (var attackedSquares in attackedSquaresByPiece.Values) {
                if (attackedSquares.Contains(kingIndex)) {
                    result = false;
                    break;
                }
            }

            // restore modified entries
            foreach ((int key, HashSet<int> value) in modifiedEntries) {
                attackedSquaresByPiece[key] = value;
            }

            // unmake move
            _game._board[from] = fromPiece;
            _game._board[to] = toPiece;

            if (capturedPawnIndex != -1) {
                _game._board[capturedPawnIndex] = new Piece(PieceType.Pawn,
                    _game.colorToMove == PieceColor.White ? PieceColor.Black : PieceColor.White);
            }

            return result;
        }

        private static HashSet<int> GetMoves(int pieceIndex, bool getProtects = false) {
            Piece piece = _game.board[pieceIndex]!.Value;
            return piece.type switch {
                PieceType.Pawn => GetPawnMoves(pieceIndex, piece.color, getProtects),
                PieceType.Knight => GetKnightMoves(pieceIndex, getProtects),
                PieceType.Bishop or PieceType.Rook or PieceType.Queen => GetSlidingMoves(pieceIndex, getProtects),
                PieceType.King => GetKingMoves(pieceIndex, getProtects),
                _ => throw new ArgumentOutOfRangeException(nameof(piece.type), "Invalid piece type")
            };
        }

        private static HashSet<int> GetPawnMoves(int index, PieceColor color, bool getProtects = false) {
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
                    case 8 when !existingPiece && !getProtects:
                        moves.AddMoveIfLegal(index, to);
                        break;
                    case 16 when !existingPiece && _game.board[to - (offset / 2)] == null && !getProtects:
                        moves.AddMoveIfLegal(index, to);
                        break;
                    case 7 or 9 when getProtects:
                        moves.Add(to);
                        break;
                    case 7 or 9 when (existingPiece && _game.board[to].Value.color != color) ||
                                     to == _game.enPassantIndex:
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
                else if (_game.board[to] == null || _game.board[to].Value.color != _game.colorToMove) {
                    moves.AddMoveIfLegal(index, to);
                }
            }

            return moves;
        }

        private static HashSet<int> GetSlidingMoves(int index, bool getProtects = false) {
            Piece piece = _game.board[index]!.Value;
            if (!_slidingPieceDirections.ContainsKey(piece.type)) {
                throw new ArgumentOutOfRangeException(nameof(piece.type), "Not a sliding piece");
            }

            HashSet<int> moves = new();
            int[] directions = _slidingPieceDirections[piece.type];

            foreach (int dir in directions) {
                int to = index;
                for (var _ = 0; _ < _distanceToEdge[index][dir]; _++) {
                    to += dir;
                    if (_game.board[to] != null) {
                        // blocked by a piece
                        if (getProtects) {
                            moves.Add(to);
                        }
                        else if (_game.board[to].Value.color != _game.colorToMove) {
                            // can capture
                            moves.AddMoveIfLegal(index, to);
                        }

                        break;
                    }

                    if (getProtects) {
                        moves.Add(to);
                    }
                    else {
                        moves.AddMoveIfLegal(index, to);
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
                else if (_game.board[to] == null || _game.board[to].Value.color != _game.colorToMove) {
                    moves.AddMoveIfLegal(index, to, kingMove: true);
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
                return _game.board[index + 1] == null && _game.board[index + 2] == null && !_game.inCheck &&
                       !_attackedSquares.Overlaps(new[] { index + 1, index + 2 });
            }

            bool CanCastleQueenSide() {
                return _game.board[index - 1] == null && _game.board[index - 2] == null && !_game.inCheck &&
                       _game.board[index - 3] == null &&
                       !_attackedSquares.Overlaps(new[] { index - 1, index - 2 });
            }
        }

        public static int CastleTargetRookPos(int kingIndex, int to) {
            return to > kingIndex ? kingIndex + 3 : kingIndex - 4;
        }
    }
}