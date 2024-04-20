using System;
using Chess;
using UnityEngine;

namespace Audio {
    public class AudioManager : MonoBehaviour {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _move;
        [SerializeField] private AudioClip _capture;
        [SerializeField] private AudioClip _check;
        [SerializeField] private AudioClip _gameEnd;
        [SerializeField] private AudioClip _promote;
        [SerializeField] private AudioClip _premove;
        private static Game _game;

        private void Awake() {
            _game = Game.instance;
            Game.MoveEnd += PlayMoveSfx;
        }

        private void PlayMoveSfx(object sender, EventArgs e) {
            if (_game.inCheck) {
                _audioSource.PlayOneShot(_check);
            }
            else if (_game.MoveWasPromotion()) {
                _audioSource.PlayOneShot(_promote);
            }
            else if (_game.MoveWasCapture()) {
                _audioSource.PlayOneShot(_capture);
            }
            else {
                _audioSource.PlayOneShot(_move);
            }
            
            if (_game.endState != EndState.Ongoing) {
                _audioSource.PlayOneShot(_gameEnd);
            }
        }
    }
}
