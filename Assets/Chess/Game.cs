/*
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
        private int _halfMoveClock;
        private int _fullMoveClock;
        public PieceColor playerColor { get; private set; }
        public PieceColor colorToMove { get; private set; }
        public CastlingRights castlingRights { get; private set; }
        public int? enPassantIndex { get; private set; }
        public bool analysisMode { get; set; } = false;
        
        public ReadOnlyCollection<Piece?> board => new(_board);
        public ReadOnlyCollection<Piece> pieces => new(_pieces);

        private Game(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR") {
            LoadFromFEN(fen);
        }

        public static Game GetInstance() {
            return _instance ??= new Game();
        }
        
        public void MovePiece(ref Piece piece, int toIndex) {
            _board[piece.index] = null;
            
            if (_board[toIndex] != null) {
                _pieces.Remove(_board[toIndex].Value);
            }
            
            piece.index = toIndex;
            _board[toIndex] = piece;
            
            IncrementState(piece.type);
            CheckState();
        }

        private void IncrementState(PieceType pieceType) {
            if (pieceType == PieceType.Pawn) {
                _halfMoveClock = 0;
            }
            else {
                _halfMoveClock++;
            }
            
            if (colorToMove == PieceColor.Black) {
                _fullMoveClock++;
            }
            
            colorToMove = ~colorToMove;
        }

        private void CheckState() {
            // TODO: Implement win/lose/draw logic
        }

        public void LoadFromFEN(in string fen) {
            // GUI input field will be validated before calling this method
            // game state defaults:
            colorToMove = PieceColor.White;
            castlingRights = CastlingRights.All;
            enPassantIndex = null;
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
                        _pieces.Add(new Piece(Notation.charToPieceInfo[c], i + file));
                        _board[i + file] = pieces.Last();
                        file++;
                    }
                }
            }

            if (fields.Length < 2) return;
            colorToMove = playerColor = fields[1] == "w" ? PieceColor.White : PieceColor.Black;
            
            if (fields.Length < 3) return;
            castlingRights = CastlingRights.None;
            if (fields[2] != "-") {
                foreach (char c in fields[2]) {
                    castlingRights |= Notation.charToCastlingRights[c];
                }
            }
            
            if (fields.Length < 4) return;
            enPassantIndex = fields[3] == "-" ? null : (int)Enum.Parse<SquarePos>(fields[3]);
            
            if (fields.Length < 5) return;
            _halfMoveClock = Int32.Parse(fields[4]);
            
            _fullMoveClock = fields.Length < 6 ? (_halfMoveClock / 2) + 1 : Int32.Parse(fields[5]);
        }
    }
}