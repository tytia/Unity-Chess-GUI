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
        private static Game game => Game.instance;

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
            if (piece.color != game.playerColor && !game.analysisMode) {
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
            game.MovePiece(pieceIndex, to);
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
            int to = squareCollider != null ? squareCollider.transform.position.ToBoardIndex(game.playerColor) : -1;

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
            Move move = game.prevMove!.Value;
            PieceGUI pieceGUI = Board.GetPieceGUI(move.to);

            MovePieceGUI(pieceGUI, move.from);

            if (PopupManager.pawnPromotionPopup.boardDim.IsActive()) {
                // cancel promotion
                game.stateManager.ApplyState(game.stateManager.last);
            }
            else {
                if (game.MoveWasPromotion()) {
                    pieceGUI.SetSprite(PieceManager.PieceToSprite(move.piece));
                    pieceGUI.name = move.piece.ToString();
                }
                
                game.stateManager.Undo();
            }
            
            HighlightPrevMove();

            if (game.board[move.to] != null) {
                // instantiate captured piece
                PieceGUI p = Instantiate(pieceGUI, move.to.ToSquarePosVector2(game.playerColor), Quaternion.identity,
                    Board.GetSquare(move.to).transform);
                p.SetSprite(PieceManager.PieceToSprite(p.piece));
                p.name = p.piece.ToString();
            }
        }
    }
}