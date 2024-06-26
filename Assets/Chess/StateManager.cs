using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Chess {
    public record GameState {
        // game data
        public Piece?[] board { get; } = new Piece?[64];
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

        public GameState(Game game) {
            // game data
            Array.Copy(game._board, board, 64);
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
        private readonly List<GameState> _undoList = new();
        private readonly List<GameState> _redoList = new();
        public static StateManager instance => _instance ??= new StateManager();
        public GameState last => _undoList.Count != 0 ? _undoList[^1] : null;
        public ReadOnlyCollection<GameState> allStates => _undoList.AsReadOnly();
        private static readonly Game _game = Game.instance;

        private StateManager() {}
        
        public void Reset() {
            _undoList.Clear();
            _redoList.Clear();
        }

        public void RecordState() {
            _undoList.Add(new GameState(_game));
            _redoList.Clear();
        }
        
        public void ApplyState(GameState state) {
            _game._board = state.board;
            _game.colorToMove = state.colorToMove;
            _game.castlingRights = state.castlingRights;
            _game.enPassantIndex = state.enPassantIndex;
            _game.halfmoveClock = state.halfmoveClock;
            _game.fullmoveNumber = state.fullmoveNumber;
            _game.prevMove = state.prevMove;
            MoveGenerator.legalMoves = state.legalMoves;
            MoveGenerator.attackedSquaresByPiece = state.attackedSquaresByPiece;
            MoveGenerator.kingIndexes = state.kingIndexes;
        }
        
        public void Undo(bool fullmove = false) {
            if (_undoList.Count == 0) {
                throw new InvalidOperationException("Undo() called before any moves were made");
            }

            _redoList.Add(new GameState(_game));
            ApplyState(_undoList[^1]);
            _undoList.RemoveAt(_undoList.Count - 1);
            
            if (fullmove && _game.colorToMove == _game.playerColor) {
                Undo();
            }
        }
        
        public void Redo() {
            if (_redoList.Count == 0) {
                throw new InvalidOperationException("Redo() called before any moves were undone");
            }

            _undoList.Add(new GameState(_game));
            ApplyState(_redoList[^1]);
            _redoList.RemoveAt(_redoList.Count - 1);
        }
    }
}