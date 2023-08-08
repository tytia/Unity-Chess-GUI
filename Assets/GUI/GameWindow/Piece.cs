using UnityEngine;

namespace GUI.GameWindow {
    public class Piece : MonoBehaviour {
        public SpriteRenderer sr { get; set; }

        private void Awake() {
            sr = gameObject.GetComponent<SpriteRenderer>();
        }

        public void SetSprite(Sprite sprite) {
            sr.sprite = sprite;
        }
    }
}