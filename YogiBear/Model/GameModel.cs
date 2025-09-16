using System;
using System.Diagnostics;
using System.Threading.Tasks;
using YogiBear.Persistence;
using static YogiBear.Persistence.Character;

namespace YogiBear.Model
{
    public class GameModel : IDisposable
    {
        private IYogiGameDataAccess dataAccess;
        private int basketCount;
        private int boardSize;
        private IYogiBoard board = null!;
        private int collectedBasketCount;
        private bool isGameOver;

        private IBasicTimer rangerMoveTimer;
        private IBasicTimer gameTimeTimer;
        private int gameTimeElapsed;

        public GameModel(IYogiGameDataAccess dataAccess, IBasicTimer rangerTimer, IBasicTimer gameTimer)
        {
            this.dataAccess = dataAccess;
            this.rangerMoveTimer = rangerTimer;
            this.gameTimeTimer = gameTimer;

            basketCount = 0;
            collectedBasketCount = 0;
            isGameOver = false;
            gameTimeElapsed = 0;
            boardSize = 0;

            rangerMoveTimer.Interval = 600;
            rangerMoveTimer.Elapsed += OnRangerMove;

            gameTimeTimer.Interval = 1000;
            gameTimeTimer.Elapsed += OnGameTimeTick;
        }

        public GameModel(IYogiBoard board, IBasicTimer rangerTimer, IBasicTimer gameTimer)
        {
            this.dataAccess = new YogiGameFileDataAccess();
            this.rangerMoveTimer = rangerTimer;
            this.gameTimeTimer = gameTimer;
            this.board = board;

            basketCount = board.BasketCount;
            collectedBasketCount = 0;
            isGameOver = false;
            gameTimeElapsed = 0;
            boardSize = board.BoardSize;

            rangerMoveTimer.Interval = 300;
            rangerMoveTimer.Elapsed += OnRangerMove;

            gameTimeTimer.Interval = 1000;
            gameTimeTimer.Elapsed += OnGameTimeTick;
        }
        public int BoardSize {  get { return boardSize; } }
        public int BasketCount {  get { return basketCount; } }
        public int CollectedBasketCount { get { return collectedBasketCount; } }
        public int GameTimeElapsed { get { return gameTimeElapsed; } }
        public bool IsGameOver { get; }

        public Pieces GetCurrentPiece(int x, int y)
        {
            return board.CurrentPieceCopy(x, y);
        }

        public event EventHandler<YogiGameEventArgs>? GameAdvanced;
        public event EventHandler<YogiGameEventArgs>? GameOver;
        public event EventHandler<YogiGameFieldEventArgs>? GameFieldChanged;
        public event EventHandler? BasketCollected;

        public async Task LoadGameAsync(string path)
        {
            if (dataAccess == null)
                throw new InvalidOperationException("Data access not initialized.");
            board = await dataAccess.LoadAsync(path);
            if(board != null)
            {
                boardSize = board.BoardSize;
                basketCount = board.BasketCount;
            }
        }

        public async Task SaveGameAsync(string path)
        {
            if (dataAccess == null)
                throw new InvalidOperationException("Data access not initialized.");
            await dataAccess.SaveAsync(path, board, collectedBasketCount);
        }

        public void Step(Direction direction)
        {
            if (isGameOver) return;

            MoveYogi(direction);

            if (collectedBasketCount >= basketCount)
            {
                OnGameOver(true);
                return;
            }

            OnGameAdvanced();
        }

        private void MoveYogi(Direction direction)
        {
            int prevX = board.Yogi.X;
            int prevY = board.Yogi.Y;

            if (board.ChangeYogiPosition(direction))
            {
                collectedBasketCount++;
                BasketCollected?.Invoke(this, EventArgs.Empty);
            }

            GameFieldChanged?.Invoke(this, new YogiGameFieldEventArgs(prevX, prevY));
            GameFieldChanged?.Invoke(this, new YogiGameFieldEventArgs(board.Yogi.X, board.Yogi.Y));
            if (board.AnyRangerCaughtYogi())
                OnGameOver(false);

        }

        private void MoveRangers()
        {
            for(int i = 0; i < board.RangerCount; i++)
            {
                int prevX;
                int prevY;
                int newX;
                int newY;
                board.ChangeRangerPosition(i, out prevX, out prevY, out newX, out newY);

                GameFieldChanged?.Invoke(this, new YogiGameFieldEventArgs(prevX, prevY));
                GameFieldChanged?.Invoke(this, new YogiGameFieldEventArgs(newX, newY));

                if (board.RangerCaughtYogi(newX, newY))
                {
                    OnGameOver(false);
                }
            }
        }

        private void OnRangerMove(object? sender, EventArgs e)
        {
            if (isGameOver)
            {
                return;
            }

            MoveRangers();
            OnGameAdvanced();
        }

        private void OnGameTimeTick(object? sender, EventArgs e)
        {
            if (isGameOver) return;

            gameTimeElapsed++;
            OnGameAdvanced();
        }

        private void OnGameAdvanced()
        {
            GameAdvanced?.Invoke(this, new YogiGameEventArgs(false, collectedBasketCount, gameTimeElapsed));
        }

        private void OnGameOver(bool won)
        {
            rangerMoveTimer.Stop();
            gameTimeTimer.Stop();
            isGameOver = true;

            GameOver?.Invoke(this, new YogiGameEventArgs(won, collectedBasketCount, gameTimeElapsed));
        }

        public void StartTimers()
        {
            rangerMoveTimer.Start();
            gameTimeTimer.Start();
        }

        public void StopTimers()
        {
            rangerMoveTimer.Stop();
            gameTimeTimer.Stop();
        }
        public void ResetGame()
        {
            isGameOver = false;
            gameTimeElapsed = 0;
            collectedBasketCount = 0;
            rangerMoveTimer.Stop();
            gameTimeTimer.Stop();
        }

        public void Dispose()
        {
            rangerMoveTimer.Elapsed -= OnRangerMove;
            gameTimeTimer.Elapsed -= OnGameTimeTick;

            rangerMoveTimer.Stop();
            gameTimeTimer.Stop();
        }
    }
}
