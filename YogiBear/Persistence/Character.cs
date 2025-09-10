using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBear.Persistence
{

    public enum Direction
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }
    public class Character : Pieces
    {
        public Character(int x, int y) : base(x, y) { }

        private void Move(Direction direction)
        {
            switch (direction)
            {
                case Direction.UP:
                    X -= 1;
                    break;
                case Direction.DOWN:
                    X += 1;
                    break;
                case Direction.LEFT:
                    Y -= 1;
                    break;
                case Direction.RIGHT:
                    Y += 1;
                    break;
                default:
                    throw new ArgumentException(nameof(direction), $"Invalid direction: {direction}");
            }
        }

        public virtual void CallMove(Direction direction) { }
        public virtual void CallMove() { }

        public class Player : Character
        {
            public Player(int x, int y) : base(x, y) { }

            private static Player? Instance;

            public static Player GetInstance(int x, int y)
            {
                if (Instance == null)
                {
                    Instance = new Player(x, y);
                }
                return Instance;

            }

            public override void CallMove(Direction direction) 
            {
                base.Move(direction);
            }

        }
        public class Ranger : Character
        {
            public Direction FixedPivot { get; private set; }
            public string Axis { get; private set; }
            public Item MyCollectible {  get; set; } 
            public bool SteppedOnCollectible { get; set; }

            public Ranger(int x, int y, string axis) : base(x, y) 
            {
                switch(axis)
                {
                    case "u":
                        FixedPivot = Direction.UP;
                        break;
                    case "d":
                        FixedPivot = Direction.DOWN;
                        break;
                    case "l":
                        FixedPivot = Direction.LEFT;
                        break;
                    case "r":
                        FixedPivot = Direction.RIGHT;
                        break;
                    default:
                        throw new ArgumentException(nameof(axis), $"Invalid direction: {FixedPivot}");
                }
                MyCollectible = null!;
                SteppedOnCollectible = false;
                Axis = axis;
            }
            public void ChangePivot()
            {
                switch (FixedPivot)
                {
                    case Direction.UP:
                        FixedPivot = Direction.DOWN;
                        Axis = "d";
                        break;
                    case Direction.DOWN:
                        FixedPivot = Direction.UP;
                        Axis = "u";
                        break;
                    case Direction.LEFT:
                        FixedPivot = Direction.RIGHT;
                        Axis = "r";
                        break;
                    case Direction.RIGHT:
                        FixedPivot = Direction.LEFT;
                        Axis = "l";
                        break;
                    default:
                        throw new ArgumentException(nameof(FixedPivot), $"Invalid direction: {FixedPivot}");
                }
            }
            public override void CallMove()
            {
                base.Move(FixedPivot);
            }
        }
    }
}
