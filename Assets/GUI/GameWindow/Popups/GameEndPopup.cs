using System;
using Chess;
using TMPro;
using UnityEngine;

namespace GUI.GameWindow.Popups {
    public class GameEndPopup : MonoBehaviour, IPopup {
        [SerializeField] private TextMeshProUGUI _gameEndText;
        private readonly Color _winColor = new Color32(77, 229, 77, 255);
        private readonly Color _loseColor = new Color32(255, 52, 52, 255);
        private readonly Color _drawColor = new Color32(233, 168, 47, 255);
        private static readonly Game _game = Game.instance;

        private void Awake() {
            PopupManager.gameEndPopup = this;
            Game.MoveEnd += CheckState;
        }

        private void Start() {
            Show(false);
        }

        public void Show(bool value) {
            gameObject.SetActive(value);
            foreach (Transform child in transform) {
                child.gameObject.SetActive(value);
            }
            
            PopupManager.ShowNewGamePopup(value);
        }

        private void CheckState(object sender, EventArgs e) {
            if (_game.endState == EndState.Ongoing) return;
            
            _gameEndText.text = _game.endState.ToString();
            if (_game.endState == EndState.Checkmate) {
                _gameEndText.color = _game.playerColor != _game.colorToMove ? _winColor : _loseColor;
            }
            else {
                _gameEndText.color = _drawColor;
            }
            
            Show(true);
        }
    }
}