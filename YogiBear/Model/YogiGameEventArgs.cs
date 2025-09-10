using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBear.Model
{
    public class YogiGameEventArgs : EventArgs
    {
        private int score;
        private bool isWon;
        private int gameTimeElapsed;

        public int Score { get { return score; } }

        public bool IsWon { get { return isWon; } }

        public int GameTimeElapsed { get { return gameTimeElapsed; } } 
        public YogiGameEventArgs(bool isWon, int score, int gameTimeElapsed)
        {
            this.isWon = isWon;
            this.score = score;
            this.gameTimeElapsed = gameTimeElapsed;
        }
    }
}
