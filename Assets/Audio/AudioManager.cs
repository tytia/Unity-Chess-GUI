using System;
using Chess;
using UnityEngine;

namespace Audio {
    public class AudioManager : MonoBehaviour {
        [SerializeField] private AudioClip _move;
        [SerializeField] private AudioClip _capture;
        [SerializeField] private AudioClip _check;
        [SerializeField] private AudioClip _gameEnd;
        [SerializeField] private AudioClip _promote;
        [SerializeField] private AudioClip _premove;
        private static AudioSource _audioSource;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
            Moves.MoveEnd += PlayMoveSfx;
        }

        private void PlayMoveSfx(object sender, EventArgs e) {
            if (Moves.MoveWasPromotion()) {
                _audioSource.PlayOneShot(_promote);
            }
            else if (Moves.MoveWasCapture()) {
                _audioSource.PlayOneShot(_capture);
            }
            else {
                _audioSource.PlayOneShot(_move);
            }
        }
    }
}
