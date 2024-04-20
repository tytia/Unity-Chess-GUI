using System;
using System.Collections.Generic;
using UnityEngine;
using Chess;
using static Utility.Notation;

namespace GUI.GameWindow {
    public class PieceManager : MonoBehaviour {
        [SerializeField] private PieceGUI _pieceGUI;
        [SerializeField] private Board _board;
        [SerializeField] private Sprite[] _sprites;
        private readonly List<PieceGUI> _pieceGUIs = new List<PieceGUI>();
        private static Sprite[] _spritesStatic;
        private static Game game => Game.instance;

        private void Awake() {
            _spritesStatic = _sprites;
        }

        private void OnEnable() {
            InitPieces(game.playerColor);
        }

        private void OnDisable() {
            RemovePieces();
        }

        public void InitPieces(PieceColor orientation) {
            foreach (int pieceIndex in game.pieceIndexes) {
                Piece piece = game.board[pieceIndex]!.Value;
                var point = piece.index.ToSquarePosVector2(orientation);
                PieceGUI p = Instantiate(_pieceGUI, point, Quaternion.identity, Board.GetSquare(piece.index).transform);
                p.piece = piece;
                p.SetSprite(PieceToSprite(piece));
                p.name = piece.ToString();
                _pieceGUIs.Add(p);
            }
        }

        public void RemovePieces() {
            if (_board == null) {
                return;
            }
            
            foreach (PieceGUI piece in _board.GetComponentsInChildren<PieceGUI>()) {
                Destroy(piece.gameObject);
            }
            
            _pieceGUIs.Clear();
        }
        
        public void Flip() {
            foreach (PieceGUI p in _pieceGUIs) {
                p.transform.Rotate(0, 0, 180);
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