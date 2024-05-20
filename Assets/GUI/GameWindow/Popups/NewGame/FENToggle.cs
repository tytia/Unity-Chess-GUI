using UnityEngine;
using UnityEngine.UI;

namespace GUI.GameWindow.Popups.NewGame {
    public class FENToggle : MonoBehaviour {
        [SerializeField] private GameObject _loadFromFEN;
        [SerializeField] private GameObject _normalButtons;
        private Toggle _toggle;
        
        private void Awake() {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(OnToggle);
        }
        
        private void OnToggle(bool value) {
            _loadFromFEN.SetActive(value);
            _normalButtons.SetActive(!value);
            if (value) {
                transform.parent.GetComponent<RectTransform>().sizeDelta -= new Vector2(0, 25);
                GetComponent<RectTransform>().position -= new Vector3(0, 25);
            }
            else {
                transform.parent.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 25);
                GetComponent<RectTransform>().position += new Vector3(0, 25);
            }
        }
    }
}