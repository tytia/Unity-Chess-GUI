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
            if (gameObject.activeSelf && _board != null && _board.GetSquare(SquarePos.h8) != null) {
                UpdateBoard();
            }
        }

        private void UpdateBoard() {
            foreach (Square sq in _board.squares) {
                Vector2 worldPos = sq.transform.position;
                sq.color = (worldPos.x + worldPos.y) % 2 == 0 ? _darkCol : _lightCol;
            }
        }
    }
}