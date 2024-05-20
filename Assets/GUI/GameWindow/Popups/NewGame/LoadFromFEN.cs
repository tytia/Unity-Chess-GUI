using Chess;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.GameWindow.Popups.NewGame {
    public class LoadFromFEN : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private TMP_InputField _fenInput;
        [SerializeField] private Button _startButton;

        private void Awake() {
            _startButton.interactable = false;
        }

        public void StartNewGame() {
            string fenText = _fenInput.text.Trim();
            _gameManager.StartNewGame(fenText);
            _fenInput.text = "";
        }

        public void OnFENChanged() {
            _startButton.interactable = Notation.IsValidFEN(_fenInput.text.Trim());
        }
    }
}
