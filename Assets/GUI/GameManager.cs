using Chess;
using GUI.GameWindow;
using GUI.GameWindow.Popups;
using UnityEngine;
using static Chess.Notation;

namespace GUI {
    /// <summary>
    /// In charge of resetting the board and starting new games.
    /// </summary>
    public class GameManager : MonoBehaviour {
        [SerializeField] private PieceManager _pieceManager;
        [SerializeField] private Board _board;
        [SerializeField] private GameEndPopup _gameEndPopup;
        private static readonly Game _game = Game.instance;

        public void StartNewGame(PieceColor playerColor) {
            _game.StartNewGame(playerColor);
            _pieceManager.RemovePieces();
            HighlightManager.ClearHighlights();
            _gameEndPopup.Show(false);

            if (_board.orientation != playerColor) {
                _board.Flip();
            }
            
            _pieceManager.InitPieces(playerColor);
        }
        
        public void StartNewGame(string fen) {
            if (fen.Trim() == "") fen = StartingFEN;
            _game.StartNewGame(fen);
            _pieceManager.RemovePieces();
            _pieceManager.InitPieces(_game.playerColor);
            HighlightManager.ClearHighlights();
            _gameEndPopup.Show(false);
            
            if (_board.orientation != _game.playerColor) {
                _board.Flip();
            }
        }
    }
}