using UnityEngine;

namespace GUI.GameWindow.Popups {
    public abstract class Popup : MonoBehaviour {
        public virtual void Show(bool value) {
            gameObject.SetActive(value);
            foreach (Transform child in transform) {
                child.gameObject.SetActive(value);
            }
        }
    }
}