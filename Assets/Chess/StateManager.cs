using System;
using System.Collections.Generic;

namespace Chess {
    public record State {
        // game data
        public Piece?[] board { get; } = new Piece?[64];
        public HashSet<int> pieces { get; }
        public PieceColor colorToMove { get; }
        public CastlingRights castlingRights { get; }
        public int? enPassantIndex { get; }
        public int halfmoveClock { get; }
        public int fullmoveNumber { get; }
        public Move? prevMove { get; }
        
        // move generator data
        public Dictionary<int, HashSet<int>> legalMoves { get; }
        public Dictionary<int, HashSet<int>> attackedSquaresByPiece { get; }
        public int[] kingIndexes { get; }

        public State(Game game) {
            // game data
            Array.Copy(game.board, board, 64);
            pieces = new HashSet<int>(game.pieceIndexes);
            colorToMove = game.colorToMove;
            castlingRights = game.castlingRights;
            enPassantIndex = game.enPassantIndex;
            halfmoveClock = game.halfmoveClock;
            fullmoveNumber = game.fullmoveNumber;
            prevMove = game.prevMove;
            
            // move generator data
            legalMoves = new Dictionary<int, HashSet<int>>();
            foreach (KeyValuePair<int, HashSet<int>> kvp in MoveGenerator.legalMoves) {
                legalMoves[kvp.Key] = new HashSet<int>(kvp.Value);
            }
            
            attackedSquaresByPiece = new Dictionary<int, HashSet<int>>();
            foreach (KeyValuePair<int, HashSet<int>> kvp in MoveGenerator.attackedSquaresByPiece) {
                attackedSquaresByPiece[kvp.Key] = new HashSet<int>(kvp.Value);
            }
            
            kingIndexes = (int[])MoveGenerator.kingIndexes.Clone();
        }
    }

    public class StateManager {
        private static StateManager _instance;
        private readonly Stack<State> _undoStack = new();
        private readonly Stack<State> _redoStack = new();
        public static StateManager instance => _instance ??= new StateManager();
        public State last => _undoStack.TryPeek(out State result) ? result : null;
        private static Game game => Game.instance;

        private StateManager() {}
        
        public void Reset() {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public void RecordState() {
            _undoStack.Push(new State(game));
            _redoStack.Clear();
        }
        
        public void ApplyState(State state) {
            game.board = state.board;
            game.pieceIndexes = new HashSet<int>(state.pieces);
            game.colorToMove = state.colorToMove;
            game.castlingRights = state.castlingRights;
            game.enPassantIndex = state.enPassantIndex;
            game.halfmoveClock = state.halfmoveClock;
            game.fullmoveNumber = state.fullmoveNumber;
            game.prevMove = state.prevMove;
            MoveGenerator.legalMoves = state.legalMoves;
            MoveGenerator.attackedSquaresByPiece = state.attackedSquaresByPiece;
            MoveGenerator.kingIndexes = state.kingIndexes;
        }
        
        public void Undo(bool fullmove = false, bool discardRedo = false) {
            if (_undoStack.Count == 0) {
                throw new InvalidOperationException("Undo() called before any moves were made");
            }

            if (!discardRedo) {
                _redoStack.Push(new State(game));
            }
            ApplyState(_undoStack.Pop());
            
            if (fullmove && game.colorToMove == game.playerColor) {
                Undo();
            }
        }
        
        public void Redo() {
            if (_redoStack.Count == 0) {
                throw new InvalidOperationException("Redo() called before any moves were undone");
            }

            _undoStack.Push(new State(game));
            ApplyState(_redoStack.Pop());
        }
    }
}