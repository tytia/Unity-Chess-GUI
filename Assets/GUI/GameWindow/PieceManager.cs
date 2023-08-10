using System;
using UnityEngine;
using Chess;
using Utility;
using static GUI.GameWindow.GameManager;

namespace GUI.GameWindow {
    public class PieceManager : MonoBehaviour {
        [SerializeField] private Piece _pieceGUI;
        [SerializeField] private Board _board;
        [SerializeField] private Sprite[] _sprites;

        private void Start() {
            InitPieces();
        }

        private void InitPieces() {
            foreach (Chess.Piece piece in GetPieces()) {
                var point = piece.pos.ToVector2();
                Piece p = Instantiate(_pieceGUI, point, Quaternion.identity, _board.GetSquare(piece.pos).transform);
                p.piece = piece;
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