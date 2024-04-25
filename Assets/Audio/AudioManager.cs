using System;
using Chess;
using UnityEngine;

namespace Audio {
    /// <summary>
    /// Plays sound effects for various game events.
    /// </summary>
    public class AudioManager : MonoBehaviour {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _move;
        [SerializeField] private AudioClip _capture;
        [SerializeField] private AudioClip _check;
        [SerializeField] private AudioClip _gameEnd;
        [SerializeField] private AudioClip _promote;
        [SerializeField] private AudioClip _premove;
        private static Game _game;

        /// <summary>
        /// Initialises <c>_game</c> and subscribes to the <c>MoveEnd</c> event.
        /// </summary>
        private void Awake() {
            _game = Game.instance;
            Moves.MoveEnd += PlayMoveSfx;
            _game.GameEnd += (sender, args) => _audioSource.PlayOneShot(_gameEnd);
        }

        /// <summary>
        /// Plays a corresponding sound effect for the move that was made.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayMoveSfx(object sender, EventArgs e) {
            if (_game.inCheck) {
                _audioSource.PlayOneShot(_check);
            }
            else if (Moves.LastMoveWasPromotion()) {
                _audioSource.PlayOneShot(_promote);
            }
            else if (Moves.LastMoveWasCapture()) {
                _audioSource.PlayOneShot(_capture);
            }
            else {
                _audioSource.PlayOneShot(_move);
            }
        }
    }
}
