using System;
using UnityEngine;
using Chess;
using static Utility.Notation;

namespace GUI.GameWindow {
    public class PieceManager : MonoBehaviour {
        [SerializeField] private PieceGUI _pieceGUI;
        [SerializeField] private Board _board;
        [SerializeField] private Sprite[] _sprites;

        private void OnEnable() {
            GameManager.pieceManager = this;
            InitPieces();
        }

        private void OnDisable() {
            RemovePieces();
        }

        public void InitPieces() {
            foreach (Piece piece in GameManager.GetPieces()) {
                var point = ((SquarePos)piece.index).ToVector2();
                PieceGUI p = Instantiate(_pieceGUI, point, Quaternion.identity, _board.GetSquare(piece.index).transform);
                p.piece = piece;
                p.SetSprite(PieceTypeToSprite(piece.type));
                PieceType color = piece.type & (PieceType.White | PieceType.Black);
                p.name = color + " " + (piece.type ^ color);
            }
        }

        public void RemovePieces() {
            if (_board == null) {
                return;
            }
            
            foreach (PieceGUI piece in _board.GetComponentsInChildren<PieceGUI>()) {
                Destroy(piece.gameObject);
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