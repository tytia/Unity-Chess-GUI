using System;
using System.Collections.Generic;

namespace Chess {
    public record State {
        // game data
        public Piece?[] board { get; } = new Piece?[64];
        public List<Piece> pieces { get; }
        public PieceColor playerColor { get; }
        public PieceColor colorToMove { get; }
        public CastlingRights castlingRights { get; }
        public int? enPassantIndex { get; }
        public int halfmoveClock { get; }
        public int fullmoveNumber { get; }
        public Move? prevMove { get; }
        
        // move generator data
        public Dictionary<int, HashSet<int>> legalMoves { get; }
        public HashSet<int> attackedSquares { get; }
        public int[] kingIndexes { get; }
        public Dictionary<int, HashSet<int>>[] kingRays { get; }
        public List<Piece> checkedBy { get; }

        public State(Game game) {
            Array.Copy(game.board, board, 64);
            pieces = new List<Piece>(game.pieces);
            playerColor = game.playerColor;
            colorToMove = game.colorToMove;
            castlingRights = game.castlingRights;
            enPassantIndex = game.enPassantIndex;
            halfmoveClock = game.halfmoveClock;
            fullmoveNumber = game.fullmoveNumber;
            prevMove = game.prevMove;
            
            legalMoves = new Dictionary<int, HashSet<int>>();
            foreach (KeyValuePair<int, HashSet<int>> kvp in MoveGenerator.legalMoves) {
                legalMoves[kvp.Key] = new HashSet<int>(kvp.Value);
            }
            
            attackedSquares = new HashSet<int>(MoveGenerator.attackedSquares);
            kingIndexes = (int[])MoveGenerator.kingIndexes.Clone();
            
            kingRays = new Dictionary<int, HashSet<int>>[2];
            for (int i = 0; i < 2; i++) {
                kingRays[i] = new Dictionary<int, HashSet<int>>();
                foreach (KeyValuePair<int, HashSet<int>> kvp in MoveGenerator.kingRays[i]) {
                    kingRays[i][kvp.Key] = new HashSet<int>(kvp.Value);
                }
            }
            
            checkedBy = MoveGenerator.checkedBy;
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
            game.pieces = new List<Piece>(state.pieces);
            game.playerColor = state.playerColor;
            game.colorToMove = state.colorToMove;
            game.castlingRights = state.castlingRights;
            game.enPassantIndex = state.enPassantIndex;
            game.halfmoveClock = state.halfmoveClock;
            game.fullmoveNumber = state.fullmoveNumber;
            game.prevMove = state.prevMove;
            MoveGenerator.legalMoves = state.legalMoves;
            MoveGenerator.attackedSquares = state.attackedSquares;
            MoveGenerator.kingIndexes = state.kingIndexes;
            MoveGenerator.kingRays = state.kingRays;
            MoveGenerator.checkedBy = state.checkedBy;
        }
        
        public void Undo(bool fullmove = false) {
            if (_undoStack.Count == 0) {
                throw new InvalidOperationException("Undo() called before any moves were made");
            }

            if (_redoStack.Count == 0) {
                // this class records up to the previous state, so if we want to be able to redo up to the present,
                // we should first record the present state into the redo stack
                _redoStack.Push(new State(game));
            }
            
            State prevState = _undoStack.Pop();
            _redoStack.Push(prevState);
            ApplyState(prevState);
            
            if (fullmove && game.colorToMove == game.playerColor) {
                Undo();
            }
        }
        
        public void Redo() {
            if (_redoStack.Count == 0) {
                throw new InvalidOperationException("Redo() called before any moves were undone");
            }

            State nextState = _redoStack.Pop();
            _undoStack.Push(nextState);
            ApplyState(nextState);
        }
    }
}