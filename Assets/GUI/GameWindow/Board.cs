using Chess;
using UnityEngine;
using static Utility.Notation;

namespace GUI.GameWindow {
    public class Board : MonoBehaviour {
        [SerializeField] private Square _square;
        [SerializeField] private Square _highlight;
        [SerializeField] private LabelManager _labelManager;
        [SerializeField] private PieceManager _pieceManager;
        public static Square[] squares { get; } = new Square[64];
        public PieceColor orientation = PieceColor.White;

        private void Awake() {
            InitBoard();
            HighlightManager.InitHighlights(_highlight);
        }

        private void InitBoard() {
            for (var i = 0; i < 64; i++) {
                var point = i.ToSquarePosVector2(orientation);
                Square sq = Instantiate(_square, point, Quaternion.identity, transform);
                sq.color = (point.x + point.y) % 2 == 0 ? Square.darkColor : Square.lightColor;
                sq.name = ((SquarePos)i).ToString();

                squares[i] = sq;
            }
        }
        
        public void Flip() {
            if (orientation == PieceColor.White) {
                transform.Rotate(Vector3.forward, 180);
                transform.position = new Vector3(7, 7, 0);
                orientation = PieceColor.Black;
            }
            else {
                transform.Rotate(Vector3.forward, 180);
                transform.position = new Vector3(0, 0, 0);
                orientation = PieceColor.White;
            }
            // transform.RotateAround(new Vector3(3.5f, 3.5f, 0), Vector3.forward, 180);
            
            _labelManager.Flip();
            // _pieceManager.Flip();
        }

        public static Square GetSquare(int index) {
            return squares[index];
        }
        
        public static PieceGUI GetPieceGUI(int index) {
            return squares[index].GetComponentInChildren<PieceGUI>();
        }
    }
}