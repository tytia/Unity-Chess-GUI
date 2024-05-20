using System;
using Chess;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.GameWindow.Popups {
    public class GameEndPopup : MonoBehaviour {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Clocks _clocks;
        [SerializeField] private TextMeshProUGUI _gameEndText;
        [SerializeField] private TextMeshProUGUI _causeText;
        [SerializeField] private Button _playAsBlack;
        [SerializeField] private Button _playAsWhite;
        [SerializeField] private GameObject _boardCover;
        private readonly Color _winColor = new Color32(77, 229, 77, 255);
        private readonly Color _loseColor = new Color32(255, 52, 52, 255);
        private readonly Color _drawColor = new Color32(233, 168, 47, 255);
        private readonly string[] _drawTypes = {"", "by repetition", "by insufficient material", "by 75-move rule"};
        private static readonly Game _game = Game.instance;

        private void Awake() {
            _playAsBlack.onClick.AddListener(delegate { _gameManager.StartNewGame(PieceColor.Black); });
            _playAsWhite.onClick.AddListener(delegate { _gameManager.StartNewGame(PieceColor.White); });
            _game.GameEnd += OnGameEnd;
            _clocks.Timeout += OnTimeout;
            _gameManager.GameStart += (sender, args) => Show(false);
            gameObject.SetActive(false);
            _boardCover.SetActive(false);
        }

        private void OnGameEnd(object sender, GameEndEventArgs e) {
            if (e.result == EndResult.Checkmate) {
                _gameEndText.color = _game.playerColor != _game.colorToMove ? _winColor : _loseColor;
                _gameEndText.text = "Checkmate";
                _causeText.gameObject.SetActive(false);
            }
            else {
                _gameEndText.color = _drawColor;
                _gameEndText.text = "Draw";
                _causeText.text = _drawTypes[(int)e.result];
                _causeText.gameObject.SetActive(true);
            }
            
            Show(true);
        }

        private void OnTimeout(object sender, EventArgs e) {
            _causeText.text = "by timeout";
            _causeText.gameObject.SetActive(true);
            
            if (_game.insufficientMaterial[(int)_game.colorToMove ^ 1]) {
                _gameEndText.color = _drawColor;
                _gameEndText.text = "Draw";
                _causeText.text += " by insufficient material";
            }
            else if (_game.playerColor == _game.colorToMove) {
                _gameEndText.color = _loseColor;
                _gameEndText.text = "Loss";
            }
            else {
                _gameEndText.color = _winColor;
                _gameEndText.text = "Win";
            }
            
            Show(true);
        }

        public void Show(bool value) {
            gameObject.SetActive(value);
            _boardCover.SetActive(value);
        }
    }
}