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
        private float gridSquareWf, gridSquareHf;

        private int baseBoardGrey = 165;
        private int emptyGrey;

        public string BoardString { get; private set; }
        public int CalibratedGrey { get; private set; }
        public int CalibratedWhite { get; private set; }
        public int CalibratedBlack { get; private set; }

        public BoardState(int size, Vector topleft, Vector bottomRight)
        {
            scaleFactor = getScalingFactor();
            int width = (int)((bottomRight.X - topleft.X) * scaleFactor);
            int height = (int)((bottomRight.Y - topleft.Y) * scaleFactor);

            bounds = new Rectangle((int)((topleft.X) * scaleFactor), (int)((topleft.Y) * scaleFactor), width, height);
            boardSize = size;

            gridSquareWf = (float)bounds.Width / ((float)boardSize - 1f);
            gridSquareHf = (float)bounds.Height / ((float)boardSize - 1f);
            gridSquareW = (int)gridSquareWf;
            gridSquareH = (int)gridSquareHf;

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
                    cx = (int)((gridSquareWf * (float)i) + gridSquareWf / 2.0f);
                    cy = (int)((gridSquareHf * (float)j) + gridSquareHf / 2.0f);
                    
                    /* debug each square
                    Bitmap bm = new Bitmap(gridSquareW, gridSquareH);
                    var g = Graphics.FromImage(bm);
                    g.DrawImage(board, new Rectangle(0, 0, gridSquareW, gridSquareH), new Rectangle(cx - gridSquareW / 2, cy - gridSquareH / 2, gridSquareW, gridSquareH), GraphicsUnit.Pixel);
                    bm.Save(i + "-" + j + ".png");
                    */
                
                    //grab not quite center px and 4 box sample points at 2 distances

                    PxInfo tl = getPixel(board, cx - (int)(gridSquareWf * 0.25f), cy - (int)(gridSquareHf * 0.25f));
                    PxInfo tr = getPixel(board, cx + (int)(gridSquareWf * 0.25f), cy - (int)(gridSquareHf * 0.25f));
                    PxInfo bl = getPixel(board, cx - (int)(gridSquareWf * 0.25f), cy + (int)(gridSquareHf * 0.25f));

                    //cardinal directions with slight offset to avoid grid line on empty board
                    PxInfo l = getPixel(board, cx - (int)(gridSquareWf * 0.37f), cy - (int)(gridSquareHf * 0.10f));
                    PxInfo r = getPixel(board, cx + (int)(gridSquareWf * 0.37f), cy + (int)(gridSquareHf * 0.10f));
                    PxInfo t = getPixel(board, cx - (int)(gridSquareWf * 0.10f), cy - (int)(gridSquareHf * 0.37f));
                    
                    int grey = (t.Grey + l.Grey + r.Grey + tl.Grey + tr.Grey + bl.Grey) / 6; // works with tygem, gopanda, ogs, lizzie, etc

                    //int max = (  tl.Max + tr.Max + bl.Max + br.Max + tl2.Max + tr2.Max + bl2.Max + br2.Max) / 8; 
                    //int min = ( tl.Min + tr.Min + bl.Min + br.Min + tl2.Min + tr2.Min + bl2.Min + br2.Min) / 8; 
                    pixels[i, j] = new PxInfo(grey, 127, 127);

                    //autocalibrate color intensities
                    if (grey < darkest)
                        darkest = grey;

                    if (grey > lightest)
                        lightest = grey;
                }

            //determine empty board space color
            if (guessEmpty) 
            {
                int darkLightAvg = (darkest + lightest) / 2;
                int darkLightDiff = Math.Abs(darkest - lightest);
                for (var i = 0; i < boardSize; i++)
                    for (var j = 0; j < boardSize; j++)
                    {
                        if (darkLightDiff > 75 && Math.Abs(darkLightAvg - pixels[i, j].Grey) < emptyDiff)
                        {
                            emptyDiff = Math.Abs((darkest + lightest) / 2 - pixels[i, j].Grey);
                            emptyGrey = pixels[i, j].Grey;
                        }
                        else if (Math.Abs(baseBoardGrey - pixels[i, j].Grey) < emptyDiff)
                        {
                            emptyDiff = Math.Abs(baseBoardGrey - pixels[i, j].Grey);
                            emptyGrey = pixels[i, j].Grey;
                        }
                    }
            }

            float thresholdDiffW = 0.18f;
            float thresholdDiffB = 0.18f;
            int blackDiff = emptyGrey - darkest;
            int whiteDiff = lightest - emptyGrey;
            
            int whiteThreshold = lightest - (int)(whiteDiff * thresholdDiffW);
            int blackThreshold = darkest + (int)(blackDiff * thresholdDiffB);

            CalibratedWhite = whiteThreshold;
            CalibratedBlack = blackThreshold;
            CalibratedGrey = emptyGrey;

            if (blackDiff < 20)
               blackThreshold = CalibratedBlack = 10;
            if (whiteDiff < 20)
               whiteThreshold = CalibratedWhite = 245;

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
