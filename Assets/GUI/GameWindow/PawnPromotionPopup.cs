using Chess;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace GUI.GameWindow {
    public class PawnPromotionPopup : MonoBehaviour, IPopup {
        [SerializeField] private Button _boardDim;
        [SerializeField] private Button _squareButtonPrefab;
        [SerializeField] private Image _pieceImagePrefab;

        private readonly PieceType[] _promotionOptions =
            { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight };

        private readonly Button[] _squareButtons = new Button[4];
        private readonly Image[] _pieceImages = new Image[4];
        private Piece _pawn;
        private static Game _game;
        
        public Button boardDim => _boardDim;

        // TODO: when board dim is clicked, cancel promotion and revert move
        /*
         * Unity UI elements are drawn based on hierarchy order.
         *
         * To make the piece sprites appear on top of the square (highlight) buttons, we need to make
         * sure the piece sprites are after the square buttons in the hierarchy.
         *
         * BoardDim is not a child but rather a sibling of the popup because we don't want it to move with the popup.
         */

        private void Awake() {
            _game = Game.instance;
            _boardDim.onClick.AddListener(CancelPromotion);
            for (var i = 0; i < _promotionOptions.Length; i++) {
                Vector3 spawnPos = transform.position + (i * Vector3.down);
                Button btn = Instantiate(_squareButtonPrefab, spawnPos, Quaternion.identity, transform);
                Image pieceImage = Instantiate(_pieceImagePrefab, spawnPos, Quaternion.identity, btn.transform);

                // AddListener() takes a method as a parameter, so we need to use a delegate to pass in arguments
                PieceType targetType = _promotionOptions[i];
                btn.onClick.AddListener(delegate { Promote(targetType); });
                btn.name = targetType + "Button";
                _squareButtons[i] = btn;
                
                pieceImage.sprite = PieceManager.PieceToSprite(new Piece(targetType, PieceColor.White));
                pieceImage.name = targetType + "Image";
                _pieceImages[i] = pieceImage;
            }

            Show(false);
        }

        private void Start() {
            PopupManager.pawnPromotion = this;
        }

        public void Assign(Piece pawn) {
            if (_pawn.color != pawn.color) {
                Vector3 direction = pawn.color == PieceColor.White ? Vector3.down : Vector3.up;
                for (var i = 0; i < _promotionOptions.Length; i++) {
                    Vector3 spawnPos = transform.position + (i * direction);
                    _squareButtons[i].transform.position = spawnPos;
                    _pieceImages[i].transform.position = spawnPos;
                    
                    _pieceImages[i].sprite = PieceManager.PieceToSprite(new Piece(_promotionOptions[i], pawn.color));
                }
            }

            _pawn = pawn;
        }

        public void Show(bool value) {
            if (value) {
                transform.position = _pawn.index.ToSquarePosVector2();
            }

            foreach (Transform child in transform) {
                child.gameObject.SetActive(value);
            }

            _boardDim.gameObject.SetActive(value);
        }

        private void Promote(PieceType promotionTarget) {
            _game.PromotePawn(_pawn, promotionTarget);
            
            var pieceGUI = Board.GetPieceGUI(_pawn.index);
            var promoted = new Piece(promotionTarget, _pawn.color);
            pieceGUI.SetSprite(PieceManager.PieceToSprite(promoted));
            pieceGUI.name = promoted.ToString();
            
            Show(false);
        }

        private void CancelPromotion() {
            MoveHandler.UndoMove();
            Show(false);
        }
    }
}