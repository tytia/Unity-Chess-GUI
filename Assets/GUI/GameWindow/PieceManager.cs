using System;
using UnityEngine;
using Chess;
using Utility;

namespace GUI.GameWindow
{
    public class PieceManager : MonoBehaviour
    {
        [SerializeField] private Piece _piece;
        [SerializeField] private Sprite[] _sprites;
        private Game _game;
        
        public Sprite[] sprites => _sprites;

        private void Awake()
        {
            _game = new Game();
        }

        private void Start()
        {
            InitPieces();
        }

        private void InitPieces()
        {
            foreach (Chess.Piece piece in _game.GetPieces())
            {
                var point = piece.pos.ToVector2();
                Piece p = Instantiate(_piece, point, Quaternion.identity, transform);
                p.SetSprite(PieceTypeToSprite(piece.type));
                PieceType color = piece.type & (PieceType.White | PieceType.Black);
                p.name = color + " " + (piece.type ^ color);
            }
        }
        
        private Sprite PieceTypeToSprite(PieceType type)
        {
            if ((type & PieceType.White) == PieceType.White)
            {
                return sprites[(int)(type ^ PieceType.White)];
            }
            if ((type & PieceType.Black) == PieceType.Black)
            {
                return sprites[(int)(type ^ PieceType.Black) + 6];
            }
            throw new ArgumentException("Piece type must have a color");
        }
    }
}