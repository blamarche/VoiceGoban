using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STTGoPlayer
{
    public class BoardState
    {
        public enum Stone
        {
            Empty,
            Black,
            White
        }

        private Stone[,] grid;//0 = empty, 1 = black, -1 = white
        private Stone[,] lastGrid = null;
        private Bitmap lastBoard;
        private Bitmap board;
        private int boardSize;
        private Rectangle bounds;
        private double scaleFactor;
        private int gridSquareW, gridSquareH;

        private int baseBoardGrey = 165;
        private int emptyGrey;
        public string BoardString { get; private set; }

        public BoardState(int size, Vector topleft, Vector bottomRight)
        {
            scaleFactor = getScalingFactor();
            int width = (int)((bottomRight.X - topleft.X) * scaleFactor);
            int height = (int)((bottomRight.Y - topleft.Y) * scaleFactor);

            bounds = new Rectangle((int)((topleft.X) * scaleFactor), (int)((topleft.Y) * scaleFactor), width, height);
            boardSize = size;

            gridSquareW = bounds.Width / (boardSize - 1);
            gridSquareH = bounds.Height / (boardSize - 1);
             
            bounds = new Rectangle(bounds.X - gridSquareW / 2, bounds.Y - gridSquareH / 2, width + gridSquareW, height + gridSquareH);

            grid = new Stone[boardSize, boardSize];

            UpdateBoardState(true);
        }

        public void UpdateBoardState(bool guessEmpty, bool savePng = false)
        {
            Bitmap bmp = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
                if (savePng)
                    bmp.Save("board.png");
                lastBoard = board;
                if (lastBoard == null)
                    lastBoard = bmp;
                board = bmp;

                copyLastGrid();
                updateGrid(guessEmpty);
            }
        }

        public BoardChange[] GetBoardChanges()
        {
            if (lastGrid == null) return null;
            List<BoardChange> c = new List<BoardChange>();

            for (int i = 0; i < boardSize; i++)
                for (int j = 0; j < boardSize; j++)
                {
                    if (grid[i, j] != lastGrid[i, j])
                        c.Add(new BoardChange(i, j, grid[i, j], lastGrid[i, j]));
                }

            return c.ToArray();
        }
        
        private PxInfo getPixel(Bitmap b, int x, int y)
        {
            if (x < 0 || y < 0 || x > b.Width || y > b.Height) return new PxInfo(127,127,127);
            Color c = b.GetPixel(x, y);
            return new PxInfo(c);
        }

        private void updateGrid(bool guessEmpty)
        {
            int emptyDiff = 999, darkest = 999, lightest = -999;
            PxInfo[,] pixels = new PxInfo[boardSize,boardSize];

            // analyze the board bitmap for average intensity and detect stone colors
            for (var i=0; i<boardSize; i++)
                for (var j=0; j<boardSize; j++)
                {
                    //dont use exact center, move out to 4 
                    int cx, cy;
                    cx = (gridSquareW * i) + gridSquareW / 2;
                    cy = (gridSquareH * j) + gridSquareH / 2;
                    //grab not quite center px and 4 box sample points
                    //PxInfo c = getPixel(board, cx - (int)(gridSquareW * 0.10f), cy - (int)(gridSquareH * 0.10f));
                    
                    PxInfo tl = getPixel(board, cx - (int)(gridSquareW * 0.25f), cy - (int)(gridSquareH * 0.25f));
                    PxInfo tr = getPixel(board, cx + (int)(gridSquareW * 0.25f), cy - (int)(gridSquareH * 0.25f));
                    PxInfo bl = getPixel(board, cx - (int)(gridSquareW * 0.25f), cy + (int)(gridSquareH * 0.25f));
                    PxInfo br = getPixel(board, cx + (int)(gridSquareW * 0.25f), cy + (int)(gridSquareH * 0.25f));
                    
                    /* testing verticals
                    PxInfo tl = getPixel(board, cx - (int)(gridSquareW * 0.45f), cy);
                    PxInfo tr = getPixel(board, cx + (int)(gridSquareW * 0.45f), cy);
                    PxInfo bl = getPixel(board, cx, cy + (int)(gridSquareH * 0.45f));
                    PxInfo br = getPixel(board, cx, cy + (int)(gridSquareH * 0.45f));
                    */

                    int grey = ( tl.Grey + tr.Grey + bl.Grey + br.Grey) / 4; 
                    int max = (  tl.Max + tr.Max + bl.Max + br.Max) / 4; 
                    int min = ( tl.Min + tr.Min + bl.Min + br.Min) / 4; 
                    pixels[i, j] = new PxInfo(grey, min, max);

                    //autocalibrate color intensities of stones and empty baseline
                    if (guessEmpty && Math.Abs(baseBoardGrey - grey) < emptyDiff)
                    {
                        emptyDiff = Math.Abs(baseBoardGrey - grey);
                        emptyGrey = grey;
                    }

                    if (grey < darkest)
                        darkest = grey;

                    if (grey > lightest)
                        lightest = grey;
                }

            float thresholdDiff = 0.5f;
            int whiteDiff = lightest - emptyGrey;
            int blackDiff = emptyGrey - darkest;
            int whiteThreshold = (int)(whiteDiff * thresholdDiff) + emptyGrey;
            int blackThreshold = emptyGrey - (int)(blackDiff * thresholdDiff);

            string debug = "";
            for (var j = 0; j < boardSize; j++)
            {
                for (var i = 0; i < boardSize; i++)
                {
                    if (pixels[i, j].Grey <= blackThreshold) {
                        grid[i, j] = Stone.Black;
                        debug += "B ";
                    }
                    else if (pixels[i, j].Grey >= whiteThreshold) {
                        grid[i, j] = Stone.White;
                        debug += "W ";
                    }
                    else {
                        grid[i, j] = Stone.Empty;
                        debug += ". ";
                    }                    
                }
                debug += "\r\n";
            }
            BoardString = debug;
        }
        
        private void copyLastGrid()
        {
            if (lastGrid == null)
            {
                lastGrid = new Stone[boardSize, boardSize];
            }

            for (int i = 0; i < boardSize; i++)
                for (int j = 0; j < boardSize; j++) 
                    lastGrid[i,j] = grid[i,j]; 
        }

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,
        }
        
        private double getScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            double ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }
    }

    class PxInfo
    {
        public int Grey { get; }
        public int Max { get; }
        public int Min { get; }

        public PxInfo(int grey, int min, int max)
        {
            Grey = grey;
            Min = min;
            Max = max;
        }

        public PxInfo(Color c)
        {
            Grey = (c.R + c.G + c.B) / 3;
            Min = Math.Min(Math.Min(c.R, c.G), c.B);
            Max = Math.Max(Math.Max(c.R, c.G), c.B);
        }
    }

}
