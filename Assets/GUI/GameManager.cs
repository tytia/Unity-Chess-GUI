using System;
using Chess;
using GUI.GameWindow;
using UnityEngine;
using static Chess.Notation;

namespace GUI {
    /// <summary>
    /// In charge of resetting the board and starting new games.
    /// </summary>
    public class GameManager : MonoBehaviour {
        [SerializeField] private PieceManager _pieceManager;
        [SerializeField] private Board _board;
        private static readonly Game _game = Game.instance;
        public static Engine engine { get; private set; }
        
        public event EventHandler GameStart;

        private void Awake() {
            engine = new Engine(PlayerPrefs.GetString("engine_path"));
        }

        private void OnApplicationQuit() {
            engine.KillProcess();
        }

        public void StartNewGame(PieceColor playerColor) {
            _game.StartNewGame(playerColor);
            _pieceManager.RemovePieces();
            HighlightManager.ClearHighlights();

            if (_board.orientation != playerColor) {
                _board.Flip();
            }
            
            _pieceManager.InitPieces(playerColor);
            GameStart?.Invoke(this, EventArgs.Empty);
        }
        
        public void StartNewGame(string fen) {
            if (fen.Trim() == "") fen = StartingFEN;
            _game.StartNewGame(fen);
            _pieceManager.RemovePieces();
            _pieceManager.InitPieces(_game.playerColor);
            HighlightManager.ClearHighlights();
            
            if (_board.orientation != _game.playerColor) {
                _board.Flip();
            }
            
            GameStart?.Invoke(this, EventArgs.Empty);
        }
    }
}