using UnityEngine;
using UnityEngine.EventSystems;
using Utility;
using GM = GUI.GameWindow.GameManager;

namespace GUI.GameWindow {
    public readonly struct Move {
        public Move(int from, int to) {
            this.from = from;
            this.to = to;
        }

        public int from { get; }
        public int to { get; }
    }

    public class MoveHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler {
        private SpriteRenderer _sr;
        private Camera _cam;

        private void Awake() {
            _sr = gameObject.GetComponent<SpriteRenderer>();
            _cam = Camera.main;
        }

        public void OnPointerDown(PointerEventData eventData) {
            _sr.sortingOrder = 1;
            MoveToMouse();
        }

        public void OnDrag(PointerEventData eventData) {
            MoveToMouse();
        }

        public void OnEndDrag(PointerEventData eventData) {
            MoveToNearestSquare();
        }

        private void MoveToMouse() {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0 - _cam.transform.position.z; // distance from camera
            transform.position = _cam.ScreenToWorldPoint(mousePos);
        }

        private void MoveToNearestSquare() {
            Vector2 mousePosWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D squareCollider = Physics2D.OverlapPoint(mousePosWorld, LayerMask.GetMask("Board"));

            if (squareCollider != null && squareCollider.transform != transform.parent) {
                foreach (PieceGUI p in squareCollider.GetComponentsInChildren<PieceGUI>()) {
                    GM.CapturePiece(p);
                    Destroy(p.gameObject);
                }

                transform.parent = squareCollider.transform;
                var pieceGUI = GetComponent<PieceGUI>();
                Move move = GM.MovePiece(pieceGUI, squareCollider.transform.position.ToBoardIndex());
                SquareHighlighter.HighlightMove(move);
            }

            transform.position = transform.parent.position;
            _sr.sortingOrder = 0;
        }
    }
}