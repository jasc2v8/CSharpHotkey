//
//https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=windowsdesktop-6.0
//
//
//  add log.Debug()
//
//
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics; //Process
using System.Threading;
using CSharpHotkeyLib;
using System.Drawing;
using System.Reflection;

namespace csHotKeyTest
{
    public partial class Form1 : Form
    {
        //[DllImport("kernel32.dll")] private static extern int GetTickCount();

        HotKey hotkeyD1 = null;
        HotKey hotkeyEscape = null;

        bool lctrlKeyPressed;
        bool altKeyPressed;
        bool d1KeyPressed;

        private string title = "";

        private CSharpHotkeyLib.CSharpHotkey Win = null;

        private void WriteLine(string text)
        {
            textBoxOutput.AppendText(text + Environment.NewLine);
        }
        void CheckKeyCombo()
        {
            if (lctrlKeyPressed)
            {
                //MessageBox.Show("lctrlKeyPressed");
            }

            if (altKeyPressed)
            {
                //MessageBox.Show("altKeyPressed");
            }

            if (d1KeyPressed)
            {
                //MessageBox.Show("d1KeyPressed");
            }

            if (lctrlKeyPressed && altKeyPressed && d1KeyPressed)
            {
                MessageBox.Show("CheckKeyCombo");
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = Assembly.GetEntryAssembly().GetName().Name + " v" + Assembly.GetEntryAssembly().GetName().Version;

            textBoxTitle.Text = "notepad _class Notepad";

            Win = new CSharpHotkeyLib.CSharpHotkey();

            Win.SetTitleMatchMode(comboBoxMatchMode.Text, comboBoxMatchCase.Text);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //MessageBox.Show("Press OK to Uninstall hook");
        }

        private void comboBoxMatchMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            Win.SetTitleMatchMode(comboBoxMatchMode.Text, comboBoxMatchCase.Text);
        }

        private void comboBoxMatchCase_SelectedIndexChanged(object sender, EventArgs e)
        {
            Win.SetTitleMatchMode(comboBoxMatchMode.Text, comboBoxMatchCase.Text);
        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();
            //comboBoxMatchMode.SelectedItem = "EndsWith";
            //comboBoxMatchCase.SelectedItem = "InSensitive";
        }
        private void buttonClick_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();

            int x, y, w, h = 0;

            Win.GetPos(out x, out y, out w, out h, this.Text);

            x -= 125;

            int delayMS = 3000;
            WriteLine("TEST: Waiting " + delayMS / 1000 + " seconds, then auto Click the Minimize button...");
            Win.Sleep(delayMS);
            Win.Click(MouseButton.Left, x, y);
            WriteLine("Text complete.");
        }
        private void buttonGet_Click(object sender, EventArgs e)
        {

            //reset log file
            const string fileName = @"D:\test.txt";
            System.IO.File.Delete(fileName);

            textBoxOutput.Clear();
            //WriteLine("Window Title Examples: Notepad: notepad _class Notepad" +                    ", Calculator: calc _class ApplicationFrameWindow\r\n");

            string testTitle = "Get Tests";

            string WinTitle = textBoxTitle.Text;

            if (!Win.Exist(WinTitle))
            {
                WriteLine("Open window then try again: " + WinTitle);
                return;
            }

            Win.NameAdd("MainWindow", this.Text);

            Win.NameAdd("TestWindow", WinTitle);

            Win.Activate("_name TestWindow");

            DoWriteInfo("_name TestWindow");

            //check string style for a given WS_ style (similar to AHK)
            string style = Win.Get(GetCommand.Style, "_name TestWindow");
            Int32.TryParse(style, System.Globalization.NumberStyles.HexNumber, null, out int bits);
            WriteLine("Style bits:\t" + bits.ToString("X"));
            int WS_MINIMIZE = 0x20000000;
            bool isMin = (bits & WS_MINIMIZE) != 0;
            WriteLine("WS_MINIMIZE:\t" + isMin);

            //now the easy way
            WriteLine("");
            WriteLine("Win.Get:");
            WriteLine("EXIST:\t\t" + Win.Get(GetCommand.Exist, "_name TestWindow"));
            WriteLine("WS_DISABLED:\t" + Win.Get(GetCommand.Disabled));
            WriteLine("WS_MAXIMIZE:\t" + Win.Get(GetCommand.Maximize));
            WriteLine("WS_MINIMIZE:\t" + Win.Get(GetCommand.Minimize));
            WriteLine("WS_VISIBLE:\t" + Win.Get(GetCommand.Visible));
            WriteLine("NORMAL: \t" + Win.Get(GetCommand.Normal));

            Win.GetActiveStats(out WinTitle, out int Width, out int Height, out int X, out int Y);
            WriteLine("ActiveStats:\tTitle: " + WinTitle +
                ", W: " + Width + ", H: " + Height + ", X: " + X + ", Y: " + Y);

            ObjActiveStats oas = new ObjActiveStats();
            oas = Win.GetActiveStats();
            WriteLine("ActiveStats:\tTitle: " + oas.Title +
                ", W: " + oas.Bounds.Width + ", H: " + oas.Bounds.Height + 
                ", X: " + oas.Bounds.X + ", Y: " + oas.Bounds.Y);

            Rectangle WinRect;
            Win.GetActiveStats(out WinTitle, out WinRect);
            WriteLine("ActiveStats:\tTitle: " + WinTitle +
                ", W: " + WinRect.Width + ", H: " + WinRect.Height +
                ", X: " + WinRect.X + ", Y: " + WinRect.Y);

            Rectangle rect = Win.GetPos();
            WriteLine("GetPos:\t\tX: " + rect.X + ", Y: " + rect.Y + ", W: " + rect.Width + ", H: " + rect.Height);

            ObjMonitor m = new ObjMonitor();
            m = Win.SysGetMonitor();
            WriteLine("Mon Bounds:\tX: " + m.Bounds.X + ", Y: " + m.Bounds.Y + ", W: " + m.Bounds.Width + ", H: " + m.Bounds.Height);
            WriteLine("Work Area :\tX: " + m.WorkingArea.X + ", Y: " + m.WorkingArea.Y + ", W: " + m.WorkingArea.Width + ", H: " + m.WorkingArea.Height);

            WriteLine("Test complete.");

            Win.Activate("_name MainWindow");

            return;

        }
        private void DoWriteInfo(string WinTitle)
        {
            WriteLine("\r\nTITLE: \t\t" + Win.GetTitle(WinTitle));
            WriteLine("ID: \t\t" + Win.Get(GetCommand.ID));
            WriteLine("PID: \t\t" + Win.Get(GetCommand.PID));
            WriteLine("ProcessName: \t" + Win.Get(GetCommand.ProcessName));
            WriteLine("ProcessPath: \t" + Win.Get(GetCommand.ProcessPath));

        }
        private void buttonHook_Click(object sender, EventArgs e)
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
        private void buttonHotKey_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();
            WriteLine("Test Hotkeys Action...");
            WriteLine("Uses public Class Hotkey");

            hotkeyD1 = new HotKey(Keys.Control | Keys.Alt, Keys.D1, (none) =>
            {
                WriteLine("D1 HOTKEY PRESSED!");
            });

            hotkeyEscape = new HotKey(Keys.None, Keys.Escape, (none) =>
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
        private void buttonInputBox_Click(object sender, EventArgs e)
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
            string IconFile = @"..\..\face-smile.ico";

            WriteLine("Opening InputBox with Timeout of " + TimeoutSeconds + " seconds");

            ObjInputBox IB = new ObjInputBox();

            IB = Win.InputBox(Title, Prompt, Hide,
                Location, TimeoutSeconds, Default, IconFile);
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
        private void buttonKeyLock_Click(object sender, EventArgs e)
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
        private void buttonKeyState_Click(object sender, EventArgs e)
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
        private void buttonKeyWait_Click(object sender, EventArgs e)
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
        private void buttonKill_Click(object sender, EventArgs e)
        {
            string WinTitle = textBoxTitle.Text;

            WriteLine("Kill window: " + WinTitle);

            bool result = Win.Kill(WinTitle, 2000);

            WriteLine("Window killed: " + result);

        }
        private void buttonLogin_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();

            bool restoreCapsLock = false;

            if (Control.IsKeyLocked(Keys.CapsLock))
            {
                Win.SetLockState(Keys.CapsLock, Keys.Up);
                restoreCapsLock = true;
            }

            string url = "http://the-internet.herokuapp.com/login";
            WriteLine("Browse to: " + url);
            WriteLine("Waiting 5 seconds for web page to load...");
            WriteLine("Then 2 seconds to login...");
            WriteLine("Finally, 2 seconds to logout...");

            ProcessStartInfo psInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psInfo);

            //window title = "The Internet - Google Chrome"
            textBoxTitle.Text = "The Internet";
            comboBoxMatchMode.SelectedItem = "StartsWith";
            comboBoxMatchCase.SelectedItem = "InSensitive";

            if (Win.Wait(textBoxTitle.Text, 4.5))
            {
                WriteLine("TIMEOUT check your internet connection and try again.");
                return;
            }

            //Win.Sleep(2000);
            //Win.Sleep(1000 * 2);
            //just for fun:
            Win.Sleep((int)TimeSpan.FromSeconds(2).TotalMilliseconds);

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

            Win.Sleep(1000 * 2);
            Rectangle rect = Win.ImageSearch(@"..\..\Resources\logout_button.png", Rectangle.Empty);
            Win.Click(MouseButton.Left, rect.X, rect.Y);

            if (restoreCapsLock)
               Win.SetLockState(Keys.CapsLock, Keys.Down);

            WriteLine("Test complete.");
        }
        private void buttonMinMax_Click(object sender, EventArgs e)
        {

            textBoxOutput.Clear();
            string testTitle = "WinMinMaxShowHide";

            string WinTitle = textBoxTitle.Text;
            int timeout = 10;

            //if (title.ToLower() == "notepad")
            //    title += " _class Notepad";

            //if (title.ToLower() == "calc")
            //    title += " _class ApplicationFrameWindow";

            WriteLine("Test window title = " + WinTitle);
            WriteLine("Test window timeout value = " + timeout + " seconds");

            Win.Set(SetCommand.AlwaysOnTop, this.Text);

            if (!Win.Exist(WinTitle))
            {
                WriteLine("Launch window within the timeout value to start the test!");

                if (Win.Wait(WinTitle, timeout)) { WriteLine("TIMEOUT!"); return; }
            }

            DoWriteInfo(WinTitle);

            Win.Activate(this.Text);
            Win.Minimize(this.Text);

            Win.MinimizeAll();
            MessageBox.Show(this, "Window state: MinimizeAll", testTitle);

            Win.Restore(WinTitle);
            MessageBox.Show(this, "Window state: Restore", testTitle);

            Win.Maximize();
            //Win.Activate(this.Text);
            //Win.Minimize(this.Text);
            //DoWriteInfo(title);
            MessageBox.Show(this, "Window state: Maximize", testTitle);

            Win.Restore();
            MessageBox.Show(this, "Window state: Restore", testTitle);

            Win.Minimize();
            MessageBox.Show(this, "Window state: Minimize", testTitle);

            Win.Maximize();
            MessageBox.Show(this, "Window state: Maximize", testTitle);

            Win.Restore();
            MessageBox.Show(this, "Window state: Restore", testTitle);

            Win.Hide();
            MessageBox.Show("Window state: Hide", testTitle);

            Win.Show();
            MessageBox.Show("Window state: Show", testTitle);

            //Win.Restore(title);
            //MessageBox.Show("Window state: Restore", testTitle);

            Win.MinimizeAllUndo();

            //Win.Close(title);
            Win.Activate(this.Text);

            MessageBox.Show(this, "Window state: MinimizeAllUndo", testTitle);
            WriteLine("WinWait test complete.");

            WriteLine("\r\nDebugString:\r\n" + Win.DebugString);

            Win.Set(SetCommand.NotTop, this.Text);

        }
        private void buttonMove_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();

            WriteLine("Test start!");

            string WinTitle = textBoxTitle.Text;

            string processName = WinTitle.Split(' ')[0];

            if (!Win.Exist(WinTitle))
            {
                try
                {
                    Process.Start(processName);
                }
                catch
                {
                    WriteLine("Error starting process: " + processName);
                    return;
                }
            }

            if (Win.Wait(WinTitle, 10))
            {
                WriteLine("TIMEOUT waiting for: " + WinTitle);
                return;
            }

            Win.Minimize(this.Text);

            Win.Restore(WinTitle);

            int taskBarHeight = System.Windows.Forms.SystemInformation.CaptionHeight;

            System.Drawing.Size winBorder = System.Windows.Forms.SystemInformation.FrameBorderSize;

            System.Drawing.Rectangle scrRect = Win.SysGetMonitor().Bounds;
            System.Drawing.Rectangle winRect = Win.GetPos();

            //Win.GetScreen(out int scrLeft, out int scrRight, out int scrTop, out int scrBottom, out int scrWidth, out int scrHeight);
            //WriteLine("\r\nGetScreen:");
            //WriteLine("Left : " + scrLeft);
            //WriteLine("Right: " + scrRight);
            //WriteLine("Top : " + scrTop);
            //WriteLine("Bottom: " + scrBottom);
            //WriteLine("Width : " + scrWidth);
            //WriteLine("Height: " + scrHeight);

            //Win.GetPos(out winX, out winY, out winWidth, out winHeight);
            //WriteLine("\r\nGetPos:");
            //WriteLine("Title : " + WinTitle);
            //WriteLine("X     : " + winX);
            //WriteLine("Y     : " + winY);
            //WriteLine("Width : " + winWidth);
            //WriteLine("Height: " + winHeight);

            int saveWidth = winRect.Width;
            int saveHeight = winRect.Height;

            //int w = SystemInformation.VirtualScreen.Width;
            //int h = SystemInformation.VirtualScreen.Height;
            //int t = Screen.PrimaryScreen.WorkingArea.Top;
            //int b = Screen.PrimaryScreen.WorkingArea.Bottom;
            //int l = Screen.PrimaryScreen.WorkingArea.Left;
            //int r = Screen.PrimaryScreen.WorkingArea.Right;

            //slow down so user can see the window move
            Win.SetWinDelay(750);

            //upper left
            int winX = 0;
            int winY = 0;
            Win.Move(winX, winY); //use LastFoundWindow

            //upper right
            //X = SystemInformation.VirtualScreen.Right - Width + frameBorderSize.Width;
            //winX = scrRight - winWidth + winBorder.Width + 1;
            winX = scrRect.Width - winRect.Width + winBorder.Width + 1;
            Win.Move(winX, winY);

            //lower left
            winX = 0;
            //winY = SystemInformation.VirtualScreen.Bottom - winHeight - taskBarHeight - frameBorderSize.Height - 1;
            //winY = scrBottom - winHeight - taskBarHeight - winBorder.Height - 1;
            winY = scrRect.Height - winRect.Height - taskBarHeight - winBorder.Height - 1;
            Win.Move(winX, winY);

            //lower right
            //winX = SystemInformation.VirtualScreen.Right - winWidth - frameBorderSize.Width;
            //winX = scrRight - winWidth - winBorder.Width + 1;
            winX = scrRect.Width - winRect.Width - winBorder.Width + 1;
            Win.Move(winX, winY);

            //center
            //winX = SystemInformation.VirtualScreen.Width / 2 - (winWidth / 2);
            //winY = SystemInformation.VirtualScreen.Height / 2 - (winHeight / 2);
            winX = scrRect.Width / 2 - (winRect.Width / 2);
            winY = scrRect.Height / 2 - (winRect.Height / 2);
            Win.Move(winX, winY);

            //half size
            int winWidth = winRect.Width / 2;
            int winHeight = winRect.Height / 2;
            winX = scrRect.Width / 2 - (winWidth / 2);
            winY = scrRect.Height / 2 - (winHeight / 2);
            Win.Move(winX, winY, winWidth, winHeight);

            //double size
            winWidth *= 4;
            winHeight *= 4;
            winX = scrRect.Width / 2 - (winWidth / 2);
            winY = scrRect.Height / 2 - (winHeight / 2);
            Win.Move(winX, winY, winWidth, winHeight);

            //orignal size
            winWidth = saveWidth;
            winHeight = saveHeight;
            winX = scrRect.Width / 2 - (winWidth / 2);
            winY = scrRect.Height / 2 - (winHeight / 2);
            Win.Move(winX, winY, winWidth, winHeight);

            //finish
            Win.Minimize();
            Win.Restore(this.Text);
            //WriteLine("Active Window: " + Win.GetActiveTitle());
            WriteLine("Test complete!");
            Win.SetWinDelay();
        }
        private void buttonProcess_Click(object sender, EventArgs e)
        {
            Win.Show(title);

            Process[] processlist = Process.GetProcesses();

            WriteLine("Process Count: " + processlist.Count());
            WriteLine("Processes with a WinTitle: ");

            int i = 1;

            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    WriteLine("  "+ i + ": " + process.ProcessName +
                        ", PID: " + process.Id +
                        ", Title: " + process.MainWindowTitle);
                    i++;
                }
            }
            WriteLine(Environment.NewLine);
        }
        private void buttonSet_Click(object sender, EventArgs e)
        {
            string WinTitle = textBoxTitle.Text;

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

            Win.Move(10,10);
            Win.Set(SetCommand.AlwaysOnTop, WinTitle);

            string oldTitle = Win.GetTitle(WinTitle);

            //enable - disable
            const string LASTFOUNDWINDOW = "";
            Win.Set(SetCommand.Disable, LASTFOUNDWINDOW);
            MessageBox.Show("Confirm window is DISABLED then press OK:");

            Win.Set(SetCommand.Enable);
            MessageBox.Show("Confirm window is ENABLED then press OK:");

            //SetTitle
            if (Win.SetTitle("New Title"))
                WriteLine("SetTitle: Success!");
            else
                WriteLine("SetTitle: FAIL!");

            MessageBox.Show(this, "Confirm WinTitle changed to 'New Title', then press OK:");

            Win.SetTitle(oldTitle);
            Win.Set(SetCommand.NotTop, WinTitle);
            Win.Minimize();
            WriteLine("Test complete!");
            Win.SetWinDelay();
        }
        private void buttonSysGet_Click(object sender, EventArgs e)
        {

            ObjMonitor monitor = new ObjMonitor();

            monitor = Win.SysGetMonitor();

            WriteLine("Count        =" + monitor.Count);
            WriteLine("Primary      =" + monitor.Primary);
            WriteLine("Bounds       =" + monitor.Bounds);
            WriteLine("WorkingArea  =" + monitor.WorkingArea);
            WriteLine("Name         =" + monitor.Name);
        }
        private void buttonTest_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();
            string WinTitle = textBoxTitle.Text;

            WriteLine("Add your own test here...");
            return;

            //Text BlockInput - must run as Admin
            if (Win.BlockInput(true))
            {
                WriteLine("Blocking keyboard and mouse input for 10 seconds...");
                Win.Sleep(1000 * 10);
                Win.BlockInput(false);
                WriteLine("Enabled keyboard and mouse input.");
            }
            else
            {
                WriteLine("ERROR BlockInput - must run as Admin.");
            }
            return;


            //Test ClipWait
            Clipboard.Clear();
            
            WriteLine("Copy Text to Clipboard within 5 seconds...");
            if (Win.ClipWait(5))
            {
                WriteLine("TIMEOUT waiting for text in Clipboard");
                return;
            }
            WriteLine("Text in Clipboard:\r\n" + Clipboard.GetText());

            return;

            //Test Inputbox with out variables
            Win.InputBox("Title", "Prompt", false, new Point(400, 800), out string Result, out string Value);
            WriteLine("Result: " + Result + ", Value: " + Value);
            return;

            //Test GetPixelColor()
            WriteLine("Position Mouse within 3 seconds...");
            Win.Sleep(1000 * 3);
            Point point = Win.MouseGetPoint();
            Color PixelColor = Win.PixelGetColor(point, PixelMode.RGB);
            WriteLine("Color: " + PixelColor.ToString());
            return;

            //Test list all screens
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
            return;

        }
        private void buttonWait_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();

            string WinTitle = textBoxTitle.Text;
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
            } else
            {
                WriteLine("The window is closed.");
            }

            WriteLine("WinWait test complete.");

        }

        private void buttonWindow_Click(object sender, EventArgs e)
        {

            string title = textBoxTitle.Text;

            Process.Start(title);

            //long way to get from seconds to milliSeconds
            //            if (Win.Wait(title, (int)TimeSpan.FromSeconds(5).TotalMilliseconds))

            if (Win.Wait(title, 5*1000))
            {
                WriteLine("TIMEOUT check your internet connection and try again.");
                return;
            }

            if (Win.Exist(title))
            {

                WriteLine("");

                WriteLine("Exists   : " + title);

                //if (Win.IsVisible("")) {
                //    WriteLine("IsVisible: " + title);

                //}
                //if (Win.IsIconic("")) {
                //    WriteLine("IsIconic : " + title);
                //}
            }
            else
            {
                WriteLine("NOT Exists: " + title);
            }

            Win.Minimize(title);

            MessageBox.Show("Press OK to close the browser window titled: " + title, "MESSAGEBOX");

            Win.Close(title);

        }
        private void buttonWindowsList_Click(object sender, EventArgs e)
        {
            List<string> windowsList = Win.GetWindowsList();
            //List<string> windowsList = Win.GetDesktopWindowsList();

            WriteLine("Window Count: " + windowsList.Count());

            int i = 1;

            foreach (string title in windowsList)
            {
                IntPtr hWnd = Win.GetID(title);

                //debug
                bool v = Win.IsVisible();
                bool m = Win.IsIconic();

                //WriteLine("  " + i + ": " + title + ": " + hWnd.ToString("X"));
                WriteLine("  " + i + ": " + title + ": " + hWnd.ToString("X") + 
                    ", Visible: " + v +
                    ", Iconic: " + m);
                i++;
            }
            WriteLine(Environment.NewLine);
        }
        private bool StartWindow(string WinTitle = "", int TimeoutSeconds = 10)
        {
            textBoxOutput.Clear();

            if (WinTitle == String.Empty)
                WinTitle = textBoxTitle.Text;

            WriteLine("Test window = " + WinTitle + ", timeout = " + TimeoutSeconds + " seconds");

            if (!Win.Exist(WinTitle))
            {
                WriteLine("Launch window to start the test!");

                if (Win.Wait(WinTitle, TimeoutSeconds))
                {
                    WriteLine("TIMEOUT!");
                    return false;
                }
            }
            return true;
        }
    }
}
