using System;
using UnityEngine;
using Chess;
using static Utility.Notation;

namespace GUI.GameWindow {
    public class PieceManager : MonoBehaviour {
        [SerializeField] private PieceGUI _pieceGUI;
        [SerializeField] private Board _board;
        [SerializeField] private Sprite[] _sprites;
        private static Sprite[] _spritesStatic;
        private static Game _game;

        private void Awake() {
            _game = Game.instance;
            _spritesStatic = _sprites;
        }

        private void OnEnable() {
            InitPieces();
        }

        private void OnDisable() {
            RemovePieces();
        }

        public void InitPieces() {
            foreach (int pieceIndex in _game.pieceIndexes) {
                Piece piece = _game.board[pieceIndex]!.Value;
                var point = piece.index.ToSquarePosVector2();
                PieceGUI p = Instantiate(_pieceGUI, point, Quaternion.identity, Board.GetSquare(piece.index).transform);
                p.piece = piece;
                p.SetSprite(PieceToSprite(piece));
                p.name = piece.ToString();
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

        public static Sprite PieceToSprite(Piece piece) {
            return piece.color switch {
                PieceColor.White => _spritesStatic[(int)piece.type],
                PieceColor.Black => _spritesStatic[(int)piece.type + 6],
                _ => throw new ArgumentException("Piece must belong to a side")
            };
        }
    }
}