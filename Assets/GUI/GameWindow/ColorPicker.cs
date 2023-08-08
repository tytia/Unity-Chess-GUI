/*
 * Class to help with picking colors for squares on the board.
 * DEVELOPMENT USE ONLY
 */

using UnityEngine;
using static Utility.Notation;

namespace GUI.GameWindow {
    public class ColorPicker : MonoBehaviour {
        [SerializeField] private Color _lightCol;
        [SerializeField] private Color _darkCol;
        [SerializeField] private Board _board;

        private void Awake() {
            _lightCol = Square.lightCol;
            _darkCol = Square.darkCol;
        }

        private void OnValidate() {
            if (gameObject.activeSelf) {
                UpdateBoard();
            }
        }

        private void UpdateBoard() {
            if (_board == null || _board.GetSquare(SquarePos.h8) == null) {
                return;
            }

            for (var pos = SquarePos.a1; pos <= SquarePos.h8; pos++) {
                var point = pos.ToVector2();
                Square sq = _board.GetSquare(pos);
                sq.color = (point.x + point.y) % 2 == 0 ? _darkCol : _lightCol;
            }
        }
    }
}