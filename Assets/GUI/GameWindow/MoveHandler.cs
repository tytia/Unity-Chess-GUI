using Chess;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility;
using static GUI.GameWindow.HighlightManager;
using static Chess.Moves;

namespace GUI.GameWindow {
    public class MoveHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        private SpriteRenderer _sr;
        private Camera _cam;
        private Piece _piece;
        private static Game _game;
        public ref Piece piece => ref _piece;

        private void Awake() {
            _sr = GetComponent<SpriteRenderer>();
            _cam = Camera.main;
            _game = Game.GetInstance();
        }

        private void Start() {
            // need to wait for piece to get properly initialised before getting it,
            // which is why this is in Start() and not Awake()
            _game.analysisMode = true; // TODO: temporarily turned on for development, remove this later
            if (piece.color != _game.playerColor && !_game.analysisMode) {
                enabled = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            _sr.sortingOrder = 1;
            HighlightLegalMoves(piece);
            FollowMouse();
        }

        public void OnDrag(PointerEventData eventData) {
            FollowMouse();
        }

        public void OnPointerUp(PointerEventData eventData) {
            Square nearestSquare = MoveToNearestSquare();
            if (highlightedPiece.Equals(piece) && nearestSquare.transform == transform.parent) {
                UnHighlightLegalMoves();
            }
            else {
                highlightedPiece = piece;
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
            int to = squareCollider != null ? squareCollider.transform.position.ToBoardIndex() : -1;

            if (piece.GetLegalSquares().Contains(to)) {
                Destroy(Board.GetPieceGUI(to)?.gameObject);
                MovePiece(ref piece, to);
                
                HighlightPrevMove(); // needs to be after MovePiece() because the MovePiece() changes prevMove
                UnHighlightLegalMoves();

                transform.parent = squareCollider.transform;
            }

            transform.position = transform.parent.position;
            _sr.sortingOrder = 0;
            
            return squareCollider.GetComponent<Square>();
        }
        
        private static void MovePieceGUI(PieceGUI pieceGUI, int to) {
            Piece piece = pieceGUI.GetComponent<MoveHandler>().piece;
            if (to == piece.index) {
                throw new System.ArgumentException("Piece's index cannot be the same as the index it's moving to");
            }
            
            Destroy(Board.GetPieceGUI(to)?.gameObject);
            MovePiece(ref piece, to);

            HighlightPrevMove();
            
            pieceGUI.transform.parent = Board.GetSquare(to).transform;
            pieceGUI.transform.position = pieceGUI.transform.parent.position;
        }
    }
}