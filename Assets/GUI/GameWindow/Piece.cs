using UnityEngine;

namespace GUI.GameWindow {
    public class Piece : MonoBehaviour {
        private SpriteRenderer _sr;
        public Chess.Piece piece { get; set; }

        private void Awake() {
            _sr = gameObject.GetComponent<SpriteRenderer>();
        }

        public void SetSprite(Sprite sprite) {
            _sr.sprite = sprite;
        }
    }
}