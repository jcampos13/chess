using System;
using System.Collections.Generic;
using System.Text;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.View;

namespace Cecs475.BoardGames.Chess.View {
	/// <summary>
	/// A chess game view for string-based console input and output.
	/// </summary>
	public class ChessConsoleView : IConsoleView {
		private static char[] LABELS = { '.', 'P', 'R', 'N', 'B', 'Q', 'K' };
		
		// Public methods.
		public string BoardToString(ChessBoard board) {
			StringBuilder str = new StringBuilder();

			for (int i = 0; i < ChessBoard.BoardSize; i++) {
				str.Append(8 - i);
				str.Append(" ");
				for (int j = 0; j < ChessBoard.BoardSize; j++) {
					var space = board.GetPieceAtPosition(new BoardPosition(i, j));
					if (space.PieceType == ChessPieceType.Empty)
						str.Append(". ");
					else if (space.Player == 1)
						str.Append($"{LABELS[(int)space.PieceType]} ");
					else
						str.Append($"{char.ToLower(LABELS[(int)space.PieceType])} ");
				}
				str.AppendLine();
			}
			str.AppendLine("  a b c d e f g h");
			return str.ToString();
		}

		/// <summary>
		/// Converts the given ChessMove to a string representation in the form
		/// "(start, end)", where start and end are board positions in algebraic
		/// notation (e.g., "a5").
		/// 
		/// If this move is a pawn promotion move, the selected promotion piece 
		/// must also be in parentheses after the end position, as in 
		/// "(a7, a8, Queen)".
		/// </summary>
		public string MoveToString(ChessMove move) {
			var row_map = new Dictionary<char, char>();

			row_map.Add('0', '8');
			row_map.Add('1', '7');
			row_map.Add('2', '6');
			row_map.Add('3', '5');
			row_map.Add('4', '4');
			row_map.Add('5', '3');
			row_map.Add('6', '2');
			row_map.Add('7', '1');


			var col_map = new Dictionary<char, char>();

			col_map.Add('0', 'a');
			col_map.Add('1', 'b');
			col_map.Add('2', 'c');
			col_map.Add('3', 'd');
			col_map.Add('4', 'e');
			col_map.Add('5', 'f');
			col_map.Add('6', 'g');
			col_map.Add('7', 'h');


			String start_position = move.StartPosition.ToString();
			
			start_position = start_position.Replace(" ", "");
			start_position = start_position.Replace("(", "");
			start_position = start_position.Replace(")", "");
			start_position = start_position.Replace(",", "");

			char[] start_array = start_position.ToCharArray(0, start_position.Length);
			start_array[0] = row_map[start_array[0]];
			start_array[1] = col_map[start_array[1]];

			String start_moveString = start_array[1].ToString() + start_array[0].ToString();


			String end_position = move.EndPosition.ToString();
			end_position = end_position.Replace(" ", "");
			end_position = end_position.Replace("(", "");
			end_position = end_position.Replace(")", "");
			end_position = end_position.Replace(",", "");
			char[] end_array = end_position.ToCharArray(0, end_position.Length);
			end_array[0] = row_map[end_array[0]];
			end_array[1] = col_map[end_array[1]];

			String end_moveString = end_array[1].ToString() + end_array[0].ToString();
			ChessMoveType movetype = move.MoveType;
			//if (movetype == ChessMoveType.PawnPromote)
			//{
			//	return ("(" + start_moveString + "," + end_moveString + "," + move.ChessPieceType + ")");
			//}
			//else 
			if (movetype == ChessMoveType.Normal)
			{
				return ("(" + start_moveString + "," + end_moveString + ")");
			}
			if (movetype == ChessMoveType.PawnPromote)
			{
				return ("(" + start_moveString + "," + end_moveString + "," + move.chessPiece + ")");

			}
			else 
			{
				return ("(" + start_moveString + "," + end_moveString + "," + move.MoveType + ")");

			}

		}
	

		public string PlayerToString(int player) {
			return player == 1 ? "White" : "Black";
		}

		/// <summary>
		/// Converts a string representation of a move into a ChessMove object.
		/// Must work with any string representation created by MoveToString.
		/// </summary>

		//public ChessMove ParseMove(string moveText) {

		//	//string[] split = moveText.Trim(new char[] { '(', ')' }).Split(',');
		//	//ChessMove move;
		//	//var col_map = new Dictionary<char, char>();

		//	//col_map.Add('a', '0');
		//	//col_map.Add('b', '1');
		//	//col_map.Add('c', '2');
		//	//col_map.Add('d', '3');
		//	//col_map.Add('e', '4');
		//	//col_map.Add('f', '5');
		//	//col_map.Add('g', '6');
		//	//col_map.Add('h', '7');

		//	//var row_map = new Dictionary<char, char>();

		//	//row_map.Add('8', '0');
		//	//row_map.Add('7', '1');
		//	//row_map.Add('6', '2');
		//	//row_map.Add('5', '3');
		//	//row_map.Add('4', '4');
		//	//row_map.Add('3', '5');
		//	//row_map.Add('2', '6');
		//	//row_map.Add('1', '7');


		//	//BoardPosition starting = new BoardPosition(row_map[split[0][1]], col_map[split[0][0]]);
		//	//BoardPosition ending = new BoardPosition(row_map[split[1][1]], col_map[split[1][0]]);

		//	//if (split.GetLength(0)==3) //try with split.Length == 3 and see if that changes

		//	string parsed = string.Join("", moveText.Split(',', ' ', '(', ')'));
		//	if (parsed.Length > 4)


		public ChessMove ParseMove(string moveText)
		{
			string move = moveText.Replace("(", "").Replace(")", "").Replace(" ", "");
			string[] posList = move.Split(',');
			if (posList.Length == 2) //no pawn promotion
				return new ChessMove(ParsePosition(posList[0]), ParsePosition(posList[1]));
			else //pawn promotion
			{
				switch (posList[2].ToLower())
				{
					case "rook": return new ChessMove(ParsePosition(posList[0]), ParsePosition(posList[1]), ChessPieceType.Rook);
					case "knight": return new ChessMove(ParsePosition(posList[0]), ParsePosition(posList[1]), ChessPieceType.Knight);
					case "bishop": return new ChessMove(ParsePosition(posList[0]), ParsePosition(posList[1]), ChessPieceType.Bishop);
					case "queen": return new ChessMove(ParsePosition(posList[0]), ParsePosition(posList[1]), ChessPieceType.Queen);
					default: throw new Exception("ParseMove is wrong!");
				}
			}
		}

		public static BoardPosition ParsePosition(string pos) {
			return new BoardPosition(8 - (pos[1] - '0'), pos[0] - 'a');
		 }
 
		public static string PositionToString(BoardPosition pos) {
			return $"{(char)(pos.Col + 'a')}{8 - pos.Row}";
		}

		#region Explicit interface implementations
		// Explicit method implementations. Do not modify these.
		string IConsoleView.BoardToString(IGameBoard board) {
			return BoardToString(board as ChessBoard);
		}

		string IConsoleView.MoveToString(IGameMove move) {
			return MoveToString(move as ChessMove);
		}

		IGameMove IConsoleView.ParseMove(string moveText) {
			return ParseMove(moveText);
		}
		#endregion
	}
}
