﻿/*
 * This class is responsible for managing the state of the game.
 *
 * Regarding draws, this implementation will use:
 * 1. The threefold repetition rule
 * 2. The 75-move rule (instant draw)
 */

using System;
using System.Collections.ObjectModel;
using System.Linq;
using static Chess.Notation;

namespace Chess {
    /// <summary>
    /// Describes the remaining castling rights of a game. Uses the flags attribute to easily
    /// check for the presence of specific rights.
    /// </summary>
    [Flags]
    public enum CastlingRights : byte {
        None = 0,
        WhiteKingSide = 1,
        WhiteQueenSide = 2,
        BlackKingSide = 4,
        BlackQueenSide = 8,
        All = WhiteKingSide | WhiteQueenSide | BlackKingSide | BlackQueenSide
    }
    
    /// <summary>
    /// Describes the possible end states of a game.
    /// </summary>
    public enum EndResult : byte {
        Checkmate,
        DrawByStalemate,
        DrawByRepitition,
        DrawByInsufficientMaterial,
        DrawBy75MoveRule
    }
    
    /// <summary>
    /// Represents the current state of a chess game.
    /// </summary>
    public class Game {
        private static Game _instance;
        internal Piece?[] _board = new Piece?[64];
        public ReadOnlyCollection<Piece?> board => Array.AsReadOnly(_board);
        private readonly bool[] _insufficientMaterial = new bool[2];
        public ReadOnlyCollection<bool> insufficientMaterial => Array.AsReadOnly(_insufficientMaterial);
        public PieceColor playerColor { get; internal set; }
        public PieceColor colorToMove { get; internal set; }
        public CastlingRights castlingRights { get; internal set; }
        public int? enPassantIndex { get; internal set; }
        public int halfmoveClock { get; internal set; }
        public int fullmoveNumber { get; internal set; }
        public Move? prevMove { get; internal set; }
        public bool inCheck { get; internal set; }
        public bool analysisMode { get; set; }
        public StateManager stateManager => StateManager.instance;
        public static Game instance => _instance ??= new Game();
        
        public event EventHandler<GameEndEventArgs> GameEnd;

        private Game() {
            StartNewGame();
        }
        
        public void StartNewGame(string fen = StartingFEN) {
            LoadFromFEN(fen);
            prevMove = null;
            analysisMode = true; // TODO: temporarily turned on for development, change this later
            if (_instance != null) {
                stateManager.Reset();
                MoveGenerator.Reset();
            }
        }
        
        public void StartNewGame(PieceColor plyrColor) {
            StartNewGame();
            playerColor = plyrColor;
        }

        internal void IncrementTurn() {
            if (prevMove == null) {
                throw new InvalidOperationException("A move must be made before a turn is incremented");
            }

            if (prevMove.Value.piece.type == PieceType.Pawn) {
                halfmoveClock = 0;
            }
            else {
                halfmoveClock++;
            }

            if (colorToMove == PieceColor.White) {
                colorToMove = PieceColor.Black;
            }
            else {
                colorToMove = PieceColor.White;
                fullmoveNumber++;
            }
            
            UpdateCastlingRights();
            UpdateEnPassantIndex();
        }

        internal void CheckGameEnd() {
            if (inCheck && MoveGenerator.legalMoves.Count == 0) {
                GameEnd?.Invoke(this, new GameEndEventArgs(EndResult.Checkmate));
            }
            else if (MoveGenerator.legalMoves.Count == 0) {
                GameEnd?.Invoke(this, new GameEndEventArgs(EndResult.DrawByStalemate));
            }
            else if (ThreefoldRepetition()) {
                GameEnd?.Invoke(this, new GameEndEventArgs(EndResult.DrawByRepitition));
            }
            else if (InsufficientMaterial()) {
                GameEnd?.Invoke(this, new GameEndEventArgs(EndResult.DrawByInsufficientMaterial));
            }
            else if (fullmoveNumber >= 75) {
                GameEnd?.Invoke(this, new GameEndEventArgs(EndResult.DrawBy75MoveRule));
            }

            return;
            
            bool ThreefoldRepetition() {
                var history = stateManager.allStates;
                return history.Count >= 8 && board.SequenceEqual(history[^4].board) &&
                       board.SequenceEqual(history[^8].board);
            }
            
            bool InsufficientMaterial() {
                // index 0 is white, index 1 is black
                int[] bishops = { 0, 0 };
                int[] knights = { 0, 0 };

                foreach (var piece in _board) {
                    if (piece == null) continue;
                
                    if (piece.Value.type == PieceType.Bishop) {
                        if (++bishops[(int)piece.Value.color] == 2) {
                            return false;
                        }
                    }
                    else if (piece.Value.type == PieceType.Knight) {
                        knights[(int)piece.Value.color]++;
                    }
                    else if (piece.Value.type != PieceType.King) {
                        return false;
                    }
                }

                _insufficientMaterial[0] = knights[0] == 1 && bishops[0] == 0 || knights[0] == 0 && bishops[0] == 1;
                _insufficientMaterial[1] = knights[1] == 1 && bishops[1] == 0 || knights[1] == 0 && bishops[1] == 1;
            
                return _insufficientMaterial[0] && _insufficientMaterial[1] 
                       || // two knights vs lone king
                       (knights[0] == 2 && bishops[0] == 0 && knights[1] == 0 && bishops[1] == 0) ||
                       (knights[1] == 2 && bishops[1] == 0 && knights[0] == 0 && bishops[0] == 0);
            }
        }

        private void UpdateCastlingRights() {
            if (prevMove!.Value.piece.type == PieceType.King) {
                castlingRights &= prevMove!.Value.piece.color == PieceColor.White
                    ? ~CastlingRights.WhiteKingSide & ~CastlingRights.WhiteQueenSide
                    : ~CastlingRights.BlackKingSide & ~CastlingRights.BlackQueenSide;
            }
            
            if (board[0] == null || board[0].Value.color != PieceColor.White) {
                castlingRights &= ~CastlingRights.WhiteQueenSide;
            }
            
            if (board[7] == null || board[7].Value.color != PieceColor.White) {
                castlingRights &= ~CastlingRights.WhiteKingSide;
            }
            
            if (board[56] == null || board[56].Value.color != PieceColor.Black) {
                castlingRights &= ~CastlingRights.BlackQueenSide;
            }
            
            if (board[63] == null || board[63].Value.color != PieceColor.Black) {
                castlingRights &= ~CastlingRights.BlackKingSide;
            }
        }
        
        private void UpdateEnPassantIndex() {
            int from = prevMove!.Value.from, to = prevMove!.Value.to;
            Piece prevPiece = prevMove!.Value.piece;
            if (prevPiece.type == PieceType.Pawn && Math.Abs(from - to) == 16) {
                enPassantIndex = from + (to - from) / 2;
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
            halfmoveClock = 0;
            fullmoveNumber = 1;

            string[] fields = fen.Split(' ');

            Array.Clear(_board, 0, _board.Length);
            string[] ranks = fields[0].Split('/');
            for (int i = (int)SquarePos.a8, j = 0; j < 8; i -= 8, j++) {
                for (int fileIndex = 0, file = 0; fileIndex < ranks[j].Length && file < 8; fileIndex++) {
                    char c = ranks[j][fileIndex];
                    if (Char.IsDigit(c)) {
                        file += c - '0';
                    }
                    else {
                        var (pieceType, pieceColor) = charToPieceInfo[c];
                        var piece = new Piece(pieceType, pieceColor);
                        _board[i + file] = piece;
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
            halfmoveClock = Int32.Parse(fields[4]);

            fullmoveNumber = fields.Length < 6 ? (halfmoveClock / 2) + 1 : Int32.Parse(fields[5]);
        }
    }
    
    public class GameEndEventArgs : EventArgs {
        public EndResult result { get; }
        public GameEndEventArgs(EndResult result) {
            this.result = result;
        }
    }
}