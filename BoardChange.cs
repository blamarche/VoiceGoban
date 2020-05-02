using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STTGoPlayer
{ 
    public class BoardChange
    {
        public int X { get; }
        public int Y { get; }
        public BoardState.Stone Stone { get; }
        public BoardState.Stone PreviousStone { get; }

        public BoardChange(int xcoord, int ycoord, BoardState.Stone s, BoardState.Stone prevs)
        {
            Stone = s;
            PreviousStone = prevs;
            X = xcoord;
            Y = ycoord;
        }
    }
}
