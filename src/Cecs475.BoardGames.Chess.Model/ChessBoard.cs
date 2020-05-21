using System;
using System.Collections.Generic;
using System.Text;
using Cecs475.BoardGames.Model;
using System.Linq;

namespace Cecs475.BoardGames.Chess.Model
{
	/// <summary>
	/// Represents the board state of a game of chess. Tracks which squares of the 8x8 board are occupied
	/// by which player's pieces.
	/// </summary>
	public class ChessBoard : IGameBoard
	{
		#region Member fields.
		// The history of moves applied to the board.
		private List<ChessMove> mMoveHistory = new List<ChessMove>();

		//contains information about the state of the board. This is going to be used along with mMoveHistory to undo a move.
		private List<GameState> stateOfBoard = new List<GameState>();

		public const int BoardSize = 8;
		public byte[] board;
		private int advValue = 0;
		private bool kingsideWhite = false;
		private bool queensideWhite = false;
		private bool kingsideBlack = false;
		private bool queensideBlack = false;

		// TODO: create a field for the board position array. You can hand-initialize
		// the starting entries of the array, or set them in the constructor.

		// TODO: Add a means of tracking miscellaneous board state, like captured pieces and the 50-move rule.
		private struct GameState
		{
			public ChessPiece pieceCaptured;
			public bool whiteCanCastle;
			public bool blackCanCastle;
			public ChessPiece pieceCapturing;
			public BoardPosition capturersStartingPos;
			public BoardPosition capturersEndingPos;
			//public int player { get; set; }

			public GameState(ChessPiece capturer, ChessPiece captured, BoardPosition start, BoardPosition end, bool black, bool white/*, int capturerPlayer*/)
			{
				pieceCapturing = capturer;
				pieceCaptured = captured;
				capturersStartingPos = start;
				capturersEndingPos = end;
				blackCanCastle = black;
				whiteCanCastle = white;
				//player = capturerPlayer;
			}
		}

		private List<int> gameDrawCounter = new List<int>();

		// TODO: add a field for tracking the current player and the board advantage.	

		private int player = 1;

		private bool CheckMate = false;

		private int drawCount = 0;

		#endregion

		#region Properties.
		// TODO: implement these properties.
		// You can choose to use auto properties, computed properties, or normal properties 
		// using a private field to back the property.

		// You can add set bodies if you think that is appropriate, as long as you justify
		// the access level (public, private).

		public bool IsFinished
		{
			get
			{
				if (IsCheckmate == true || DrawCounter==100 || IsStalemate==true)
				{
					return true;
				}
				else
					return false;
			}
		}

		public int CurrentPlayer
		{
			get
			{

				return player == 1 ? 1 : 2;

			}
		}

		public GameAdvantage CurrentAdvantage
		{
			get
			{
				if (updateAdvantage() < 0)
				{
					return new GameAdvantage(2, updateAdvantage() * -1);
				}
				else if (updateAdvantage() > 0)
				{
					return new GameAdvantage(1, updateAdvantage());
				}
				else
				{
					return new GameAdvantage(0, 0);
				}
			}
		}

        public IReadOnlyList<ChessMove> MoveHistory => mMoveHistory;

		// TODO: implement IsCheck, IsCheckmate, IsStalemate

		//DONE?
		public bool IsCheck
		{
			get
			{
				return PositionIsAttacked(GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer).First(), CurrentPlayer == 1 ? 2 : 1)
					&& GetPossibleMoves().Count() > 0;
			}

		}

		public bool IsCheckmate
		{
			get
			{
				return PositionIsAttacked(GetPositionsOfPiece(ChessPieceType.King, CurrentPlayer).First(), CurrentPlayer == 1 ? 2 : 1)
					&& GetPossibleMoves().Count() == 0;
			}
		}

		public bool IsStalemate
		{
			get
			{
				if (IsCheckmate == false && GetPossibleMoves().Count()==0) 
				{
					return true;
				}
				else
					return false;
			}

		}

		public bool IsDraw
		{
			get
			{
				if (DrawCounter == 100)
				{
					return true;
				}
				else
					return false;
			}
		}

		/// <summary>
		/// Tracks the current draw counter, which goes up by 1 for each non-capturing, non-pawn move, and resets to 0
		/// for other moves. If the counter reaches 100 (50 full turns), the game is a draw.
		/// </summary>
		public int DrawCounter
		{
			get
			{
				return drawCount;
			}
		}
		#endregion

		public int updateAdvantage()
		{

			int black = 0;
			int white = 0;
			for (int i = 0; i < 8; i++) 
			{
				for (int j = 0; j < 8; j++) 
				{
					ChessPiece chessT = GetPieceAtPosition(new BoardPosition(i, j));
					
					if(chessT.Player==1)
					{
						if(chessT.PieceType==ChessPieceType.Pawn)
						{
							white += 1;
						}
						if(chessT.PieceType==ChessPieceType.Knight||chessT.PieceType==ChessPieceType.Bishop)
						{
							white += 3;
						}
						if(chessT.PieceType==ChessPieceType.Rook)
						{
							white += 5;
						}
						if (chessT.PieceType == ChessPieceType.Queen)
						{
							white += 9;
						}
					}

					if(chessT.Player==2)
					{
						if (chessT.PieceType == ChessPieceType.Pawn)
						{
							black += 1;
						}
						if (chessT.PieceType == ChessPieceType.Knight || chessT.PieceType == ChessPieceType.Bishop)
						{
							black += 3;
						}
						if (chessT.PieceType == ChessPieceType.Rook)
						{
							black += 5;
						}
						if (chessT.PieceType == ChessPieceType.Queen)
						{
							black += 9;
						}
					}
				}
			}

			advValue = white - black;
			return advValue;
		}

		#region Public methods.
		public IEnumerable<ChessMove> GetPossibleMoves()
		{

			ISet<BoardPosition> attackPos = GetAttackedPositions(CurrentPlayer);
			ChessMove move;
			BoardPosition pos;
			List<ChessMove> allMoves = new List<ChessMove>();
			List<ChessMove> validMoves = new List<ChessMove>();

			for (int m = 0; m <= 8; m++)
			{
				for (int n = 0; n <= 8; n++)
				{
					pos = new BoardPosition(m, n);
					if (GetPieceAtPosition(new BoardPosition(m, n)).Player == CurrentPlayer)
					{

						BoardPosition posLU; bool LUCheck = false;
						BoardPosition posRU; bool RUCheck = false;
						BoardPosition posLD; bool LDCheck = false;
						BoardPosition posRD; bool RDCheck = false;
						BoardPosition posLefts; bool LeftCheck = false;
						BoardPosition posRights; bool RightCheck = false;
						BoardPosition posUps; bool UpCheck = false;
						BoardPosition posDowns; bool DownCheck = false;
						BoardPosition rightDiagonal;
						BoardPosition leftDiagonal;
						BoardPosition upOne;
						BoardPosition UpTwo;
						BoardPosition downOne;
						BoardPosition leftOne;
						BoardPosition rightOne;
						BoardPosition leftUp;
						BoardPosition rightUp;
						BoardPosition leftDown;
						BoardPosition rightDown;

						//QUEEN
						ChessPiece p = GetPieceAtPosition(pos);
						if (GetPieceAtPosition(pos).PieceType == ChessPieceType.Queen)
						{
							for (int i = 1; i <= board.Length; i++)
							{
								posLU = new BoardPosition(pos.Row - i, pos.Col - i);
								if (PositionInBounds(posLU) && LUCheck == false)
								{
									if ((attackPos.Contains(posLU) && PositionIsEmpty(posLU)) || (attackPos.Contains(posLU) && PositionIsEnemy(posLU, CurrentPlayer)))
									{
										if (PositionIsEnemy(posLU, CurrentPlayer) || !PositionIsEmpty(posLU))
										{
											LUCheck = true;
										}
										move = new ChessMove(pos, posLU, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posLU, CurrentPlayer))
									{
										LUCheck = true;
									}
								}
								posRU = new BoardPosition(pos.Row - i, pos.Col + i);
								if (PositionInBounds(posRU) && RUCheck == false)
								{
									if ((attackPos.Contains(posRU) && PositionIsEmpty(posRU)) || (attackPos.Contains(posRU) && PositionIsEnemy(posRU, CurrentPlayer)))
									{
										if (PositionIsEnemy(posRU, CurrentPlayer) || !PositionIsEmpty(posRU))
										{
											RUCheck = true;
										}
										move = new ChessMove(pos, posRU, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posRU, CurrentPlayer))
									{
										RUCheck = true;
									}
								}
								posLD = new BoardPosition(pos.Row + i, pos.Col - i);
								if (PositionInBounds(posLD) && LDCheck == false)
								{
									if ((attackPos.Contains(posLD) && PositionIsEmpty(posLD)) || (attackPos.Contains(posLD) && PositionIsEnemy(posLD, CurrentPlayer)))
									{
										if (PositionIsEnemy(posLD, CurrentPlayer) || !PositionIsEmpty(posLD))
										{
											LDCheck = true;
										}
										move = new ChessMove(pos, posLD, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posLD, CurrentPlayer))
									{
										LDCheck = true;
									}
								}
								posRD = new BoardPosition(pos.Row + i, pos.Col + i);
								if (PositionInBounds(posRD) && RDCheck == false)
								{
									if ((attackPos.Contains(posRD) && PositionIsEmpty(posRD)) || (attackPos.Contains(posRD) && PositionIsEnemy(posRD, CurrentPlayer)))
									{
										if (PositionIsEnemy(posRD, CurrentPlayer) || !PositionIsEmpty(posRD))
										{
											RDCheck = true;
										}
										move = new ChessMove(pos, posRD, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posRD, CurrentPlayer))
									{
										RDCheck = true;
									}
								}
								posRights = new BoardPosition(pos.Row, pos.Col + i);
								if (PositionInBounds(posRights) && RightCheck == false)
								{
									if ((attackPos.Contains(posRights) && PositionIsEmpty(posRights)) || (attackPos.Contains(posRights) && PositionIsEnemy(posRights, CurrentPlayer)))
									{
										if (PositionIsEnemy(posRights, CurrentPlayer) || !PositionIsEmpty(posRights))
										{
											RightCheck = true;
										}
										move = new ChessMove(pos, posRights, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posRights, CurrentPlayer))
									{
										RightCheck = true;
									}
								}
								posLefts = new BoardPosition(pos.Row, pos.Col - i);
								if (PositionInBounds(posLefts) && LeftCheck == false)
								{
									if ((attackPos.Contains(posLefts) && PositionIsEmpty(posLefts)) || (attackPos.Contains(posLefts) && PositionIsEnemy(posLefts, CurrentPlayer)))
									{
										if (PositionIsEnemy(posLefts, CurrentPlayer) || !PositionIsEmpty(posLefts))
										{
											LeftCheck = true;
										}
										move = new ChessMove(pos, posLefts, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posLefts, CurrentPlayer))
									{
										LeftCheck = true;
									}
								}
								posUps = new BoardPosition(pos.Row - i, pos.Col);
								if (PositionInBounds(posUps) && UpCheck == false)
								{
									if ((attackPos.Contains(posUps) && PositionIsEmpty(posUps)) || (attackPos.Contains(posUps) && PositionIsEnemy(posUps, CurrentPlayer)))
									{
										if (PositionIsEnemy(posUps, CurrentPlayer) || !PositionIsEmpty(posUps))
										{
											UpCheck = true;
										}
										move = new ChessMove(pos, posUps, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posUps, CurrentPlayer))
									{
										UpCheck = true;
									}
								}
								posDowns = new BoardPosition(pos.Row + i, pos.Col);
								if (PositionInBounds(posDowns) && DownCheck == false)
								{
									if ((attackPos.Contains(posDowns) && PositionIsEmpty(posDowns)) || (attackPos.Contains(posDowns) && PositionIsEnemy(posDowns, CurrentPlayer)))
									{
										if (PositionIsEnemy(posDowns, CurrentPlayer) || !PositionIsEmpty(posDowns))
										{
											DownCheck = true;
										}
										move = new ChessMove(pos, posDowns, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posDowns, CurrentPlayer))
									{
										DownCheck = true;
									}
								}
							}
						}

						//KNIGHT
						if (GetPieceAtPosition(pos).PieceType == ChessPieceType.Knight)
						{
							//two lower left diagonals
							BoardPosition posLD1;
							BoardPosition posLD2;
							//two upper left diagonals
							BoardPosition posLU1;
							BoardPosition posLU2;
							//two lower right diagonals
							BoardPosition posRD1;
							BoardPosition posRD2;
							//two upper right diagonals
							BoardPosition posRU1;
							BoardPosition posRU2;

							posLD1 = new BoardPosition(pos.Row - 1, pos.Col - 2);
							posLD2 = new BoardPosition(pos.Row - 2, pos.Col - 1);
							posLU1 = new BoardPosition(pos.Row + 1, pos.Col - 2);
							posLU2 = new BoardPosition(pos.Row + 2, pos.Col - 1);
							posRD1 = new BoardPosition(pos.Row - 1, pos.Col + 2);
							posRD2 = new BoardPosition(pos.Row - 2, pos.Col + 1);
							posRU1 = new BoardPosition(pos.Row + 1, pos.Col + 2);
							posRU2 = new BoardPosition(pos.Row + 2, pos.Col + 1);

							if (PositionInBounds(posLD1))
							{
								if ((attackPos.Contains(posLD1) && PositionIsEmpty(posLD1)) || (attackPos.Contains(posLD1) && PositionIsEnemy(posLD1, CurrentPlayer)))
								{
									move = new ChessMove(pos, posLD1, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}
							if (PositionInBounds(posLD2))
							{
								if ((attackPos.Contains(posLD2) && PositionIsEmpty(posLD2)) || (attackPos.Contains(posLD2) && PositionIsEnemy(posLD2, CurrentPlayer)))
								{
									move = new ChessMove(pos, posLD2, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}
							if (PositionInBounds(posLU1))
							{
								if ((attackPos.Contains(posLU1) && PositionIsEmpty(posLU1)) || (attackPos.Contains(posLU1) && PositionIsEnemy(posLU1, CurrentPlayer)))
								{
									move = new ChessMove(pos, posLU1, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}
							if (PositionInBounds(posLU2))
							{
								if ((attackPos.Contains(posLU2) && PositionIsEmpty(posLU2)) || (attackPos.Contains(posLU2) && PositionIsEnemy(posLU2, CurrentPlayer)))
								{
									move = new ChessMove(pos, posLU2, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}
							if (PositionInBounds(posRD1))
							{
								if ((attackPos.Contains(posRD1) && PositionIsEmpty(posRD1)) || (attackPos.Contains(posRD1) && PositionIsEnemy(posRD1, CurrentPlayer)))
								{
									move = new ChessMove(pos, posRD1, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}
							if (PositionInBounds(posRD2))
							{
								if ((attackPos.Contains(posRD2) && PositionIsEmpty(posRD2)) || (attackPos.Contains(posRD2) && PositionIsEnemy(posRD2, CurrentPlayer)))
								{
									move = new ChessMove(pos, posRD2, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}
							if (PositionInBounds(posRU1))
							{
								if ((attackPos.Contains(posRU1) && PositionIsEmpty(posRU1)) || (attackPos.Contains(posRU1) && PositionIsEnemy(posRU1, CurrentPlayer)))
								{
									move = new ChessMove(pos, posRU1, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}
							if (PositionInBounds(posRU2))
							{
								if ((attackPos.Contains(posRU2) && PositionIsEmpty(posRU2)) || (attackPos.Contains(posRU2) && PositionIsEnemy(posRU2, CurrentPlayer)))
								{
									move = new ChessMove(pos, posRU2, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}

						}

						//BISHOP
						if (GetPieceAtPosition(pos).PieceType == ChessPieceType.Bishop)
						{
							for (int i = 1; i <= board.Length; i++)
							{
								posLU = new BoardPosition(pos.Row - i, pos.Col - i);
								if (PositionInBounds(posLU) && LUCheck == false)
								{
									if ((attackPos.Contains(posLU) && PositionIsEmpty(posLU)) || (attackPos.Contains(posLU) && PositionIsEnemy(posLU, CurrentPlayer)))
									{
										if (PositionIsEnemy(posLU, CurrentPlayer) || !PositionIsEmpty(posLU))
										{
											LUCheck = true;
										}
										move = new ChessMove(pos, posLU, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posLU, CurrentPlayer))
									{
										LUCheck = true;
									}
								}
								posRU = new BoardPosition(pos.Row - i, pos.Col + i);
								if (PositionInBounds(posRU) && RUCheck == false)
								{
									if ((attackPos.Contains(posRU) && PositionIsEmpty(posRU)) || (attackPos.Contains(posRU) && PositionIsEnemy(posRU, CurrentPlayer)))
									{
										if (PositionIsEnemy(posRU, CurrentPlayer) || !PositionIsEmpty(posRU))
										{
											RUCheck = true;
										}
										move = new ChessMove(pos, posRU, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posRU, CurrentPlayer))
									{
										RUCheck = true;
									}
								}
								posLD = new BoardPosition(pos.Row + i, pos.Col - i);
								if (PositionInBounds(posLD) && LDCheck == false)
								{
									if ((attackPos.Contains(posLD) && PositionIsEmpty(posLD)) || (attackPos.Contains(posLD) && PositionIsEnemy(posLD, CurrentPlayer)))
									{
										if (PositionIsEnemy(posLD, CurrentPlayer) || !PositionIsEmpty(posLD))
										{
											LDCheck = true;
										}
										move = new ChessMove(pos, posLD, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posLD, CurrentPlayer))
									{
										LDCheck = true;
									}
								}
								posRD = new BoardPosition(pos.Row + i, pos.Col + i);
								if (PositionInBounds(posRD) && RDCheck == false)
								{
									if ((attackPos.Contains(posRD) && PositionIsEmpty(posRD)) || (attackPos.Contains(posRD) && PositionIsEnemy(posRD, CurrentPlayer)))
									{
										if (PositionIsEnemy(posRD, CurrentPlayer) || !PositionIsEmpty(posRD))
										{
											RDCheck = true;
										}
										move = new ChessMove(pos, posRD, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posRD, CurrentPlayer))
									{
										RDCheck = true;
									}
								}
							}
						}

						//ROOK
						if (GetPieceAtPosition(pos).PieceType == ChessPieceType.Rook)
						{
							for (int i = 1; i <= 8; i++)
							{
								posRights = new BoardPosition(pos.Row, pos.Col + i);
								if (PositionInBounds(posRights) && RightCheck == false)
								{
									if ((attackPos.Contains(posRights) && PositionIsEmpty(posRights)) || (attackPos.Contains(posRights) && PositionIsEnemy(posRights, CurrentPlayer)))
									{
										if (PositionIsEnemy(posRights, CurrentPlayer) || !PositionIsEmpty(posRights)) 
										{
											RightCheck = true;
										}
										move = new ChessMove(pos, posRights, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posRights, CurrentPlayer))
									{
										RightCheck = true;
									}
								}
								posLefts = new BoardPosition(pos.Row, pos.Col - i);
								if (PositionInBounds(posLefts) && LeftCheck == false)
								{
									if ((attackPos.Contains(posLefts) && PositionIsEmpty(posLefts)) || (attackPos.Contains(posLefts) && PositionIsEnemy(posLefts, CurrentPlayer)))
									{
										if (PositionIsEnemy(posLefts, CurrentPlayer) || !PositionIsEmpty(posLefts)) 
										{
											LeftCheck = true;
										}
										move = new ChessMove(pos, posLefts, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posLefts, CurrentPlayer))
									{
										LeftCheck = true;
									}
								}
								posUps = new BoardPosition(pos.Row - i, pos.Col);
								if (PositionInBounds(posUps) && UpCheck == false)
								{
									if ((attackPos.Contains(posUps) && PositionIsEmpty(posUps)) || (attackPos.Contains(posUps) && PositionIsEnemy(posUps, CurrentPlayer)))
									{
										if (PositionIsEnemy(posUps, CurrentPlayer) || !PositionIsEmpty(posUps)) 
										{
											UpCheck = true;
										}
										move = new ChessMove(pos, posUps, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posUps, CurrentPlayer))
									{
										UpCheck = true;
									}
								}
								posDowns = new BoardPosition(pos.Row + i, pos.Col);
								if (PositionInBounds(posDowns) && DownCheck == false)
								{
									if ((attackPos.Contains(posDowns) && PositionIsEmpty(posDowns)) || (attackPos.Contains(posDowns) && PositionIsEnemy(posDowns, CurrentPlayer)))
									{
										if (PositionIsEnemy(posDowns, CurrentPlayer) || !PositionIsEmpty(posDowns)) 
										{
											DownCheck = true;
										}
										move = new ChessMove(pos, posDowns, ChessMoveType.Normal);
										allMoves.Add(move);
									}
									else if (!PositionIsEnemy(posDowns, CurrentPlayer))
									{
										DownCheck = true;
									}
								}
							}

						}

						//KING
						if (GetPieceAtPosition(pos).PieceType == ChessPieceType.King)
						{
							upOne = new BoardPosition(pos.Row + 1, pos.Col);
							if (PositionInBounds(upOne))
							{
								if ((attackPos.Contains(upOne) && PositionIsEmpty(upOne)) || (attackPos.Contains(upOne) && PositionIsEnemy(upOne, CurrentPlayer)))
								{

									move = new ChessMove(pos, upOne, ChessMoveType.Normal);
									allMoves.Add(move);


								}
							}

							downOne = new BoardPosition(pos.Row - 1, pos.Col);
							if (PositionInBounds(downOne))
							{
								if ((attackPos.Contains(downOne) && PositionIsEmpty(downOne)) || (attackPos.Contains(downOne) && PositionIsEnemy(downOne, CurrentPlayer)))
								{
									move = new ChessMove(pos, downOne, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}

							leftOne = new BoardPosition(pos.Row, pos.Col - 1);
							BoardPosition leftTwo = new BoardPosition(pos.Row, pos.Col - 2);
							if (PositionInBounds(leftOne))
							{
								if ((attackPos.Contains(leftOne) && PositionIsEmpty(leftOne)) || (attackPos.Contains(leftOne) && PositionIsEnemy(leftOne, CurrentPlayer)))
								{
									if ((pos.Col == 4 && pos.Row == 7) || (pos.Col == 4 && pos.Row == 0))
									{
										if (PositionIsEmpty(new BoardPosition(leftOne.Row, leftOne.Col - 1)))
										{
											if (PositionIsEmpty(new BoardPosition(leftOne.Row, leftOne.Col - 2)) && GetPositionsOfPiece(ChessPieceType.Rook, CurrentPlayer).Count() != 0 && !PositionIsAttacked(pos, CurrentPlayer == 1 ? 2 : 1))
											{
												if (CurrentPlayer == 1 && queensideWhite == false && PositionIsEmpty(leftTwo) && !PositionIsAttacked(leftTwo, CurrentPlayer == 1 ? 2 : 1) && PositionIsEmpty(leftOne) && !PositionIsAttacked(leftOne, CurrentPlayer == 1 ? 2 : 1))
												{

													move = new ChessMove(pos, new BoardPosition(leftOne.Row, leftOne.Col - 1), ChessMoveType.CastleQueenSide);
													allMoves.Add(move);

												}
												if (CurrentPlayer == 2 && queensideBlack == false && PositionIsEmpty(leftTwo) && !PositionIsAttacked(leftTwo, CurrentPlayer == 1 ? 2 : 1) && PositionIsEmpty(leftOne) && !PositionIsAttacked(leftOne, CurrentPlayer == 1 ? 2 : 1)) 
												{

													move = new ChessMove(pos, new BoardPosition(leftOne.Row, leftOne.Col - 1), ChessMoveType.CastleQueenSide);
													allMoves.Add(move);
												}
											}
										}
									}
									move = new ChessMove(pos, leftOne, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}

							rightOne = new BoardPosition(pos.Row, pos.Col + 1);
							BoardPosition rightTwo = new BoardPosition(pos.Row, pos.Col + 2);

							if (PositionInBounds(rightOne))
							{

								if ((attackPos.Contains(rightOne) && PositionIsEmpty(rightOne)) || (attackPos.Contains(rightOne) && PositionIsEnemy(rightOne, CurrentPlayer)))
								{
									if ((pos.Row == 0 && pos.Col == 4) || (pos.Col == 4 && pos.Row == 7))
									{
										if (PositionIsEmpty(new BoardPosition(rightOne.Row, rightOne.Col + 1)) && GetPositionsOfPiece(ChessPieceType.Rook, CurrentPlayer).Count() != 0 && !PositionIsAttacked(pos, CurrentPlayer == 1 ? 2 : 1)) 
										{
											if (CurrentPlayer == 1 && kingsideWhite == false && PositionIsEmpty(rightTwo) && !PositionIsAttacked(rightOne, CurrentPlayer == 1 ? 2 : 1)) 
											{

												move = new ChessMove(pos, rightTwo, ChessMoveType.CastleKingSide);
												allMoves.Add(move);

											}
											if (CurrentPlayer == 2 && kingsideBlack == false && PositionIsEmpty(rightTwo) && !PositionIsAttacked(rightOne, CurrentPlayer == 1 ? 2 : 1))
											{
												BoardPosition rightRook = new BoardPosition(0, 7);

												move = new ChessMove(pos, rightTwo, ChessMoveType.CastleKingSide);
												allMoves.Add(move);
											}
										}
									}
									move = new ChessMove(pos, rightOne, ChessMoveType.Normal);
									allMoves.Add(move);
								}

							}

							leftUp = new BoardPosition(pos.Row + 1, pos.Col - 1);
							if (PositionInBounds(leftUp))
							{

								if ((attackPos.Contains(leftUp) && PositionIsEmpty(leftUp)) || (attackPos.Contains(leftUp) && PositionIsEnemy(leftUp, CurrentPlayer)))
								{
									move = new ChessMove(pos, leftUp, ChessMoveType.Normal);
									allMoves.Add(move);
								}

							}

							rightUp = new BoardPosition(pos.Row - 1, pos.Col + 1);
							if (PositionInBounds(rightUp))
							{
								
									if ((attackPos.Contains(rightUp) && PositionIsEmpty(rightUp)) || (attackPos.Contains(rightUp) && PositionIsEnemy(rightUp, CurrentPlayer)))
									{
										move = new ChessMove(pos, rightUp, ChessMoveType.Normal);
										allMoves.Add(move);
									}
							}

							leftDown = new BoardPosition(pos.Row - 1, pos.Col - 1);
							if (PositionInBounds(leftDown))
							{
								if ((attackPos.Contains(leftDown) && PositionIsEmpty(leftDown)) || (attackPos.Contains(leftDown) && PositionIsEnemy(leftDown, CurrentPlayer)))
								{
									move = new ChessMove(pos, leftDown, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}

							rightDown = new BoardPosition(pos.Row + 1, pos.Col + 1);
							if (PositionInBounds(rightDown))
							{
								if ((attackPos.Contains(rightDown) && PositionIsEmpty(rightDown)) || (attackPos.Contains(rightDown) && PositionIsEnemy(rightDown, CurrentPlayer)))
								{
									move = new ChessMove(pos, rightDown, ChessMoveType.Normal);
									allMoves.Add(move);
								}
							}
						}

						//PAWN
						if (GetPieceAtPosition(pos).PieceType == ChessPieceType.Pawn)
						{

							//PAWN PLAYER 1
							if (CurrentPlayer == 1 /*&& !PositionIsEmpty(pos)*/)
							{
								posLefts = new BoardPosition(pos.Row, pos.Col - 1);
								posRights = new BoardPosition(pos.Row, pos.Col + 1);
								rightDiagonal = new BoardPosition(pos.Row - 1, pos.Col + 1);
								//get left diagonal
								leftDiagonal = new BoardPosition(pos.Row - 1, pos.Col - 1);
								upOne = new BoardPosition(pos.Row - 1, pos.Col);
								UpTwo = new BoardPosition(pos.Row - 2, pos.Col);

								//PAWN PROMOTION?
								if (PositionInBounds(leftDiagonal) && PositionIsEnemy(leftDiagonal, CurrentPlayer))
								{

									if (pos.Row == 1)
									{
									
										allMoves.Add(new ChessMove(pos, leftDiagonal, ChessPieceType.Bishop));
										allMoves.Add(new ChessMove(pos, leftDiagonal, ChessPieceType.Queen));
										allMoves.Add(new ChessMove(pos, leftDiagonal, ChessPieceType.Rook));
										allMoves.Add(new ChessMove(pos, leftDiagonal, ChessPieceType.Knight));
									}

									else
									{
										move = new ChessMove(pos, leftDiagonal);
										allMoves.Add(move);
									}

								}

								if (PositionInBounds(rightDiagonal) && PositionIsEnemy(rightDiagonal, CurrentPlayer))
								{

									if (pos.Row == 1)
									{

										allMoves.Add(new ChessMove(pos, rightDiagonal, ChessPieceType.Bishop));
										allMoves.Add(new ChessMove(pos, rightDiagonal, ChessPieceType.Queen));
										allMoves.Add(new ChessMove(pos, rightDiagonal, ChessPieceType.Rook));
										allMoves.Add(new ChessMove(pos, rightDiagonal, ChessPieceType.Knight));

									}
									else
									{
										move = new ChessMove(pos, rightDiagonal, ChessMoveType.Normal);
										allMoves.Add(move);
									}

								}

								if (PositionInBounds(upOne) /*&& upOne.Row != 0*/)
								{
									if (PositionIsEmpty(upOne))
									{
										if (pos.Row == 1)
										{

											allMoves.Add(new ChessMove(pos, upOne, ChessPieceType.Bishop));
											allMoves.Add(new ChessMove(pos, upOne, ChessPieceType.Queen));
											allMoves.Add(new ChessMove(pos, upOne, ChessPieceType.Rook));
											allMoves.Add(new ChessMove(pos, upOne, ChessPieceType.Knight));

										}
										else
										{
											move = new ChessMove(pos, upOne, ChessMoveType.Normal);
											allMoves.Add(move);
										}
									}
								}

								if (pos.Row == 6)
								{
									if (PositionIsEmpty(upOne))
									{
										if (PositionIsEmpty(UpTwo))
										{
											move = new ChessMove(pos, UpTwo, ChessMoveType.Normal);
											allMoves.Add(move);
										}
									}
								}
								
								if (upOne.Row == 2) //en passant
								{
									if (mMoveHistory.Count() != 0)
									{
										if (GetPieceAtPosition(mMoveHistory.Last().EndPosition).PieceType == ChessPieceType.Pawn)
										{
											if (mMoveHistory.Last().EndPosition == posLefts)
											{
												move = new ChessMove(pos, leftDiagonal, ChessMoveType.EnPassant);
												allMoves.Add(move);
											}
											if (mMoveHistory.Last().EndPosition == posRights)
											{
												move = new ChessMove(pos, rightDiagonal, ChessMoveType.EnPassant);
												allMoves.Add(move);
											}

										}
									}
								}

							}

							//PAWN PLAYER 2
							if (CurrentPlayer == 2)
							{
								posLefts = new BoardPosition(pos.Row, pos.Col + 1);
								posRights = new BoardPosition(pos.Row, pos.Col - 1);
								rightDiagonal = new BoardPosition(pos.Row + 1, pos.Col + 1);
								//get left diagonal
								leftDiagonal = new BoardPosition(pos.Row + 1, pos.Col - 1);
								upOne = new BoardPosition(pos.Row + 1, pos.Col);
								UpTwo = new BoardPosition(pos.Row + 2, pos.Col);

								IEnumerable<BoardPosition> pojjs = GetPositionsOfPiece(ChessPieceType.Pawn,CurrentPlayer);
								if (PositionInBounds(leftDiagonal) && !PositionIsEmpty(leftDiagonal) && PositionIsEnemy(leftDiagonal,CurrentPlayer))
								{
									if (pos.Row == 6)
									{

										allMoves.Add(new ChessMove(pos, leftDiagonal, ChessPieceType.Bishop));
										allMoves.Add(new ChessMove(pos, leftDiagonal, ChessPieceType.Queen));
										allMoves.Add(new ChessMove(pos, leftDiagonal, ChessPieceType.Rook));
										allMoves.Add(new ChessMove(pos, leftDiagonal, ChessPieceType.Knight));

									}
									else
									{

										move = new ChessMove(pos, leftDiagonal, ChessMoveType.Normal);
										allMoves.Add(move);
									}
								}

								if (PositionInBounds(rightDiagonal) && !PositionIsEmpty(rightDiagonal) && PositionIsEnemy(rightDiagonal, CurrentPlayer))
								{

									if (pos.Row == 6)
									{

										allMoves.Add(new ChessMove(pos, rightDiagonal, ChessPieceType.Bishop));
										allMoves.Add(new ChessMove(pos, rightDiagonal, ChessPieceType.Queen));
										allMoves.Add(new ChessMove(pos, rightDiagonal, ChessPieceType.Rook));
										allMoves.Add(new ChessMove(pos, rightDiagonal, ChessPieceType.Knight));

									}
									else
									{
										move = new ChessMove(pos, rightDiagonal, ChessMoveType.Normal);
										allMoves.Add(move);
									}

								}

								if (PositionInBounds(upOne) /*&& upOne.Row != 7*/)
								{
									if (PositionIsEmpty(upOne))
									{
										if (pos.Row == 6)
										{

											allMoves.Add(new ChessMove(pos, upOne, ChessPieceType.Bishop));
											allMoves.Add(new ChessMove(pos, upOne, ChessPieceType.Queen));
											allMoves.Add(new ChessMove(pos, upOne, ChessPieceType.Rook));
											allMoves.Add(new ChessMove(pos, upOne, ChessPieceType.Knight));

										}
										else
										{
											move = new ChessMove(pos, upOne, ChessMoveType.Normal);
											allMoves.Add(move);
										}
									}
								}

								if (pos.Row == 1)
								{
									if (PositionIsEmpty(upOne))
									{
										if (PositionIsEmpty(UpTwo))
										{
											move = new ChessMove(pos, UpTwo, ChessMoveType.Normal);
											allMoves.Add(move);
										}
									}
								}
								
								if (upOne.Row == 5)
								{
									if (GetPieceAtPosition(mMoveHistory.Last().EndPosition).PieceType == ChessPieceType.Pawn)
									{
										if (mMoveHistory.Last().EndPosition == posLefts)
										{
											move = new ChessMove(pos, rightDiagonal, ChessMoveType.EnPassant);
											allMoves.Add(move);
										}
										if (mMoveHistory.Last().EndPosition == posRights)
										{
											move = new ChessMove(pos, leftDiagonal, ChessMoveType.EnPassant);
											allMoves.Add(move);
										}

									}

								}//en passant

							}
						}

					}
				}
			}

			// for each move in Allmoves:
			//    try applying the move
			//		if the king is not under attack, keep that move
			//		otherwise get rid of it
			//    undo the move and go to next loop

			foreach (ChessMove m in allMoves)
			{
				BoardPosition kingPos;

				ApplyMove(m);

				if(CurrentPlayer==1)
				{
					kingPos = GetPositionsOfPiece(ChessPieceType.King, 2).First();
				}
				else
				{
					kingPos = GetPositionsOfPiece(ChessPieceType.King, 1).First();
				}

				bool posAttacked = PositionIsAttacked(kingPos, CurrentPlayer);


				if (!posAttacked)
				{
					validMoves.Add(m);
				}

				UndoLastMove();
			}

			IEnumerable<ChessMove> moves = validMoves;
	
			return moves;
		}

		//NEEDS WORK BUT ALMOST DONE :)
		public void ApplyMove(ChessMove m)
		{
			//pieces involved in the move
			ChessPiece pieceAtEndPos = GetPieceAtPosition(m.EndPosition);
			ChessPiece pieceAtStartPos = GetPieceAtPosition(m.StartPosition);

			//castling checks
			if (pieceAtStartPos.PieceType == ChessPieceType.Rook && pieceAtStartPos.Player == 1 && m.StartPosition.Row == 7 && m.StartPosition.Col == 7)
			{
				kingsideWhite = true;
			}

			if (pieceAtStartPos.PieceType == ChessPieceType.Rook && pieceAtStartPos.Player == 1 && m.StartPosition.Row == 7 && m.StartPosition.Col == 0)
			{
				queensideWhite = true;
			}

			if (pieceAtStartPos.PieceType == ChessPieceType.Rook && pieceAtStartPos.Player == 2 && m.StartPosition.Row == 0 && m.StartPosition.Col == 7)
			{
				kingsideBlack = true;
			}

			if (pieceAtStartPos.PieceType == ChessPieceType.Rook && pieceAtStartPos.Player == 2 && m.StartPosition.Row == 0 && m.StartPosition.Col == 0)
			{
				queensideBlack = true;
			}

			//draw count
			if (!PositionIsEnemy(m.EndPosition, CurrentPlayer) && pieceAtStartPos.PieceType != ChessPieceType.Pawn)
			{
				drawCount += 1;
			}
			else
			{
				drawCount = 0;
			}

			gameDrawCounter.Add(drawCount);

			//normal moves
			if (m.MoveType == ChessMoveType.Normal)
			{
				if (pieceAtEndPos.Player == 2)
				{
					m.capturedPiece = new ChessPiece(GetPieceAtPosition(m.EndPosition).PieceType, 2);
					SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
					SetPieceAtPosition(m.EndPosition, pieceAtStartPos);
					SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
				}

				if (pieceAtEndPos.Player == 1)
				{
					m.capturedPiece = new ChessPiece(GetPieceAtPosition(m.EndPosition).PieceType, 1);
					SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
					SetPieceAtPosition(m.EndPosition, pieceAtStartPos);
					SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
				}

				if (pieceAtEndPos.Player == 0)
				{
					SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
					SetPieceAtPosition(m.EndPosition, pieceAtStartPos);
					SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
				}
			}

			//en passant
			if (m.MoveType == ChessMoveType.EnPassant)
			{
				//mMoveHistory.Add(m);
				ChessMove lastMoveFromHist = mMoveHistory.Last();

				ChessPiece type = GetPieceAtPosition(lastMoveFromHist.EndPosition);

				if (pieceAtStartPos.Player == 1)
				{

					//en passant
					if (type.PieceType == ChessPieceType.Pawn && type.Player != CurrentPlayer && m.MoveType == ChessMoveType.EnPassant)
					{
						if (GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1)).PieceType == type.PieceType && (lastMoveFromHist.EndPosition.Row==m.StartPosition.Row && lastMoveFromHist.EndPosition.Col==m.StartPosition.Col+1))
						{
							m.capturedPiece = GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row - 1, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Pawn, 1));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Empty, 0));
						}

						else if (GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1)).PieceType == type.PieceType && (lastMoveFromHist.EndPosition.Row == m.StartPosition.Row && lastMoveFromHist.EndPosition.Col == m.StartPosition.Col - 1))
						{
							m.capturedPiece = GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row - 1, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Pawn, 1));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Empty, 0));
						}
					}
				}

				if (pieceAtStartPos.Player == 2)
				{
					//en passant
					if (type.PieceType == ChessPieceType.Pawn && type.Player != CurrentPlayer && m.MoveType == ChessMoveType.EnPassant)
					{
						if (GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1)).PieceType == type.PieceType)
						{
							m.capturedPiece = GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row + 1, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Pawn, 2));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Empty, 0));
						}

						else if (GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1)).PieceType == type.PieceType)
						{
							m.capturedPiece = GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row + 1, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Pawn, 2));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
							SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Empty, 0));
						}
					}

				}
			}

			//king side castle
			if (m.MoveType == ChessMoveType.CastleKingSide)
			{
				//kingside castle PLAYER 1
				if (/*lastMoveFromHist m.MoveType == ChessMoveType.CastleKingSide && */pieceAtStartPos.Player == 1)
				{

					SetPieceAtPosition(new BoardPosition(7, 6), new ChessPiece(ChessPieceType.King, 1));
					SetPieceAtPosition(new BoardPosition(7, 7), new ChessPiece(ChessPieceType.Empty, 0));
					SetPieceAtPosition(new BoardPosition(7, 5), new ChessPiece(ChessPieceType.Rook, 1));
					SetPieceAtPosition(new BoardPosition(7, 4), new ChessPiece(ChessPieceType.Empty, 0));

				}

				//kingside castle PLAYER 2
				if (/*m.MoveType == ChessMoveType.CastleKingSide && */pieceAtStartPos.Player == 2)
				{

					SetPieceAtPosition(new BoardPosition(0, 6), new ChessPiece(ChessPieceType.King, 2));
					SetPieceAtPosition(new BoardPosition(0, 7), new ChessPiece(ChessPieceType.Empty, 0));
					SetPieceAtPosition(new BoardPosition(0, 5), new ChessPiece(ChessPieceType.Rook, 2));
					SetPieceAtPosition(new BoardPosition(0, 4), new ChessPiece(ChessPieceType.Empty, 0));
				}
			}

			//queen side castle
			if (m.MoveType == ChessMoveType.CastleQueenSide)
			{
				if (/*m.MoveType == ChessMoveType.CastleQueenSide &&*/ pieceAtStartPos.Player == 2)
				{

					SetPieceAtPosition(new BoardPosition(0, 3), new ChessPiece(ChessPieceType.Rook, 2));
					SetPieceAtPosition(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Empty, 0));
					SetPieceAtPosition(new BoardPosition(0, 2), new ChessPiece(ChessPieceType.King, 2));
					SetPieceAtPosition(new BoardPosition(0, 4), new ChessPiece(ChessPieceType.Empty, 0));

				}

				//queenside castle PLAYER 1
				if (/*m.MoveType == ChessMoveType.CastleQueenSide && */pieceAtStartPos.Player == 1)
				{

					SetPieceAtPosition(new BoardPosition(7, 3), new ChessPiece(ChessPieceType.Rook, 1));
					SetPieceAtPosition(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Empty, 0));
					SetPieceAtPosition(new BoardPosition(7, 2), new ChessPiece(ChessPieceType.King, 1));
					SetPieceAtPosition(new BoardPosition(7, 4), new ChessPiece(ChessPieceType.Empty, 0));
				}
			}

			//pawn promote
			if (m.MoveType == ChessMoveType.PawnPromote)
			{
				if (pieceAtStartPos.Player == 1)
				{
					if (pieceAtEndPos.Player == 2)
					{
						m.capturedPiece = new ChessPiece(GetPieceAtPosition(m.EndPosition).PieceType, 2);
					}

					SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
					SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col), new ChessPiece(m.PromoteTo, 1));
					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
				}

				if (pieceAtStartPos.Player == 2)
				{

					if (pieceAtEndPos.Player == 1)
					{
						m.capturedPiece = new ChessPiece(GetPieceAtPosition(m.EndPosition).PieceType, 1);
					}

					SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
					SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col), new ChessPiece(m.PromoteTo, 2));
					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
				}
			}

			#region
			//special moves
			//if (/*m.MoveType == ChessMoveType.EnPassant ||*/ m.MoveType == ChessMoveType.CastleKingSide || m.MoveType == ChessMoveType.CastleQueenSide || m.MoveType == ChessMoveType.PawnPromote)
			//{

			//	if (mMoveHistory.Count != 0 || queensideBlack == false || kingsideBlack == false || queensideWhite == false || kingsideWhite == false)
			//	{
			//		//mMoveHistory.Add(m);
			//		ChessMove lastMoveFromHist = mMoveHistory.Last();

			//		ChessPiece type = GetPieceAtPosition(lastMoveFromHist.EndPosition);

			//		if (pieceAtStartPos.Player == 1)
			//		{

			//			//en passant
			//			if (type.PieceType == ChessPieceType.Pawn && type.Player != CurrentPlayer && m.MoveType == ChessMoveType.EnPassant)
			//			{
			//				if (GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1)).PieceType == type.PieceType)
			//				{
			//					m.capturedPiece = GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row - 1, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Pawn, 1));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Empty, 0));
			//				}

			//				if (GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1)).PieceType == type.PieceType)
			//				{
			//					m.capturedPiece = GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row - 1, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Pawn, 1));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Empty, 0));
			//				}
			//			}

			//			//pawn promote
			//			if (m.MoveType == ChessMoveType.PawnPromote)
			//			{
			//				if (pieceAtEndPos.Player == 2)
			//				{
			//					m.capturedPiece = new ChessPiece(GetPieceAtPosition(m.EndPosition).PieceType, 2);
			//				}

			//				SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//				SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col), new ChessPiece(m.PromoteTo, 1));
			//				SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//			}

			//		}

			//		if (pieceAtStartPos.Player == 2)
			//		{
			//			//en passant
			//			if (type.PieceType == ChessPieceType.Pawn && type.Player != CurrentPlayer && m.MoveType == ChessMoveType.EnPassant)
			//			{
			//				if (GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1)).PieceType == type.PieceType)
			//				{
			//					m.capturedPiece = GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row + 1, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Pawn, 2));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Empty, 0));
			//				}

			//				if (GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1)).PieceType == type.PieceType)
			//				{
			//					m.capturedPiece = GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row + 1, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Pawn, 2));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//					SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Empty, 0));
			//				}
			//			}

			//			//pawn promote
			//			if (m.MoveType == ChessMoveType.PawnPromote)
			//			{

			//				if (pieceAtEndPos.Player == 1)
			//				{
			//					m.capturedPiece = new ChessPiece(GetPieceAtPosition(m.EndPosition).PieceType, 1);
			//				}

			//				SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//				SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col), new ChessPiece(m.PromoteTo, 2));
			//				SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//			}

			//		}

			//		//kingside castle PLAYER 1
			//		if (/*lastMoveFromHist*/m.MoveType == ChessMoveType.CastleKingSide && pieceAtStartPos.Player == 1)
			//		{

			//			SetPieceAtPosition(new BoardPosition(7, 6), new ChessPiece(ChessPieceType.King, 1));
			//			SetPieceAtPosition(new BoardPosition(7, 7), new ChessPiece(ChessPieceType.Empty, 0));
			//			SetPieceAtPosition(new BoardPosition(7, 5), new ChessPiece(ChessPieceType.Rook, 1));
			//			SetPieceAtPosition(new BoardPosition(7, 4), new ChessPiece(ChessPieceType.Empty, 0));

			//		}

			//		//kingside castle PLAYER 2
			//		if (m.MoveType == ChessMoveType.CastleKingSide && pieceAtStartPos.Player == 2)
			//		{

			//			SetPieceAtPosition(new BoardPosition(0, 6), new ChessPiece(ChessPieceType.King, 2));
			//			SetPieceAtPosition(new BoardPosition(0, 7), new ChessPiece(ChessPieceType.Empty, 0));
			//			SetPieceAtPosition(new BoardPosition(0, 5), new ChessPiece(ChessPieceType.Rook, 2));
			//			SetPieceAtPosition(new BoardPosition(0, 4), new ChessPiece(ChessPieceType.Empty, 0));
			//		}

			//		//queenside castle PLAYER 2
			//		if (m.MoveType == ChessMoveType.CastleQueenSide && pieceAtStartPos.Player == 2)
			//		{

			//			SetPieceAtPosition(new BoardPosition(0, 3), new ChessPiece(ChessPieceType.Rook, 2));
			//			SetPieceAtPosition(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Empty, 0));
			//			SetPieceAtPosition(new BoardPosition(0, 2), new ChessPiece(ChessPieceType.King, 2));
			//			SetPieceAtPosition(new BoardPosition(0, 4), new ChessPiece(ChessPieceType.Empty, 0));

			//		}

			//		//queenside castle PLAYER 1
			//		if (m.MoveType == ChessMoveType.CastleQueenSide && pieceAtStartPos.Player == 1)
			//		{

			//			SetPieceAtPosition(new BoardPosition(7, 3), new ChessPiece(ChessPieceType.Rook, 1));
			//			SetPieceAtPosition(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Empty, 0));
			//			SetPieceAtPosition(new BoardPosition(7, 2), new ChessPiece(ChessPieceType.King, 1));
			//			SetPieceAtPosition(new BoardPosition(7, 4), new ChessPiece(ChessPieceType.Empty, 0));
			//		}

			//		#region
			//		//if (m.MoveType.Equals(lastMoveFromHist.MoveType == ChessMoveType.EnPassant) && pieceAtStartPos.Player == 1)
			//		//{
			//		//	int castlePos = lastMoveFromHist.EndPosition.Col;

			//		//	if (castlePos == m.StartPosition.Col + 1 && GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1)).PieceType == ChessPieceType.Pawn && PositionIsEnemy(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1), 1) == true)
			//		//	{
			//		//		SetPieceAtPosition(new BoardPosition(m.EndPosition.Row - 1, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Pawn, 1));
			//		//		SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Empty, 0));
			//		//		SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//		//	}
			//		//	if (castlePos == m.StartPosition.Col - 1 && GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1)).PieceType == ChessPieceType.Pawn && PositionIsEnemy(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1), 1) == true)
			//		//	{
			//		//		SetPieceAtPosition(new BoardPosition(m.EndPosition.Row - 1, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Pawn, 1));
			//		//		SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Empty, 0));
			//		//		SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//		//	}

			//		//}

			//		//if (lastMoveFromHist.MoveType == ChessMoveType.EnPassant && pieceAtStartPos.Player == 2)
			//		//{
			//		//	ChessMove castling2 = mMoveHistory.Take(mMoveHistory.Count() - 1).LastOrDefault();
			//		//	int castlePos = castling2.EndPosition.Col;

			//		//	if (castlePos == m.StartPosition.Col + 1 && GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1)).PieceType == ChessPieceType.Pawn && PositionIsEnemy(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1), 2) == true)
			//		//	{
			//		//		SetPieceAtPosition(new BoardPosition(m.EndPosition.Row + 1, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Pawn, 2));
			//		//		SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Empty, 0));
			//		//		SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//		//	}
			//		//	if (castlePos == m.StartPosition.Col - 1 && GetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1)).PieceType == ChessPieceType.Pawn && PositionIsEnemy(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col + 1), 2) == true)
			//		//	{
			//		//		SetPieceAtPosition(new BoardPosition(m.EndPosition.Row + 1, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Pawn, 2));
			//		//		SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Empty, 0));
			//		//		SetPieceAtPosition(new BoardPosition(m.StartPosition.Row, m.StartPosition.Col), new ChessPiece(ChessPieceType.Empty, 0));
			//		//	}

			//		//}
			//		#endregion
			//	}
			//}
			#endregion
			
			mMoveHistory.Add(m);
			updateAdvantage();

			if (player == 0)
			{
				player = 1;
			}
			else
				player = 0;
		}

		//WORKING ON IT
		//DONT: reset overall table state to get to the previous state
		//SHOULD: record a second vector of move states that reflects other facts like what was captured, rook castling example
		//undo moving resets to prev state
		//which facts need to be recorded??
		public void UndoLastMove()
		{

			if (mMoveHistory.Count >= 1)
			{
				//the move before the current player
				ChessMove lastMove = mMoveHistory.ElementAt(mMoveHistory.Count()-1);

				//makes castling possible again
				if (lastMove.StartPosition.Row == 7 && lastMove.StartPosition.Col == 7 && GetPieceAtPosition(lastMove.EndPosition).PieceType == ChessPieceType.Rook )
				{
					kingsideWhite = false;
				}
				if (lastMove.StartPosition.Row == 0 && lastMove.StartPosition.Col == 0 && GetPieceAtPosition(lastMove.EndPosition).PieceType == ChessPieceType.Rook)
				{
					queensideBlack = false;
				}
				if (lastMove.StartPosition.Row == 0 && lastMove.StartPosition.Col == 7 && GetPieceAtPosition(lastMove.EndPosition).PieceType == ChessPieceType.Rook)
				{
					kingsideBlack = false;
				}
				if (lastMove.StartPosition.Row == 7 && lastMove.StartPosition.Col == 0 && GetPieceAtPosition(lastMove.EndPosition).PieceType == ChessPieceType.Rook)
				{
					queensideWhite = false;
				}

				if (lastMove.capturedPiece.PieceType == ChessPieceType.Empty)
				{
					SetPieceAtPosition(lastMove.StartPosition, GetPieceAtPosition(lastMove.EndPosition));
					SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
				}

				else if (lastMove.capturedPiece.PieceType != ChessPieceType.Empty)
				{
					if (lastMove.MoveType == ChessMoveType.EnPassant)
					{
						SetPieceAtPosition(lastMove.StartPosition, GetPieceAtPosition(lastMove.EndPosition));
						SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));

						if(lastMove.capturedPiece.Player==2)
						{
							if(lastMove.EndPosition.Col==lastMove.StartPosition.Col+1)
							{
								SetPieceAtPosition(new BoardPosition(lastMove.StartPosition.Row, lastMove.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Pawn, 2));
							}
							if(lastMove.EndPosition.Col == lastMove.StartPosition.Col - 1)
							{
								SetPieceAtPosition(new BoardPosition(lastMove.StartPosition.Row, lastMove.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Pawn, 2));
							}
						}

						if(lastMove.capturedPiece.Player == 1)
						{
							if (lastMove.EndPosition.Col == lastMove.StartPosition.Col + 1)
							{
								SetPieceAtPosition(new BoardPosition(lastMove.StartPosition.Row, lastMove.StartPosition.Col + 1), new ChessPiece(ChessPieceType.Pawn, 1));
							}
							if (lastMove.EndPosition.Col == lastMove.StartPosition.Col - 1)
							{
								SetPieceAtPosition(new BoardPosition(lastMove.StartPosition.Row, lastMove.StartPosition.Col - 1), new ChessPiece(ChessPieceType.Pawn, 1));
							}
						}
					}

					else if(lastMove.MoveType != ChessMoveType.EnPassant)
					{

						SetPieceAtPosition(lastMove.StartPosition, GetPieceAtPosition(lastMove.EndPosition));
						SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
						SetPieceAtPosition(lastMove.EndPosition, lastMove.capturedPiece);
					}
				}

				if(lastMove.MoveType==ChessMoveType.PawnPromote)
				{
					if(lastMove.EndPosition.Row==0 && lastMove.capturedPiece.PieceType==ChessPieceType.Empty)
					{
						SetPieceAtPosition(lastMove.StartPosition, new ChessPiece(ChessPieceType.Pawn, 1));
						SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
					}

					if(lastMove.EndPosition.Row == 0 && lastMove.capturedPiece.PieceType != ChessPieceType.Empty)
					{
						SetPieceAtPosition(lastMove.StartPosition, new ChessPiece(ChessPieceType.Pawn, 1));
						SetPieceAtPosition(lastMove.EndPosition, lastMove.capturedPiece);
					}

					if (lastMove.EndPosition.Row == 7 && lastMove.capturedPiece.PieceType == ChessPieceType.Empty)
					{
						SetPieceAtPosition(lastMove.StartPosition, new ChessPiece(ChessPieceType.Pawn, 2));
						SetPieceAtPosition(lastMove.EndPosition, new ChessPiece(ChessPieceType.Empty, 0));
					}

					if (lastMove.EndPosition.Row == 7 && lastMove.capturedPiece.PieceType != ChessPieceType.Empty)
					{
						SetPieceAtPosition(lastMove.StartPosition, new ChessPiece(ChessPieceType.Pawn, 2));
						SetPieceAtPosition(lastMove.EndPosition, lastMove.capturedPiece);
					}
				}

				if(lastMove.MoveType==ChessMoveType.CastleQueenSide)
				{
					if (GetPlayerAtPosition(lastMove.StartPosition)==1)
					{
						SetPieceAtPosition(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Rook, 1));
						SetPieceAtPosition(new BoardPosition(lastMove.EndPosition.Row, lastMove.EndPosition.Col + 1), new ChessPiece(ChessPieceType.Empty, 0));
					}

					if (GetPlayerAtPosition(lastMove.StartPosition) == 2)
					{
						SetPieceAtPosition(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Rook, 2));
						SetPieceAtPosition(new BoardPosition(lastMove.EndPosition.Row, lastMove.EndPosition.Col + 1), new ChessPiece(ChessPieceType.Empty, 0));
					}
				}

				if(lastMove.MoveType == ChessMoveType.CastleKingSide)
				{
					if (GetPlayerAtPosition(lastMove.StartPosition) == 1)
					{
						SetPieceAtPosition(new BoardPosition(7, 7), new ChessPiece(ChessPieceType.Rook, 1));
						SetPieceAtPosition(new BoardPosition(lastMove.EndPosition.Row, lastMove.EndPosition.Col - 1), new ChessPiece(ChessPieceType.Empty, 0));
					}

					if (GetPlayerAtPosition(lastMove.StartPosition) == 2)
					{
						SetPieceAtPosition(new BoardPosition(0, 7), new ChessPiece(ChessPieceType.Rook, 2));
						SetPieceAtPosition(new BoardPosition(lastMove.EndPosition.Row, lastMove.EndPosition.Col - 1), new ChessPiece(ChessPieceType.Empty, 0));
					}
				}

				if (gameDrawCounter.Any())
				{
					if (gameDrawCounter.Count >= 2)
					{
						drawCount = gameDrawCounter.ElementAt(gameDrawCounter.Count - 2);
						gameDrawCounter.RemoveAt(gameDrawCounter.Count - 1);
					}
					else if(gameDrawCounter.Count==1)
					{
						drawCount = 0;
						gameDrawCounter.Clear();

					}
				}

				updateAdvantage();
				mMoveHistory.RemoveAt(mMoveHistory.Count() - 1);

				#region
				//foreach (var x in stateOfBoard)
				//{
				//	BoardPosition startpos = lastMoveOfCurrentPlayer.StartPosition;
				//	BoardPosition endpos = lastMoveOfCurrentPlayer.EndPosition;
				//	for (int i = 0; i < stateOfBoard.Count; i++)
				//	{
				//		if (lastMoveOfEnemyPlayer.MoveType == ChessMoveType.Normal || lastMoveOfEnemyPlayer.MoveType == ChessMoveType.EnPassant || lastMoveOfEnemyPlayer.MoveType == ChessMoveType.PawnPromote)
				//		{

				//			SetPieceAtPosition(lastMoveOfEnemyPlayer.StartPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCapturing.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCapturing.Player));
				//			SetPieceAtPosition(lastMoveOfEnemyPlayer.EndPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCaptured.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCaptured.Player));

				//			SetPieceAtPosition(lastMoveOfCurrentPlayer.StartPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCapturing.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCapturing.Player));
				//			SetPieceAtPosition(lastMoveOfCurrentPlayer.EndPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCaptured.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCaptured.Player));
				//		}

				//		if (lastMoveOfCurrentPlayer.MoveType == ChessMoveType.CastleKingSide)
				//		{

				//			SetPieceAtPosition(lastMoveOfEnemyPlayer.StartPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCapturing.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCapturing.Player));
				//			SetPieceAtPosition(lastMoveOfEnemyPlayer.EndPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCaptured.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCaptured.Player));

				//			if (lastMoveOfCurrentPlayer.Player == 1)
				//			{
				//				SetPieceAtPosition(lastMoveOfCurrentPlayer.StartPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCapturing.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCapturing.Player));
				//				SetPieceAtPosition(lastMoveOfCurrentPlayer.EndPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCaptured.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCaptured.Player));
				//				//SET ROOK
				//				SetPieceAtPosition(new BoardPosition(7, 7), new ChessPiece(ChessPieceType.Rook, 1));
				//				SetPieceAtPosition(new BoardPosition(7, 5), new ChessPiece(ChessPieceType.Empty, 0));
				//			}
				//			if (lastMoveOfCurrentPlayer.Player == 2)
				//			{
				//				SetPieceAtPosition(lastMoveOfCurrentPlayer.StartPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCapturing.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCapturing.Player));
				//				SetPieceAtPosition(lastMoveOfCurrentPlayer.EndPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCaptured.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCaptured.Player));
				//				//SET ROOK
				//				SetPieceAtPosition(new BoardPosition(0, 7), new ChessPiece(ChessPieceType.Rook, 2));
				//				SetPieceAtPosition(new BoardPosition(0, 5), new ChessPiece(ChessPieceType.Empty, 0));
				//			}
				//		}
				//		if (lastMoveOfCurrentPlayer.MoveType == ChessMoveType.CastleQueenSide)
				//		{

				//			SetPieceAtPosition(lastMoveOfEnemyPlayer.StartPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCapturing.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCapturing.Player));
				//			SetPieceAtPosition(lastMoveOfEnemyPlayer.EndPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCaptured.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 1).pieceCaptured.Player));

				//			if (lastMoveOfCurrentPlayer.Player == 1)
				//			{
				//				SetPieceAtPosition(lastMoveOfCurrentPlayer.StartPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCapturing.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCapturing.Player));
				//				SetPieceAtPosition(lastMoveOfCurrentPlayer.EndPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCaptured.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCaptured.Player));
				//				//SET ROOK
				//				SetPieceAtPosition(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Rook, 1));
				//				SetPieceAtPosition(new BoardPosition(7, 3), new ChessPiece(ChessPieceType.Empty, 0));
				//			}
				//			if (lastMoveOfCurrentPlayer.Player == 2)
				//			{
				//				SetPieceAtPosition(lastMoveOfCurrentPlayer.StartPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCapturing.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCapturing.Player));
				//				SetPieceAtPosition(lastMoveOfCurrentPlayer.EndPosition, new ChessPiece(stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCaptured.PieceType, stateOfBoard.ElementAt(stateOfBoard.Count - 2).pieceCaptured.Player));
				//				//SET ROOK
				//				SetPieceAtPosition(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Rook, 2));
				//				SetPieceAtPosition(new BoardPosition(0, 3), new ChessPiece(ChessPieceType.Empty, 0));
				//			}
				//		}




				//	}
				//}
				#endregion

				if (player == 0)
				{
					player = 1;
				}
				else
					player = 0;	
			}

			else
			{
				throw new System.InvalidOperationException("cannot undo moves if there haven't been any made!");
			}
		}

		//DONE
		/// <summary>
		/// Returns whatever chess piece is occupying the given position.
		/// </summary>
		public ChessPiece GetPieceAtPosition(BoardPosition position)
		{

			int _player = 0;
			//holds the bits that indicate the piece that is occupying the spot
			int pieceBits = 0;

			//checks that a valid row and column are given
			if (PositionInBounds(position))
			{
				//holds the array index we need to look in
				int index = calculateIndex(position.Row, position.Col);

				//holds the byte at the index
				var bytes = board[index];

				//checks whether the column passed in was even. This tells us to get the left-most bits in the byte
				if ((position.Col) % 2 == 0)
				{
					var low = bytes >> 4;
					pieceBits = low & 7;
					_player = (low >> 3) & 1;
					_player += 1;
				}

				//checks whether the column passed in was odd. This tells us to get the right-most bits in the byte
				else
				{
					var high = bytes & 0x0F;
					pieceBits = high & 7;
					_player = (high >> 3) & 1;
					_player += 1;
				}
			}

			//returns the right chessPiece along with the player
			return (pieceBits == 1 ? new ChessPiece(ChessPieceType.Pawn, _player)
				: pieceBits == 2 ? new ChessPiece(ChessPieceType.Rook, _player)
				: pieceBits == 3 ? new ChessPiece(ChessPieceType.Knight, _player)
				: pieceBits == 4 ? new ChessPiece(ChessPieceType.Bishop, _player)
				: pieceBits == 5 ? new ChessPiece(ChessPieceType.Queen, _player)
				: pieceBits == 6 ? new ChessPiece(ChessPieceType.King, _player)
				: new ChessPiece(ChessPieceType.Empty, 0));

		}

		//DONE
		/// <summary>
		/// Returns whatever player is occupying the given position.
		/// </summary>
		public int GetPlayerAtPosition(BoardPosition pos)
		{
			// gets the piece at the position.
			ChessPiece pieceAtPos = GetPieceAtPosition(pos);
			//returns the piece's player
			return pieceAtPos.Player;
		}

		//DONE
		/// <summary>
		/// Returns true if the given position on the board is empty.
		/// </summary>
		/// <remarks>returns false if the position is not in bounds</remarks>
		public bool PositionIsEmpty(BoardPosition pos)
		{

			//int player = 0;
			//holds the bits that indicate the piece that is occupying the spot
			int pieceBits = 0;

			//checks that a valid row and column are given
			if (PositionInBounds(pos))
			{
				//holds the array index we need to look in
				int index = calculateIndex(pos.Row, pos.Col);

				//holds the byte at the index
				var bytes = board[index];

				//checks whether the column passed in was even. This tells us to get the left-most bits in the byte
				if ((pos.Col) % 2 == 0)
				{
					var low = bytes >> 4;
					pieceBits = low;
					//player = (low >> 3) & 1;
				}

				//checks whether the column passed in was odd. This tells us to get the right-most bits in the byte
				else
				{
					var high = bytes & 0x0F;
					pieceBits = high;
					//player = (high >> 3) & 1;
				}

				if (pieceBits == 0)
				{
					return true;
				}
			}
			return false;
		}

		//DONE
		/// <summary>
		/// Returns true if the given position contains a piece that is the enemy of the given player.
		/// </summary>
		/// <remarks>returns false if the position is not in bounds</remarks>
		public bool PositionIsEnemy(BoardPosition pos, int player)
		{
			ChessPiece piece = GetPieceAtPosition(pos);

			if (piece.Player != player && piece.PieceType != ChessPieceType.Empty)
			{
				return true;
			}
			else
				return false;
		}

		//DONE
		/// <summary>
		/// Returns true if the given position is in the bounds of the board.
		/// </summary>
		public static bool PositionInBounds(BoardPosition pos)
		{
			if (pos.Col < BoardSize && pos.Row < BoardSize && pos.Col >= 0 && pos.Row >= 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		//DONE
		/// <summary>
		/// Returns all board positions where the given piece can be found.
		/// </summary>
		public IEnumerable<BoardPosition> GetPositionsOfPiece(ChessPieceType piece, int player)
		{

			//holds the list of boardPositions
			List<BoardPosition> positionsOfPiece = new List<BoardPosition>();

			for (int i = 0; i </*=*/8; i++)
			{
				for (int j = 0; j </*=*/ 8; j++)
				{
					//takes the piece at each position of the board
					ChessPiece chessT = GetPieceAtPosition(new BoardPosition(i, j));

					//checks if the piece at that position matches the type and the player
					if (chessT.Player == player && chessT.PieceType == piece)
					{
						//then adds it to the list
						positionsOfPiece.Add(new BoardPosition(i, j));
					}
				}
			}
			return positionsOfPiece;
		}

		/*Plan an architecture for getting all attacked positions.A position is attacked by a player if the
		player owns a piece that could take an enemy piece at the position, if there was one there. Each
		piece type has its own rules for determining which positions it threatens; your design should at the
		minimum use different methods for the different piece types.Implement GetAttackededPositions
		and PositionIsAttacked in ChessBoard.
		*/

		/// <summary>
		/// Returns true if the given player's pieces are attacking the given position.
		/// </summary>
		/// UNDER REVIEW
		public bool PositionIsAttacked(BoardPosition position, int byPlayer)
		{
			ISet<BoardPosition> PlayerAttackPositions = GetAttackedPositions(byPlayer);
			if (PlayerAttackPositions.Contains(position))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Returns a set of all BoardPositions that are attacked by the given player.
		/// </summary>
		/// DONE 
		public ISet<BoardPosition> GetAttackedPositions(int byPlayer)
		{

			ISet<BoardPosition> allPos = new HashSet<BoardPosition>();

			IEnumerable<BoardPosition> positions = GetPositionsOfPiece(ChessPieceType.Pawn, byPlayer);
			//PAWNS
			foreach (BoardPosition posPawn in positions)
			{
				BoardPosition rightDiagonal;
				BoardPosition leftDiagonal;

				if (byPlayer == 1)
				{
					//get right diagonal
					rightDiagonal = new BoardPosition(posPawn.Row /*+*/- 1, posPawn.Col + 1);
					//get left diagonal
					leftDiagonal = new BoardPosition(posPawn.Row /*+*/- 1, posPawn.Col - 1);
					if (PositionInBounds(rightDiagonal) == true)
					{
						allPos.Add(rightDiagonal);
					}
					if (PositionInBounds(leftDiagonal) == true)
					{
						allPos.Add(leftDiagonal);
					}
				}
				else if (byPlayer == 2)
				{
					//get right diagonal
					rightDiagonal = new BoardPosition(posPawn.Row + 1, posPawn.Col + 1);
					//get left diagonal
					leftDiagonal = new BoardPosition(posPawn.Row + 1, posPawn.Col - 1);
					if (PositionInBounds(rightDiagonal) == true)
					{
						allPos.Add(rightDiagonal);
					}
					if (PositionInBounds(leftDiagonal) == true)
					{
						allPos.Add(leftDiagonal);
					}
				}
			}

			IEnumerable<BoardPosition> positionsBishop = GetPositionsOfPiece(ChessPieceType.Bishop, byPlayer);
			//BISHOPS
			foreach (BoardPosition posBish in positionsBishop)
			{
				bool check1 = false;
				bool check2 = false;
				bool check3 = false;
				bool check4 = false;
				BoardPosition posLU;
				BoardPosition posRU;
				BoardPosition posLD;
				BoardPosition posRD;

				if (byPlayer == 1)
				{
					for (int i = 1; i <= 8; i++)
					{
						posLU = new BoardPosition(posBish.Row - i, posBish.Col - i);
						if (PositionInBounds(posLU) && check1 == false)
						{
							if (!PositionIsEmpty(posLU))
							{
								check1 = true;
							}
							allPos.Add(posLU);

						}

						posRU = new BoardPosition(posBish.Row - i, posBish.Col + i);

						if (PositionInBounds(posRU) && check2 == false)
						{
							if (!PositionIsEmpty(posRU))
							{
								check2 = true;
							}
							allPos.Add(posRU);
						}

						posLD = new BoardPosition(posBish.Row + i, posBish.Col - i);

						if (PositionInBounds(posLD) && check3 == false)
						{
							if (!PositionIsEmpty(posLD))
							{
								check3 = true;
							}
							allPos.Add(posLD);
						}

						posRD = new BoardPosition(posBish.Row + i, posBish.Col + i);
						if (PositionInBounds(posRD) && check4 == false)
						{
							if (!PositionIsEmpty(posRD))
							{
								check4 = true;
							}
							allPos.Add(posRD);
						}
					}
				}

				if (byPlayer == 2)
				{
					for (int i = 1; i <= 8; i++)
					{
						posLU = new BoardPosition(posBish.Row + i, posBish.Col + i);
						if (PositionInBounds(posLU) && check1 == false)
						{
							if (!PositionIsEmpty(posLU))
							{
								check1 = true;
							}
							allPos.Add(posLU);

						}

						posRU = new BoardPosition(posBish.Row + i, posBish.Col - i);

						if (PositionInBounds(posRU) && check2 == false)
						{
							if (!PositionIsEmpty(posRU))
							{
								check2 = true;
							}
							allPos.Add(posRU);
						}

						posLD = new BoardPosition(posBish.Row - i, posBish.Col + i);

						if (PositionInBounds(posLD) && check3 == false)
						{
							if (!PositionIsEmpty(posLD))
							{
								check3 = true;
							}
							allPos.Add(posLD);
						}

						posRD = new BoardPosition(posBish.Row - i, posBish.Col - i);
						if (PositionInBounds(posRD) && check4 == false)
						{
							if (!PositionIsEmpty(posRD))
							{
								check4 = true;
							}
							allPos.Add(posRD);
						}
					}

				}
			}
			//ROOKS
			foreach (BoardPosition posRook in GetPositionsOfPiece(ChessPieceType.Rook, byPlayer))
			{
				bool check1 = false;
				bool check2 = false;
				bool check3 = false;
				bool check4 = false;
				BoardPosition posLefts;
				BoardPosition posRights;
				BoardPosition posUps;
				BoardPosition posDowns;


				if (byPlayer == 1)
				{
					for (int i = 1; i <= /*board.Length*/8; i++)
					{
						posRights = new BoardPosition(posRook.Row, posRook.Col + i);
						if (PositionInBounds(posRights) && check1 == false)
						{
							if (!PositionIsEmpty(posRights))
							{
								check1 = true;
							}
							allPos.Add(posRights);
						}
						posLefts = new BoardPosition(posRook.Row, posRook.Col - i);
						if (PositionInBounds(posLefts) && check2 == false)
						{
							if (!PositionIsEmpty(posLefts))
							{
								check2 = true;
							}
							allPos.Add(posLefts);
						}
						posUps = new BoardPosition(posRook.Row - i, posRook.Col);
						if (PositionInBounds(posUps) && check3 == false)
						{
							if (!PositionIsEmpty(posUps))
							{
								check3 = true;
							}
							allPos.Add(posUps);
						}
						posDowns = new BoardPosition(posRook.Row + i, posRook.Col);
						if (PositionInBounds(posDowns) && check4 == false)
						{
							if (!PositionIsEmpty(posDowns))
							{
								check4 = true;
							}
							allPos.Add(posDowns);
						}
					}
				}

				if (byPlayer == 2)
				{
					for (int i = 1; i <= /*board.Length*/8; i++)
					{
						posRights = new BoardPosition(posRook.Row, posRook.Col - i);
						if (PositionInBounds(posRights) && check1 == false)
						{
							if (!PositionIsEmpty(posRights))
							{
								check1 = true;
							}
							allPos.Add(posRights);
						}
						posLefts = new BoardPosition(posRook.Row, posRook.Col + i);
						if (PositionInBounds(posLefts) && check2 == false)
						{
							if (!PositionIsEmpty(posLefts))
							{
								check2 = true;
							}
							allPos.Add(posLefts);
						}
						posUps = new BoardPosition(posRook.Row + i, posRook.Col);
						if (PositionInBounds(posUps) && check3 == false)
						{
							if (!PositionIsEmpty(posUps))
							{
								check3 = true;
							}
							allPos.Add(posUps);
						}
						posDowns = new BoardPosition(posRook.Row - i, posRook.Col);
						if (PositionInBounds(posDowns) && check4 == false)
						{
							if (!PositionIsEmpty(posDowns))
							{
								check4 = true;
							}
							allPos.Add(posDowns);
						}
					}
				}
			}
			IEnumerable<BoardPosition> positionsKings = GetPositionsOfPiece(ChessPieceType.King, byPlayer);
			//KINGS
			foreach (BoardPosition posK in positionsKings)
			{
				BoardPosition upOne, downOne, leftOne, rightOne, leftUp, rightUp, leftDown, rightDown;

				if (byPlayer == 1)
				{
					upOne = new BoardPosition(posK.Row - 1, posK.Col);
					if (PositionInBounds(upOne))
					{
						allPos.Add(upOne);
					}
					downOne = new BoardPosition(posK.Row + 1, posK.Col);
					if (PositionInBounds(downOne))
					{
						allPos.Add(downOne);
					}
					leftOne = new BoardPosition(posK.Row, posK.Col - 1);
					if (PositionInBounds(leftOne))
					{
						allPos.Add(leftOne);
					}
					rightOne = new BoardPosition(posK.Row, posK.Col + 1);
					if (PositionInBounds(rightOne))
					{
						allPos.Add(rightOne);
					}
					leftUp = new BoardPosition(posK.Row - 1, posK.Col - 1);
					if (PositionInBounds(leftUp))
					{
						allPos.Add(leftUp);
					}
					rightUp = new BoardPosition(posK.Row - 1, posK.Col + 1);
					if (PositionInBounds(rightUp))
					{
						allPos.Add(rightUp);
					}
					leftDown = new BoardPosition(posK.Row + 1, posK.Col - 1);
					if (PositionInBounds(leftDown))
					{
						allPos.Add(leftDown);
					}
					rightDown = new BoardPosition(posK.Row + 1, posK.Col + 1);
					if (PositionInBounds(rightDown))
					{
						allPos.Add(rightDown);
					}
				}

				if (byPlayer == 2)
				{
					upOne = new BoardPosition(posK.Row + 1, posK.Col);
					if (PositionInBounds(upOne))
					{
						allPos.Add(upOne);
					}
					downOne = new BoardPosition(posK.Row - 1, posK.Col);
					if (PositionInBounds(downOne))
					{
						allPos.Add(downOne);
					}
					leftOne = new BoardPosition(posK.Row, posK.Col + 1);
					if (PositionInBounds(leftOne))
					{
						allPos.Add(leftOne);
					}
					rightOne = new BoardPosition(posK.Row, posK.Col - 1);
					if (PositionInBounds(rightOne))
					{
						allPos.Add(rightOne);
					}
					leftUp = new BoardPosition(posK.Row + 1, posK.Col + 1);
					if (PositionInBounds(leftUp))
					{
						allPos.Add(leftUp);
					}
					rightUp = new BoardPosition(posK.Row + 1, posK.Col - 1);
					if (PositionInBounds(rightUp))
					{
						allPos.Add(rightUp);
					}
					leftDown = new BoardPosition(posK.Row - 1, posK.Col + 1);
					if (PositionInBounds(leftDown))
					{
						allPos.Add(leftDown);
					}
					rightDown = new BoardPosition(posK.Row - 1, posK.Col - 1);
					if (PositionInBounds(rightDown))
					{
						allPos.Add(rightDown);
					}
				}
			}

			//QUEENS
			foreach (BoardPosition posQ in GetPositionsOfPiece(ChessPieceType.Queen, byPlayer))
			{

				bool check1 = false;
				bool check2 = false;
				bool check3 = false;
				bool check4 = false;
				BoardPosition posLU;
				BoardPosition posRU;
				BoardPosition posLD;
				BoardPosition posRD;
				bool check5 = false;
				bool check6 = false;
				bool check7 = false;
				bool check8 = false;
				BoardPosition posLefts;
				BoardPosition posRights;
				BoardPosition posUps;
				BoardPosition posDowns;

				if (byPlayer == 1)
				{
					for (int i = 1; i <= 8; i++)
					{
						posLU = new BoardPosition(posQ.Row - i, posQ.Col - i);
						if (PositionInBounds(posLU) && check1 == false)
						{
							if (!PositionIsEmpty(posLU))
							{
								check1 = true;
							}
							allPos.Add(posLU);

						}

						posRU = new BoardPosition(posQ.Row - i, posQ.Col + i);

						if (PositionInBounds(posRU) && check2 == false)
						{
							if (!PositionIsEmpty(posRU))
							{
								check2 = true;
							}
							allPos.Add(posRU);
						}

						posLD = new BoardPosition(posQ.Row + i, posQ.Col - i);

						if (PositionInBounds(posLD) && check3 == false)
						{
							if (!PositionIsEmpty(posLD))
							{
								check3 = true;
							}
							allPos.Add(posLD);
						}

						posRD = new BoardPosition(posQ.Row + i, posQ.Col + i);
						if (PositionInBounds(posRD) && check4 == false)
						{
							if (!PositionIsEmpty(posRD))
							{
								check4 = true;
							}
							allPos.Add(posRD);
						}
						posRights = new BoardPosition(posQ.Row, posQ.Col + i);
						if (PositionInBounds(posRights) && check5 == false)
						{
							if (!PositionIsEmpty(posRights))
							{
								check5 = true;
							}
							allPos.Add(posRights);
						}
						posLefts = new BoardPosition(posQ.Row, posQ.Col - i);
						if (PositionInBounds(posLefts) && check6 == false)
						{
							if (!PositionIsEmpty(posLefts))
							{
								check6 = true;
							}
							allPos.Add(posLefts);
						}
						posUps = new BoardPosition(posQ.Row - i, posQ.Col);
						if (PositionInBounds(posUps) && check7 == false)
						{
							if (!PositionIsEmpty(posUps))
							{
								check7 = true;
							}
							allPos.Add(posUps);
						}
						posDowns = new BoardPosition(posQ.Row + i, posQ.Col);
						if (PositionInBounds(posDowns) && check8 == false)
						{
							if (!PositionIsEmpty(posDowns))
							{
								check8 = true;
							}
							allPos.Add(posDowns);
						}
					}
				}

				if (byPlayer == 2)
				{
					for (int i = 1; i <= 8; i++)
					{
						posLU = new BoardPosition(posQ.Row + i, posQ.Col + i);
						if (PositionInBounds(posLU) && check1 == false)
						{
							if (!PositionIsEmpty(posLU))
							{
								check1 = true;
							}
							allPos.Add(posLU);

						}

						posRU = new BoardPosition(posQ.Row + i, posQ.Col - i);

						if (PositionInBounds(posRU) && check2 == false)
						{
							if (!PositionIsEmpty(posRU))
							{
								check2 = true;
							}
							allPos.Add(posRU);
						}

						posLD = new BoardPosition(posQ.Row - i, posQ.Col + i);

						if (PositionInBounds(posLD) && check3 == false)
						{
							if (!PositionIsEmpty(posLD))
							{
								check3 = true;
							}
							allPos.Add(posLD);
						}

						posRD = new BoardPosition(posQ.Row - i, posQ.Col - i);
						if (PositionInBounds(posRD) && check4 == false)
						{
							if (!PositionIsEmpty(posRD))
							{
								check4 = true;
							}
							allPos.Add(posRD);
						}
						posRights = new BoardPosition(posQ.Row, posQ.Col - i);
						if (PositionInBounds(posRights) && check5 == false)
						{
							if (!PositionIsEmpty(posRights))
							{
								check5 = true;
							}
							allPos.Add(posRights);
						}
						posLefts = new BoardPosition(posQ.Row, posQ.Col + i);
						if (PositionInBounds(posLefts) && check6 == false)
						{
							if (!PositionIsEmpty(posLefts))
							{
								check6 = true;
							}
							allPos.Add(posLefts);
						}
						posUps = new BoardPosition(posQ.Row + i, posQ.Col);
						if (PositionInBounds(posUps) && check7 == false)
						{
							if (!PositionIsEmpty(posUps))
							{
								check7 = true;
							}
							allPos.Add(posUps);
						}
						posDowns = new BoardPosition(posQ.Row - i, posQ.Col);
						if (PositionInBounds(posDowns) && check8 == false)
						{
							if (!PositionIsEmpty(posDowns))
							{
								check8 = true;
							}
							allPos.Add(posDowns);
						}
					}
				}
			}

			//KNIGHTS
			foreach (BoardPosition posKni in GetPositionsOfPiece(ChessPieceType.Knight, byPlayer))
			{
				//two lower left diagonals
				BoardPosition posLD1;
				BoardPosition posLD2;
				//two upper left diagonals
				BoardPosition posLU1;
				BoardPosition posLU2;
				//two lower right diagonals
				BoardPosition posRD1;
				BoardPosition posRD2;
				//two upper right diagonals
				BoardPosition posRU1;
				BoardPosition posRU2;


				if (byPlayer == 1)
				{
					posLD1 = new BoardPosition(posKni.Row + 2, posKni.Col - 1);
					posLD2 = new BoardPosition(posKni.Row + 1, posKni.Col - 2);
					posLU1 = new BoardPosition(posKni.Row - 1, posKni.Col - 2);
					posLU2 = new BoardPosition(posKni.Row - 2, posKni.Col - 1);
					posRD1 = new BoardPosition(posKni.Row + 2, posKni.Col + 1);
					posRD2 = new BoardPosition(posKni.Row + 1, posKni.Col + 2);
					posRU1 = new BoardPosition(posKni.Row - 1, posKni.Col + 2);
					posRU2 = new BoardPosition(posKni.Row - 2, posKni.Col + 1);

					if (PositionInBounds(posLD1))
					{
						allPos.Add(posLD1);
					}
					if (PositionInBounds(posLD2))
					{
						allPos.Add(posLD2);
					}
					if (PositionInBounds(posLU1))
					{
						allPos.Add(posLU1);
					}
					if (PositionInBounds(posLU2))
					{
						allPos.Add(posLU2);
					}
					if (PositionInBounds(posRD1))
					{
						allPos.Add(posRD1);
					}
					if (PositionInBounds(posRD2))
					{
						allPos.Add(posRD2);
					}
					if (PositionInBounds(posRU1))
					{
						allPos.Add(posRU1);
					}
					if (PositionInBounds(posRU2))
					{
						allPos.Add(posRU2);
					}
				}

				if (byPlayer == 2)
				{
					posLD1 = new BoardPosition(posKni.Row - 2, posKni.Col + 1);
					posLD2 = new BoardPosition(posKni.Row - 1, posKni.Col + 2);
					posLU1 = new BoardPosition(posKni.Row + 1, posKni.Col + 2);
					posLU2 = new BoardPosition(posKni.Row + 2, posKni.Col + 1);
					posRD1 = new BoardPosition(posKni.Row - 2, posKni.Col - 1);
					posRD2 = new BoardPosition(posKni.Row - 1, posKni.Col - 2);
					posRU1 = new BoardPosition(posKni.Row + 1, posKni.Col - 2);
					posRU2 = new BoardPosition(posKni.Row + 2, posKni.Col - 1);

					if (PositionInBounds(posLD1))
					{
						allPos.Add(posLD1);
					}
					if (PositionInBounds(posLD2))
					{
						allPos.Add(posLD2);
					}
					if (PositionInBounds(posLU1))
					{
						allPos.Add(posLU1);
					}
					if (PositionInBounds(posLU2))
					{
						allPos.Add(posLU2);
					}
					if (PositionInBounds(posRD1))
					{
						allPos.Add(posRD1);
					}
					if (PositionInBounds(posRD2))
					{
						allPos.Add(posRD2);
					}
					if (PositionInBounds(posRU1))
					{
						allPos.Add(posRU1);
					}
					if (PositionInBounds(posRU2))
					{
						allPos.Add(posRU2);
					}
				}
			}
			return allPos;
		}

		#endregion

		//DONE
		#region Private methods.
		/// <summary>
		/// Mutates the board state so that the given piece is at the given position.
		/// </summary>
		private void SetPieceAtPosition(BoardPosition position, ChessPiece piece)
		{


			//stores the byte associated with the new move
			int updatedByte = 0;

			//holds the value for the left 4 bits of the byte
			int left = 0;

			//holds the value for the right 4 bits of the byte
			int right = 0;

			//locates the index in the array for the destination position
			int index = calculateIndex(position.Row, position.Col);

			//extracts the current byte at that index
			var bytes = board[index];

			//assigning with the leftmost 4 bits
			left = bytes >> 4;

			//assigning with the rightmost 4 bits
			right = bytes & 0x0F;


			//determines the value of the new piece type to place at the new location
			int pieceType = piece.PieceType == ChessPieceType.King ? 6
				: piece.PieceType == ChessPieceType.Queen ? 5
				: piece.PieceType == ChessPieceType.Bishop ? 4
				: piece.PieceType == ChessPieceType.Knight ? 3
				: piece.PieceType == ChessPieceType.Rook ? 2
				: piece.PieceType == ChessPieceType.Pawn ? 1
				: 0;

			//is the destination on an even numbered column?
			if ((position.Col) % 2 == 0)
			{
				if (piece.Player >= 1)
				{
					//override the original left 4 bits
					left = (byte)((piece.Player - 1) << 3) + (byte)(pieceType);
				}
				else
				{
					left = 0;
				}
			}
			//it's an odd numbered column...
			else
			{
				if (piece.Player >= 1)
				{
					//override the original right 4 bits
					right = (byte)((piece.Player - 1) << 3) + pieceType;
				}
				else
				{
					right = 0;
				}
			}

			//combine the left and right parts
			updatedByte = ((byte)(left << 4) + (byte)(right));

			//place back in the array
			board[index] = (byte)(updatedByte);

		}

		//DONE
		private int calculateIndex(int row, int col)
		{
			return (4 * (row)) + ((col) / 2);
		}

		#endregion

		#region Explicit IGameBoard implementations.
		IEnumerable<IGameMove> IGameBoard.GetPossibleMoves()
		{
			return GetPossibleMoves();
		}
		void IGameBoard.ApplyMove(IGameMove m)
		{
			ApplyMove(m as ChessMove);
		}
		IReadOnlyList<IGameMove> IGameBoard.MoveHistory => mMoveHistory;
		#endregion

		// You may or may not need to add code to this constructor.
		public ChessBoard()
		{
			board = new byte[] { 171, 205, 236, 186,
								 153, 153, 153, 153,
								0, 0, 0, 0,
								0, 0, 0, 0,
								0, 0, 0, 0,
								0, 0, 0, 0,
								17, 17, 17, 17,
								35, 69, 100, 50};
		}

		public ChessBoard(IEnumerable<Tuple<BoardPosition, ChessPiece>> startingPositions)
			: this()
		{
			var king1 = startingPositions.Where(t => t.Item2.Player == 1 && t.Item2.PieceType == ChessPieceType.King);
			var king2 = startingPositions.Where(t => t.Item2.Player == 2 && t.Item2.PieceType == ChessPieceType.King);
			if (king1.Count() != 1 || king2.Count() != 1)
			{
				throw new ArgumentException("A chess board must have a single king for each player");
			}

			foreach (var position in BoardPosition.GetRectangularPositions(8, 8))
			{
				SetPieceAtPosition(position, ChessPiece.Empty);
			}

			int[] values = { 0, 0 };
			foreach (var pos in startingPositions)
			{
				SetPieceAtPosition(pos.Item1, pos.Item2);
				// TODO: you must calculate the overall advantage for this board, in terms of the pieces
				// that the board has started with. "pos.Item2" will give you the chess piece being placed
				// on this particular position.
				if (pos.Item2.Player == 1)
				{
					if (pos.Item2.PieceType == ChessPieceType.Pawn)
					{
						advValue -= 1;
					}
					if (pos.Item2.PieceType == ChessPieceType.Knight || pos.Item2.PieceType == ChessPieceType.Bishop)
					{
						advValue -= 3;
					}
					if (pos.Item2.PieceType == ChessPieceType.Rook)
					{
						advValue -= 5;
					}
					if (pos.Item2.PieceType == ChessPieceType.Queen)
					{
						advValue -= 9;
					}
				}
				if (pos.Item2.Player == 2)
				{
					if (pos.Item2.PieceType == ChessPieceType.Pawn)
					{
						advValue += 1;
					}
					if (pos.Item2.PieceType == ChessPieceType.Knight || pos.Item2.PieceType == ChessPieceType.Bishop)
					{
						advValue += 3;
					}
					if (pos.Item2.PieceType == ChessPieceType.Rook)
					{
						advValue += 5;
					}
					if (pos.Item2.PieceType == ChessPieceType.Queen)
					{
						advValue += 9;
					}
				}
			}
		}
	}
}
