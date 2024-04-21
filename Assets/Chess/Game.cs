/*
 * This class is responsible for managing the state of the game.
 *
 * Regarding draws, this implementation will use:
 * 1. The threefold repetition rule
 * 2. The 75-move rule (instant draw)
 */

using System;
using System.Collections.ObjectModel;
using System.Linq;
using GUI.GameWindow;
using GUI.GameWindow.Popups;
using UnityEngine.Assertions;
using static Utility.Notation;
using static UnityEngine.Object;

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
    
    public enum EndState : byte {
        Ongoing,
        Checkmate,
        Stalemate,
        Draw
    }

    public class Game {
        private static Game _instance;
        internal Piece?[] _board = new Piece?[64];
        public ReadOnlyCollection<Piece?> board => Array.AsReadOnly(_board);
        public PieceColor playerColor { get; internal set; }
        public PieceColor colorToMove { get; internal set; }
        public CastlingRights castlingRights { get; internal set; }
        public int? enPassantIndex { get; internal set; }
        public int halfmoveClock { get; internal set; }
        public int fullmoveNumber { get; internal set; }
        public Move? prevMove { get; internal set; }
        public bool inCheck { get; internal set; }
        public EndState endState { get; internal set; }
        public bool analysisMode { get; set; }
        public StateManager stateManager => StateManager.instance;
        public static Game instance => _instance ??= new Game();

        public static event EventHandler MoveEnd;

        private Game() {
            StartNewGame();
        }
        
        internal void OnMoveEnd() {
            IncrementTurn();
            MoveGenerator.UpdateData();
            UpdateEndState();
            MoveEnd?.Invoke(typeof(Game), EventArgs.Empty);
        }
        
        public void StartNewGame(string fen = StartingFEN) {
            LoadFromFEN(fen);
            prevMove = null;
            analysisMode = true; // TODO: temporarily turned on for development, change this later
            stateManager.Reset();
            if (_instance != null) MoveGenerator.Reset();
        }
        
        public void StartNewGame(PieceColor plyrColor) {
            StartNewGame();
            playerColor = plyrColor;
        }

        private void IncrementTurn() {
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

        private void UpdateEndState() {
            endState = EndState.Ongoing;
            if (inCheck && MoveGenerator.legalMoves.Count == 0) {
                endState = EndState.Checkmate;
            }
            else if (MoveGenerator.legalMoves.Count == 0) {
                endState = EndState.Stalemate;
            }
            else if (fullmoveNumber >= 50) {
                endState = EndState.Draw;
            }
            else if (ThreefoldRepetition()) {
                endState = EndState.Draw;
            }

            return;
            
            bool ThreefoldRepetition() {
                var history = stateManager.allStates;
                return history.Count >= 9 && board.SequenceEqual(history[^5].board) &&
                       board.SequenceEqual(history[^9].board);
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
        
        public void MovePiece(int from, int to) {
            stateManager.RecordState();
            var move = new Move(from, to);

            prevMove = move;

            _board[to] = _board[from];
            _board[from] = null;

            if (MoveWasCastle()) {
                CastleRookMove(move.from);
            }
            else if (MoveWasEnPassant()) {
                EnPassant(move.from);
            }
            else if (MoveWasPromotion()) {
                // because PromotePawn() does not modify the argument pawn, but instead directly changes the type
                // of the pawn on the board, we need to call it after the move is made.
                PopupManager.ShowPawnPromotionPopup(to);
                return; // PromotePawn() will conclude the move
            }

            OnMoveEnd();
            return;

            void CastleRookMove(int kingIndex) {
                int rookTo = kingIndex + (to - kingIndex) / 2;
                int rookFrom = MoveGenerator.CastleTargetRookPos(kingIndex, to);
                PieceGUI rookGUI = Board.GetPieceGUI(rookFrom)!;

                _board[rookTo] = _board[rookFrom];
                _board[rookFrom] = null;

                rookGUI.transform.parent = Board.GetSquare(rookTo).transform;
                rookGUI.transform.position = rookGUI.transform.parent.position;
            }

            void EnPassant(int pawnIndex) {
                int captureIndex = colorToMove == PieceColor.White ? to - 8 : to + 8;

                _board[captureIndex] = null;
                Destroy(Board.GetPieceGUI(captureIndex).gameObject);
            }
        }

        // public void MovePiece(int from, int to) {
        //     if (board[from] == null) {
        //         throw new ArgumentException("No piece at index " + from);
        //     }
        //
        //     MovePiece(board[from].Value, to);
        // }

        public void PromotePawn(int pawnIndex, PieceType type) {
            if (stateManager.last is not null) {
                Assert.AreNotEqual(_board, stateManager.last.board,
                    "Move is already finished; PromotePawn() should be called to conclude the move.");
            }
            
            Piece pawn = _board[pawnIndex]!.Value;
            if (pawn.type != PieceType.Pawn) {
                throw new ArgumentException("Piece must be a pawn");
            }

            if (pawnIndex is > 7 and < 56) {
                throw new ArgumentException("Pawn must be on the last rank");
            }

            Piece promoted = new(type, pawn.color);
            _board[pawnIndex] = promoted;

            OnMoveEnd();
        }

        public bool MoveWasCapture() {
            if (prevMove == null) {
                throw new InvalidOperationException("MoveWasCapture() called before any moves were made");
            }

            Move move = prevMove.Value;
            return stateManager.last.board[move.to] != null;
        }

        public bool MoveWasCastle() {
            if (prevMove == null) {
                throw new InvalidOperationException("MoveWasCastle() called before any moves were made");
            }

            Move move = prevMove.Value;
            return move.piece.type == PieceType.King && Math.Abs(move.from - move.to) == 2;
        }

        public bool MoveWasEnPassant() {
            if (prevMove == null) {
                throw new InvalidOperationException("MoveWasEnPassant() called before any moves were made");
            }

            Move move = prevMove.Value;
            return move.piece.type == PieceType.Pawn && move.to == enPassantIndex;
        }

        public bool MoveWasPromotion() {
            if (prevMove == null) {
                throw new InvalidOperationException("MoveWasPromotion() called before any moves were made");
            }

            Move move = prevMove.Value;
            return move.piece.type == PieceType.Pawn && (move.to is < 8 or > 55);
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
}