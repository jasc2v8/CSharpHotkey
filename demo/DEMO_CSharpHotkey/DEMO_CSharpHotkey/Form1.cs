//
//  Add methods to test with the prefix "DEMO_", Example:
//  public void DEMO_NewMethod(); //must be public void
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Clipboard = System.Windows.Forms.Clipboard;
using Cursor = System.Windows.Forms.Cursor;

using CSharpHotkeyLib;

namespace DEMO_CSharpHotkey
{
    public partial class Form1 : Form
    {

        private static string PREFIX = "DEMO_";
        private static bool HIDE_PREFIX = true;

        private static string logFilePath = Assembly.GetExecutingAssembly().GetName().Name + ".log";
        
        private CSharpHotkey Win = null;

        private bool AbortFlag { get; set; }
        private bool RunningFlag { get; set; }

        private int DemoDelay { get; set; }
        private void WriteLine(string text)
        {
            //if (textBoxOutput.TabStop == true)
            //    textBoxOutput.TabStop = false;
            textBoxOutput.AppendText(text + Environment.NewLine);
        }
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = Assembly.GetEntryAssembly().GetName().Name + " v" + Assembly.GetEntryAssembly().GetName().Version;

            textBoxWinTitle.Text = "Untitled - Notepad _class Notepad";

            Win = new CSharpHotkeyLib.CSharpHotkey();

            Win.SetTitleMatchMode(MATCH_MODE.Contains, MATCH_CASE.InSensitive);

            WriteLine("This is a DEMO of CSharpHotkey" + Environment.NewLine +
                Environment.NewLine +
                "1. Select a demo, " + Environment.NewLine +
                "2. Press the DEMO button to start the selection," + Environment.NewLine +
                "3. Watch until the demo completes." + Environment.NewLine +
                Environment.NewLine +
                "Recommended first DEMO: Move." + Environment.NewLine +
                Environment.NewLine +
                "See TEST_CSharpHotkey for interactive tests and examples." + Environment.NewLine +
                Environment.NewLine +
                "Press ESCAPE to abort the selected demo.");


            LoadListBox();

            listBoxDemos.SetSelected(listBoxDemos.FindString("Move"), true);
        }
        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();
        }
        private void buttonOpenLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(logFilePath))
            {
                Process.Start(logFilePath);

            } else
            {
                textBoxOutput.Text += "Log file not found: " + logFilePath + Environment.NewLine;
            }
        }
        private void buttonDEMO_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();

            if (listBoxDemos.SelectedItems.Count == 0)
            {
                WriteLine("Nothing selected.");
                return;
            }

            string selection = listBoxDemos.SelectedItems[0].ToString();

            toolStripStatusLabel1.Text = "Demo: " + selection;

            if (HIDE_PREFIX)
            {
                selection = PREFIX + selection;
            }

            try
            {
                MethodInfo mi = GetType().GetMethod(selection);
                mi.Invoke(this, null);
            }
            catch (Exception ex)
            {
                WriteLine("ERROR ex =" + ex.Message);
            }

        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxDemos.SelectedIndex < 0)
                return;

            toolStripStatusLabel1.Text = "Selected: " + listBoxDemos.SelectedItems[0].ToString();
        }

        private void LoadListBox()
        {

            string path = Path.GetFullPath(@"..\..\") + "Form1.cs";

            List<string> methodNames = new List<string>();

            string[] lines = File.ReadAllLines(path);

            const string methodPattern =
                //@"(public|private|bool|int|string|static|void)\s* ([\w]+)\(.*\)";
                @"(public|void)\s* ([\w]+)\(.*\)";

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("public void " + PREFIX))
                {

                    Match m = Regex.Match(line, methodPattern, RegexOptions.IgnorePatternWhitespace);

                    if (m.Success)
                    {
                        Group g = m.Groups[m.Groups.Count - 1];

                        if (HIDE_PREFIX)
                        {
                            methodNames.Add(g.Value.Substring(PREFIX.Length));
                        }
                        else
                        {
                            methodNames.Add(g.Value);
                        }
                    }
                }
            }

            methodNames.Sort();

            foreach (var item in methodNames)
                listBoxDemos.Items.Add(item);

        }
        private bool StartNotepad()
        {
            string WinTitle = "Untitled - Notepad";

            string processName = WinTitle.Split('-')[1].Trim();

            if (!Win.Exist(WinTitle))
            {
                try
                {
                    Process.Start(processName);
                }
                catch
                {
                    return true; //error
                }
            }
            return false; //no error

        }
        private void MoveCenter(string WinTitle = "")
        {

            //get screen size
            int taskBarHeight = SystemInformation.CaptionHeight;
            Rectangle scrRect = Win.SysGetMonitor().Bounds;

            //save WinTitle original size
            Rectangle winRect = Win.GetPos(WinTitle);
            int saveX = winRect.X;
            int saveY = winRect.Y;
            int saveW = winRect.Width;
            int saveH = winRect.Height;

            //get WinTitle winBorder size
            Size winBorder = SystemInformation.FrameBorderSize;

            //set starting size
            int winWidth = scrRect.Width / 4;
            int winHeight = scrRect.Height / 4;

            //calc center position
            int winXcenter = scrRect.Width / 2 - (winRect.Width / 2);
            int winYcenter = scrRect.Height / 2 - (winRect.Height / 2);

            //center WinTitle and set starting size
            int winX = winXcenter;
            int winY = winYcenter;
            Win.Move(winX, winY, winWidth, winHeight, WinTitle);
        }
        public void DEMO_Move()
        {
            textBoxOutput.Clear();

            InputHook hookEscape = null;

            hookEscape = new InputHook(Keys.None, Keys.Escape, Keys.Down, Keys.N, (hook) =>
            {
                Win.SoundBeep(0, 300);
                AbortFlag = true;
                hookEscape.Stop();
                return;
            });

            string WinTitle = GetWinTitle();

            if (!Win.Exist(WinTitle))
            {
                string[] processName = WinTitle.Split(' ');
                WinTitle = processName[processName.Length - 1];
            }

            if (Win.Exist(WinTitle))
            {
                WriteLine("Window found: " + WinTitle);
            }
            else
            {
                WriteLine("Window not found: " + WinTitle);
                WriteLine("Starting Process: " + WinTitle);
                try
                {
                    Process.Start(WinTitle);
                }
                catch
                {
                    WriteLine("Error starting process: " + WinTitle);
                    return;
                }
            }

            if (Win.Wait(WinTitle, 10))
            {
                WriteLine("TIMEOUT waiting for: " + WinTitle);
                return;
            }

            //optionally, slow down if necessary
            Win.SetWinDelay(100); //default=100
            Win.SetKeyDelay(1);     //default=10
            DemoDelay = 250;

            //move this DEMO window out of the way
            //Win.Minimize(this.Text);
            Win.MinimizeAll();

            //restore Wintitle if minimized
            Win.Restore(WinTitle);

            //get screen size
            int taskBarHeight = SystemInformation.CaptionHeight;
            Rectangle scrRect = Win.SysGetMonitor().Bounds;

            //save WinTitle original size
            Rectangle winRect = Win.GetPos(WinTitle);
            int saveX = winRect.X;
            int saveY = winRect.Y;
            int saveW = winRect.Width;
            int saveH = winRect.Height;

            //get WinTitle winBorder size
            Size winBorder = SystemInformation.FrameBorderSize;

            //set starting size
            int winWidth = scrRect.Width / 4;
            int winHeight = scrRect.Height / 3;

            //calc center position
            int winXcenter = scrRect.Width / 2 - (winRect.Width / 2);
            int winYcenter = scrRect.Height / 2 - (winRect.Height / 2);

            AbortFlag = false;
            RunningFlag = true;
 
            while (!AbortFlag && RunningFlag)
            {

                //center WinTitle and set starting size
                int winX = winXcenter;
                int winY = winYcenter;
                Win.Move(winX, winY, winWidth, winHeight);

                //turn off Caps Lock
                Win.SetLockState(Keys.CapsLock, Keys.Up);

                //clear any existing text in Notepad
                //Win.Send("^(A){DEL}");

                //change font size
                Win.Send("%OF{TAB}{TAB}^C26{ENTER}");

                //type instructions
                Win.Send("Please don't move mouse!{ENTER}");
                Win.Send("Font size changed to 26.{ENTER}");
                Win.Send("Press ENTER to continue...{ENTER}");
                Win.Sleep(DemoDelay);
                if (AbortFlag) break;

                //wait 5 seconds
                if (Win.KeyWait(Keys.Enter, Keys.Down, 5))
                {
                    Win.Send("Press ENTER to continue...{ENTER}");

                    //wait another 5 seconds
                    if (Win.KeyWait(Keys.Enter, Keys.Down, 5))
                    {
                        Win.Send("TIMEOUT!");
                        Win.Sleep(DemoDelay);
                        AbortFlag = true;
                        if (AbortFlag) break;
                    }
                }

                //start demo
                Win.Send("^(A){DEL}");
                Win.Send("Press ESCAPE to abort!{ENTER}");
                Win.Send("Starting DEMO...");
                Win.Sleep(DemoDelay);
                Win.Send("^(A){DEL}");

                //upper left
                winX = 0;
                winY = 0;
                Win.Move(winX, winY, winWidth, winHeight);
                Win.Send("Upper Left...{ENTER}");
                Win.Sleep(DemoDelay);
                Win.Send("^(A){DEL}");
                if (AbortFlag) break;

                //upper right
                winX = scrRect.Width - winWidth + 1;
                Win.Move(winX, winY);
                Win.Send("^(A)Upper Right...");
                Win.Sleep(DemoDelay);
                Win.Send("^(A){DEL}");
                if (AbortFlag) break;

                //lower left
                winX = 0;
                winY = scrRect.Height - winHeight - taskBarHeight - winBorder.Height - 2;
                Win.Move(winX, winY);
                Win.Send("^(A)Lower Left...");
                Win.Sleep(DemoDelay);
                Win.Send("^(A){DEL}");
                if (AbortFlag) break;

                //lower right
                winX = scrRect.Width - winWidth + 1;
                Win.Move(winX, winY);
                Win.Send("^(A)Lower Right...");
                Win.Sleep(DemoDelay);
                Win.Send("^(A){DEL}");
                if (AbortFlag) break;

                //center
                winX = winXcenter;
                winY = winYcenter;
                Win.Move(winX, winY);
                Win.Send("^(A)Center...");
                Win.Sleep(DemoDelay);
                Win.Send("^(A){DEL}");
                if (AbortFlag) break;

                //half size
                winWidth = winWidth / 2;
                winHeight = winHeight / 2;
                winX = scrRect.Width / 2 - (winWidth / 2);
                winY = scrRect.Height / 2 - (winHeight / 2);
                Win.Move(winX, winY, winWidth, winHeight);
                Win.Send("^(A)Half Size...");
                Win.Sleep(DemoDelay);
                Win.Send("^(A){DEL}");
                if (AbortFlag) break;

                //double size
                winWidth *= 4;
                winHeight *= 4;
                winX = scrRect.Width / 2 - (winWidth / 2);
                winY = scrRect.Height / 2 - (winHeight / 2);
                Win.Move(winX, winY, winWidth, winHeight);
                Win.Send("^(A)Double Size...");
                Win.Sleep(DemoDelay);
                Win.Send("^(A){DEL}");
                if (AbortFlag) break;

                RunningFlag = false;
            }

            //finish
            if (hookEscape.InProgress())
                hookEscape.Stop();

            //restore font size
            Win.Send("%(OF){TAB}{TAB}^(V)");
            Win.Sleep(DemoDelay);
            Win.Send("{ENTER}");
            Win.Send("Font size restored.");
            Win.Sleep(DemoDelay);
            Win.Send("^(A){DEL}");

            Win.MinimizeAllUndo();
            Win.Sleep(500);
            Win.Activate(WinTitle);
            WinSendNotepad("MinimizeAllUndo...");
            Win.Sleep(DemoDelay);

            //orignal size
            Win.Move(saveX, saveY, saveW, saveH);
            Win.Close(WinTitle);
            Win.Activate(this.Text);

            //report results
            if (AbortFlag)
                WriteLine("Test aborted!");
            else
                WriteLine("Test complete.");

            //reset WinDelay to default
            Win.SetWinDelay();

            Win.Set(SET.NotTop, this.Text);
        }
        private void WinSendNotepad(string Text)
        {
            Win.Send("^(A)" + Text);
            Win.Sleep(DemoDelay);
            Win.Send("^(A){DEL}");
        }
        private string GetWinTitle()
        {
            Win.SetTitleMatchMode(comboBoxMatchMode.Text, comboBoxMatchCase.Text);
            return textBoxWinTitle.Text;
        }
        public void DEMO_BlockInput()
        {
            textBoxOutput.Clear();

            ObjInputBox IB = new ObjInputBox();

            string iconPath = Path.GetFullPath(@"..\..\Resources\csharp_128_multi.ico");

            IB = Win.InputBox("DEMO_BlockInput", "Enter 1=Keyboard, 2=Mouse, 3=both", false, new Point(-1, -1),
                        int.MaxValue, "1", iconPath);

            if (IB.Result == "Cancel")
            {
                WriteLine("Demo cancelled!");
                return;
            }

            BlockInputHook hookBlock = null;

            if (IB.Value == "1")
            {
                WriteLine("Keyboard Input Blocked.");

                hookBlock = new BlockInputHook(true, false, Keys.Escape, (hook) =>
                {
                    WriteLine("ESCAPE PRESSED - Keyboard Unblocked!");
                    hookBlock.Stop();
                });
            }
            else if (IB.Value == "2")
            {
                WriteLine("Mouse Input Blocked.");

                hookBlock = new BlockInputHook(false, true, Keys.Escape, (hook) =>
                {
                    WriteLine("ESCAPE PRESSED - Mouse Unblocked!");
                    hookBlock.Stop();
                });
            }
            else if (IB.Value == "3")
            {
                WriteLine("Keyboard & Mouse Input Blocked.");

                hookBlock = new BlockInputHook(true, true, Keys.Escape, (hook) =>
                {
                    WriteLine("ESCAPE PRESSED - Keyboard & Mouse Unblocked!");
                    hookBlock.Stop();
                });
            }
            else
            {
                WriteLine("Invalid choice: " + IB.Value);
                return;
            }
            WriteLine("Press ESCAPE to abort and unblock both.");
        }
        public void DEMO_Click()
        {
            textBoxOutput.Clear();

            int x, y, w, h = 0;

            Win.GetPos(out x, out y, out w, out h, this.Text);

            x -= 125;

            int delayMS = 3000;
            WriteLine("This DEMO will wait " + delayMS / 1000 + " seconds, then Click the Minimize button...");
            Win.Sleep(delayMS);
            Win.Click(MOUSE_BUTTON.Left, x, y);
            WriteLine("Text complete.");
        }
        public void DEMO_Get()
        {
            //demo Get() and NameAdd()

            textBoxOutput.Clear();

            string WinTitle = GetWinTitle();

            if (!Win.Exist(WinTitle))
            {
                WriteLine("WinTitle not found: " + WinTitle);
                return;
            }

            Win.NameAdd("MainWindow", this.Text);
            Win.NameAdd("TestWindow", WinTitle);

            string padRight = "{0,-16}";

            WriteLine(String.Format(padRight, "TITLE:") + Win.GetTitle(WinTitle));
            WriteLine(String.Format(padRight, "ID:") + Win.Get(GET.ID));
            WriteLine(String.Format(padRight, "ProcessName:") + Win.Get(GET.ProcessName));
            WriteLine(String.Format(padRight, "ProcessPath:") + Win.Get(GET.ProcessPath));

            //check string style for a given WS_ style (similar to AHK)
            string style = Win.Get(GET.Style, "_name TestWindow");
            Int32.TryParse(style, System.Globalization.NumberStyles.HexNumber, null, out int bits);
            WriteLine(String.Format(padRight, "Style bits:") + bits.ToString("X"));

            int WS_MINIMIZE = 0x20000000;
            bool isMin = (bits & WS_MINIMIZE) != 0;
            WriteLine(String.Format(padRight, "WS_MINIMIZE:") + isMin);

            //now the easy way
            //WriteLine(String.Format(padRight, "EXIST:") +

            WriteLine(String.Format(padRight, "EXIST:") + Win.Get(GET.Exist, "_name TestWindow"));
            WriteLine(String.Format(padRight, "WS_DISABLED:") + Win.Get(GET.Disabled));
            WriteLine(String.Format(padRight, "WS_MAXIMIZE:") + Win.Get(GET.Maximize));
            WriteLine(String.Format(padRight, "WS_MINIMIZE:") + Win.Get(GET.Minimize));
            WriteLine(String.Format(padRight, "WS_VISIBLE:") + Win.Get(GET.Visible));
            WriteLine(String.Format(padRight, "NORMAL:") + Win.Get(GET.Normal));

            //demo 3 ways to get ActiveStats: AHK out format, Object, Rectangle
            Win.GetActiveStats(out WinTitle, out int Width, out int Height, out int X, out int Y);
            WriteLine(String.Format(padRight, "ActiveStats:") + "Title: " + WinTitle +
                ", W: " + Width + ", H: " + Height + ", X: " + X + ", Y: " + Y);

            ObjActiveStats oas = new ObjActiveStats();
            oas = Win.GetActiveStats();
            WriteLine(String.Format(padRight, "ActiveStats:") + "Title: " + oas.Title +
                ", W: " + oas.Bounds.Width + ", H: " + oas.Bounds.Height +
                ", X: " + oas.Bounds.X + ", Y: " + oas.Bounds.Y);

            Rectangle WinRect;
            Win.GetActiveStats(out WinTitle, out WinRect);
            WriteLine(String.Format(padRight, "ActiveStats:") + "Title: " + WinTitle +
                ", W: " + WinRect.Width + ", H: " + WinRect.Height +
                ", X: " + WinRect.X + ", Y: " + WinRect.Y);

            Rectangle rect = Win.GetPos();
            WriteLine(String.Format(padRight, "GetPos:") + 
                "X: " + rect.X + ", Y: " + rect.Y + ", W: " + rect.Width + ", H: " + rect.Height);

            ObjMonitor m = new ObjMonitor();
            m = Win.SysGetMonitor();
            WriteLine(String.Format(padRight, "Mon Bounds:") +
                "X: " + m.Bounds.X + ", Y: " + m.Bounds.Y + ", W: " + m.Bounds.Width + ", H: " + m.Bounds.Height);
            WriteLine(String.Format(padRight, "Work Area:") + 
                "X: " + m.WorkingArea.X + ", Y: " + m.WorkingArea.Y + ", W: " + m.WorkingArea.Width + ", H: " + m.WorkingArea.Height);

            //WriteLine("Test complete.");

            Win.Activate("_name MainWindow");

            return;

        }

        public void DEMO_MinMax()
        {

            textBoxOutput.Clear();

            string WinTitle = "Untitled - Notepad _class Notepad";

            int timeout = 10;

            WriteLine("Test window title = " + WinTitle);
            WriteLine("Test window timeout value = " + timeout + " seconds");

            //optionally, slow down if necessary
            Win.SetWinDelay(100); //default=100
            Win.SetKeyDelay();     //default=0
            DemoDelay = 250;

            //turn off Caps Lock
            Win.SetLockState(Keys.CapsLock, Keys.Up);

            AbortFlag = false;

            RunningFlag = true;

            InputHook hookEscape = null;

            hookEscape = new InputHook(Keys.None, Keys.Escape, Keys.Down, Keys.N, (hook) =>
            {
                Win.SoundBeep(0,300);
                AbortFlag = true;
                hookEscape.Stop();
                return;
            });

            while (!AbortFlag && RunningFlag)
            {
                Win.MinimizeAll();

                if (StartNotepad())
                {
                    WriteLine("Error starting Notepad.");
                    return;
                }

                if (Win.Wait(WinTitle, 10))
                {
                    WriteLine("TIMEOUT waiting for: " + WinTitle);
                    return;
                }

                Win.Activate(WinTitle);
                MoveCenter();

                //change font size
                Win.Send("%OF{TAB}{TAB}^C26{ENTER}");

                //type instructions
                Win.Send("Please don't move mouse!{ENTER}");
                Win.Send("Font size changed to 26.{ENTER}");
                Win.Send("Press ENTER to continue...{ENTER}");
                Win.Sleep(DemoDelay);
                if (AbortFlag) break;

                //wait 5 seconds
                if (Win.KeyWait(Keys.Enter, Keys.Down, 5))
                {
                    Win.Send("Press ENTER to continue...{ENTER}");

                    //wait another 5 seconds
                    if (Win.KeyWait(Keys.Enter, Keys.Down, 5))
                    {
                        Win.Send("TIMEOUT!");
                        Win.Sleep(DemoDelay);
                        AbortFlag = true;
                        if (AbortFlag) break;
                    }
                }

                //start demo
                Win.Restore();
                WinSendNotepad("Minimize All...");
                Win.Sleep(DemoDelay);
                if (AbortFlag) break;

                Win.Maximize();
                WinSendNotepad("Maximize...");
                Win.Sleep(DemoDelay);
                if (AbortFlag) break;

                Win.Restore();
                WinSendNotepad("Restore.{ENTER}Next is Minimize...");
                Win.Sleep(DemoDelay);
                if (AbortFlag) break;

                Win.Minimize();
                WinSendNotepad("Minimize...");
                Win.Sleep(DemoDelay);
                if (AbortFlag) break;

                Win.Maximize();
                WinSendNotepad("Maximize...");
                Win.Sleep(DemoDelay);
                if (AbortFlag) break;

                Win.Restore();
                WinSendNotepad("Restore.{ENTER}Next is Hide...");
                Win.Sleep(DemoDelay);
                if (AbortFlag) break;

                Win.Hide();
                WinSendNotepad("Hide...");
                Win.Sleep(DemoDelay);
                if (AbortFlag) break;

                Win.Show();
                WinSendNotepad("Show...");
                Win.Sleep(DemoDelay);
                if (AbortFlag) break;

                RunningFlag = false;
            }

            //restore font size
            Win.Send("%(OF){TAB}{TAB}^(V)");
            Win.Sleep(DemoDelay);
            Win.Send("{ENTER}");
            Win.Send("Font size restored.");
            Win.Sleep(DemoDelay);
            Win.Send("^(A){DEL}");

            Win.MinimizeAllUndo();
            Win.Sleep(500);
            Win.Activate(WinTitle);
            WinSendNotepad("MinimizeAllUndo...");
            Win.Sleep(DemoDelay);
            Win.Close(WinTitle);

            Win.Activate(this.Text);

            if (AbortFlag)
                WriteLine("Test aborted!");
            else
                WriteLine("Test complete.");

            Win.Set(SET.NotTop, this.Text);

            Win.SetWinDelay();
        }
        public void DEMO_Hook()
        {
            textBoxOutput.Clear();

            bool SupressYes = true, SupressNo = false;
            bool KeyDown = true, KeyUp = false;

            InputHook hookT = null;
            InputHook hookF = null;
            InputHook hookD1 = null;
            InputHook hook_LBUTTONDOWN = null;
            InputHook hook_MBUTTONDOWN = null;
            InputHook hook_RBUTTONDOWN = null;
            InputHook hook_XButtonDown = null;
            InputHook hook_MOUSEWHEEL = null;
            InputHook hookEscapeDown = null;

            hookT = new InputHook(Keys.None, Keys.T, Keys.Down, Keys.Y, (hook) =>
            {
                WriteLine("T pressed (supressed), vkCode = " + hook.vkCode + ", scanCode = " + hook.scanCode);
            });

            hookF = new InputHook(Keys.None, Keys.F, Keys.Down, Keys.N, (hook) =>
            {
                WriteLine("F pressed (not supressed), vkCode = " + hook.vkCode + ", scanCode = " + hook.scanCode);
            });

            hookEscapeDown = new InputHook(Keys.None, Keys.Escape, Keys.Down, Keys.N, (hook) =>
            {
                WriteLine("ESCAPE DOWN, vkCode = " + hook.vkCode + ", scanCode = " + hook.scanCode);

                if (hookT.InProgress()) hookT.Stop();
                if (hookF.InProgress()) hookF.Stop();
                if (hookD1.InProgress()) hookD1.Stop();
                if (hook_LBUTTONDOWN.InProgress()) hook_LBUTTONDOWN.Stop();
                if (hook_MBUTTONDOWN.InProgress()) hook_MBUTTONDOWN.Stop();
                if (hook_RBUTTONDOWN.InProgress()) hook_RBUTTONDOWN.Stop();
                if (hook_XButtonDown.InProgress()) hook_XButtonDown.Stop();
                if (hook_MOUSEWHEEL.InProgress()) hook_MOUSEWHEEL.Stop();
                if (hookEscapeDown.InProgress()) hookEscapeDown.Stop();
                WriteLine("All InputHooks Stopped.");
            });

            hookD1 = new InputHook(Keys.Control | Keys.Alt, Keys.D1, Keys.Down, Keys.N, (hook) =>
            {
                WriteLine("Ctrl-Alt-D1 PRESSED, vkCode = " + hook.vkCode + ", scanCode = " + hook.scanCode);
            });

            hook_LBUTTONDOWN = new InputHook(Keys.Alt, Keys.LButton, Keys.Down, Keys.N, (hook) =>
            {
                Point mousePoint = Control.MousePosition;

                WriteLine("Alt-LButton PRESSED! X: " + mousePoint.X + ", Y: " + mousePoint.Y);
            });

            hook_MBUTTONDOWN = new InputHook(Keys.None, Keys.MButton, Keys.Down, Keys.N, (obj) =>
            {
                WriteLine("hook_MBUTTONDOWN: " + obj.vkCode.ToString("X4"));
            });

            hook_RBUTTONDOWN = new InputHook(Keys.None, Keys.RButton, Keys.Down, Keys.Y, (obj) =>
            {
                WriteLine("hook_RBUTTONDOWN: " + obj.vkCode.ToString("X4"));
            });

            hook_XButtonDown = new InputHook(Keys.None, Keys.XButton1, Keys.Down, Keys.N, (hook) =>
            {
                if (hook.data == 1)
                    WriteLine("XButtonDown BACK");
                else if (hook.data == 2)
                    WriteLine("XButtonDown FORWARD");
            });

            hook_MOUSEWHEEL = new InputHook(Keys.None, InputHook.Wheels.Wheel, Keys.N, (hook) =>
            {
                if (hook.data == 120)
                    WriteLine("hook_MOUSEWHEEL FORWARD: X: " + hook.point.X + ", Y: " + hook.point.Y);
                else if (hook.data == -120)
                    WriteLine("hook_MOUSEWHEEL BACK: X: " + hook.point.X + ", Y: " + hook.point.Y);

            });

            WriteLine("Installed Keyboard and Mouse Hooks...");
            WriteLine("Press T (suppress = true)");
            WriteLine("Press F (suppress = false)");
            WriteLine("InputHooks started for keys   : Ctrl-Alt-D1, T (suppressed), F, ESCAPE.");
            WriteLine("InputHooks started for buttons: Alt-LButton, MButton, RButton, Wheel, XButton.");
            WriteLine("Press ESCAPE to terminate the tests and uninstall the hooks.");
        }
        public void DEMO_HotKey()
        {
            textBoxOutput.Clear();
            WriteLine("Test Hotkeys Action...");
            WriteLine("Uses public Class Hotkey");

            HotKey hotkeyD1 = null;
            HotKey hotkeyEscape = null;

            hotkeyD1 = new HotKey(CSharpHotkeyLib.HotKey.Modifiers.ControlAlt, Keys.D1, (none) =>
            {
                WriteLine("D1 HOTKEY PRESSED!");
            });

            hotkeyEscape = new HotKey(MODKEY.None, Keys.Escape, (none) =>
            {
                WriteLine("");
                WriteLine("ESCAPE HOTKEY PRESSED!");
                hotkeyD1.UnRegister();
                hotkeyEscape.UnRegister();
                WriteLine("Hotkeys UnRegistered.");
            });

            WriteLine("Hotkeys Registered: Ctrl-Alt-D1, and Escape.");
            WriteLine("Press Escape to end test.");
        }
        public void DEMO_InputBox()
        {
            textBoxOutput.Clear();

            //Rectangle screenRect = Screen.PrimaryScreen.Bounds;
            //Point Location = new Point(screenRect.Width / 5, screenRect.Height / 5);

            string Title = "TEST InputBox";
            string Prompt = "Enter TimeoutSeconds:";
            bool Hide = false;
            Point Location = new Point(-1, -1);
            int TimeoutSeconds = 30;
            string Default = "5"; //seconds
            string IconFile = Path.GetFullPath(@"..\..\Resources\face-smile.ico");

            WriteLine("Opening InputBox with Timeout of " + TimeoutSeconds + " seconds");

            ObjInputBox IB = new ObjInputBox();

            IB = Win.InputBox(Title, Prompt, Hide, Location, TimeoutSeconds, Default, IconFile);
            if (IB.Result == "OK")
            {
                Int32.TryParse(IB.Value, out TimeoutSeconds);
            }
            else
            {
                Int32.TryParse(Default, out TimeoutSeconds);
            }

            WriteLine("Result: " + IB.Result + ", Value: " + IB.Value);

            Prompt = "Enter some text:";
            Default = "";

            WriteLine("\r\nOpening InputBox with Timeout of " + TimeoutSeconds + " ms");

            IB = Win.InputBox(Title, Prompt, Hide,
                Location, TimeoutSeconds, Default, IconFile);

            WriteLine("Result: " + IB.Result + ", Value: " + IB.Value);

            Prompt = "Enter a Password:";
            Default = "";
            Hide = true;

            WriteLine("\r\nOpening InputBox with Timeout of " + TimeoutSeconds + " seconds");

            IB = Win.InputBox(Title, Prompt, Hide,
                Location, TimeoutSeconds, Default, IconFile);

            WriteLine("Result: " + IB.Result + ", Value: " + IB.Value);
        }
        public void DEMO_KeyLock()
        {
            textBoxOutput.Clear();

            if (Control.IsKeyLocked(Keys.CapsLock))
            {
                Win.SetLockState(Keys.CapsLock, Keys.Up);
                WriteLine("CapsLock OFF");
            }
            else
            {
                Win.SetLockState(Keys.CapsLock, Keys.Down);
                WriteLine("CapsLock ON");
            }
        }
        public void DEMO_KeyState()
        {

            string _GetKeyState(Keys _key)
            {
                if (Win.GetKeyState(_key)) { return ", " + _key.ToString(); } else { return ""; }
            }
            string _GetLockedState(Keys _key)
            {
                if (Control.IsKeyLocked(_key)) { return ", " + _key.ToString(); } else { return ""; }
            }
            string _GetModifierState(Keys _key)
            {
                if (Control.ModifierKeys == _key) { return ", " + _key.ToString(); } else { return ""; }
            }

            int timeoutSeconds = 10;

            bool running = true;
            bool timeout = false;

            Keys key = Keys.J;

            WriteLine("Press the key " + key.ToString() +
                " with Modifiers within " + timeoutSeconds + " seconds, or press Escape to exit loop.");

            DateTime startTime = DateTime.Now;

            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate
            {
                while (running)
                {
                    Win.Sleep(66);

                    if (Win.GetKeyState(key))
                    {
                        string msg = "KEY STATE: " + key.ToString();

                        //left modifier keys, will detect multiple 
                        msg += _GetKeyState(Keys.LMenu);
                        msg += _GetKeyState(Keys.LControlKey);
                        msg += _GetKeyState(Keys.LShiftKey);

                        //right modifier keys omitted

                        //left or right modifier keys, will NOT detect multiple!
                        msg += _GetModifierState(Keys.Alt);
                        msg += _GetModifierState(Keys.Control);
                        msg += _GetModifierState(Keys.Shift);

                        //locked keys, will detect multiple
                        msg += _GetLockedState(Keys.CapsLock);
                        msg += _GetLockedState(Keys.Insert);
                        msg += _GetLockedState(Keys.NumLock);
                        msg += _GetLockedState(Keys.Scroll);

                        WriteLine(msg);
                        startTime = DateTime.Now;
                    }

                    if (Win.GetKeyState(Keys.Escape))
                    {
                        WriteLine("ESCAPE!");
                        timeout = false;
                        running = false;
                    }

                    TimeSpan ts = DateTime.Now - startTime;

                    if ((timeoutSeconds - ts.TotalSeconds) <= 0)
                    {
                        WriteLine("timeout!");
                        timeout = true;
                        running = false;
                    }
                }

            }));

            WriteLine("Test complete.");
        }
        public void DEMO_KeyWait()
        {
            textBoxOutput.Focus();
            textBoxOutput.Clear();

            int keyDelayMilliseconds = 150;
            int timeoutSeconds = 5;
            string waitString = "D T" + timeoutSeconds.ToString();

            WriteLine("Press the number 1 key within " + timeoutSeconds + " seconds...");

            if (Win.KeyWait(Keys.D1, waitString)) { WriteLine("TIMEOUT"); return; }
            WriteLine("KEY PRESS: " + Keys.D1);
            Win.Sleep(keyDelayMilliseconds);

            WriteLine("\r\nPress LCtrl-1 within " + timeoutSeconds + " seconds...");

            if (Win.KeyWait(Keys.D1, waitString)) { WriteLine("TIMEOUT"); return; }
            WriteLine("KEY PRESS: " + Keys.D1);
            //Win.Sleep(keyDelayMilliseconds);

            if (Win.KeyWait(Keys.LControlKey, waitString)) { WriteLine("TIMEOUT"); return; }
            WriteLine("KEY PRESS: " + Keys.LControlKey);
            WriteLine("KEY COMBO PRESSED");
            Win.Sleep(keyDelayMilliseconds);

            WriteLine("\r\nPress LCtrl-LAtl-1 within " + timeoutSeconds + " seconds...");

            if (Win.KeyWait(Keys.D1, waitString))
            { WriteLine("TIMEOUT"); return; }
            WriteLine("KEY PRESS: " + Keys.D1);
            //Win.Sleep(keyDelayMilliseconds);

            if (Win.KeyWait(Keys.LControlKey, Keys.Down, 5)) { WriteLine("TIMEOUT"); return; }
            WriteLine("KEY PRESS: " + Keys.LControlKey);
            //Win.Sleep(keyDelayMilliseconds);

            if (Win.KeyWait(Keys.LMenu, Keys.Down, 5)) { WriteLine("TIMEOUT"); return; }
            WriteLine("KEY PRESS: " + Keys.LMenu);
            WriteLine("KEY COMBO PRESSED");

            WriteLine("Test complete.");
        }
        public void DEMO_Kill()
        {
            string WinTitle = "notepad";

            WriteLine("Kill window: " + WinTitle);

            bool result = Win.Kill(WinTitle, 2000);

            WriteLine("Window killed: " + result);

        }
        public void DEMO_Login()
        {
            textBoxOutput.Clear();

            string imageFile = Path.GetFullPath(@"..\..\Resources\logout_button.png");

            if (!File.Exists(imageFile))
            {
                WriteLine("Image file not found: " + imageFile);
            }

            bool restoreCapsLock = false;

            if (Control.IsKeyLocked(Keys.CapsLock))
            {
                Win.SetLockState(Keys.CapsLock, Keys.Up);
                restoreCapsLock = true;
            }

            string url = "http://the-internet.herokuapp.com/login";
            WriteLine("Browse to: " + url);

            ProcessStartInfo psInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psInfo);

            //window title = "The Internet - Google Chrome"
            string WinTitle = "The Internet";

            WriteLine("Waiting 5 seconds for web page to load...");
            if (Win.Wait(WinTitle, 4.5))
            {
                WriteLine("TIMEOUT check your internet connection and try again.");
                return;
            }

            WriteLine("Wait 0.5 seconds to login...");
            //Win.Sleep(2000);
            //Win.Sleep(1000 * 2);
            //just for fun:
            Win.Sleep((int)TimeSpan.FromSeconds(0.5).TotalMilliseconds);

            Win.Send("FLUSH");
            Win.Send("{tab}");
            Win.Send("{tab}");
            Win.Send("tomsmith");
            Win.Send("{tab}");
            Win.Send("SuperSecretPassword!");
            Win.Send("{enter}");

            //Win.Minimize("A");
            //MessageBox.Show("Press OK to close the browser window.", "MESSAGEBOX");
            //Win.Close("");

            WriteLine("Wait 5 seconds to logout...");
            //Rectangle rect = Win.ImageSearch(@"..\..\Resources\logout_button.png", Rectangle.Empty);
            Rectangle rect = Win.WaitImageSearch(imageFile, Rectangle.Empty, 0 ,10);
            if (rect == Rectangle.Empty)
            {
                Win.Activate(this.Text);
                WriteLine("Timeout WaitImageSearch");
                return;
            }
            Win.Click(MOUSE_BUTTON.Left, rect.X, rect.Y);

            //Win.Activate("The Internet");

            //if (Win.WaitActive("The Internet", 5))
            //{
            //    WriteLine("Timeout ....");
            //}
            //else
            //{
            //    WriteLine("ACTIVE!");
            //}

            WriteLine("Finally, logout and close the browser tab.");
            Win.Send("^(w)");

            if (restoreCapsLock)
                Win.SetLockState(Keys.CapsLock, Keys.Down);

            Win.Activate(this.Text);

            WriteLine("Test complete.");
        }
        public void DEMO_Send()
        {

            void _DoSend(int delay)
            {
                Win.SetKeyDelay(delay);
                Win.Send("This is a test with key delay = " + delay.ToString() + "{ENTER}");
            }
            WriteLine("DEMO Send starting...");

            textBoxOutput.Focus();

            //_DoSend(0);
            //_DoSend(100);

            //Win.SetLockState(Keys.Capital, Keys.Up);

            string WinTitle = "wordpad"; // textBoxTitle.Text;

           if (!Win.Exist(WinTitle))
              Process.Start(WinTitle);

            Win.Activate(WinTitle);
            Win.WaitActive();

            Win.Sleep(250);

            //Win.Send("{TAB}Starting...{ENTER}");

            //delete any existing text
            Win.SetKeyDelay(0);
            Win.Send("^aStarting DEMO - ^uPLEASE don't touch keyboard or mouse!^u{ENTER}");
            Win.Send("Changing Font Size...{ENTER}");
            //Win.Send("^bbold, ^iitalic, ^uunderline...{enter}");

            //change font size slowly so user can see the changes
            Win.SetKeyDelay(500);
            Win.Send("%HS1^C26{ENTER}");

            Win.SetKeyDelay(0);
            Win.Send("Font size changed to 26.{ENTER}");
            Win.Sleep(500);

            //restore font size slowly
            Win.Send("Restoring Font size....{ENTER}");
            Win.SetKeyDelay(500);
            Win.Send("%HS1^v{ENTER}");  //wordpad

            //send every possible combination
            Win.SetKeyDelay(0);
            Win.Send("Sending all possible combinations........{bs 5}{enter}");
            Win.Send("CapsLock state: " + Control.IsKeyLocked(Keys.CapsLock) + "~"); //tilde same as {ENTER}
            Win.Send("^(biu)PASSword, {+}{^}{%} {{} {}} {[} {]} {h 10}, +(ec), +ec^(biu){enter}");
            Win.Send("DEMO complete.~");
            Win.Sleep(1000);
            
            Win.Minimize(WinTitle);

            Win.SetKeyDelay();

            WriteLine("DEMO Send complete.");

        }
        public void DEMO_Set()
        {
            string WinTitle = "notepad"; // textBoxTitle.Text;

            if (!Win.Exist(WinTitle))
            {
                try
                {
                    Process.Start(WinTitle);
                }
                catch
                {
                    WriteLine("Can't start process: " + WinTitle);
                    return;
                }
            }

            Win.Restore(WinTitle);

            if (Win.Wait(WinTitle, 10))
            {
                WriteLine("TIMEOUT waiting for: " + WinTitle);
                return;
            }

            Win.Move(10, 10);
            Win.Set(SET.AlwaysOnTop, WinTitle);

            string oldTitle = Win.GetTitle(WinTitle);

            //enable - disable
            const string LASTFOUNDWINDOW = "";
            Win.Set(SET.Disable, LASTFOUNDWINDOW);
            MessageBox.Show("Confirm window is DISABLED then press OK:");

            Win.Set(SET.Enable);
            MessageBox.Show("Confirm window is ENABLED then press OK:");

            //SetTitle
            if (Win.SetTitle("New Title"))
                WriteLine("SetTitle: Success!");
            else
                WriteLine("SetTitle: FAIL!");

            MessageBox.Show(this, "Confirm WinTitle changed to 'New Title', then press OK:");

            Win.SetTitle(oldTitle);
            Win.Set(SET.NotTop, WinTitle);
            Win.Minimize();
            WriteLine("Test complete!");
            Win.SetWinDelay();
        }
        public void DEMO_SysGet()
        {

            ObjMonitor monitor = new ObjMonitor();

            monitor = Win.SysGetMonitor();

            WriteLine("Count        =" + monitor.Count);
            WriteLine("Primary      =" + monitor.Primary);
            WriteLine("Bounds       =" + monitor.Bounds);
            WriteLine("WorkingArea  =" + monitor.WorkingArea);
            WriteLine("Name         =" + monitor.Name);
        }
        public void DEMO_ClipWait()
        {
            Clipboard.Clear();
            WriteLine("Copy Text to Clipboard within 5 seconds...");
            if (Win.ClipWait(5))
            {
                WriteLine("TIMEOUT waiting for text in Clipboard");
                return;
            }
            WriteLine("Text in Clipboard:\r\n" + Clipboard.GetText());
        }

        public void DEMO_GetPixelColor()
        {
            WriteLine("Position Mouse within 3 seconds...");
            Win.Sleep(1000 * 3);
            Point point = Win.MouseGetPoint();
            Color PixelColor = Win.PixelGetColor(point, PIXEL_MODE.RGB);
            WriteLine("Color: " + PixelColor.ToString());
        }
        public void DEMO_AllScreens()
        {
            foreach (var screen in Screen.AllScreens)
            {
                // For each screen, add the screen properties to a list box.
                WriteLine("Device Name: " + screen.DeviceName);
                WriteLine("Bounds: " + screen.Bounds.ToString());
                WriteLine("Type: " + screen.GetType().ToString());
                WriteLine("Working Area: " + screen.WorkingArea.ToString());
                WriteLine("Primary Screen: " + screen.Primary.ToString());
                WriteLine("BitsPerPixel: " + screen.BitsPerPixel);
                WriteLine("Primary: " + screen.Primary);
            }
        }
        public void DEMO_Wait()
        {
            textBoxOutput.Clear();

            string WinTitle = "notepad"; // textBoxTitle.Text;
            int TimeoutSeconds = 10;

            WriteLine("Test window title  : " + WinTitle);
            WriteLine("Test window timeout: " + TimeoutSeconds + " seconds");

            if (!Win.Exist(WinTitle))
            {
                WriteLine("Launch window to start the test!");

                if (Win.Wait(WinTitle, TimeoutSeconds)) { WriteLine("TIMEOUT!"); return; }
            }

            //WriteLine("DEBUG: LastFoundWindowHandle: " + Win.GetID("").ToString("X"));

            if (!Win.Active())
            {
                WriteLine("Click on the window to make it Active...");

                if (Win.WaitActive("", TimeoutSeconds)) { WriteLine("TIMEOUT!"); return; }
            }

            WriteLine("The window is open and active.");
            WriteLine("Click on a different window or minimize it to make it Not Active...");

            if (Win.WaitNotActive("", TimeoutSeconds)) { WriteLine("TIMEOUT!"); return; }

            WriteLine("Click on the window to make it Active, or wait " + TimeoutSeconds + " seconds for a timeout...");

            if (Win.WaitActive("", TimeoutSeconds))
            {
                WriteLine("TIMEOUT!");
            }

            WriteLine("Finally, close the window...");

            if (Win.WaitClose("", TimeoutSeconds))
            {
                WriteLine("TIMEOUT!");
            }
            else
            {
                WriteLine("The window is closed.");
            }

            WriteLine("WinWait test complete.");

        }
    }
}


