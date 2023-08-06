using System.Collections.Generic;
using UnityEngine;

namespace GUI.GameWindow
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private Square _square;
        [SerializeField] private Transform _cam;
        private readonly Dictionary<string, Square> _squares = new();
        private static readonly int BoardSize = 8;
        
        public static int boardSize => BoardSize;
        
        private void Start()
        {
            InitBoard();
        }

        private void InitBoard()
        {
            for (int file = 0; file < BoardSize; file++)
            {
                for (int rank = 0; rank < BoardSize; rank++)
                {
                    Square sq = Instantiate(_square, new Vector3(file, rank), Quaternion.identity, transform);
                    sq.color = (file + rank) % 2 == 0 ? Square.darkCol : Square.lightCol;
                    sq.name = $"{(char)('a' + file)}{rank + 1}";
                    
                    _squares.Add(sq.name, sq);
                }
            }

            _cam.transform.position = new Vector3((float)BoardSize / 2 - 0.5f, (float)BoardSize / 2 - 0.5f, _cam.transform.position.z);
        }

        public Square GetSquare(string pos)
        {
            return _squares.TryGetValue(pos, out Square sq) ? sq : null;
        }
    }
}
