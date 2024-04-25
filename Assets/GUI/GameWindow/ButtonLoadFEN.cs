using Chess;
using TMPro;
using UnityEngine;

namespace GUI.GameWindow {
    public class ButtonLoadFEN : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _fenInput;
        [SerializeField] private GameObject _errorText;
        [SerializeField] private GameManager _gameManager;

        public void LoadFEN() {
            string fenText = _fenInput.text.Trim();
            if (!Notation.IsValidFEN(fenText)) {
                _errorText.SetActive(true);
                return;
            }
            
            _gameManager.StartNewGame(fenText);
            _fenInput.text = "";
            _errorText.SetActive(false);
        }
    }
}
