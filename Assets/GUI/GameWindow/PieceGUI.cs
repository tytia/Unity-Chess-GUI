using Chess;
using UnityEngine;

namespace GUI.GameWindow {
    public class PieceGUI : MonoBehaviour {
        private SpriteRenderer _sr;
        private MoveHandler _moveHandler;
        
        public Piece piece {
            get => _moveHandler.piece;
            set => _moveHandler.piece = value;
        }

        private void Awake() {
            _sr = GetComponent<SpriteRenderer>();
            _moveHandler = GetComponent<MoveHandler>();
        }
        
        public void SetSprite(Sprite sprite) {
            _sr.sprite = sprite;
        }
    }
}