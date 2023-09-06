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
        public record GameState {
            public Piece?[] board { get; } = new Piece?[64];
            public List<Piece> pieces { get; }
            public PieceColor playerColor { get; }
            public PieceColor colorToMove { get; }
            public CastlingRights castlingRights { get; }
            public int? enPassantIndex { get; }
            public int halfmoveClock { get; }
            public int fullmoveNumber { get; }
            public Move? prevMove { get; }

            public GameState(Game game) {
                Array.Copy(game.board, board, 64);
                pieces = new List<Piece>(game.pieces);
                playerColor = game.playerColor;
                colorToMove = game.colorToMove;
                castlingRights = game.castlingRights;
                enPassantIndex = game.enPassantIndex;
                halfmoveClock = game._halfmoveClock;
                fullmoveNumber = game._fullmoveNumber;
                prevMove = game.prevMove;
            }
        }
        
        private static Game _instance;
        private int _halfmoveClock;
        private int _fullmoveNumber;
        private readonly List<GameState> _history = new();
        public Piece?[] board { get; } = new Piece?[64];
        public List<Piece> pieces { get; private set; } = new(32);
        public PieceColor playerColor { get; private set; }
        public PieceColor colorToMove { get; private set; }
        public CastlingRights castlingRights { get; private set; }
        public int? enPassantIndex { get; private set; }
        public Move? prevMove { get; set; }
        public bool analysisMode { get; set; }
        public int historyIndex { get; set; } = -1;
        public ReadOnlyCollection<GameState> history => _history.AsReadOnly();


        private Game() {
            StartNewGame();
        }

        public static Game GetInstance() {
            return _instance ??= new Game();
        }
        
        public void StartNewGame(string fen = StartingFEN) {
            LoadFromFEN(fen);
            _history.Clear();
            prevMove = null;
            analysisMode = true; // TODO: temporarily turned on for development, change this later
            historyIndex = -1;
            RecordState();
        }

        public void IncrementTurn() {
            if (prevMove == null) {
                throw new InvalidOperationException("IncrementTurn() called before first move was made");
            }

            if (prevMove.Value.piece.type == PieceType.Pawn) {
                _halfmoveClock = 0;
            }
            else {
                _halfmoveClock++;
            }

            if (colorToMove == PieceColor.White) {
                colorToMove = PieceColor.Black;
            }
            else {
                colorToMove = PieceColor.White;
                _fullmoveNumber++;
            }
            
            UpdateCastlingRights();
            UpdateEnPassantIndex();
            RecordState();
        }

        public void CheckState() {
            // TODO: Implement win/lose/draw logic
        }
        
        private void RecordState() {
            if (historyIndex < _history.Count - 1) {
                _history.RemoveRange(historyIndex + 1, _history.Count - (historyIndex + 1));
            }
            
            _history.Add(new GameState(this));
            historyIndex++;
        }

        public void ApplyState(GameState state) {
            Array.Copy(state.board, board, 64);
            pieces = new List<Piece>(state.pieces);
            playerColor = state.playerColor;
            colorToMove = state.colorToMove;
            castlingRights = state.castlingRights;
            enPassantIndex = state.enPassantIndex;
            _halfmoveClock = state.halfmoveClock;
            _fullmoveNumber = state.fullmoveNumber;
            prevMove = state.prevMove;
        }

        private void UpdateCastlingRights() {
            if (prevMove!.Value.piece.type == PieceType.King) {
                castlingRights &= prevMove!.Value.piece.color == PieceColor.White
                    ? ~CastlingRights.WhiteKingSide & ~CastlingRights.WhiteQueenSide
                    : ~CastlingRights.BlackKingSide & ~CastlingRights.BlackQueenSide;
            }
            else if (board[0] == null || board[0].Value.color != PieceColor.White) {
                castlingRights &= ~CastlingRights.WhiteQueenSide;
            }
            else if (board[7] == null || board[7].Value.color != PieceColor.White) {
                castlingRights &= ~CastlingRights.WhiteKingSide;
            }
            else if (board[56] == null || board[56].Value.color != PieceColor.Black) {
                castlingRights &= ~CastlingRights.BlackQueenSide;
            }
            else if (board[63] == null || board[63].Value.color != PieceColor.Black) {
                castlingRights &= ~CastlingRights.BlackKingSide;
            }
        }
        
        private void UpdateEnPassantIndex() {
            Piece prevPiece = prevMove!.Value.piece;
            int toIndex = prevMove!.Value.to;
            if (prevPiece.type == PieceType.Pawn && Math.Abs(prevPiece.index - toIndex) == 16) {
                enPassantIndex = prevPiece.index + (toIndex - prevPiece.index) / 2;
            }
            else {
                enPassantIndex = null;
            }
        }

        private void LoadFromFEN(in string fen) {
            // game state defaults:
            colorToMove = PieceColor.White;
            castlingRights = CastlingRights.All;
            enPassantIndex = null;
            _halfmoveClock = 0;
            _fullmoveNumber = 1;

            string[] fields = fen.Split(' ');

            Array.Clear(board, 0, board.Length);
            pieces.Clear();
            string[] ranks = fields[0].Split('/');
            for (int i = (int)SquarePos.a8, j = 0; j < 8; i -= 8, j++) {
                for (int fileIndex = 0, file = 0; fileIndex < ranks[j].Length && file < 8; fileIndex++) {
                    char c = ranks[j][fileIndex];
                    if (Char.IsDigit(c)) {
                        file += c - '0';
                    }
                    else {
                        var (pieceType, pieceColor) = charToPieceInfo[c];
                        pieces.Add(new Piece(pieceType, pieceColor, i + file));
                        board[i + file] = pieces.Last();
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
                    castlingRights |= charToCastlingRights[c];
                }
            }

            if (fields.Length < 4) return;
            enPassantIndex = fields[3] == "-" ? null : (int)Enum.Parse<SquarePos>(fields[3]);

            if (fields.Length < 5) return;
            _halfmoveClock = Int32.Parse(fields[4]);

            _fullmoveNumber = fields.Length < 6 ? (_halfmoveClock / 2) + 1 : Int32.Parse(fields[5]);
        }
    }
}