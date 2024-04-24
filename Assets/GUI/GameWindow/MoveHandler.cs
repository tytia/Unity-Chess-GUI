using Chess;
using GUI.GameWindow.Popups;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility;
using static GUI.GameWindow.HighlightManager;

namespace GUI.GameWindow {
    public class MoveHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
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

        private void MovePiece(int to) {
            Moves.MovePiece(pieceIndex, to);
            HighlightPrevMove(); // needs to be after MovePiece() because MovePiece() changes prevMove
            _pieceGUI.index = to;
        }

        private static void MovePieceGUI(PieceGUI pieceGUI, int to) {
            // it is important to note that this method only affects the GUI and updates
            // which piece the specified PieceGUI is referring to
            if (to == pieceGUI.index) {
                throw new System.ArgumentException("Piece's index should not be the same as the index it's moving to");
            }

            Destroy(Board.GetPieceGUI(to)?.gameObject);

            pieceGUI.transform.parent = Board.GetSquare(to).transform;
            pieceGUI.transform.position = pieceGUI.transform.parent.position;
            pieceGUI.index = to;
        }

        public static void MovePieceGUI(int from, int to) {
            PieceGUI pieceGUI = Board.GetPieceGUI(from);
            if (pieceGUI is null) {
                throw new System.ArgumentException("No piece at index " + from);
            }
            
            MovePieceGUI(pieceGUI, to);
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
                MovePiece(to);
                UnHighlightLegalMoves();

                transform.parent = squareCollider.transform;
            }

            transform.position = transform.parent.position;
            _sr.sortingOrder = 0;

            return squareCollider != null ? squareCollider.GetComponent<Square>() : null;
        }

        public static void UndoMove() {
            Move move = _game.prevMove!.Value;
            PieceGUI pieceGUI = Board.GetPieceGUI(move.to);

            MovePieceGUI(pieceGUI, move.from);

            if (PopupManager.pawnPromotionPopup.boardDim.IsActive()) {
                // cancel promotion
                _game.stateManager.ApplyState(_game.stateManager.last);
            }
            else {
                if (Moves.LastMoveWasPromotion()) {
                    pieceGUI.SetSprite(PieceManager.PieceToSprite(move.piece));
                    pieceGUI.name = move.piece.ToString();
                    pieceGUI.index = move.from;
                }
                
                _game.stateManager.Undo();
            }
            
            HighlightPrevMove();

            if (_game.board[move.to] != null) {
                // instantiate captured piece
                PieceGUI revivedPieceGUI = Instantiate(pieceGUI, move.to.ToSquarePosVector2(_game.playerColor), Quaternion.identity,
                    Board.GetSquare(move.to).transform);
                revivedPieceGUI.index = move.to;
                revivedPieceGUI.name = revivedPieceGUI.piece.ToString();
                revivedPieceGUI.SetSprite(PieceManager.PieceToSprite(revivedPieceGUI.piece));
            }
        }
    }
}