using Chess;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility;

namespace GUI.GameWindow {
    public class MoveHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        private SpriteRenderer _sr;
        private Piece _piece;
        private Camera _cam;
        private Game _game;

        private void Awake() {
            _sr = GetComponent<SpriteRenderer>();
            _cam = Camera.main;
            _game = Game.GetInstance();
        }

        private void Start() {
            // need to wait for piece to get properly initialised before getting it,
            // which is why this is in Start() and not Awake()
            _piece = GetComponent<PieceGUI>().piece;
            if (_piece.color != _game.playerColor) {
                enabled = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            _sr.sortingOrder = 1;
            MoveToMouse();
        }

        public void OnDrag(PointerEventData eventData) {
            MoveToMouse();
        }

        public void OnPointerUp(PointerEventData eventData) {
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
                    Destroy(p.gameObject);
                }

                transform.parent = squareCollider.transform;
                int to = squareCollider.transform.position.ToBoardIndex();
                SquareHighlighter.HighlightMove(_piece.index, to);
                _game.MovePiece(ref _piece, to);
            }

            transform.position = transform.parent.position;
            _sr.sortingOrder = 0;
        }
    }
}