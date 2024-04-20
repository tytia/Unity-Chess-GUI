using Chess;
using GUI.GameWindow;
using GUI.GameWindow.Popups;
using UnityEngine;
using static Utility.Notation;

namespace GUI {
    /// <summary>
    /// In charge of resetting the board and starting new games.
    /// </summary>
    public class GameManager : MonoBehaviour {
        [SerializeField] private PieceManager _pieceManager;
        [SerializeField] private Board _board;
        private static Game game => Game.instance;

        public void StartNewGame(PieceColor playerColor) {
            game.StartNewGame(playerColor);
            _pieceManager.RemovePieces();
            HighlightManager.ClearHighlights();
            PopupManager.ShowGameEndPopup(false);

            if (_board.orientation != playerColor) {
                _board.Flip();
            }
            
            _pieceManager.InitPieces(playerColor);
        }
        
        public void StartNewGame(string fen) {
            if (fen.Trim() == "") fen = StartingFEN;
            game.StartNewGame(fen);
            _pieceManager.RemovePieces();
            _pieceManager.InitPieces(game.playerColor);
            HighlightManager.ClearHighlights();
            PopupManager.ShowGameEndPopup(false);
            
            if (_board.orientation != game.playerColor) {
                _board.Flip();
            }
        }
    }
}