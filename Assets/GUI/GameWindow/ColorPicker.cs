/*
 * Class to help with picking colors for squares on the board.
 * DEVELOPMENT USE ONLY
 */

using UnityEngine;
using static GUI.GameWindow.HighlightManager;

namespace GUI.GameWindow {
    public class ColorPicker : MonoBehaviour {
        [SerializeField] private Color _lightCol;
        [SerializeField] private Color _darkCol;
        [SerializeField] private Color _legalMovesCol;
        [SerializeField] private Board _board;

        private void Awake() {
            _lightCol = Square.lightCol;
            _darkCol = Square.darkCol;
            _legalMovesCol = Square.legalMovesCol;
        }

        private void OnValidate() {
            if (gameObject.activeSelf) {
                UpdateSquares();
                UpdateHighlights();
            }
        }

        private void UpdateSquares() {
            foreach (Square sq in _board.squares) {
                Vector2 worldPos = sq.transform.position;
                sq.color = (worldPos.x + worldPos.y) % 2 == 0 ? _darkCol : _lightCol;
            }
        }

        private void UpdateHighlights() {
            foreach (Square sq in highlights) {
                if (sq.isActiveAndEnabled && sq.color != Square.prevMoveCol) {
                    sq.color = _legalMovesCol;
                }
            }
        }
    }
}