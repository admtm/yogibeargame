using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YogiBear.Persistence;

namespace YogiBear.Persistence
{
    public interface IYogiBoard
    {
        bool ChangeYogiPosition(Direction direction);
        void ChangeRangerPosition(int i, out int prevX, out int prevY, out int newX, out int newY);
        bool AnyRangerCaughtYogi();
        bool RangerCaughtYogi(int x, int y);
        int BasketCount { get; }
        int BoardSize { get; }
        int RangerCount { get; }
        Pieces[,] BoardPieces { get; }
        Character.Player Yogi { get; }
        List<Character.Ranger> Rangers { get; }
        Pieces CurrentPieceCopy(int x, int y);
    }
}
