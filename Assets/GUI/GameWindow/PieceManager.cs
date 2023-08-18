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
                p.SetSprite(PieceTypeToSprite(piece));
                p.name = piece.side + " " + piece.type;
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

        private Sprite PieceTypeToSprite(Piece piece) {
            return piece.side switch {
                Side.White => _sprites[(int)piece.type],
                Side.Black => _sprites[(int)piece.type + 6],
                _ => throw new ArgumentException("Piece must belong to a side")
            };
        }
    }
}