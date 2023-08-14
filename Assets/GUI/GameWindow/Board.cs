using System.Collections.ObjectModel;
using UnityEngine;
using static Utility.Notation;

namespace GUI.GameWindow {
    public class Board : MonoBehaviour {
        [SerializeField] private Square _square;
        [SerializeField] private Square _highlight;
        [SerializeField] private Transform _cam;
        private readonly Square[] _squares = new Square[64];
        private readonly Square[] _highlights = new Square[64];
        
        public ReadOnlyCollection<Square> squares => new(_squares);

        private void Awake() {
            InitBoard();
            SquareHighlighter.Init(this);
        }

        private void InitBoard() {
            Transform highlightsParent = Instantiate(new GameObject("Highlights"), transform).transform;
            for (var pos = SquarePos.a1; pos <= SquarePos.h8; pos++) {
                var point = pos.ToVector2();
                Square sq = Instantiate(_square, point, Quaternion.identity, transform);
                Square sqHighlight = Instantiate(_highlight, point, Quaternion.identity, highlightsParent);
                sq.color = (point.x + point.y) % 2 == 0 ? Square.darkCol : Square.lightCol;
                sq.name = sqHighlight.name = pos.ToString();
                sq.name = pos.ToString();

                _squares[(int)pos] = sq;
                _highlights[(int)pos] = sqHighlight;
            }

            _cam.position = new Vector3((float)8 / 2 - 0.5f, (float)8 / 2 - 0.5f, _cam.position.z);
        }

        public Square GetSquare(int index) {
            return _squares[index];
        }
        
        public Square GetHighlight(int index) {
            return _highlights[index];
        }
    }
}