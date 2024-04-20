using Chess;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.GameWindow.Popups {
    public class NewGamePopup : MonoBehaviour, IPopup {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Button _playAsBlack;
        [SerializeField] private Button _playAsWhite;

        private void Awake() {
            PopupManager.newGamePopup = this;
            _playAsBlack.onClick.AddListener(delegate { _gameManager.StartNewGame(PieceColor.Black); });
            _playAsWhite.onClick.AddListener(delegate { _gameManager.StartNewGame(PieceColor.White); });
        }
        
        // no need to Show(false) in Start() because it's already done in GameEndPopup.cs

        public void Show(bool value) {
            gameObject.SetActive(value);
            foreach (Transform child in transform) {
                child.gameObject.SetActive(value);
            }
        }
    }
}