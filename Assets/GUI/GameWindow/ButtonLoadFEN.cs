using Chess;
using TMPro;
using UnityEngine;
using Utility;

namespace GUI.GameWindow {
    public class ButtonLoadFEN : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _fenInput;
        [SerializeField] private GameObject _errorText;
        [SerializeField] private PieceManager _pieceManager;
        private Game _game;

        private void Awake() {
            _game = Game.GetInstance();
        }

        public void LoadFEN() {
            string fenText = _fenInput.text.Trim();
            if (!Notation.IsValidFEN(fenText)) {
                _errorText.SetActive(true);
                return;
            }
            
            StartNewGame(fenText);
            
            _fenInput.text = "";
            _errorText.SetActive(false);
        }
        
        private void StartNewGame(string fen) {
            _game.StartNewGame(fen);
            Moves.RefreshData();
            _pieceManager.RemovePieces();
            _pieceManager.InitPieces();
            HighlightManager.ClearHighlights();
        }
    }
}
