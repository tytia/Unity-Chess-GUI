﻿/*
 * This class is responsible for managing the state of the game.
 *
 * Regarding draws, this implementation will use:
 * 1. The threefold repetition rule
 * 2. The 75-move rule (instant draw)
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utility;
using static Utility.Notation;

namespace Chess {
    public enum Side : byte {
        White,
        Black
    }

    [Flags]
    public enum CastlingRights : byte {
        None = 0,
        WhiteKingSide = 1,
        WhiteQueenSide = 2,
        BlackKingSide = 4,
        BlackQueenSide = 8,
        All = WhiteKingSide | WhiteQueenSide | BlackKingSide | BlackQueenSide
    }
    
    public class Game {
        private static Game _instance;
        private readonly Piece?[] _board = new Piece?[64];
        private readonly List<Piece> _pieces = new(32);
        private Side _sideToMove;
        private CastlingRights _castlingRights;
        private int? _enPassantIndex;
        private int _halfMoveClock;
        private int _fullMoveClock;
        
        public ReadOnlyCollection<Piece> pieces => new(_pieces);

        private Game(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR") {
            LoadFromFEN(fen);
        }

        public static Game GetInstance() {
            return _instance ??= new Game();
        }
        
        public void MovePiece(ref Piece piece, int toIndex) {
            _board[piece.index] = null;
            piece.index = toIndex;
            _board[toIndex] = piece;
        }
        
        public void CapturePiece(Piece piece) {
            _pieces.Remove(piece);
            _board[piece.index] = null;
        }

        public void LoadFromFEN(in string fen) {
            // GUI input field will be validated before calling this method
            // game state defaults:
            _sideToMove = Side.White;
            _castlingRights = CastlingRights.All;
            _enPassantIndex = null;
            _halfMoveClock = 0;
            _fullMoveClock = 1;
            
            string[] fields = fen.Split(' ');

            Array.Clear(_board, 0, _board.Length);
            _pieces.Clear();
            string[] ranks = fields[0].Split('/');
            for (int i = (int)SquarePos.a8, j = 0; j < 8; i -= 8, j++) {
                for (int fileIndex = 0, file = 0; fileIndex < ranks[j].Length && file < 8; fileIndex++) {
                    char c = ranks[j][fileIndex];
                    if (Char.IsDigit(c)) {
                        file += c - '0';
                    }
                    else {
                        _pieces.Add(new Piece(Notation.charToPieceType[c], i + file));
                        _board[i + file] = pieces.Last();
                        file++;
                    }
                }
            }

            if (fields.Length < 2) return;
            _sideToMove = fields[1] == "w" ? Side.White : Side.Black;
            
            if (fields.Length < 3) return;
            _castlingRights = CastlingRights.None;
            if (fields[2] != "-") {
                foreach (char c in fields[2]) {
                    _castlingRights |= Notation.charToCastlingRights[c];
                }
            }
            
            if (fields.Length < 4) return;
            _enPassantIndex = fields[3] == "-" ? null : (int)Enum.Parse<SquarePos>(fields[3]);
            
            if (fields.Length < 5) return;
            _halfMoveClock = Int32.Parse(fields[4]);
            
            _fullMoveClock = fields.Length < 6 ? (_halfMoveClock / 2) + 1 : Int32.Parse(fields[5]);
        }
    }
}