using System;
using UnityEngine;
using Chess;
using static Utility.Notation;

namespace GUI.GameWindow {
    public class PieceManager : MonoBehaviour {
        [SerializeField] private PieceGUI _pieceGUI;
        [SerializeField] private Board _board;
        [SerializeField] private Sprite[] _sprites;
        private Game _game;

        private void Awake() {
            _game = Game.GetInstance();
        }

        private void OnEnable() {
            InitPieces();
        }

        private void OnDisable() {
            RemovePieces();
        }

        public void InitPieces() {
            foreach (Piece piece in _game.pieces) {
                var point = ((SquarePos)piece.index).ToVector2();
                PieceGUI p = Instantiate(_pieceGUI, point, Quaternion.identity, _board.GetSquare(piece.index).transform);
                p.piece = piece;
                p.SetSprite(PieceTypeToSprite(piece));
                p.name = piece.color + " " + piece.type;
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
            return piece.color switch {
                PieceColor.White => _sprites[(int)piece.type],
                PieceColor.Black => _sprites[(int)piece.type + 6],
                _ => throw new ArgumentException("Piece must belong to a side")
            };
        }
    }
}