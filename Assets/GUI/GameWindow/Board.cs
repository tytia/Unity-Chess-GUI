using UnityEngine;
using static Utility.Notation;

namespace GUI.GameWindow {
    public class Board : MonoBehaviour {
        [SerializeField] private Square _square;
        [SerializeField] private Square _highlight;
        [SerializeField] private Transform _cam;
        public static Square[] squares { get; } = new Square[64];

        private void Awake() {
            InitBoard();
            HighlightManager.InitHighlights(_highlight);
        }

        private void InitBoard() {
            for (var i = 0; i < 64; i++) {
                var point = i.ToSquarePosVector2();
                Square sq = Instantiate(_square, point, Quaternion.identity, transform);
                sq.color = (point.x + point.y) % 2 == 0 ? Square.darkColor : Square.lightColor;
                sq.name = ((SquarePos)i).ToString();

                squares[i] = sq;
            }
        }

        public static Square GetSquare(int index) {
            return squares[index];
        }
        
        public static PieceGUI GetPieceGUI(int index) {
            return squares[index].GetComponentInChildren<PieceGUI>();
        }
    }
}