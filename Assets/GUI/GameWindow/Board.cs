using UnityEngine;
using static Utility.Notation;

namespace GUI.GameWindow
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private Square _square;
        [SerializeField] private Transform _cam;
        private readonly Square[] _squares = new Square[64];

        private void Start()
        {
            InitBoard();
        }

        private void InitBoard()
        {
            for (SquarePos pos = SquarePos.a1; pos <= SquarePos.h8; pos++)
            {
                Vector2 point = pos.ToVector2();
                Square sq = Instantiate(_square, point, Quaternion.identity, transform);
                sq.color = (point.x + point.y) % 2 == 0 ? Square.darkCol : Square.lightCol;
                sq.name = pos.ToString();

                _squares[(int)pos] = sq;
            }

            _cam.transform.position = new Vector3((float)8 / 2 - 0.5f, (float)8 / 2 - 0.5f, _cam.transform.position.z);
        }

        public Square GetSquare(SquarePos pos)
        {
            return _squares[(int)pos];
        }
    }
}