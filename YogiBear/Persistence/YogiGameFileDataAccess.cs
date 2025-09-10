using System.Runtime.Remoting;
using System.Security.Principal;
using System.Transactions;
using YogiBear.Model;
using static YogiBear.Persistence.Character;

namespace YogiBear.Persistence
{
    public class YogiGameFileDataAccess : IYogiGameDataAccess
    {
        public async Task<IYogiBoard> LoadAsync(string path)
        {
			try
			{
				using(StreamReader reader = new StreamReader(path))
				{
					string line = await reader.ReadLineAsync() ?? String.Empty;
					string[] tokens = line.Split(' ');
					int boardSize = int.Parse(tokens[0]);
					int basketCount = int.Parse(tokens[1]);
					YogiBoard board = new YogiBoard(boardSize, basketCount);
                    if (board.BoardSize == 0) throw new Exception();
                    
                    while (!reader.EndOfStream)
					{
                        line = await reader.ReadLineAsync() ?? String.Empty;
                        tokens = line.Split(' ');
                        Pieces current;
                        int x = int.Parse(tokens[1]);
                        int y = int.Parse(tokens[2]);
                        switch (tokens[0])
                        {
                            case "Y":
                                current = new Player(x, y);
                                break;
                            case "R":
                                current = new Ranger(x, y, tokens[3]);
                                break;
                            case "P":
                                current = new Item(ItemType.PICNICBASKET, x, y);
                                break;
                            case "T":
                                current = new Item(ItemType.TREE, x, y);
                                break;
                            default:
                                throw new ArgumentException(nameof(tokens), $"Invalid piece type: {tokens[0]}");
                        }
                        board.SetBoardPiece(x, y, current);
                    }
                    return board;
                }
			}
			catch(Exception e)
			{
                throw new YogiBoardDataException("Failed to load the Yogi board from file.", e);
            }
        }

        public async Task SaveAsync(string path, IYogiBoard board, int collectedBasketCount)
        {
            try
            {
                using(StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(board.BoardSize);
                    await writer.WriteLineAsync(" " + (board.BasketCount - collectedBasketCount));
                    for (int i = 0; i < board.BoardSize; i++)
                    {
                        for (int j = 0; j < board.BoardSize; j++)
                        {
                            string line = string.Empty;
                            if (board.BoardPieces![i, j] != null)
                            {

                                Pieces current = board.BoardPieces[i, j];
                                switch (current)
                                {
                                    case Player:
                                        line = "Y " + current.X + " " + current.Y;
                                        break;
                                    case Ranger r:
                                        line = "R " + r.X + " " + r.Y + " " + r.Axis;
                                        break;
                                    case Item it:
                                        line = it.WhatAmI() + " " + it.X + " " + it.Y;
                                        break;
                                    default:
                                        throw new ArgumentException($"Unknown piece type: {current.GetType().Name}");
                                }
                             await writer.WriteLineAsync(line);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new YogiBoardDataException("Failed to save the Yogi board to file.", e);
            }
        }
    }
}
