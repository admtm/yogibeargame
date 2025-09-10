using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBear.Persistence
{
    public class Pieces
    {
        public int X { get; protected set; }
        public int Y { get; protected set; }

        public Pieces(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
