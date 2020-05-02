using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.IO;
using WpfApplicationHotKey.WinApi;
using System.Windows.Threading;

namespace STTGoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int BOARD_STATE_TICK_MS = 333;

        static SpeechSynthesizer ss = new SpeechSynthesizer();
        static SpeechRecognitionEngine sre;

        static bool ascending = false;
        static bool icoordEnabled = false;
        static bool enableReadback = false;
        static bool autoPlay = false;
        static bool enableOpponentColorRead = true;
        static int boardSize = 19;
        static bool enableHotkeys = true;
        static VoiceGender computervoice = VoiceGender.Female;
        static Vector topLeft = new Vector();
        static Vector bottomRight = new Vector();

        static MainWindow win;
        static bool speechOn = true;

        static BoardState.Stone currentGameColor;
        static BoardState currentGame;
        static DispatcherTimer boardStateTimer;

        public MainWindow()
        {
            InitializeComponent();
            win = this;
             
            LoadSettings();

            currentGameColor = BoardState.Stone.Empty;

            ss.SetOutputToDefaultAudioDevice();
            ss.SelectVoiceByHints(computervoice);
            
            CultureInfo ci = new CultureInfo("en-us");
            sre = new SpeechRecognitionEngine(ci);
            sre.SetInputToDefaultAudioDevice();
            sre.SpeechRecognized += sre_SpeechRecognized;

            try
            {
                win.tHelp.Text = File.ReadAllText("help.txt") + "\r\n";
            } catch (Exception e)
            {
                MessageBox.Show("Error reading help.txt. Help will be blank.");
            }
            
            Choices ch_StartStopCommands = new Choices();
            ch_StartStopCommands.Add("speech on");
            ch_StartStopCommands.Add("speech off");
            ch_StartStopCommands.Add("male voice");
            ch_StartStopCommands.Add("female voice");
            GrammarBuilder gb_StartStop = new GrammarBuilder();
            gb_StartStop.Append(ch_StartStopCommands);
            Grammar g_StartStop = new Grammar(gb_StartStop);

            Choices ch_play = new Choices();
            ch_play.Add("click");
            ch_play.Add("configure top left");
            ch_play.Add("configure bottom right");
            ch_play.Add("disable eye coordinate");
            ch_play.Add("enable eye coordinate");
            ch_play.Add("disable reading opponent");
            ch_play.Add("enable reading opponent");
            ch_play.Add("disable auto");
            ch_play.Add("enable auto");
            ch_play.Add("disable read back");
            ch_play.Add("enable read back");
            ch_play.Add("start game as black");
            ch_play.Add("start game as white");
            ch_play.Add("stop game");
            ch_play.Add("disable hot keys");
            ch_play.Add("enable hot keys");
            ch_play.Add("ascending");
            ch_play.Add("descending");
            ch_play.Add("board size 19");
            ch_play.Add("board size 13");
            ch_play.Add("board size 9");
            ch_play.Add("troubleshoot");
            GrammarBuilder gb_play = new GrammarBuilder();
            gb_play.Append(ch_play);
            Grammar g_play = new Grammar(gb_play);
                
            Choices ch_Numbers = new Choices();
            for (int i = 1; i <= 19; i++)
            {
                ch_Numbers.Add(i.ToString());
            }
            Choices ch_Chars = new Choices();
            //ch_Chars.Add("Aiy"); //to fix "A" needing to be said more like "Uh"
            ch_Chars.Add("A "); //to fix "A" needing to be said more like "Uh"
            for (int i = 1; i <= 20; i++)
            {
                ch_Chars.Add(Char.ToString((char)(i + 64)));                
            }            
            ch_Chars.Add("Alpha");
            ch_Chars.Add("Bravo");
            ch_Chars.Add("Charlie");
            ch_Chars.Add("Delta");
            ch_Chars.Add("Echo");
            ch_Chars.Add("Foxtrot");
            ch_Chars.Add("Golf");
            ch_Chars.Add("Hotel");
            ch_Chars.Add("India");
            ch_Chars.Add("Juliett");
            ch_Chars.Add("Kilo");
            ch_Chars.Add("Lima");
            ch_Chars.Add("Mike");
            ch_Chars.Add("November");
            ch_Chars.Add("Oscar");
            ch_Chars.Add("Papa");
            ch_Chars.Add("Quebec");
            ch_Chars.Add("Romeo");
            ch_Chars.Add("Sierra");
            ch_Chars.Add("Tango");
            GrammarBuilder gb_Coord = new GrammarBuilder();
            gb_Coord.Append(ch_Chars);
            gb_Coord.Append(ch_Numbers);
            Grammar g_Coord = new Grammar(gb_Coord);

            sre.LoadGrammarAsync(g_StartStop);
            sre.LoadGrammarAsync(g_play);
            sre.LoadGrammarAsync(g_Coord);

            if (enableHotkeys)
                EnableHotkeys();
            
            boardStateTimer = new DispatcherTimer();
            boardStateTimer.Tick += boardStateTimer_Tick;
            boardStateTimer.Interval = new TimeSpan(0, 0, 0, 0, BOARD_STATE_TICK_MS);            

            sre.RecognizeAsync(RecognizeMode.Multiple);
            win.lStatus.Content = "Listening...";
            if (enableReadback)
                ss.SpeakAsync("Voice Go Bon Listening...");
        
        }

        private void boardStateTimer_Tick(object sender, EventArgs e)
        {
            currentGame.UpdateBoardState(true);
            var changes = currentGame.GetBoardChanges();
            if (changes != null && changes.Length>0)
            {
                //int count = 0;
                foreach (var c in changes)
                {
                    if (c.PreviousStone==BoardState.Stone.Empty && c.Stone!= currentGameColor)
                    {
                        speechOn = false;

                        var s = currentGame.BoardString;
                        int x = c.X + 65;
                        if (!icoordEnabled && c.X > 7)
                            x += 1;

                        string ch = Char.ToString((char)(x));
                        string color = (enableOpponentColorRead ? c.Stone.ToString()+", " : "");

                        if (ascending) //not async because this might cause duplicate commands to be detected
                            ss.Speak(color + ch + ", " + (c.Y + 1));
                        else
                            ss.Speak(color + ch + ", " + (boardSize - c.Y));

                        //count++;
                        //if (count >= 2)
                        speechOn = true;
                        break;
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //future use
        }

        private HotKey hk_Tab, hk_Left, hk_Right, hk_Up, hk_Down, hk_Numpad5, hk_BrowserHome, hk_BrowserBack;
        void EnableHotkeys()
        {
            hk_Tab = new HotKey(ModifierKeys.None, Keys.Tab, this);
            hk_Tab.HotKeyPressed += (k) => {ClickMouse();};

            hk_Numpad5 = new HotKey(ModifierKeys.None, Keys.Clear, this);
            hk_Numpad5.HotKeyPressed += (k) => { ClickMouse(); };

            hk_BrowserHome = new HotKey(ModifierKeys.None, Keys.BrowserHome, this);
            hk_BrowserHome.HotKeyPressed += (k) => { ClickMouse(); };

            hk_BrowserBack = new HotKey(ModifierKeys.None, Keys.BrowserBack, this);
            hk_BrowserBack.HotKeyPressed += (k) => { ClickMouse(); };

            hk_Left = new HotKey(ModifierKeys.None, Keys.Left, this);
            hk_Left.HotKeyPressed += (k) => { MoveMouseRelative(MouseDirection.LEFT); };

            hk_Right = new HotKey(ModifierKeys.None, Keys.Right, this);
            hk_Right.HotKeyPressed += (k) => { MoveMouseRelative(MouseDirection.RIGHT); };

            hk_Up = new HotKey(ModifierKeys.None, Keys.Up, this);
            hk_Up.HotKeyPressed += (k) => { MoveMouseRelative(MouseDirection.UP); };

            hk_Down = new HotKey(ModifierKeys.None, Keys.Down, this);
            hk_Down.HotKeyPressed += (k) => { MoveMouseRelative(MouseDirection.DOWN); };
        }

        static void SpeakAndLabel(string speak, string label="")
        {
            win.lStatus.Content = (label=="" ? speak.ToUpper():label.ToUpper());
            ss.SpeakAsync(speak);
        }

        const float CONFIDENCE_THRESHOLD = 0.60f;
        static void sre_SpeechRecognized(object sender,SpeechRecognizedEventArgs e)
        {
            string txt = e.Result.Text;
            float confidence = e.Result.Confidence;
            win.lStatus.Content = ("Recognized: " + txt);
            if (confidence < CONFIDENCE_THRESHOLD)
            {
                win.lStatus.Content = ("Unrecognized (" + ((int)(confidence*100)).ToString() + "): " + txt);
                return;
            }

            //turn whole system on/off
            if (txt.IndexOf("speech on") >= 0)
            {
                SpeakAndLabel("Speech detection on");
                speechOn = true;
            }
            else if (txt.IndexOf("speech off") >= 0)
            {
                SpeakAndLabel("Speech detection off");
                speechOn = false;
            }
            if (speechOn == false) return;

            //commands for settings
            if (txt.IndexOf("help")>0)
            {
                ToggleHelp();
            }
            else if (txt.IndexOf("configure top")>=0)
            {
                SaveMousePosition(true);
                SpeakAndLabel("Top left saved", "SET TOP LEFT");
            }
            else if (txt.IndexOf("configure bottom") >= 0)
            {
                SaveMousePosition(false);
                SpeakAndLabel("Bottom right saved", "SET BOTTOM RIGHT");
            }
            else if (txt.IndexOf("start game as") >= 0)
            {
                if (txt.IndexOf("white") >= 0)
                {
                    currentGameColor = BoardState.Stone.White;
                    SpeakAndLabel("Game started as white player");
                }
                else
                {
                    currentGameColor = BoardState.Stone.Black;
                    SpeakAndLabel("Game started as black player");
                }
                currentGame = new BoardState(boardSize, topLeft, bottomRight);                
                boardStateTimer.Start();
            }
            else if (txt.IndexOf("stop game") >= 0)
            {
                currentGameColor = BoardState.Stone.Empty;
                SpeakAndLabel("Game reading stopped");                
                boardStateTimer.Stop();
            }
            else if (txt.IndexOf("disable eye") >= 0)
            {
                SpeakAndLabel("I coordinate disabled");
                icoordEnabled = false;
            }
            else if (txt.IndexOf("enable eye") >= 0)
            {
                SpeakAndLabel("I coordinate enabled");
                icoordEnabled = true;
            }
            else if (txt.IndexOf("disable reading opponent") >= 0)
            {
                SpeakAndLabel("Reading opponent color disabled");
                enableOpponentColorRead = false;
            }
            else if (txt.IndexOf("enable reading opponent") >= 0)
            {
                SpeakAndLabel("Reading opponent color enabled");
                enableOpponentColorRead = true;
            }
            else if (txt.IndexOf("disable hot keys") >= 0)
            {
                win.lStatus.Content = "Hotkeys disabled";
                SpeakAndLabel("Hot keys disabled. Please restart Voice Go bon");
                enableHotkeys = false;
            }
            else if (txt.IndexOf("enable hot keys") >= 0)
            {
                win.lStatus.Content = "Hotkeys enabled";
                SpeakAndLabel("Hot keys enabled. Please restart Voice Go bon");
                enableHotkeys = true;
            }
            else if (txt.IndexOf("female voice") >= 0)
            {
                SpeakAndLabel("Female voice");
                computervoice = VoiceGender.Female;
                ss.SelectVoiceByHints(computervoice);
            }
            else if (txt.IndexOf("male voice") >= 0)
            {
                SpeakAndLabel("Male voice");
                computervoice = VoiceGender.Male;
                ss.SelectVoiceByHints(computervoice);
            }
            else if (txt.IndexOf("disable auto") >= 0)
            {
                SpeakAndLabel("Autoplay disabled");
                autoPlay = false;
            }
            else if (txt.IndexOf("enable auto") >= 0)
            {
                SpeakAndLabel("Autoplay enabled");
                autoPlay = true;
            }
            else if (txt.IndexOf("disable read") >= 0)
            {
                SpeakAndLabel("Coord readback disabled");
                enableReadback = false;
            }
            else if (txt.IndexOf("enable read") >= 0)
            {
                SpeakAndLabel("Coord readback enabled");
                enableReadback = true;
            }
            else if (txt.IndexOf("ascending") >= 0)
            {
                SpeakAndLabel("Ascending coordinate order");
                ascending = true;
            }
            else if (txt.IndexOf("descending") >= 0)
            {
                SpeakAndLabel("Descending coordinate order");
                ascending = false;
            }
            else if (txt.IndexOf("board size 19") >= 0)
            {
                SpeakAndLabel("board size 19");
                boardSize = 19;
            }
            else if (txt.IndexOf("board size 13") >= 0)
            {
                SpeakAndLabel("board size 13");
                boardSize = 13;
            }
            else if (txt.IndexOf("board size 9") >= 0)
            {
                SpeakAndLabel("board size 9");
                boardSize = 9;
            }
            else if (txt.IndexOf("click") >= 0)
            {
                ClickMouse();
                SpeakAndLabel("click");
            }
            else if (txt.IndexOf("troubleshoot") >= 0)
            {
                if (currentGame!=null)
                {
                    SpeakAndLabel("troubleshoot");
                    currentGame.UpdateBoardState(true, true);
                    win.tHelp.FontFamily = new FontFamily("Courier New");
                    win.tHelp.Text = currentGame.BoardString;
                    win.tHelp.Text += "\r\n\r\n" + currentGame.CalibratedGrey + ", " + currentGame.CalibratedBlack + ", " + currentGame.CalibratedWhite;
                    //System.Diagnostics.Process.Start("board.png");
                }
            }
            else //move mouse to coordinate
            {
                if (txt.Length == 0) return;
                txt = txt.Trim().ToUpper();
                char letter = txt[0];
                if (letter == 'I' && !icoordEnabled) return;

                int hindex = (int)letter - 64;
                if (hindex > 8 && !icoordEnabled) hindex -= 1;

                string[] tokens = txt.Split(' ');
                if (tokens.Length <= 1) return;
                int vindex = int.Parse(tokens[1].Trim());//txt.Substring(1).Trim()

                if (vindex>0 && vindex<=boardSize && hindex>0 && hindex<=boardSize)
                {
                    win.lStatus.Content = "MOVE: "+txt.ToUpper();

                    MoveMouseTo(hindex, vindex);

                    if (autoPlay)
                        ClickMouse();
                    if (enableReadback)
                        ss.Speak(txt.Replace("AIY", "A")); //not async because this might cause duplicate commands to be detected
                }
                return;//skip setting save
            }

            //got this far? save settings
            SaveSettings();

        } // sre_SpeechRecognized

        const string SETTINGS_FILE = "settings.txt";
        private static void SaveSettings()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\VoiceGoban\\";
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            var s = "";
            s += ascending+"\n";
            s += icoordEnabled + "\n";
            s += enableReadback + "\n";
            s += autoPlay + "\n";
            s += boardSize + "\n";
            s += computervoice + "\n";
            s += topLeft.X + "\n";
            s += topLeft.Y + "\n";
            s += bottomRight.X + "\n";
            s += bottomRight.Y + "\n";
            s += enableHotkeys + "\n";
            s += enableOpponentColorRead + "\n";

            File.WriteAllText(path + SETTINGS_FILE, s);
        }
        
        private static void ReadSettingLine(int i, string[] lines)
        {
            if (i < lines.Length)
            {
                switch (i)
                {
                    case 0: ascending = bool.Parse(lines[i]); break;
                    case 1: icoordEnabled = bool.Parse(lines[i]); break;
                    case 2: enableReadback = bool.Parse(lines[i]); break;
                    case 3: autoPlay = bool.Parse(lines[i]); break;
                    case 4: boardSize = int.Parse(lines[i]); break;
                    case 5: computervoice = (VoiceGender)Enum.Parse(typeof(VoiceGender), lines[i]); break;
                    case 6: topLeft.X = double.Parse(lines[i]); break;
                    case 7: topLeft.Y = double.Parse(lines[i]); break;
                    case 8: bottomRight.X = double.Parse(lines[i]); break;
                    case 9: bottomRight.Y = double.Parse(lines[i]); break;
                    case 10: enableHotkeys = bool.Parse(lines[i]); break;
                    case 11: enableOpponentColorRead = bool.Parse(lines[i]); break;
                }
            }
        }

        private static void LoadSettings()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\VoiceGoban\\";
            try
            {
                Directory.CreateDirectory(path);
                if (File.Exists(path + SETTINGS_FILE))
                {
                    string f = File.ReadAllText(path+SETTINGS_FILE);
                    string[] t = f.Split('\n');
                    for (int i=0; i<t.Length+1; i++)
                    {
                        ReadSettingLine(i, t);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        private static void SaveMousePosition(bool asTopLeft)
        {
            var p = Pos();
            
            if (asTopLeft)
            {
                topLeft = new Vector(p.X, p.Y);//store as double for easier use with precision later
            }
            else
            {
                bottomRight = new Vector(p.X, p.Y);
            }
        }

        private static double GetGridVertPx()
        {
            double w = bottomRight.X - topLeft.X;
            double wpx = w / (double)(boardSize - 1);
            return wpx;
        }

        private static double GetGridHorizPx()
        {
            double h = bottomRight.Y - topLeft.Y;
            double hpx = h / (double)(boardSize - 1);
            return hpx;
        }

        enum MouseDirection
        {
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        private const double SCREEN_CONVERT_AMOUNT = 65535.0f;
        private static void MoveMouseRelative(MouseDirection d)
        {
            double wpx = GetGridHorizPx();
            double hpx = GetGridVertPx();

            var p = Pos();
            Vector v = new Vector(p.X, p.Y);
           
            switch (d)
            {
                case MouseDirection.LEFT:
                    v.X -= wpx;
                    break;
                case MouseDirection.RIGHT:
                    v.X += wpx;
                    break;
                case MouseDirection.UP:
                    v.Y -= hpx;
                    break;
                case MouseDirection.DOWN:
                    v.Y += hpx;
                    break;
            }

            var x = SCREEN_CONVERT_AMOUNT * v.X / System.Windows.SystemParameters.PrimaryScreenWidth;
            var y = SCREEN_CONVERT_AMOUNT * v.Y / System.Windows.SystemParameters.PrimaryScreenHeight;
            InputHelper.SendMouse((uint)(InputHelper.MouseEventF.MOUSEEVENTF_ABSOLUTE | InputHelper.MouseEventF.MOUSEEVENTF_MOVE), 0, (int)x, (int)y);
        }

        private static void MoveMouseTo(int hindex, int vindex)
        {
            hindex -= 1;
            vindex -= 1;
            if (!ascending)
            {
                vindex = boardSize - vindex - 1;
            }
            //NOTE: we must have 'false' set in dpi-aware app.manifest for this calculation to work

            double wpx = GetGridHorizPx();
            double hpx = GetGridVertPx();
            double woffset = wpx * hindex + topLeft.X;
            double hoffset = hpx * vindex + topLeft.Y;

            var x = SCREEN_CONVERT_AMOUNT * woffset / System.Windows.SystemParameters.PrimaryScreenWidth;
            var y = SCREEN_CONVERT_AMOUNT * hoffset / System.Windows.SystemParameters.PrimaryScreenHeight;
            InputHelper.SendMouse((uint)(InputHelper.MouseEventF.MOUSEEVENTF_ABSOLUTE | InputHelper.MouseEventF.MOUSEEVENTF_MOVE), 0, (int)x, (int)y);
        }

        private static void ClickMouse()
        {
            InputHelper.SendMouse((uint)InputHelper.MouseEventF.MOUSEEVENTF_LEFTDOWN);
            InputHelper.SendMouse((uint)InputHelper.MouseEventF.MOUSEEVENTF_LEFTUP);
        }
        
        private void BHelp_Click(object sender, RoutedEventArgs e)
        {
            ToggleHelp();
        }

        private static void ToggleHelp()
        {
            win.tHelp.IsEnabled = !win.tHelp.IsEnabled;
            if (win.tHelp.IsEnabled)
            {
                win.rHelp.Height = new GridLength(9, GridUnitType.Star);
                win.Height += 480;
                win.Width += 270;
            }
            else
            {
                win.rHelp.Height = new GridLength(0, GridUnitType.Pixel);
                win.Height -= 480;
                win.Width -= 270;
            }
        }

        //mouse helpers to get pos
        [StructLayout(LayoutKind.Sequential)]
        private struct PointInter
        {
            public int X;
            public int Y;
            public static explicit operator Point(PointInter point) => new Point(point.X, point.Y);
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out PointInter lpPoint);

        // For your convenience
        private static PointInter Pos()
        {
            PointInter lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }
        
    }
}
