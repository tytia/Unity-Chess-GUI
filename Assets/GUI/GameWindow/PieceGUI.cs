using UnityEngine;

namespace GUI.GameWindow {
    public class PieceGUI : MonoBehaviour {
        private SpriteRenderer _sr;
        private Chess.Piece _piece;
        public ref Chess.Piece piece => ref _piece;

        private void Awake() {
            _sr = GetComponent<SpriteRenderer>();
        }
        
        public void SetSprite(Sprite sprite) {
            _sr.sprite = sprite;
        }
    }
}