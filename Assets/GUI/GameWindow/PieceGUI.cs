using Chess;
using UnityEngine;

namespace GUI.GameWindow {
    public class PieceGUI : MonoBehaviour {
        private SpriteRenderer _sr;
        private static readonly Game _game = Game.instance;
        
        public int index { get; set; }
        public Piece piece => _game.board[index]!.Value;

        private void Awake() {
            _sr = GetComponent<SpriteRenderer>();
        }
        
        public void SetSprite(Sprite sprite) {
            _sr.sprite = sprite;
        }
    }
}