using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static YogiBear.Persistence.Character;

namespace YogiBear.Persistence
{
    public class YogiBoard : IYogiBoard
    {
        private Player yogi;
        private List<Ranger> rangers;
        private Pieces[,] boardPieces;
        private int basketCount;
        private int boardSize;

        public Player Yogi { get { return yogi; } }
        public List<Ranger> Rangers
        {
            get
            {
                List<Ranger> copy = new List<Ranger>();
                foreach (Ranger ranger in rangers)
                {
                    copy.Add((Ranger)(Copy(ranger)));
                }
                return copy;
            }
        }
        public Pieces[,] BoardPieces
        {
            get
            {
                Pieces[,] copy = new Pieces[boardSize, boardSize];
                for (int i = 0; i < boardSize; i++)
                {
                    for (int j = 0; j < boardSize; j++)
                    {
                        copy[i, j] = Copy(boardPieces[i, j]);
                    }
                }
                return copy;
            }
        }

        public Pieces Copy(Pieces piece)
        {
            if (piece == null)
            {
                return null!;
            }
            switch (piece)
            {
                case Player p:
                    return new Player(p.X, p.Y);
                case Ranger r:
                    return new Ranger(r.X, r.Y, r.Axis);
                case Item i:
                    return new Item(i.Type, i.X, i.Y);
                default:
                    throw new ArgumentException("Invalid item type", nameof(piece));
            }

        }

        public Pieces CurrentPieceCopy(int x, int y)
        {
            return Copy(boardPieces[x, y]);
        }

        public int BasketCount { get { return basketCount; } }
        public int BoardSize { get { return boardSize; } }
        public int RangerCount { get { return rangers.Count; } }

        public YogiBoard(int boardSize, int basketCount)
        {
            if (boardSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardSize), "The board dimensions must be greater than 0.");
            if (basketCount < 0)
                throw new ArgumentOutOfRangeException(nameof(basketCount), "Counts for baskets cannot be negative.");
            this.basketCount = basketCount;
            this.boardSize = boardSize;
            yogi = new Player(0, 0);
            rangers = new List<Ranger> { };
            boardPieces = new Pieces[boardSize, boardSize];
            boardPieces[0,0] = yogi;
            
        }
        public YogiBoard(Pieces[,] copyPieces, int basketCount)
        {
            if (copyPieces == null)
                throw new NullReferenceException(nameof(copyPieces));
            this.basketCount = basketCount;
            this.boardSize = copyPieces.GetLength(0);
            yogi = new Player(0, 0);
            rangers = new List<Ranger> { };
            boardPieces = copyPieces;
        }

        public void SetBoardPiece(int x, int y, Pieces piece)
        {
            if (x < 0 || x >= boardPieces.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= boardPieces.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");
            if (piece == null)
                throw new ArgumentNullException(nameof(piece), "The argument was null.");
            boardPieces[x, y] = piece;
            if (piece is Ranger)
            {
                rangers.Add((Ranger)piece);
            }
            else if (piece is Player player)
            {
                yogi = player;
                boardPieces[0, 0] = null!;
            }
        }
        public bool ChangeYogiPosition(Direction direction)
        {
            bool collectible;
            if (CanCharacterMove(direction, yogi, out collectible))
            {
                boardPieces[yogi.X, yogi.Y] = null!;
                yogi.CallMove(direction);
                boardPieces[yogi.X, yogi.Y] = yogi;
            }
            return collectible;
        }
        public void ChangeRangerPosition(int i, out int prevX, out int prevY, out int newX, out int newY)
        {
            Ranger ranger = rangers[i];
            prevX = ranger.X;
            prevY = ranger.Y;
            bool collectible;
            if (CanCharacterMove(ranger.FixedPivot, ranger, out collectible))
            {
                boardPieces[ranger.X, ranger.Y] = null!;

                if (ranger.SteppedOnCollectible && ranger.MyCollectible != null &&
                (ranger.X == ranger.MyCollectible.X && ranger.Y == ranger.MyCollectible.Y))
                {
                    boardPieces[ranger.MyCollectible.X, ranger.MyCollectible.Y] = ranger.MyCollectible;
                    ranger.SteppedOnCollectible = false;
                    ranger.MyCollectible = null!;
                }

                ranger.CallMove();
                if (collectible && boardPieces[ranger.X, ranger.Y] is Item item)
                {
                    ranger.MyCollectible = item;
                    ranger.SteppedOnCollectible = true;
                }

                boardPieces[ranger.X, ranger.Y] = ranger;
            }
            else
            {
                ranger.ChangePivot();
            }
            newX = ranger.X;
            newY = ranger.Y;
        }
        private bool CanCharacterMove(Direction direction, Character character, out bool collectible)
        {
            collectible = false;
            int testX = character.X;
            int testY = character.Y;

            switch (direction)
            {
                case Direction.UP:
                    testX--;
                    break;
                case Direction.DOWN:
                    testX++;
                    break;
                case Direction.LEFT:
                    testY--;
                    break;
                case Direction.RIGHT:
                    testY++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), $"Invalid direction: {direction}");
            }

            if (testX < 0 || testX >= boardPieces.GetLength(0) || testY < 0 || testY >= boardPieces.GetLength(1))
                return false;

            if (boardPieces[testX, testY] != null)
                return (collectible = IsItCollectible(boardPieces[testX, testY]));

            return true;
        }
        public bool IsItCollectible(Pieces piece)
        {
            if (piece is Item item)
            {
                return item.AmICollectible();
            }
            return false;
        }
        public bool RangerCaughtYogi(int x, int y)
        {
            return Math.Abs(x - yogi.X) <= 1 && Math.Abs(y - yogi.Y) <= 1;   
        }
        public bool AnyRangerCaughtYogi()
        {
            foreach(Ranger ranger in rangers)
            {
                if (RangerCaughtYogi(ranger.X, ranger.Y)) return true;
            }
            return false;
        }
    }

}
