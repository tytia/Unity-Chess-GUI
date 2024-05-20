using System;
using Chess;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.GameWindow {
    public class Clocks : MonoBehaviour {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Image _playerClockBackground;
        [SerializeField] private TextMeshProUGUI _playerClockText;
        [SerializeField] private Image _opponentClockBackground;
        [SerializeField] private TextMeshProUGUI _opponentClockText;
        private readonly Color32 _defaultColor = new(128, 128, 128, 26);
        private readonly Color32 _warningColor = new(167, 83, 83, 26+39);
        private const float _AlphaModifier = 39f / 255;
        private readonly (Image clockBackground, TextMeshProUGUI clockText)[] _clocks = new (Image, TextMeshProUGUI)[2];
        private readonly float[] _secondsLeft = new float[2];
        private int _sideToMove;
        private int _secondsLeftWarning;
        private static readonly Game _game = Game.instance;

        public event EventHandler Timeout;

        public float startingMinutes { get; set; } = Single.PositiveInfinity;
        public float incrementSeconds { get; set; }

        private void Awake() {
            _gameManager.GameStart += OnNewGame;
            _game.GameEnd += (sender, args) => enabled = false;
            gameObject.SetActive(false);
        }

        private void Update() {
            if (_secondsLeft[_sideToMove] <= 0) {
                _clocks[_sideToMove].clockText.text = FormatSeconds(0);
                Timeout?.Invoke(this, EventArgs.Empty);
                enabled = false;
                return;
            }
            
            (Image clockBackground, TextMeshProUGUI clockText) = _clocks[_sideToMove];
            _secondsLeft[_sideToMove] -= Time.deltaTime;
            clockText.text = FormatSeconds(_secondsLeft[_sideToMove]);

            if (_secondsLeft[_sideToMove] <= _secondsLeftWarning) {
                clockBackground.color = _warningColor;
            }
        }

        private void OnDisable() {
            Moves.MoveEnd -= SwitchClocks;
        }

        private string FormatSeconds(float seconds) {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            
            if (t.Hours > 0) {
                return t.ToString(@"h\:mm\:ss");
            }

            if (t.TotalSeconds >= 20) {
                return t.ToString(@"mm\:ss");
            }
            
            return t.ToString(@"mm\:ss\.ff");
        }

        private void SwitchClocks(object sender, EventArgs e) {
            _secondsLeft[_sideToMove] += incrementSeconds;
            _clocks[_sideToMove].clockText.text = FormatSeconds(_secondsLeft[_sideToMove]);
            _clocks[_sideToMove].clockBackground.color -= new Color(0, 0, 0, _AlphaModifier);
            _clocks[_sideToMove].clockText.fontStyle = FontStyles.Normal;

            _sideToMove ^= 1;

            _clocks[_sideToMove].clockBackground.color += new Color(0, 0, 0, _AlphaModifier);
            _clocks[_sideToMove].clockText.fontStyle = FontStyles.Bold;
        }

        private void OnNewGame(object sender, EventArgs e) {
            if (Single.IsInfinity(startingMinutes)) {
                gameObject.SetActive(false);
                return;
            }
            
            Moves.MoveEnd += SwitchClocks;
            
            if (_game.playerColor == PieceColor.White) {
                _clocks[0] = (_playerClockBackground, _playerClockText);
                _clocks[1] = (_opponentClockBackground, _opponentClockText);
            }
            else {
                _clocks[0] = (_opponentClockBackground, _opponentClockText);
                _clocks[1] = (_playerClockBackground, _playerClockText);
            }

            _secondsLeft[0] = _secondsLeft[1] = startingMinutes * 60;
            _clocks[0].clockBackground.color = _clocks[1].clockBackground.color = _defaultColor;
            _clocks[0].clockText.text = FormatSeconds(_secondsLeft[0]);
            _clocks[1].clockText.text = FormatSeconds(_secondsLeft[1]);
            
            _sideToMove = 0;
            _clocks[_sideToMove].clockBackground.color += new Color(0, 0, 0, _AlphaModifier);
            _clocks[_sideToMove].clockText.fontStyle = FontStyles.Bold;

            _secondsLeftWarning = startingMinutes switch {
                <= 0.5f => 10,
                <= 1 => 20,
                <= 5 => 30,
                < 20 => 60,
                <= 30 => 120,
                < 60 => 180,
                60 => 300,
                _ => 600
            };

            enabled = true;
            gameObject.SetActive(true);
        }
    }
}