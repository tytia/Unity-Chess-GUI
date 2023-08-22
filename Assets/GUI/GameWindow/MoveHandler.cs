using Chess;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility;
using static GUI.GameWindow.HighlightManager;

namespace GUI.GameWindow {
    public class MoveHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        private SpriteRenderer _sr;
        private Camera _cam;
        private Game _game;
        private Piece _piece;

        private void Awake() {
            _sr = GetComponent<SpriteRenderer>();
            _cam = Camera.main;
            _game = Game.GetInstance();
        }

        private void Start() {
            // need to wait for piece to get properly initialised before getting it,
            // which is why this is in Start() and not Awake()
            _piece = GetComponent<PieceGUI>().piece;
            _game.analysisMode = true; // TODO: temporarily turned on for development, remove this later
            if (_piece.color != _game.playerColor && !_game.analysisMode) {
                enabled = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            _sr.sortingOrder = 1;
            HighlightLegalMoves(_piece);
            MoveToMouse();
        }

        public void OnDrag(PointerEventData eventData) {
            MoveToMouse();
        }

        public void OnPointerUp(PointerEventData eventData) {
            Square nearestSquare = MoveToNearestSquare();
            if (highlightedPiece.Equals(_piece) && nearestSquare.transform == transform.parent) {
                UnHighlightLegalMoves();
            }
            else {
                highlightedPiece = _piece;
            }
        }

        private void MoveToMouse() {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0 - _cam.transform.position.z; // distance from camera
            transform.position = _cam.ScreenToWorldPoint(mousePos);
        }

        private Square MoveToNearestSquare() {
            Vector2 mousePosWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D squareCollider = Physics2D.OverlapPoint(mousePosWorld, LayerMask.GetMask("Board"));
            // assign an impossible index if collider is null
            int to = squareCollider != null ? squareCollider.transform.position.ToBoardIndex() : -1;

            if (_piece.GetLegalSquares().Contains(to)) {
                foreach (PieceGUI p in squareCollider.GetComponentsInChildren<PieceGUI>()) {
                    Destroy(p.gameObject);
                }

                transform.parent = squareCollider.transform;
                HighlightMove(_piece.index, to); // needs to be before MovePiece() because the piece's index will change
                _game.MovePiece(ref _piece, to);
                UnHighlightLegalMoves();
            }

            transform.position = transform.parent.position;
            _sr.sortingOrder = 0;
            
            return squareCollider.GetComponent<Square>();
        }
    }
}