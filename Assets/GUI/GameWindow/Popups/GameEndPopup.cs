using Chess;
using TMPro;
using UnityEngine;

namespace GUI.GameWindow.Popups {
    public class GameEndPopup : Popup {
        [SerializeField] private TextMeshProUGUI _gameEndText;
        private readonly Color _winColor = new Color32(77, 229, 77, 255);
        private readonly Color _loseColor = new Color32(255, 52, 52, 255);
        private readonly Color _drawColor = new Color32(233, 168, 47, 255);
        private static readonly Game _game = Game.instance;

        private void Awake() {
            _game.GameEnd += ShowResult;
            Show(false);
        }

        private void ShowResult(object sender, GameEndEventArgs e) {
            _gameEndText.text = e.result.ToString();
            if (e.result == EndResult.Checkmate) {
                _gameEndText.color = _game.playerColor != _game.colorToMove ? _winColor : _loseColor;
            }
            else {
                _gameEndText.color = _drawColor;
            }
            
            Show(true);
        }
    }
}