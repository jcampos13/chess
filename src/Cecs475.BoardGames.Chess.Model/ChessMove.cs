﻿using System;
using Cecs475.BoardGames.Model;

namespace Cecs475.BoardGames.Chess.Model {
	/// <summary>
	/// Represents a single move to be applied to a chess board.
	/// </summary>
	public class ChessMove : IGameMove, IEquatable<ChessMove> {
		// You can add additional fields, properties, and methods as you find
		// them necessary, but you cannot MODIFY any of the existing implementations.

		/// <summary>
		/// The starting position of the move.
		/// </summary>
		public BoardPosition StartPosition { get; }

		public ChessPieceType chessPiece { get; }

		/// <summary>
		/// The ending position of the move.
		/// </summary>
		public BoardPosition EndPosition { get; }

		/// <summary>
		/// The type of move being applied.
		/// </summary>
		public ChessMoveType MoveType { get; }

		public ChessPieceType PromoteTo { get; }

		public ChessPiece capturedPiece { get; set; }

		// You must set this property when applying a move.
		public int Player { get; set; }

		/// <summary>
		/// Constructs a ChessMove that moves a piece from one position to another
		/// </summary>
		/// <param name="start">the starting position of the piece to move</param>
		/// <param name="end">the position where the piece will end up</param>
		/// <param name="moveType">the type of move represented</param>
		public ChessMove(BoardPosition start, BoardPosition end, ChessMoveType moveType = ChessMoveType.Normal) {
			StartPosition = start;
			EndPosition = end;
			MoveType = moveType;
		}

		public ChessMove(BoardPosition start, BoardPosition end, ChessPieceType promTo)
		{
			StartPosition = start;
			EndPosition = end;
			PromoteTo = promTo;
			MoveType = ChessMoveType.PawnPromote;
			chessPiece = promTo;
		}

		// TODO: You must write this method.
		public virtual bool Equals(ChessMove other)
		{
			// Most chess moves are equal to each other if they have the same start and end position.
			// PawnPromote moves must also be promoting to the same piece type.
			if (other.MoveType == ChessMoveType.PawnPromote && other.PromoteTo == ChessPieceType.Queen)
			{
				return this.StartPosition == other.StartPosition && this.EndPosition == (other.EndPosition) && this.PromoteTo ==
					other.PromoteTo && this.MoveType == other.MoveType;
			}
			else if (other.MoveType == ChessMoveType.PawnPromote && other.PromoteTo == ChessPieceType.Knight)
			{
				return this.StartPosition == other.StartPosition && this.EndPosition == (other.EndPosition) && this.PromoteTo ==
					other.PromoteTo && this.MoveType == (other.MoveType);
			}
			else if (other.MoveType == ChessMoveType.PawnPromote && other.PromoteTo == ChessPieceType.Bishop)
			{
				return this.StartPosition == other.StartPosition && this.EndPosition == (other.EndPosition) && this.PromoteTo ==
					other.PromoteTo && this.MoveType == (other.MoveType);
			}
			else if (other.MoveType == ChessMoveType.PawnPromote && other.PromoteTo == ChessPieceType.Rook)
			{
				return this.StartPosition == other.StartPosition && this.EndPosition == (other.EndPosition) && this.PromoteTo ==
					other.PromoteTo && this.MoveType == (other.MoveType);
			}

			else
			{
				return this.StartPosition.Equals(other.StartPosition) && this.EndPosition.Equals(other.EndPosition);
			}
		}

		// Equality methods.
		bool IEquatable<IGameMove>.Equals(IGameMove other) {
			ChessMove m = other as ChessMove;
			return this.Equals(m);
		}

		public override bool Equals(object other) {
			return Equals(other as ChessMove);
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = StartPosition.GetHashCode();
				hashCode = (hashCode * 397) ^ EndPosition.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)MoveType;
				return hashCode;
			}
		}

		public override string ToString() {
			return $"{StartPosition} to {EndPosition}";
		}
	}
}
