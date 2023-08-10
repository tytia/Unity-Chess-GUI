using UnityEngine;
using static GUI.GameWindow.GameManager;

namespace GUI.GameWindow {
    public class MoveHandler : MonoBehaviour {
        private Transform _pieceToDrag;
        private Camera _cam;

        private void Awake() {
            _cam = Camera.main;
        }

        private void OnMouseDown() {
            _pieceToDrag = transform.childCount != 0 ? transform.GetChild(0) : null;
            MoveToMouse();
        }

        private void OnMouseDrag() {
            MoveToMouse();
        }

        private void OnMouseUp() {
            MoveToNearestSquare();
        }

        private void MoveToMouse() {
            if (_pieceToDrag == null) {
                return;
            }

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0 - _cam.transform.position.z; // distance from camera
            _pieceToDrag.position = _cam.ScreenToWorldPoint(mousePos);
        }

        private void MoveToNearestSquare() {
            if (_pieceToDrag == null) {
                return;
            }
            
            Vector2 mousePosWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D pointCollider = Physics2D.OverlapPoint(mousePosWorld);
            
            if (pointCollider != null) {
                foreach (Piece piece in pointCollider.GetComponentsInChildren<Piece>()) {
                    CapturePiece(piece);
                }
                
                _pieceToDrag.parent = pointCollider.transform;
                _pieceToDrag.position = _pieceToDrag.parent.position;
            }
            else {
                // return to original square
                _pieceToDrag.position = transform.position;
            }
        }
    }
}