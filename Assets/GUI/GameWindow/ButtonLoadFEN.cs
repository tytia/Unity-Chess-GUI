using TMPro;
using UnityEngine;
using Utility;

namespace GUI.GameWindow {
    public class ButtonLoadFEN : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _fenInput;
        [SerializeField] private GameObject _errorText;
        
        public void LoadFEN() {
            string fenText = _fenInput.text.Trim();
            if (!Notation.IsValidFEN(fenText)) {
                _errorText.SetActive(true);
                return;
            }
            
            _errorText.SetActive(false);
            GameManager.StartNewGame(fenText);
            _fenInput.text = "";
        }
    }
}
