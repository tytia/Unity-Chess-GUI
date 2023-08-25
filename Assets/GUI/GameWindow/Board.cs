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
            for (var pos = SquarePos.a1; pos <= SquarePos.h8; pos++) {
                var point = pos.ToVector2();
                Square sq = Instantiate(_square, point, Quaternion.identity, transform);
                sq.color = (point.x + point.y) % 2 == 0 ? Square.darkColor : Square.lightColor;
                sq.name = pos.ToString();

                squares[(int)pos] = sq;
            }

            _cam.position = new Vector3((float)8 / 2 - 0.5f, (float)8 / 2 - 0.5f, _cam.position.z);
        }

        public static Square GetSquare(int index) {
            return squares[index];
        }
        
        public static PieceGUI GetPieceGUI(int index) {
            return squares[index].GetComponentInChildren<PieceGUI>();
        }
    }
}