using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Chess {
    public class PerftTest : MonoBehaviour {
        private static readonly Game _game = Game.instance;
        [SerializeField] private int _depth = 1;
        [SerializeField] private string _fen = Notation.StartingFEN;

        private void Update() {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P)) {
                Debug.Log("perft started");

                if (_fen == "") {
                    _game.StartNewGame();
                }
                else {
                    _game.StartNewGame(_fen);
                }
                
                Stopwatch stopwatch = new();
                
                stopwatch.Start();
                ulong result = Perft(_depth);
                stopwatch.Stop();
                
                Debug.Log($"perft({_depth}) = {result}, took {stopwatch.ElapsedMilliseconds} milliseconds");
            }
        }

        private ulong Perft(int depth) {
            // runs perft to a given depth, starting from the current position
            if (depth == 0) {
                return 1;
            }

            ulong totalNodes = 0;
            ulong nodes = 0;
            foreach (int from in MoveGenerator.legalMoves.Keys!.ToArray()) {
                foreach (int to in MoveGenerator.legalMoves[from]) {
                    if (depth == _depth) nodes = 0;
                    if (Moves.LastMoveWasPromotion()) {
                        var promotionTargets = new [] {PieceType.Queen, PieceType.Knight, PieceType.Bishop, PieceType.Rook};
                        foreach (var promotionTarget in promotionTargets) {
                            Moves.PromotePawn(from, promotionTarget);
                            var res = Perft(depth - 1);
                            nodes += res;
                            totalNodes += res;
                            _game.stateManager.Undo();
                            MovePieceNoGUI(from, to);
                        }
                    }
                    else {
                        var res = Perft(depth - 1);
                        nodes += res;
                        totalNodes += res;
                    }
                    _game.stateManager.Undo();
                    
                    if (depth == _depth) Debug.Log($"{(Notation.SquarePos)from}{(Notation.SquarePos)to}: {nodes}");
                }
            }
            
            return totalNodes;
        }
        
        private static void MovePieceNoGUI(int from, int to) {
            // optimized version of MovePiece for perft
            if (_game.board[from] == null) {
                throw new ArgumentException("No piece at index " + from);
            }
            _game.stateManager.RecordState();
            _game.prevMove = new Move(from, to);
            
            _game._board[to] = _game.board[from];
            _game._board[from] = null;

            if (Moves.LastMoveWasCastle()) {
                CastleRookMove(from);
            }
            else if (Moves.LastMoveWasEnPassant()) {
                EnPassant(from);
            }
            else if (Moves.LastMoveWasPromotion()) {
                return; // PromotePawn() will conclude the move
            }

            Moves.OnMoveEnd();
            return;

            void CastleRookMove(int kingIndex) {
                int rookTo = kingIndex + (to - kingIndex) / 2;
                int rookFrom = MoveGenerator.GetCastleTargetRookPos(kingIndex, to);

                _game._board[rookTo] = _game.board[rookFrom];
                _game._board[rookFrom] = null;
            }

            void EnPassant(int captorIndex) {
                int captureIndex = to - captorIndex > 0 ? to - 8 : to + 8;

                _game._board[captureIndex] = null;
            }
        }
    }
}
