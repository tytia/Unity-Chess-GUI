/*
 * Class to help with picking colors for squares on the board.
 * DEVELOPMENT USE ONLY
 */

using System;
using UnityEngine;
using static Utility.Notation;

namespace GUI.GameWindow
{
    public class ColorPicker : MonoBehaviour
    {
        [SerializeField] private Color _lightCol;
        [SerializeField] private Color _darkCol;
        [SerializeField] private GameWindow.Board _board;

        private void Awake()
        {
            _lightCol = Square.lightCol;
            _darkCol = Square.darkCol;
        }

        private void OnValidate()
        {
            if (gameObject.activeSelf)
            {
                UpdateBoard();
            }
        }
        
        private void UpdateBoard()
        {
            if (_board == null || _board.GetSquare(SquarePos.h8) == null)
            {
                return;
            }
            
            for (SquarePos pos = SquarePos.a1; pos <= SquarePos.h8; pos++)
            {
                int file = (int)pos % 8, rank = (int)pos / 8;
                Square sq = _board.GetSquare(pos);
                sq.color = (file + rank) % 2 == 0 ? _darkCol : _lightCol;
            }
        }
    }
}
