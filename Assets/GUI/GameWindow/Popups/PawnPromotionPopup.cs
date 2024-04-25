using Chess;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.GameWindow.Popups {
    public class PawnPromotionPopup : Popup {
        [SerializeField] private Button _boardDim;
        [SerializeField] private RectTransform _stripWindow;
        [SerializeField] private Button _squareButtonPrefab;
        [SerializeField] private Image _pieceImagePrefab;

        private readonly PieceType[] _promotionOptions =
            { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight };

        private readonly Button[] _squareButtons = new Button[4];
        private readonly Image[] _pieceImages = new Image[4];
        private int _pawnIndex;
        private static readonly Game _game = Game.instance;
        
        /*
         * Unity UI elements are drawn based on hierarchy order.
         *
         * To make the piece sprites appear on top of the square (highlight) buttons, we need to make
         * sure the piece sprites are after the square buttons in the hierarchy.
         *
         * BoardDim is not a child but rather a sibling of the popup because we don't want it to move with the popup.
         */

        private void Awake() {
            _boardDim.onClick.AddListener(CancelPromotion);
            InitialiseDialog();
            Moves.Promotion += (sender, args) => {
                Assign(args.pawnIndex);
                Show(true);
            };
            Show(false);
        }

        private void InitialiseDialog() {
            for (var i = 0; i < _promotionOptions.Length; i++) {
                Vector3 spawnPos = transform.position + (i * Vector3.down);
                Button btn = Instantiate(_squareButtonPrefab, spawnPos, Quaternion.identity, _stripWindow);
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
        }

        private void Assign(int pawnIndex) {
            /*
             * There is no need to account for the case where we need to show the popup upwards, because the player
             * will always be facing upwards on the board regardless of what colour they are playing as.
             */
            for (var i = 0; i < _promotionOptions.Length; i++) {
                _pieceImages[i].sprite =
                    PieceManager.PieceToSprite(new Piece(_promotionOptions[i], _game.board[pawnIndex]!.Value.color));
            }

            _pawnIndex = pawnIndex;
            _stripWindow.position = _pawnIndex.ToSquarePosVector2(_game.playerColor);
        }

        private void Promote(PieceType promotionTarget) {
            Moves.PromotePawn(_pawnIndex, promotionTarget);
            
            var pieceGUI = Board.GetPieceGUI(_pawnIndex);
            var promoted = new Piece(promotionTarget, _game.board[_pawnIndex]!.Value.color);
            pieceGUI.SetSprite(PieceManager.PieceToSprite(promoted));
            pieceGUI.name = promoted.ToString();
            
            Show(false);
        }

        private void CancelPromotion() {
            SystemMoveHandler.UndoMove();
            Show(false);
        }
    }
}