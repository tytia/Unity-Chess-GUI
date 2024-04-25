using Chess;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.GameWindow.Popups {
    public class NewGamePopup : Popup {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Button _playAsBlack;
        [SerializeField] private Button _playAsWhite;

        private void Awake() {
            _playAsBlack.onClick.AddListener(delegate { StartNewGame(PieceColor.Black); });
            _playAsWhite.onClick.AddListener(delegate { StartNewGame(PieceColor.White); });
            Game.instance.GameEnd += (sender, args) => Show(true);
            Show(false);
        }
        
        private void StartNewGame(PieceColor playerColor) {
            _gameManager.StartNewGame(playerColor);
            Show(false);
        }
    }
}