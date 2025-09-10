using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YogiBear.Model;
using YogiBear.Persistence;
using static YogiBear.Persistence.Character;

namespace YogiBear.Test
{
    [TestClass]
    public class GameModelTest :IDisposable
    {
        private GameModel model = null!;
        private YogiBoard testBoard = null!;

        private bool isGameOver;
        private int collectedBasketCount;
        private bool rangerMove;

        [TestInitialize]
        public void Initialize()
        {
            testBoard = new YogiBoard(9, 5);
            model = new GameModel(testBoard, new BasicTimerAggregation(), new BasicTimerAggregation());
            isGameOver = false;
            collectedBasketCount = 0;
            rangerMove = false;
            model.GameAdvanced += new EventHandler<YogiGameEventArgs>(Model_GameAdvanced!);
            model.GameOver += new EventHandler<YogiGameEventArgs>(Model_GameOver!);
        }

        private void Model_GameAdvanced(object sender, YogiGameEventArgs e)
        {
            Assert.AreEqual(e.Score, model.CollectedBasketCount);
            Assert.IsFalse(e.IsWon);
            collectedBasketCount = e.Score;
            rangerMove = true;
        }

        private void Model_GameOver(object sender, YogiGameEventArgs e)
        {
            collectedBasketCount = e.Score;
            isGameOver = true;
        }
        [TestMethod]
        public void InitializeGameStateTest()
        {
            Assert.AreEqual(5, model.BasketCount);
            Assert.AreEqual(9, model.BoardSize);
            Assert.AreEqual(0, model.CollectedBasketCount);
            Assert.AreEqual(0, model.GameTimeElapsed);
        }
        [TestMethod]
        public void StepDoesNotIncreaseTest()
        {
            for (int i = 0; i < testBoard.BoardSize; i++)
            {
                model.Step(Direction.RIGHT);
                testBoard.ChangeYogiPosition(Direction.RIGHT);
            }
            Assert.AreEqual(0, model.CollectedBasketCount);
            Assert.AreEqual(0, collectedBasketCount);

            Assert.AreEqual(0, testBoard.Yogi.X);
            Assert.AreEqual(8, testBoard.Yogi.Y);
            Assert.IsFalse(isGameOver);
        }
        [TestMethod]
        public void YogiStepIncreasesScoreTest()
        {
            testBoard.SetBoardPiece(1, 1, new Item(ItemType.PICNICBASKET, 1, 1));
            model.Step(Direction.DOWN);
            model.Step(Direction.RIGHT);

            Assert.AreEqual(1, collectedBasketCount);
            Assert.IsFalse(isGameOver);
        }

        [TestMethod]
        public void GameEndsWhenCaughtByRangerTest()
        {
            testBoard.SetBoardPiece(1, 0, new Ranger(1, 0, "d"));
            model.Step(Direction.DOWN);
            Assert.IsTrue(isGameOver);
        }
        [TestMethod]
        public void TimerUpdatesGameTimeTest()
        {
            model.StartTimers();
            Task.Delay(1500).Wait();
            Assert.IsTrue(model.GameTimeElapsed >= 1);
            model.StopTimers();

            int elapsedTime = model.GameTimeElapsed;
            Task.Delay(1000).Wait();
            Assert.AreEqual(elapsedTime, model.GameTimeElapsed);
        }
        [TestMethod]
        public void GetCurrentPieceReturnsCorrectTypeTest()
        {
            Item basket = new Item(ItemType.PICNICBASKET, 2, 2);
            testBoard.SetBoardPiece(2, 2, basket);

            Pieces piece = model.GetCurrentPiece(2, 2);

            Assert.IsNotNull(piece);
            Assert.IsInstanceOfType(piece, typeof(Item));
            Assert.AreEqual(ItemType.PICNICBASKET, ((Item)piece).Type);

            Pieces emptyPiece = model.GetCurrentPiece(8, 8);
            Assert.IsNull(emptyPiece);
        }

        [TestMethod]
        public void RangerMovementCausesGameOverTest()
        {
            testBoard.SetBoardPiece(2, 1, new Ranger(1, 1, "u"));
            model.StartTimers();
            Task.Delay(500).Wait();

            Assert.IsTrue(isGameOver);
        }
        [TestMethod]
        public void YogiCannotMoveThroughWallsTest()
        {
            model.Step(Direction.UP);
            Assert.AreEqual(0, testBoard.Yogi.X);
            Assert.AreEqual(0, testBoard.Yogi.Y);

            model.Step(Direction.LEFT);
            Assert.AreEqual(0, testBoard.Yogi.X);
            Assert.AreEqual(0, testBoard.Yogi.Y);
        }

        [TestMethod]
        public void YogiCannotMoveOntoTreesTest()
        {
            testBoard.SetBoardPiece(0, 1, new Item(ItemType.TREE, 0, 1));
            testBoard.SetBoardPiece(1, 0, new Item(ItemType.TREE, 1, 0));
            model.Step(Direction.RIGHT);

            Assert.AreEqual(0, testBoard.Yogi.X);
            Assert.AreEqual(0, testBoard.Yogi.Y);

            model.Step(Direction.DOWN);
            Assert.AreEqual(0, testBoard.Yogi.X);
            Assert.AreEqual(0, testBoard.Yogi.Y);
        }

        [TestMethod]
        public void RangersReverseAtWallsTest()
        {
            Ranger ranger = new Ranger(0, 8, "r");
            testBoard.SetBoardPiece(0, 8, ranger);

            model.StartTimers();
            Task.Delay(500).Wait();
            Assert.AreEqual("l", ranger.Axis);
            Assert.AreEqual(Direction.LEFT, ranger.FixedPivot);
            Assert.AreEqual(0, ranger.X);
            Assert.AreEqual(8, ranger.Y);
        }
        [TestMethod]
        public void GameWonTest()
        {
            for (int i = 1; i < testBoard.BasketCount + 1; i++)
            {
                testBoard.SetBoardPiece(0, i, new Item(ItemType.PICNICBASKET, 0, i));
                model.Step(Direction.RIGHT);
            }

            Assert.AreEqual(testBoard.BasketCount, model.CollectedBasketCount);
            Assert.AreEqual(testBoard.BasketCount, collectedBasketCount);
            Assert.IsTrue(isGameOver);
        }

        [TestMethod]
        public void RangersReverseAtTreesTest()
        {
            Ranger ranger = new Ranger(2, 2, "d");
            testBoard.SetBoardPiece(2, 2, ranger);
            testBoard.SetBoardPiece(3, 2, new Item(ItemType.TREE, 3, 2));

            model.StartTimers();
            Task.Delay(500).Wait();

            Assert.AreEqual("u" ,ranger.Axis);
            Assert.AreEqual(Direction.UP, ranger.FixedPivot);
            Assert.AreEqual(2, ranger.X);
            Assert.AreEqual(2, ranger.Y);
        }

        [TestMethod]
        public void RangerTimerStartsAndRequestsMovesTest()
        {
            model.StartTimers();
            Task.Delay(500).Wait();

            Assert.IsTrue(rangerMove);
            rangerMove = false;
            model.StopTimers();

            Task.Delay(500).Wait();
            Assert.IsFalse(rangerMove);
        }
        [TestMethod]
        public void RangerDoesNotMoveSooner()
        {
            Ranger ranger = new Ranger(8, 8, "u");
            testBoard.SetBoardPiece(8, 8, ranger);
            model = new GameModel(testBoard, new BasicTimerAggregation(), new BasicTimerAggregation());

            model.StartTimers();
            Task.Delay(200).Wait();
            model.StopTimers();
            Assert.AreEqual(typeof(Ranger), model.GetCurrentPiece(8,8).GetType());
            Assert.IsNull(model.GetCurrentPiece(7,8));
        }



        public void Dispose()
        {
            model.Dispose();
        }
    }
}
