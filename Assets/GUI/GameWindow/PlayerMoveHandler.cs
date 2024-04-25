using Chess;
using UnityEngine;
using UnityEngine.EventSystems;
using static GUI.GameWindow.HighlightManager;

namespace GUI.GameWindow {
    public class PlayerMoveHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        private SpriteRenderer _sr;
        private Camera _cam;
        private PieceGUI _pieceGUI;
        private static readonly Game _game = Game.instance;

        private int pieceIndex => _pieceGUI.index;
        private Piece piece => _pieceGUI.piece;

        private void Awake() {
            _sr = GetComponent<SpriteRenderer>();
            _cam = Camera.main;
            _pieceGUI = GetComponent<PieceGUI>();
        }

        private void Start() {
            // need to wait for piece to be properly initialised before getting it,
            // which is why this is in Start() and not Awake()
            if (piece.color != _game.playerColor && !_game.analysisMode) {
                enabled = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            _sr.sortingOrder = 1;
            HighlightLegalMoves(pieceIndex);
            FollowMouse();
        }

        public void OnDrag(PointerEventData eventData) {
            FollowMouse();
        }

        public void OnPointerUp(PointerEventData eventData) {
            Square nearestSquare = MoveToNearestSquare();
            if (nearestSquare != null && selectedIndex.Equals(pieceIndex) && nearestSquare.transform == transform.parent) {
                UnHighlightLegalMoves();
            }
            else {
                selectedIndex = pieceIndex;
            }
        }

        private void FollowMouse() {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0 - _cam.transform.position.z; // distance from camera
            transform.position = _cam.ScreenToWorldPoint(mousePos);
        }

        private Square MoveToNearestSquare() {
            Vector2 mousePosWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D squareCollider = Physics2D.OverlapPoint(mousePosWorld, LayerMask.GetMask("Board"));
            // assign an impossible index if collider is null
            int to = squareCollider != null ? squareCollider.transform.position.ToBoardIndex(_game.playerColor) : -1;

            if (MoveGenerator.GetLegalSquares(pieceIndex).Contains(to)) {
                Destroy(Board.GetPieceGUI(to)?.gameObject);
                Moves.MovePiece(pieceIndex, to);
                HighlightPrevMove(); // needs to be after MovePiece() because MovePiece() changes prevMove
                _pieceGUI.index = to;
                UnHighlightLegalMoves();

                transform.parent = squareCollider.transform;
            }

            transform.position = transform.parent.position;
            _sr.sortingOrder = 0;

            return squareCollider != null ? squareCollider.GetComponent<Square>() : null;
        }
    }
}