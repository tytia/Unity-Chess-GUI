using System;
using UnityEngine;
using Chess;
using Utility;

namespace GUI.GameWindow {
    public class PieceManager : MonoBehaviour {
        [SerializeField] private Piece _piece;
        [SerializeField] private Board _board;
        [SerializeField] private Sprite[] _sprites;
        private Game _game;

        private void Awake() {
            _game = new Game();
        }

        private void Start() {
            InitPieces();
        }

        private void InitPieces() {
            foreach (Chess.Piece piece in _game.pieces) {
                var point = piece.pos.ToVector2();
                Piece p = Instantiate(_piece, point, Quaternion.identity, _board.GetSquare(piece.pos).transform);
                p.SetSprite(PieceTypeToSprite(piece.type));
                PieceType color = piece.type & (PieceType.White | PieceType.Black);
                p.name = color + " " + (piece.type ^ color);
            }
        }

        private Sprite PieceTypeToSprite(PieceType type) {
            if (type.HasFlag(PieceType.White)) {
                return _sprites[(int)(type ^ PieceType.White)];
            }

            if (type.HasFlag(PieceType.Black)) {
                return _sprites[(int)(type ^ PieceType.Black) + 6];
            }

            throw new ArgumentException("Piece type must have a color");
        }
    }
}