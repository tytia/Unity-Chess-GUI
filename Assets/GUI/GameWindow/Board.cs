using System.Collections.ObjectModel;
using UnityEngine;
using static Utility.Notation;

namespace GUI.GameWindow {
    public class Board : MonoBehaviour {
        [SerializeField] private Square _square;
        [SerializeField] private Transform _cam;
        private readonly Square[] _squares = new Square[64];
        
        public ReadOnlyCollection<Square> squares => new(_squares);

        private void Awake() {
            InitBoard();
        }

        private void InitBoard() {
            for (var pos = SquarePos.a1; pos <= SquarePos.h8; pos++) {
                var point = pos.ToVector2();
                Square sq = Instantiate(_square, point, Quaternion.identity, transform);
                sq.color = (point.x + point.y) % 2 == 0 ? Square.darkCol : Square.lightCol;
                sq.name = pos.ToString();

                _squares[(int)pos] = sq;
            }

            _cam.position = new Vector3((float)8 / 2 - 0.5f, (float)8 / 2 - 0.5f, _cam.position.z);
        }

        public Square GetSquare(int index) {
            return _squares[index];
        }
        
        public Square GetSquare(SquarePos pos) {
            return _squares[(int)pos];
        }
    }
}