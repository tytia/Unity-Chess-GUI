using TMPro;
using UnityEngine;
using Utility;

namespace GUI.GameWindow {
    public class ButtonLoadFEN : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _fenInput;
        [SerializeField] private GameObject _errorText;
        
        public void LoadFEN() {
            print("Validating FEN");
            if (!Notation.IsValidFEN(_fenInput.text)) {
                _errorText.SetActive(true);
                return;
            }
            
            _errorText.SetActive(false);
            print("Starting new game");
            GameManager.StartNewGame(_fenInput.text);
            _fenInput.text = "";
        }
    }
}
