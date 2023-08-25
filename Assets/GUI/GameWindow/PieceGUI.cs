using UnityEngine;

namespace GUI.GameWindow {
    public class PieceGUI : MonoBehaviour {
        private SpriteRenderer _sr;

        private void Awake() {
            _sr = GetComponent<SpriteRenderer>();
        }
        
        public void SetSprite(Sprite sprite) {
            _sr.sprite = sprite;
        }
    }
}