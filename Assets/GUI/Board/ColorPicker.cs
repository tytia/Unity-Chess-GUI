/*
 * Class to help with picking colors for squares on the board.
 * DEVELOPMENT USE ONLY!
 */

using System;
using UnityEngine;

namespace GUI.Board
{
    public class ColorPicker : MonoBehaviour
    {
        [SerializeField] private Color _lightCol = Square.lightCol;
        [SerializeField] private Color _darkCol = Square.darkCol;
        [SerializeField] private Board _board;

        private void OnValidate()
        {
            if (gameObject.activeSelf)
            {
                UpdateBoard();
            }
        }
        
        private void UpdateBoard()
        {
            if (_board == null || _board.GetSquare("h8") == null)
            {
                return;
            }
            
            for (int file = 0; file < Board.boardSize; file++)
            {
                for (int rank = 0; rank < Board.boardSize; rank++)
                {
                    Square sq = _board.GetSquare($"{(char)('a' + file)}{rank + 1}");
                    sq.color = (file + rank) % 2 == 0 ? _darkCol : _lightCol;
                }
            }
        }
    }
}
