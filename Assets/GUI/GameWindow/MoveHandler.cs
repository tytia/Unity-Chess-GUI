using UnityEngine;

namespace GUI.GameWindow {
    public class MoveHandler : MonoBehaviour {
        private Transform _pieceToDrag;
        private SpriteRenderer _pieceSpriteRenderer;
        private Camera _cam;

        private void Awake() {
            _cam = Camera.main;
        }

        private void OnMouseDown() {
            _pieceToDrag = transform.childCount != 0 ? transform.GetChild(0) : null;
            
            if (_pieceToDrag != null) {
                _pieceSpriteRenderer = _pieceToDrag.GetComponent<SpriteRenderer>();
                _pieceSpriteRenderer.sortingOrder = 1;
            }
            
            MovePieceToMouse();
        }

        private void OnMouseDrag() {
            MovePieceToMouse();
        }

        private void OnMouseUp() {
            MovePieceToNearestSquare();
        }

        private void MovePieceToMouse() {
            if (_pieceToDrag == null) {
                return;
            }

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0 - _cam.transform.position.z; // distance from camera
            _pieceToDrag.position = _cam.ScreenToWorldPoint(mousePos);
        }

        private void MovePieceToNearestSquare() {
            if (_pieceToDrag == null) {
                return;
            }
            
            Vector2 mousePosWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D pointCollider = Physics2D.OverlapPoint(mousePosWorld);
            
            if (pointCollider != null) {
                foreach (Piece piece in pointCollider.GetComponentsInChildren<Piece>()) {
                    GameManager.CapturePiece(piece);
                }
                
                _pieceToDrag.parent = pointCollider.transform;
                _pieceToDrag.position = _pieceToDrag.parent.position;
            }
            else {
                // return to original square
                _pieceToDrag.position = transform.position;
            }
            
            _pieceSpriteRenderer.sortingOrder = 0;
        }
    }
}