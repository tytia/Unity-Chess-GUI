using Chess;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace GUI.GameWindow.Popups.NewGame {
    public class NewGamePopup : MonoBehaviour {
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Button _playAsBlack;
        [SerializeField] private Button _playAsWhite;
        [SerializeField] private Button _playAsRandom;
        [SerializeField] private GameObject _boardDimCover;

        private void Awake() {
            _playAsBlack.onClick.AddListener(delegate { _gameManager.StartNewGame(PieceColor.Black); });
            _playAsWhite.onClick.AddListener(delegate { _gameManager.StartNewGame(PieceColor.White); });
            _playAsRandom.onClick.AddListener(delegate {
                _gameManager.StartNewGame(new Random().NextDouble() < 0.5 ? PieceColor.Black : PieceColor.White);
            });
            
            _gameManager.GameStart += (sender, args) => Show(false);
            
            Show(true);
        }

        private void Show(bool value) {
            gameObject.SetActive(value);
            _boardDimCover.SetActive(value);
        }
    }
}