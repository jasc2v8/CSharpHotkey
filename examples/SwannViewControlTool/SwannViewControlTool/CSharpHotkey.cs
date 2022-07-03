/*

    2022-06-28-1215 v0.0.10 Fix Activate() and Restore(), global replace UpDown with DownOrUp
    2022-06-21-0900 v0.0.09 SendMode(SendModeCommand.Event or SendModeCommand.Input), renamed DownOrUp to DownOrUp
    2022-06-19-1650 v0.0.08 SendInput(Keys Key, Keys DownOrUp = Keys.None)
    2022-06-19-1045 v0.0.07 Send(Keys Key, Keys DownOrUp = Keys.None), renamed Objects (ex: ObjMonitor to MonitorObj)
    2022-06-18-1430 v0.0.06 Add Sending.Key so Send() doesn't trigger InputHook() or HotKey()
    2022-06-18-0630 v0.0.05 Remove reference to PresentationCore, add MouseButton
    2022-06-17-1500 v0.0.04 Change Struct to Enum for function parameters
    2022-06-17-0500 v0.0.03 Add Send(Keys.Key), fix SetLockState()
    2022-06-14-1620 v0.0.02 
    
    TODO:
      continue testing and development

        
    OVERVIEW:
      README at https://github.com/jasc2v8/CSharpHotkey
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Clipboard = System.Windows.Forms.Clipboard;
using Cursor = System.Windows.Forms.Cursor;

namespace CSharpHotkeyLib
{

    #region Public Class Objects
    public class ActiveStatsObj
    {
        public string Title { get; set; }
        public Rectangle Bounds { get; set; }
    }
    public class InputBoxObj
    {
        public string Result { get; set; }
        public string Value { get; set; }
    }
    public class MonitorObj
    {
        public int Count { get; } = Screen.AllScreens.Count();
        public int Primary { get; set; } = 1;
        public Rectangle Bounds { get; set; } = Screen.PrimaryScreen.Bounds;
        public Rectangle WorkingArea { get; set; } = Screen.PrimaryScreen.WorkingArea;
        public string Name { get; set; } = Screen.PrimaryScreen.DeviceName;
    }
    public class MousePosObj
    {
        public Point Point;
        public IntPtr Handle;
    }
    public static class Sending
    {
        public static Keys Key;
    }
    #endregion Public Class Objects

    #region Public Enum

    public enum GetCommand
    {
        ID = 1,
        MinMax = 2,
        PID = 3,
        ProcessName = 4,
        ProcessPath = 5,
        Style = 6,
        Disabled = 7,
        Exist = 8,
        Maximize = 9,
        Minimize = 10,
        Normal = 11,
        Visible = 12,
    }
    public enum SendModeCommand
    {
        Event = 0,
        Input = 1,
    }
    public enum SetCommand
    {
        AlwaysOnTop = 1,
        Bottom = 2,
        Disable = 3,
        Enable = 4,
        NotTop = 5,
        Top = 6,
    }
    public enum MatchMode
    {
        StartsWith = 1,
        Contains = 2,
        
        Exact = 3,
    
        EndsWith = 4,
    }
    public enum MatchCase
    {
        InSensitive = 1,
        Sensitive = 2,
    }
    public enum MouseButton
    {
        //System.Windows.Input.MouseButton Enum
        Left = 0,
        Middle = 1,
        Right = 2,
    }
    public enum PixelMode
    {
        BGR = 1,
        RGB = 2,
    }

    #endregion Public Enum

    public partial class CSharpHotkey
    {

        #region Private Struct

        [StructLayout(LayoutKind.Sequential)]
        private struct DBLCLK
        {
            public static MouseButtons mouseButton = MouseButtons.None;
            public static int clickCount = 0;
            public static DateTime clickedTime = DateTime.Now;
            public static int x = 0;
            public static int y = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class MouseLLHookStruct
        {
            public Point pt;
            public int mouseData;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        #endregion

        #region Properties

        private bool USE_LOG = false;
        public string DebugString { get; set; }

        //LastFoundWindow
        //In AHK, this is the window most recently found by
        //  IfWinExist, IfWinNotExist, WinExist(), IfWinActive, IfWinNotActive, WinActive(),
        //  WinWaitActive, WinWaitNotActive, or WinWait.
        //Here it's the window found by GetID(), which is used in most functions
        private static IntPtr LastFoundWindowHandle { get; set; }
        private static MatchMode TitleMatchMode { get; set; }
        private static MatchCase TitleMatchCase { get; set; }
        private static int KeyDelay { get; set; }
        private static int MouseDelay { get; set; }
        private static SendModeCommand SendModeSelection { get; set; } = SendModeCommand.Event;
        private static int WinDelay { get; set; }
        public bool MouseEventHandledFlag { get; set; }
        #endregion Properties

        #region Variables
        private static Dictionary<string, IntPtr> windowHandleDict = new Dictionary<string, IntPtr>();
        #endregion Variables

        #region Delegates
        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        #endregion

        #region Constructor
        public CSharpHotkey()
        {
            USE_LOG = false;

            LastFoundWindowHandle = IntPtr.Zero;

            TitleMatchMode = MatchMode.Exact;
            TitleMatchCase = MatchCase.Sensitive;

            KeyDelay = 10;
            MouseDelay = 10;
            WinDelay = 100;

            MouseEventHandledFlag = false; //not used. never supress mouse activity

        }
        #endregion Constructor

        #region Public Methods
        public bool Activate(string WinTitle = "")
        {
            bool result1 = false;
            bool result2 = false;

            if (Get(GetCommand.Minimize, WinTitle) == "1")
                result1 = Win32.ShowWindow(GetID(WinTitle), Win32.SW_RESTORE);

            result2 = Win32.SetForegroundWindow(GetID(WinTitle));
            DoDelay(WinDelay);
            return result1 | result2;
        }

        public void ActivateBottom_UNIMPLEMENTED()
        {
            //rarely used
        }
        public bool Active(string WinTitle = "")
        {
            if (GetID(WinTitle) == Win32.GetForegroundWindow())
                return true;

            return false;
        }
        
        public bool BlockInput(bool Flag)
        {
            //BlockInput may only work if the script has been run as administrator.
            return Win32.BlockInput(Flag);
        }
        public void Click(MouseButton mb = MouseButton.Left, Point MousePoint = default)
        {
            Click(mb, MousePoint.X, MousePoint.Y);

        }
        public void Click(MouseButton mb = MouseButton.Left, int x = -1, int y = -1)
        {
            //yes, I know mouse_event is depreciated
            //if a more robust solution is needed, there are several on Github and Nuget:
            //https://github.com/search?q=InputSimulator+language%3AC%23&type=Repositories&ref=advsearch&l=C%23&l=
            //https://www.nuget.org/packages?q=InputSimulator

            uint EVENT_DOWN = 0;
            uint EVENT_UP = 0;

            switch (mb)
            {
                case MouseButton.Left:
                    EVENT_DOWN = Win32.MOUSEEVENTF_LEFTDOWN;
                    EVENT_UP = Win32.MOUSEEVENTF_LEFTUP;
                    break;
                case MouseButton.Middle:
                    EVENT_DOWN = Win32.MOUSEEVENTF_MIDDLEDOWN;
                    EVENT_UP = Win32.MOUSEEVENTF_MIDDLEUP;
                    break;
                case MouseButton.Right:
                    EVENT_DOWN = Win32.MOUSEEVENTF_RIGHTDOWN;
                    EVENT_UP = Win32.MOUSEEVENTF_RIGHTUP;
                    break;
                default:
                    EVENT_DOWN = Win32.MOUSEEVENTF_LEFTDOWN;
                    EVENT_UP = Win32.MOUSEEVENTF_LEFTUP;
                    break;
            }

            if (x >= 0 && y >= 0)
                System.Windows.Forms.Cursor.Position = new Point(x, y);

            Win32.mouse_event(EVENT_DOWN, 0, 0, 0, 0);
            Thread.Sleep(10);
            Win32.mouse_event(EVENT_UP, 0, 0, 0, 0);

            DoDelay(MouseDelay);
        }
        public bool ClipWait(double TimeoutSeconds, string DataFormats = "Text")
        {
            //use System.Windows.Clipboard class

            if (TimeoutSeconds < 0) { TimeoutSeconds = Int32.MaxValue; }
            if (TimeoutSeconds == 0) { TimeoutSeconds = 0.5; }
            DateTime waitTime = DateTime.Now.AddSeconds(TimeoutSeconds);

            while (DateTime.Now < waitTime)
            {
                DoDelay(10);

                if (Clipboard.ContainsData(DataFormats))
                    return false; //no timeout
            }
            return true; //timeout
        }
        public bool Close(string WinTitle = "")
        {
            IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
            bool result = Win32.PostMessage(new HandleRef(hWnd, GetID(WinTitle)), Win32.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            DoDelay(WinDelay);
            return result;
        }
        public bool Exist(string WinTitle = "")
        {
            if (GetID(WinTitle) != IntPtr.Zero) { return true; } else { return false; }
        }
        public string Get(GetCommand Command, string WinTitle = "")
        {
            IntPtr hWnd = GetID(WinTitle);

            if (hWnd == IntPtr.Zero)
                return String.Empty;

            int pid = 0;
            Win32.GetWindowThreadProcessId(hWnd, out pid);

            int bits = Win32.GetWindowLong(hWnd, Win32.GWL_STYLE);

            bool isDisabled = (bits & Win32.WS_DISABLED) != 0;  //Win32.IsWindowEnabled
            bool isExist = (hWnd != IntPtr.Zero);
            bool isIconic = (bits & Win32.WS_ICONIC) != 0;
            bool isMax = (bits & Win32.WS_MAXIMIZE) != 0;
            bool isMin = (bits & Win32.WS_MINIMIZE) != 0;       //Win32.IsIconic
            bool isVisible = (bits & Win32.WS_VISIBLE) != 0;    //Win32.IsVisible
            bool isNormal = (!isMax & !isMin & isVisible);

            string result = String.Empty;

            switch (Command)
            {
                case GetCommand.ID:
                    break;
            }

            if (Command == GetCommand.ID)
            {
                result = hWnd.ToString("X");
            } 
            else if (Command == GetCommand.PID)
            {
                result = pid.ToString("X");
            }
            else if (Command == GetCommand.ProcessName)
            {
                Win32.GetWindowThreadProcessId(hWnd, out pid);
                Process p = Process.GetProcessById(pid);
                result = p.ProcessName;
            }
            else if (Command == GetCommand.ProcessPath)
            {
                const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
                IntPtr hproc = Win32.OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
                StringBuilder buffer = new StringBuilder(Win32.MAX_PATH);
                Win32.GetModuleFileNameEx(hproc, IntPtr.Zero, buffer, buffer.Capacity);
                string processPath = buffer.ToString();
                result = processPath;
            }
            else if (Command == GetCommand.Style)
            {
                int style = Win32.GetWindowLong(hWnd, Win32.GWL_STYLE);
                result = style.ToString("X");
            }
            else if (Command == GetCommand.Disabled)
            {
                if (isDisabled) result = "1";
            }
            else if (Command == GetCommand.Exist)
            {
                if (isExist) result = "1";
            }
            else if (Command == GetCommand.Maximize)
            {
                if (isMax) result = "1";
            }
            else if (Command == GetCommand.Minimize)
            {
                if (isMin) result = "1";
            }
            else if (Command == GetCommand.Normal)
            {
                if (isNormal) result = "1";
            }
            else if (Command == GetCommand.Visible)
            {
                if (isVisible) result = "1";
            }
            return result;
        }
        public ActiveStatsObj GetActiveStats()
        {
            ActiveStatsObj o = new ActiveStatsObj();
            o.Title = GetActiveTitle();
            o.Bounds = GetPos(o.Title);
            return o;
        }
        public void GetActiveStats(out string WinTitle, out Rectangle WinRect)
        { 
            WinTitle = GetActiveTitle();
            WinRect = GetPos(WinTitle);
        }
        public void GetActiveStats(out string WinTitle, out int Width, out int Height, out int X, out int Y)
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;

            WinTitle = GetActiveTitle();

            GetPos(out X, out Y, out Width, out Height, WinTitle);
        }
        public string GetActiveTitle()
        {
            IntPtr hWnd = GetID("A");
            string windowTitle = String.Empty;
            StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
            if (Win32.GetWindowText(hWnd, sb, sb.Capacity) != 0)
                windowTitle = sb.ToString();
            return windowTitle;
        }
        public string GetClass(string WinTitle = "")
        {
            IntPtr hWnd = GetID(WinTitle);
            string className = String.Empty;
            StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
            if (Win32.GetClassName(hWnd, sb, sb.Capacity) != 0)
                className = sb.ToString();
            return className;
        }
        public IntPtr GetID(string WinTitle = "")
        {
            /*
                Usage: WinTitle = [MyTitle [_class MyClassName]]

                Omit the WinTitle to operate on the last found window
                Use the letter A to operate on the active window

                Implemented:
                ------------
                Title   Matching Behaviour
                -----   ------------------
                (empty)	Last Found Window
                A       The Active Window
                _class  Window Class
                _id     Unique HWND
                _pid	Process ID
                _exe	Process Name/Path
                _name   Saves window titles and handles in a dictionary for later reference by a given key name
                        This is much faster as it will avoid additional enum searches for the same window
                        The Key can be overwritten with a different hWnd value
                        Example     Win.NameAdd("MyCalculator", "calc _class ApplicationFrameWindow");
                        Usage       Win.Activate("MyCalculator"); //case sensitive
                        Internal    Dictionary of <string keyName, IntPtr hWnd = GetID(WinTitle)>

                Not Implemented:
                -------------------
                _group	Window Group (use _name with fewer features instead)
             */

            if (WinTitle == "")
            {
                return LastFoundWindowHandle;
            }
            else if (WinTitle == "A")
            {
                LastFoundWindowHandle = Win32.GetForegroundWindow();
                return LastFoundWindowHandle;
            }

            string pTitle = String.Empty;
            string pClass = String.Empty;
            string pHWND = String.Empty;
            string pID = String.Empty;
            string pExe = String.Empty;
            string pName = String.Empty;

            string windowTitle = String.Empty;
            string windowClass = String.Empty;
            string processHWND = String.Empty;
            string processID = String.Empty;
            string processName = String.Empty;
            string processPath = String.Empty;
            int pid = 0;

            string[] output = WinTitle.Split('_');

            foreach (string param in output)
            {
                //WriteLog("### param = " + param);

                if (param.StartsWith("class")) { pClass = param.Substring(6).Trim(); }
                else if (param.StartsWith("id")) { pHWND = param.Substring(3).Trim(); }
                else if (param.StartsWith("pid")) { pID = param.Substring(4).Trim(); }
                else if (param.StartsWith("exe")) { pExe = param.Substring(4).Trim(); }
                else if (param.StartsWith("name")) { pName = param.Substring(5).Trim(); }
                else { pTitle = param.Trim(); }
            }

            IntPtr foundWindowHandle = IntPtr.Zero;

            if (pName != String.Empty)
            {
                LastFoundWindowHandle = windowHandleDict[pName];
                return LastFoundWindowHandle;
            }

            Win32.EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (!Win32.IsWindowVisible(hWnd))
                    return true; //true=continue enum loop

                if (pHWND != String.Empty)
                    processHWND = hWnd.ToString();

                if (pID != String.Empty)
                {
                    Win32.GetWindowThreadProcessId(hWnd, out pid);
                    processID = pid.ToString();
                }

                if (pExe != String.Empty)
                {
                    Win32.GetWindowThreadProcessId(hWnd, out pid);
                    processID = pid.ToString();

                    Process p = Process.GetProcessById(pid);
                    processName = p.ProcessName;

                    const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
                    IntPtr hproc = Win32.OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
                    StringBuilder buffer = new StringBuilder(Win32.MAX_PATH);
                    Win32.GetModuleFileNameEx(hproc, IntPtr.Zero, buffer, buffer.Capacity);
                    processPath = buffer.ToString();

                    if (processPath == string.Empty) { processPath = processName; }
                }

                if (pTitle != String.Empty)
                {
                    int length = Win32.GetWindowTextLength(hWnd);
                    if (length != 0)
                    {
                        StringBuilder sb = new StringBuilder(length);
                        if (Win32.GetWindowText(hWnd, sb, length + 1) != 0)
                            windowTitle = sb.ToString();

                        if (windowTitle == "Program Manager" ||
                            windowTitle == "Windows Shell Experience Host")
                            return true;
                    }
                }

                if (pClass != String.Empty)
                {
                    StringBuilder ClassName = new StringBuilder(256);
                    if (Win32.GetClassName(hWnd, ClassName, ClassName.Capacity) != 0)
                        windowClass = ClassName.ToString();
                }
                
                if (IsMatch(windowTitle, pTitle))
                {
                    WriteLog("");
                    WriteLog("MATCH TITLE ONLY: ");
                    WriteLog("pTitle     : [" + pTitle + "]");
                    WriteLog("pClass     : [" + pClass + "]");
                    WriteLog("pHWND      : [" + pHWND + "]");
                    WriteLog("pID        : [" + pID + "]");
                    WriteLog("pExe       : [" + pExe + "]");
                    WriteLog("windowHWND : [" + hWnd.ToString("x") + "]");
                    WriteLog("windowTitle: [" + windowTitle + "]");
                    WriteLog("windowClass: [" + windowClass + "]");
                    WriteLog("processHWND: [" + processHWND + "]");
                    WriteLog("processID  : [" + processID + "]");
                    WriteLog("processName: [" + processName + "]");
                    WriteLog("processPath: [" + processPath + "]");
                }

                bool matchTitle = true;
                bool matchClass = true;
                bool matchHWND = true;
                bool matchPID = true;
                bool matchExeName = true;
                bool matchExePath = true;

                if (pTitle != String.Empty && !IsMatch(windowTitle, pTitle))
                    matchTitle = false;

                if ((pClass != String.Empty) && (windowClass != String.Empty) && (!IsMatch(windowClass, pClass)))
                    matchClass = false;

                if ((pHWND != String.Empty) && (processHWND != String.Empty) && (!IsMatch(processHWND, pHWND)))
                    matchHWND = false;

                if ((pID != String.Empty) && (processID != String.Empty) && (!IsMatch(processID, pID)))
                    matchPID = false;

                if ((pExe != String.Empty) && (processName != String.Empty) && (!IsMatch(processName, pExe)))
                    matchExeName = false;

                if ((pExe != String.Empty) && (processPath != String.Empty) && (!IsMatch(processPath, pExe)))
                    matchExePath = false;

                if (matchTitle & matchClass & matchHWND & matchPID & matchExeName & matchExePath)
                {

                    WriteLog("");
                    WriteLog("MATCH TITLE: ");
                    WriteLog("pTitle     : [" + pTitle + "]");
                    WriteLog("pClass     : [" + pClass + "]");
                    WriteLog("pHWND      : [" + pHWND + "]");
                    WriteLog("pID        : [" + pID + "]");
                    WriteLog("pExe       : [" + pExe + "]");
                    WriteLog("windowHWND : [" + hWnd.ToString("x") + "]");
                    WriteLog("windowTitle: [" + windowTitle + "]");
                    WriteLog("windowClass: [" + windowClass + "]");
                    WriteLog("processHWND: [" + processHWND + "]");
                    WriteLog("processID  : [" + processID + "]");
                    WriteLog("processName: [" + processName + "]");
                    WriteLog("processPath: [" + processPath + "]");

                    foundWindowHandle = hWnd;
                    return false; //false=stop enum loop
                }
                else
                {
                    //WriteLog("NOT Match: " + windowTitle + ", and: " + pTitle);
                    return true; //true=continue enum loop
                }
            }, 0);

            if (foundWindowHandle != IntPtr.Zero)
                LastFoundWindowHandle = foundWindowHandle;

            Thread.Sleep(10); //short delay between calls improves reliability

            return foundWindowHandle;
        }
        public bool GetKeyState(Keys key)
        {
            return (Win32.GetKeyState((int)key) & 0x80) == 0x80 ? true : false;
        }

        public Rectangle GetPos(string WinTitle = "")
        {
            //The Win32 RECT structure is not compatible with the .NET System.Drawing.Rectangle structure.
            //The RECT structure has left, top, right, and bottom members
            //the System.Drawing.Rectangle uses left, top, width, and height.

            IntPtr hWnd = GetID(WinTitle);

            Win32.RECT rct;

            if (!Win32.GetWindowRect(hWnd, out rct))
                return Rectangle.Empty;

            Rectangle myRect = new Rectangle();
            myRect.X = rct.Left;
            myRect.Y = rct.Top;
            myRect.Width = rct.Right - rct.Left + 1;
            myRect.Height = rct.Bottom - rct.Top + 1;

            return myRect;
        }
        public void GetPos(out int X, out int Y, out int Width, out int Height, string WinTitle = "")
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;

            //The Win32 RECT structure is not compatible with the .NET System.Drawing.Rectangle structure.
            //The RECT structure has left, top, right, and bottom members
            //The System.Drawing.Rectangle uses left, top, width, and height.
            //The pixel at (right, bottom) lies immediately outside the rectangle, therefore subtract 1.

            IntPtr hWnd = GetID(WinTitle);

            Win32.RECT rct;

            if (!Win32.GetWindowRect(hWnd, out rct))
                return;

            X = rct.Left;
            Y = rct.Top;
            Width = rct.Right - rct.Left + 1;
            Height = rct.Bottom - rct.Top + 1;
        }
        public Process GetProcess(string WinTitle) //not AHK
        {
            Process[] processlist = Process.GetProcesses();

            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    if (IsMatch(process.MainWindowTitle, WinTitle))
                    {
                        return process;
                    }
                }
            }
            return null;
        }
        public void GetText_UNIMPLEMENTED()
        {
            //modern desktop apps don't use controls
        }
        
        public string GetTitle(string WinTitle = "")
        {
            string windowTitle = String.Empty;

            int length = Win32.GetWindowTextLength(GetID(WinTitle));

            if (length != 0)
            {
                StringBuilder sb = new StringBuilder(length);
                if (Win32.GetWindowText(GetID(WinTitle), sb, length + 1) != 0)
                    windowTitle = sb.ToString();
            }
            return windowTitle;
        }
        public void Hide(string WinTitle = "")
        {
            Win32.ShowWindow(GetID(WinTitle), Win32.SW_HIDE);
            DoDelay(WinDelay);
        }
        public void Hotkey_UNIMPLEMENTED()
        {
            //Use Hotkey Class instead
        }
        public void Hotstring_UNIMPLEMENTED()
        {
            //Use Hotkey Class instead
        }
        public Rectangle ImageSearch(string imageFile, Rectangle ScreenRect, int Variation = 0)
        {
            //ImageFile:    filename of image to search

            Bitmap ImageBmp;

            if (File.Exists(imageFile))
                ImageBmp = new Bitmap(imageFile);
            else
                return Rectangle.Empty;

            return ImageSearch(ImageBmp, ScreenRect, Variation);
        }
        public Rectangle ImageSearch(Bitmap ImageBmp, Rectangle ScreenRect, int Variation = 0)
        {
            //ImageBmp:     bitmap to search
            //ScreenRect:   area of screen to search
            //Variation:    the allowable shades of color +/- 0 to 255 (false match if too high)
            //              png matches with zero or low variation
            //              jpg matches with ~91 or greater variation

            //source1: https://www.codeproject.com/Articles/38619/Finding-a-Bitmap-contained-inside-another-Bitmap
            //source2: https://social.msdn.microsoft.com/Forums/en-US/41fd70cf-22e9-47f6-9c63-1e4fbd2af5b3/how-do-i-find-a-pictureimage-on-the-screen-and-get-the-xy-coordinates-cnet?forum=csharpgeneral/

            //check image to search
            if (ImageBmp == null)
                return Rectangle.Empty;

            //check screen area rectangle
            if (ScreenRect == Rectangle.Empty)
                ScreenRect = new Rectangle(
                    Screen.PrimaryScreen.Bounds.X,
                    Screen.PrimaryScreen.Bounds.Y,
                    Screen.PrimaryScreen.Bounds.Width,
                    Screen.PrimaryScreen.Bounds.Height
                    );

            Variation = Variation < 0 ? 0 : Variation;
            Variation = Variation > 255 ? 255 : Variation;

            //load screenshot to a bmp
            Bitmap ScreenBmp = new Bitmap(ScreenRect.Width,
                               ScreenRect.Height,
                               PixelFormat.Format32bppArgb);
            Graphics GFX = Graphics.FromImage(ScreenBmp);
            GFX.CopyFromScreen(ScreenRect.X,
                            ScreenRect.Y,
                            0,
                            0,
                            ScreenRect.Size,
                            CopyPixelOperation.SourceCopy);

            //copy bmp to array
            BitmapData iBmd =
                ImageBmp.LockBits(new Rectangle(0, 0, ImageBmp.Width, ImageBmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            BitmapData sBmd =
                ScreenBmp.LockBits(new Rectangle(0, 0, ScreenBmp.Width, ScreenBmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            byte[] smallBytes = new byte[(Math.Abs(iBmd.Stride) * ImageBmp.Height)];
            byte[] bigBytes = new byte[(Math.Abs(sBmd.Stride) * ScreenBmp.Height)];

            Marshal.Copy(iBmd.Scan0, smallBytes, 0, smallBytes.Length);
            Marshal.Copy(sBmd.Scan0, bigBytes, 0, bigBytes.Length);

            //prepare for search
            int smallStride = iBmd.Stride;
            int bigStride = sBmd.Stride;

            int bigWidth = ScreenBmp.Width;
            int bigHeight = ScreenBmp.Height - ImageBmp.Height + 1;
            int smallWidth = ImageBmp.Width * 3;
            int smallHeight = ImageBmp.Height;

            Rectangle location = Rectangle.Empty;

            //do search
            int pSmall = 0;
            int pBig = 0;

            int smallOffset = smallStride - ImageBmp.Width * 3;
            int bigOffset = bigStride - ScreenBmp.Width * 3;

            bool matchFound = true;

            for (int y = 0; y < bigHeight; y++)
            {
                for (int x = 0; x < bigWidth; x++)
                {
                    int pBigBackup = pBig;
                    int pSmallBackup = pSmall;

                    //Look for the small picture.
                    for (int i = 0; i < smallHeight; i++)
                    {
                        int j = 0;
                        matchFound = true;

                        for (j = 0; j < smallWidth; j++)
                        {
                            if (Math.Abs(bigBytes[pBig] - smallBytes[pSmall]) > Variation)
                            {
                                matchFound = false;
                                break;
                            }

                            pBig++;
                            pSmall++;
                        }

                        if (!matchFound) break;

                        //restore the indexes
                        pSmall = pSmallBackup;
                        pBig = pBigBackup;

                        //Next rows of the small and big pictures
                        pSmall += smallStride * (1 + i);
                        pBig += bigStride * (1 + i);
                    }

                    //If match found, return
                    if (matchFound)
                    {
                        location.X = x;
                        location.Y = y;
                        location.Width = ImageBmp.Width;
                        location.Height = ImageBmp.Height;
                        break;
                    }
                    //If no match found, restore the indexes and continue
                    else
                    {
                        pBig = pBigBackup;
                        pSmall = pSmallBackup;
                        pBig += 3;
                    }
                }

                if (matchFound) break;

                pBig += bigOffset;
            }

            ScreenBmp.UnlockBits(sBmd);
            ImageBmp.UnlockBits(iBmd);

            return location;
        }
        public void Input_UNIMPLEMENTED()
        {
            //use InputBox instead
        }
        public void InputBox(string Title, string Prompt, bool Hide, Point Location,
            out string Result, out string Value,
            double TimeoutSeconds = int.MaxValue, string Default = "", string IconFile = "")
        {
            InputBoxObj IB = new InputBoxObj();
            IB = InputBox(Title, Prompt, Hide, Location, TimeoutSeconds, Default, IconFile);
            Result = IB.Result;
            Value = IB.Value;
        }
        public InputBoxObj InputBox(string Title, string Prompt, bool Hide, Point Location, 
            double TimeoutSeconds = int.MaxValue, string Default = "", string IconFile = "")
        {
            //AHK_L_v1 ErrorLevel: 0=OK, 1=CANCEL, 2=Timeout
            //AHK_L_v2 Object IB: IB.Result, IB.Value

            //Result: "OK", "Cancel", "Timeout"
            //Value : User input, even if cancel or timeout

            InputBoxObj IB = new InputBoxObj();

            if (Title == String.Empty)
                Title = this.GetType().Name;// "CSharpHotkey";

            var startPos = FormStartPosition.Manual;
            if (Location.X < 0 || Location.Y < 0)
                startPos = FormStartPosition.CenterScreen;

            //AHK_L_v1 max timout = 2147483(24.8 days), int.MaxValue = 2147483647
            if (TimeoutSeconds > int.MaxValue / 1000) TimeoutSeconds = int.MaxValue / 1000;
            double TimeoutMilliSeconds = TimeoutSeconds * 1000;

            //AHK_L_v1(w,h) = 375, 189
            int minWidth = 360, minHeight = 120;
            int margin = 25;

            Form form = new Form();
            Label labelPrompt = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.ClientSize = new Size(minWidth, minHeight);
            form.Location = new Point(Location.X, Location.Y);
            form.MinimumSize = new Size(minWidth, minHeight);
            form.Controls.AddRange(new Control[] { labelPrompt, textBox, buttonOk, buttonCancel });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = startPos;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;
            if (File.Exists(IconFile)) form.Icon = new Icon(IconFile);
            form.Text = Title;

            labelPrompt.SetBounds(9, 10, minWidth - margin + 8, 30);
            labelPrompt.AutoSize = false;
            labelPrompt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelPrompt.Text = Prompt;

            textBox.SetBounds(12, 45, minWidth - margin - 0, 20);
            textBox.AutoSize = false;
            textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBox.Text = Default;
            textBox.UseSystemPasswordChar = Hide;

            buttonOk.SetBounds(minWidth - margin - 63, 85, 75, 23);
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOk.Text = "&OK";
            buttonOk.DialogResult = DialogResult.OK;

            buttonCancel.SetBounds(minWidth - margin - 145, 85, 75, 23);
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Text = "&Cancel";

            bool timeout = false;

            var timer = new System.Timers.Timer(TimeoutMilliSeconds) { AutoReset = false };
            var timerHandle = GCHandle.Alloc(timer);
            timer.Elapsed += (sender, args) =>
            {
                timeout = true;
                form.Close();
                timerHandle.Free();
                timer.Dispose();
            };

            timer.Start();

            DialogResult dialogResult = form.ShowDialog();

            IB.Result = dialogResult.ToString();
            IB.Value = textBox.Text;

            if (timeout)
            {
                IB.Result = "Timeout";
                
            }
            else
            {
                timer.Stop();
                timerHandle.Free();
                timer.Dispose();
            }
            return IB;
        }
        public void InputHook_UNIMPLEMENTED()
        {
            //use InputHook class instead
        }
        public bool KeyWait(Keys Key, Keys DownOrUp, double TimeoutSeconds)
        {
            //Key               Keys.A to Keys.Zoom
            //DownOrUp            Keys.Up or Keys.Down
            //TimeoutSeconds    0 = no wait, -1 = indefinite. T or t case insensitive

            if (DownOrUp != Keys.Up & DownOrUp != Keys.Down)
                DownOrUp = Keys.Down;

            if (TimeoutSeconds < 0) { TimeoutSeconds = Int32.MaxValue; }

            DateTime waitTime = DateTime.Now.AddSeconds(TimeoutSeconds);

            const uint LSB = 1 << 0;    // least significant bit = 1
            const uint MSB = 1 << 15;   // most  significant bit = 32768

            while (DateTime.Now < waitTime)
            {
                DoDelay(10);

                short x = Win32.GetAsyncKeyState(Key); //must be Async

                if ((DownOrUp == Keys.Down) && (x & MSB) == MSB)
                    return false;   //no timeout

                if ((DownOrUp == Keys.Up) && (x & MSB) == 0)
                    return false;   //no timeout
            }
            return true; //timeout
            }
        public bool KeyWait(Keys Key, string Options = "")
        {
            //Options:
            //  D     =  wait for key down else wait for key up (default). D or d case insensitive
            //  U     =  wait for key up
            //  Tn    =  TimeoutSeconds: 0 = no wait, -1 = indefinite. T or t case insensitive
            //  Empty =  wait indefinitely for the specified key or mouse/joystick button to be physically released by the user.

            string KeyDownOrUp = "U";
            int TimeoutSeconds = -1;

            const uint LSB = 1 << 0;    // least significant bit = 1
            const uint MSB = 1 << 15;   // most  significant bit = 32768

            Options = Options.ToUpper();

            if (Options.Contains("D"))
                KeyDownOrUp = "D";

            if (Options.Contains("T"))
            {
                int pos = Options.IndexOf("T");
                string timeString = Options.Substring(pos+1);
                Int32.TryParse(timeString, out TimeoutSeconds);
            }

            if (TimeoutSeconds < 0) { TimeoutSeconds = Int32.MaxValue; }

            DateTime waitTime = DateTime.Now.AddSeconds(TimeoutSeconds);

            while (DateTime.Now < waitTime)
            {
                DoDelay(10);

                short x = Win32.GetAsyncKeyState(Key); //must be Async

                if ((KeyDownOrUp == "D") && (x & MSB) == MSB)
                    return false;   //no timeout

                if ((KeyDownOrUp == "U") && (x & MSB) == 0)
                    return false;   //no timeout
            }
            return true; //timeout
        }
        public bool Kill(string WinTitle = "", int TimeoutSeconds = 0)
        {
            if (TimeoutSeconds < 1) TimeoutSeconds = 500;

            bool result = false;

            if (!Exist(WinTitle))
                return result;

            Close(WinTitle);

            if (WaitClose(WinTitle, TimeoutSeconds))
            {
                //timeout so terminate process instead
                Process proc = GetProcess(WinTitle);
                proc.Kill();
                if (proc.HasExited)
                    result = true;
            }
            else
            {
                result = true;
            }
            return result;

        }
        public bool Maximize(string WinTitle = "")
        {
            bool result = Win32.ShowWindow(GetID(WinTitle), Win32.SW_MAXIMIZE);
            DoDelay(WinDelay);
            return result;
        }
        public void MenuSelectItem_UNIMPLEMENTED()
        {
            //modern desktop apps don't use controls
        }
        public bool Minimize(string WinTitle = "")
        {


            //public const int SC_SIZE = 0xF000;
            //public const int SC_MOVE = 0xF010;
            //public const int SC_MINIMIZE = 0xF020;
            //public const int SC_MAXIMIZE = 0xF030;
            //public const int SC_CLOSE = 0xF060;
            //public const int SC_RESTORE = 0xF120;


            //PostMessage(hWnd, WM_SYSCOMMAND, SC_MINIMIZE, 0);

            int SC_MINIMIZE = 0xF020;

            bool result = Win32.PostMessageA(GetID(WinTitle), Win32.WM_SYSCOMMAND, SC_MINIMIZE, 0);


            //bool result = Win32.ShowWindow(GetID(WinTitle), Win32.SW_MINIMIZE);
            DoDelay(WinDelay);
            return result;
        }
        public void MinimizeAll()
        {
            IntPtr OutResult;

            IntPtr hWnd = Win32.FindWindow("Shell_TrayWnd", null);

            Win32.SendMessageTimeout(hWnd, Win32.WM_COMMAND, (IntPtr)Win32.MIN_ALL, 
                IntPtr.Zero, Win32.SMTO_ABORTIFHUNG, 2000, out OutResult);

            DoDelay(WinDelay);
        }
        public void MinimizeAllUndo()
        {
            IntPtr OutResult;

            IntPtr lHwnd = Win32.FindWindow("Shell_TrayWnd", null);

            Win32.SendMessageTimeout(lHwnd, Win32.WM_COMMAND, (IntPtr)Win32.MIN_ALL_UNDO,
                IntPtr.Zero, Win32.SMTO_ABORTIFHUNG, 2000, out OutResult);

            DoDelay(WinDelay);
        }
        public void MouseClick_UNIMPLEMENTED()
        {
            //use Click()
        }
        public void MouseClickDrag_UNIMPLEMENTED()
        {
            //drag not implemented
        }
        public Point MouseGetPoint()
        {
            Point pt = new Point();
            Win32.GetCursorPos(out pt);
            return pt;
        }
        public MousePosObj MouseGetPos()
        {
            MousePosObj MP = new MousePosObj();
            Win32.GetCursorPos(out MP.Point);
            Cursor cursor = new Cursor(Cursor.Current.Handle);
            MP.Handle = cursor.Handle;
            return MP;
        }
        public void MouseGetPos(out Point pt, out IntPtr hWnd)
        {
            Win32.GetCursorPos(out pt);

            System.Windows.Forms.Cursor cursor = new System.Windows.Forms.Cursor(System.Windows.Forms.Cursor.Current.Handle);
            hWnd = cursor.Handle;
        }
        public void MouseMove(Point pt)
        {
            Win32.SetCursorPos(pt.X, pt.Y);
            DoDelay(MouseDelay);
        }
        public void Move(int X, int Y, int Width = 0, int Height = 0, string WinTitle = "")
        {
            uint uFlags = Win32.SWP_SHOWWINDOW;

            if (Width + Height == 0)
                uFlags |= Win32.SWP_NOZORDER | Win32.SWP_NOSIZE;

            Win32.SetWindowPos(GetID(WinTitle), (IntPtr)Win32.HWND_TOP, X, Y, Width, Height, uFlags);
            DoDelay(WinDelay);
        }
        public bool NameAdd(string Key, string WinTitle)
        {
            //See GetID for explanation of _name functions

            bool result = true;

            IntPtr hWnd = GetID(WinTitle);

            if (hWnd == IntPtr.Zero)
                return false;

            IntPtr temp = IntPtr.Zero;

            try
            {
                if (!windowHandleDict.TryGetValue(Key, out temp))
                {
                    windowHandleDict.Add(Key, hWnd);
                }
                else
                {
                    windowHandleDict.Remove(Key);
                    windowHandleDict.Add(Key, hWnd);
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
        public Dictionary<string, IntPtr> NameGetDict()
        {
            return windowHandleDict;
        }
        public Color PixelGetColor(Point point, PixelMode Mode = PixelMode.RGB)
        {
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            Bitmap ScreenBmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            Graphics GFX = Graphics.FromImage(ScreenBmp);
            GFX.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
            Color PixelColor = ScreenBmp.GetPixel(point.X, point.Y); // ARGB (alpha, red, green, blue) color.

            //GetPixel Color is RGB
            //if CSHK_PIXEL_MODE.BGR then convert from RGB to BGR
            if (Mode == PixelMode.BGR)
            {
                int iColorBGR = ColorTranslator.ToWin32(PixelColor);
                PixelColor = Color.FromArgb(iColorBGR);
            }
            return PixelColor;

        }
        public Point PixelSearch(Rectangle ScreenRect,
            uint PixelColor, PixelMode Mode = PixelMode.BGR,
            int Variation = 0)
        {
            //ScreenRect:   area of screen to search
            //PixelColor    if CSHK_PIXEL_MODE.RGB then PixelColor is RGB else BGR
            //Variation:    the allowable shades of color +/- 0 to 255 (false match if too high)

            if (ScreenRect == Rectangle.Empty)
                ScreenRect = Screen.PrimaryScreen.Bounds;

            //bitmap bytes are BGR. if CSHK_PIXEL_MODE.RGB then convert PixelColor from RGB to BGR
            if (Mode == PixelMode.RGB)
            {
                Color aColor = Color.FromArgb((int)PixelColor);
                PixelColor = (uint)ColorTranslator.ToWin32(aColor);
            }

            Variation = Variation <   0 ?   0 : Variation;
            Variation = Variation > 255 ? 255 : Variation;

            //Color.FromArgb adds Alpha bits
            //PixelColorBGR = PixelColorBGR + uint.Parse("FF000000", System.Globalization.NumberStyles.HexNumber);
            Color searchColor = Color.FromArgb((int)PixelColor);

            //capture entire screen in bmp, then copy the ScreenRect area to search
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            Bitmap SearchBmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            Graphics GFX = Graphics.FromImage(SearchBmp);
            GFX.CopyFromScreen(ScreenRect.X, ScreenRect.Y, 0, 0, ScreenRect.Size, CopyPixelOperation.SourceCopy);

            BitmapData sBmd = SearchBmp.LockBits(new Rectangle(0, 0,
                SearchBmp.Width, SearchBmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            byte[] searchBytes = new byte[(Math.Abs(sBmd.Stride) * SearchBmp.Height)];
            Marshal.Copy(sBmd.Scan0, searchBytes, 0, searchBytes.Length);

            Point result = Point.Empty;

            int i = 0;

            for (int y = 0; y < SearchBmp.Height; y++)
            {
                for (int x = 0; x < SearchBmp.Width; x++)
                {
                    //searchColor is BGR byte[0]=B, byte[1]=G, byte[2]=R, thisColor is BGR
                    Color thisColor = Color.FromArgb(searchBytes[i], searchBytes[i + 1], searchBytes[i + 2]);

                    if (Math.Abs(thisColor.R - searchColor.R) <= Variation &&
                        Math.Abs(thisColor.G - searchColor.G) <= Variation &&
                        Math.Abs(thisColor.B - searchColor.B) <= Variation)
                    {
                        result = new Point(x, y);
                        break;
                    }
                    i += 3;
                }
                if (result != Point.Empty)
                    break;
            }
            SearchBmp.UnlockBits(sBmd);
            
            return result;
        }
        
        public bool Restore(string WinTitle = "")
        {
            bool result1 = Win32.ShowWindow(GetID(WinTitle), Win32.SW_RESTORE);
            bool result2 = Win32.SetForegroundWindow(GetID(WinTitle));
            DoDelay(WinDelay);
            return result1 | result2;
        }
        public void SendMode(SendModeCommand Mode)
        {
            SendModeSelection = Mode;
        }
        public void Send(Keys Key, Keys DownOrUp = Keys.None)
        {
            if (SendModeSelection == SendModeCommand.Event)
                SendEvent(Key, DownOrUp);
            else
                SendInput(Key, DownOrUp);
        }
        public void SendEvent(Keys Key, Keys DownOrUp = Keys.None)
        {
            Sending.Key = Key; //signal InputHook() and HotKey() to ignore this Key

            if (DownOrUp == Keys.Down)
            {
                Win32.keybd_event((byte)Key, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            }
            else if (DownOrUp == Keys.Up)
            {
                Win32.keybd_event((byte)Key, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            else
            {
                Win32.keybd_event((byte)Key, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                Win32.keybd_event((byte)Key, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            DoDelay(KeyDelay);
        }
        public void SendInput(Keys Key, Keys DownOrUp = Keys.None)
        {
            void _KeyDown(ushort VKey)
            {
                Win32.INPUT[] inputs = new Win32.INPUT[1];
                inputs[0].type = Win32.INPUT_KEYBOARD;
                inputs[0].UNION.ki.wVk = (ushort)Key;
                inputs[0].UNION.ki.dwFlags = 0;
                inputs[0].UNION.ki.wScan = 0;
                if (Win32.SendInput(1, inputs, Marshal.SizeOf(typeof(Win32.INPUT))) != 1)
                    throw new Exception("Could not send key: " + Key);
            }
            void _KeyUp(ushort VKey)
            {
                Win32.INPUT[] inputs = new Win32.INPUT[1];
                inputs[0].type = Win32.INPUT_KEYBOARD;
                inputs[0].UNION.ki.wVk = (ushort)Key;
                inputs[0].UNION.ki.dwFlags = Win32.KEYEVENTF_KEYUP;
                inputs[0].UNION.ki.wScan = 0;
                if (Win32.SendInput(1, inputs, Marshal.SizeOf(typeof(Win32.INPUT))) != 1)
                    throw new Exception("Could not send key: " + Key);
            }
            void _KeyPress(ushort VKey)
            {
                _KeyDown(VKey);
                _KeyUp(VKey);
            }

            Sending.Key = Key; //signal InputHook() and HotKey() to ignore this Key

            if (DownOrUp == Keys.Down)
            {
                _KeyDown((ushort)Key);
            }
            else if (DownOrUp == Keys.Up)
            {
                _KeyUp((ushort)Key);
            }
            else
            {
                _KeyPress((ushort)Key);
            }

            DoDelay(KeyDelay);
        }
        public void Send(string KeyStrokes, bool SendWait = true)
        {
            // sends KeyStrokes with a KeyDelay between each key
            // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys
            // + = Shift, ^ = Control, % = Alt
            // ^a = Ctrl then A, ^%(a) = Ctrl+Alt+A, 
            // Hold down SHIFT while E and C are pressed, use "+(EC)"
            // Hold down SHIFT while E is pressed, followed by C without SHIFT, use "+EC".
            // Literals must be escaped within curly braces: + ^ % ~ { } [ ], example: {^} = ^
            // Many apps require lower case modifer keys: "^a" versus "^A"
            // Therefore all modifer keys are changed to lower case
            //
            // If your app relies on consistent behavior regardless of the versions of Windows and and options,
            //  you can force the SendKeys class to use the new implementation by adding the following to app.config:
            //  <appSettings>
            //      <add key = "SendKeys" value= "SendInput"/>
            //  </appSettings>
            //
            // NOTE: This Send function WILL trigger InputHook() and HotKey().
            //       Send(Keys Key) will NOT trigger InputHook() and HotKey().
            //

            //TEST_Send
            //
            //very limited testing, Send:
            //  stopWatch = 0ms
            //  Win.SetKeyDelay(0):   Send("0") =  31-56ms  =  59 ms
            //  Win.SetKeyDelay():    Send("0") =  47-78ms  =  80 ms (default=10);
            //  Win.SetKeyDelay(25):  Send("0") =  63-79ms  = 102 ms 
            //  Win.SetKeyDelay(50):  Send("0") =  95-110ms = 150 ms 
            //  Win.SetKeyDelay(75):  Send("0") = 109-140ms = 179 ms
            //  Win.SetKeyDelay(100): Send("0") = 142-170ms = 156 ms

            //very limited testing, SendInput:
            //  stopWatch = 0ms
            //  Win.SetKeyDelay(0):   Send("0") =  31-44  =  53 ms
            //  Win.SetKeyDelay():    Send("0") =  47-61  =  54 ms (default=10);
            //  Win.SetKeyDelay(25):  Send("0") =  61-63  =  92 ms
            //  Win.SetKeyDelay(50):  Send("0") =  94-95  =  94 ms
            //  Win.SetKeyDelay(75):  Send("0") = 110-127 = 118 ms 
            //  Win.SetKeyDelay(100): Send("0") = 140-157 = 148 ms

            string _DoSend(string Buffer)
            {
                try
                {
                    if (SendWait)
                        SendKeys.SendWait(Buffer);
                    else
                        SendKeys.Send(Buffer);
                }
                catch
                {
                    if (SendWait)
                        SendKeys.SendWait("?");
                    else
                        SendKeys.Send("?");
                }
                if (!SendWait)
                    SendKeys.Flush();

                DoDelay(KeyDelay);
                return String.Empty;
            }

            bool modChar = false;
            bool openParen = false;
            bool openBrace = false;
            string wordBuffer = String.Empty;

            foreach (char c in KeyStrokes)
            {
                if ("+^&".Contains(c.ToString()))
                {
                    modChar = true;
                    wordBuffer += c;
                    continue;
                }
                else if (c == ')')
                {
                    openParen = false;
                    modChar = false;
                    wordBuffer = _DoSend(wordBuffer += c);
                    continue;

                }
                else if (c == '}')
                {
                    openBrace = false;
                    modChar = false;

                    if (wordBuffer.EndsWith("{"))
                    {
                        openBrace = false;
                        wordBuffer += '}';
                        continue;
                    }

                    wordBuffer = _DoSend(wordBuffer += char.ToLower(c));
                    continue;
                }
                else if (openParen == true | openBrace == true)
                {
                    wordBuffer += char.ToLower(c);
                    continue;
                }
                else if (c == '(' & modChar == true)
                {
                    openParen = true;
                    wordBuffer += c;
                    continue;
                }
                else if (c == '{')
                {
                    openBrace = true;
                    wordBuffer += c;
                    continue;
                }
                else if (modChar == true) //[and if !(+^%) tested above]
                {
                    modChar = false;
                    wordBuffer = _DoSend(wordBuffer += char.ToLower(c));
                    continue;
                }
                else
                {
                    wordBuffer = _DoSend(wordBuffer += c);
                }
            }
        }
        public bool Set(SetCommand Command, string WinTitle = "")
        {
            /*
                implemented:
                ----------------
                AlwaysOnTop: Makes a window stay on top of all other windows.
                Bottom: Sends a window to the bottom of stack; that is, beneath all other windows.
                Top: Brings a window to the top of the stack without explicitly activating it.
                NotTop:
                Disable: Disables a window.
                Enable: Enables a window.

                not implemented:
                ----------------
                Redraw: Redraws a window.
                Style: Changes the style of a window.
                ExStyle: Changes the extended style of a window.
                Region: Changes the shape of a window to be the specified rectangle, ellipse, or polygon.
                Transparent: Makes a window semi-transparent.
                TransColor: Makes all pixels of the chosen color invisible inside the target window.
            */

            IntPtr hWnd = GetID(WinTitle);
            IntPtr hWndInsertAfter = IntPtr.Zero;

            switch (Command)
            {
                case SetCommand.AlwaysOnTop:
                    hWndInsertAfter = (IntPtr)Win32.HWND_TOPMOST;
                    break;
                case SetCommand.Bottom:
                    hWndInsertAfter = (IntPtr)Win32.HWND_BOTTOM;
                    break;
                case SetCommand.Top:
                    hWndInsertAfter = (IntPtr)Win32.HWND_TOP;
                    break;
                case SetCommand.NotTop:
                    hWndInsertAfter = (IntPtr)Win32.HWND_NOTTOPMOST;
                    break;
                case SetCommand.Disable:
                    Win32.EnableWindow(hWnd, false);
                    Win32.UpdateWindow(hWnd);
                    break;
                case SetCommand.Enable:
                    Win32.EnableWindow(hWnd, true);
                    Win32.UpdateWindow(hWnd);
                    break;
                default:
                    hWndInsertAfter = IntPtr.Zero;
                    break;
            }
            bool result = Win32.SetWindowPos(GetID(WinTitle), hWndInsertAfter, 0, 0, 0, 0, Win32.SWP_NOMOVE | Win32.SWP_NOSIZE);
            DoDelay(WinDelay);
            return result;
        }
        public void SetLockState(Keys Key, Keys DownOrUp)
        {
            if (Key != Keys.CapsLock & Key != Keys.Scroll & Key != Keys.NumLock & Key != Keys.Insert)
                return;

            if (DownOrUp == Keys.Down && Control.IsKeyLocked(Key) || (DownOrUp != Keys.Down && !Control.IsKeyLocked(Key)))
                return;

            Send(Key);

            DoDelay(KeyDelay);
        }
        public bool SetTitle(string NewTitle, string WinTitle = "")
        {
            //return Win32.SetWindowText(GetID(WinTitle), NewTitle);
            IntPtr hWnd = GetID(WinTitle);
            bool result = Win32.SetWindowText(hWnd, NewTitle);
            Win32.UpdateWindow(hWnd);
            DoDelay(WinDelay);
            return result;
        }
        public void SetTitleMatchMode(MatchMode Mode = MatchMode.Exact, MatchCase Case = MatchCase.Sensitive)
        {
            TitleMatchMode = Mode;
            TitleMatchCase = Case;
        }
        public void SetTitleMatchMode(string mode = "", string modeCase = "")
        {
            switch (mode)
            {
                case "Contains":
                    TitleMatchMode = MatchMode.Contains;
                    break;
                case "EndsWith":
                    TitleMatchMode = MatchMode.EndsWith;
                    break;
                case "Exact":
                    TitleMatchMode = MatchMode.Exact;
                    break;
                case "StartsWith":
                    TitleMatchMode = MatchMode.StartsWith;
                    break;
                default:
                    TitleMatchMode = MatchMode.Exact;
                    break;
            }
            switch (modeCase)
            {
                case "Sensitive":
                    TitleMatchCase = MatchCase.Sensitive;
                    break;
                case "InSensitive":
                    TitleMatchCase = MatchCase.InSensitive;
                    break;
                default:
                    TitleMatchCase = MatchCase.Sensitive;
                    break;
            }
        }
        public void SetKeyDelay(int milliSeconds = 10)
        {
            //Time in milliSeconds: -1 for no delay, 0 for the smallest possible delay, 10 is the default.

            KeyDelay = milliSeconds;
        }
        public void SetMouseDelay(int milliSeconds = 100)
        {
            //Time in milliSeconds: -1 for no delay, 0 for the smallest possible delay, 100 is the default.

            MouseDelay = milliSeconds;
        }
        public void SetWinDelay(int milliSeconds = 100)
        {
            //Time in milliSeconds: -1 for no delay, 0 for the smallest possible delay, 100 is the default.

            WinDelay = milliSeconds;
        }
        public void Show(string WinTitle = "")
        {
            Win32.ShowWindow(GetID(WinTitle), Win32.SW_SHOW);
            DoDelay(WinDelay);
        }
        public void Sleep(int TimeMilliseconds)
        {
            if (TimeMilliseconds < 0) return;
            DoDelay(TimeMilliseconds);
        }
        public void SoundBeep(int Frequency = 523, int Duration = 150)
        {
            if (Frequency <= 0) Frequency = 523;
            if (Duration  <= 0) Duration = 523;

            Console.Beep(Frequency, Duration);
        }
        public MonitorObj SysGetMonitor(int MonitorNumber = 0)
        {
            MonitorObj o = new MonitorObj();

            int i = (MonitorNumber < o.Count | MonitorNumber > o.Count)
                ? Screen.AllScreens.Length - 1 : MonitorNumber;

            for (int j = 0; j < Screen.AllScreens.Count(); j++)
                if (Screen.AllScreens[j].Primary) o.Primary = j + 1;

            o.Bounds = Screen.AllScreens[i].Bounds;
            o.WorkingArea = Screen.AllScreens[i].WorkingArea;
            o.Name = Screen.AllScreens[i].DeviceName;

            return o;
        }
        public bool Wait(string WinTitle = "", double TimeoutSeconds = -1)
        {
            //TimeoutSeconds: How many seconds to wait before timing out
            //Omit or -1:   wait indefinitely
            //0:            wait a minimum of 0.5 seconds

            if (TimeoutSeconds <  0) { TimeoutSeconds = Int32.MaxValue; }
            if (TimeoutSeconds == 0) { TimeoutSeconds = 0.5; }

            DateTime waitTime = DateTime.Now.AddSeconds(TimeoutSeconds);

            while (DateTime.Now < waitTime)
            {
                DoDelay(10);

                if (Exist(WinTitle))
                    return false; //no timeout
            }
            return true; //timeout
        }
        public bool WaitActive(string WinTitle = "", double TimeoutSeconds = -1)
        {
            //TimeoutSeconds: How many seconds to wait before timing out
            //Omit or -1:   wait indefinitely
            //0:            wait a minimum of 0.5 seconds

            if (TimeoutSeconds < 0) { TimeoutSeconds = Int32.MaxValue; }
            if (TimeoutSeconds == 0) { TimeoutSeconds = 0.5; }

            DateTime waitTime = DateTime.Now.AddSeconds(TimeoutSeconds);

            while (DateTime.Now < waitTime)
            {
                DoDelay(10);

                if (GetID(WinTitle) == Win32.GetForegroundWindow())
                    return false;   //no timeout
            }
            return true; //timeout
        }
        public bool WaitNotActive(string WinTitle = "", double TimeoutSeconds = -1)
        {
            //TimeoutSeconds: How many seconds to wait before timing out
            //Omit or -1:   wait indefinitely
            //0:            wait a minimum of 0.5 seconds

            if (TimeoutSeconds < 0) { TimeoutSeconds = Int32.MaxValue; }
            if (TimeoutSeconds == 0) { TimeoutSeconds = 0.5; }

            DateTime waitTime = DateTime.Now.AddSeconds(TimeoutSeconds);

            while (DateTime.Now < waitTime)
            {
                DoDelay(10);

                if (GetID(WinTitle) != Win32.GetForegroundWindow())
                    return false; //no timeout
            }
            return true; //timeout
        }
        public bool WaitClose(string WinTitle = "", double TimeoutSeconds = -1)
        {
            //TimeoutSeconds: How many seconds to wait before timing out
            //Omit or -1:   wait indefinitely
            //0:            wait a minimum of 0.5 seconds

            if (TimeoutSeconds < 0) { TimeoutSeconds = Int32.MaxValue; }
            if (TimeoutSeconds == 0) { TimeoutSeconds = 0.5; }

            DateTime waitTime = DateTime.Now.AddSeconds(TimeoutSeconds);

            while (DateTime.Now < waitTime)
            {
                DoDelay(10);

                if (!Exist(WinTitle))
                    return false; //no timeout
            }
            return true; //timeout
        }
        public Rectangle WaitImageSearch(string ImageFile, Rectangle ScreenRect, int Variation = 0, double TimeoutSeconds = -1)
        {
            //TimeoutSeconds: How many seconds to wait before timing out
            //Omit or -1:   wait indefinitely
            //0:            wait a minimum of 0.5 seconds

            if (TimeoutSeconds < 0) { TimeoutSeconds = Int32.MaxValue; }
            if (TimeoutSeconds == 0) { TimeoutSeconds = 0.5; }

            DateTime waitTime = DateTime.Now.AddSeconds(TimeoutSeconds);

            while (DateTime.Now < waitTime)
            {
                DoDelay(10);

                Rectangle rect = ImageSearch(ImageFile, ScreenRect, Variation);

                if (rect.X != 0 & rect.Y != 0)
                    return rect; //no timeout
            }
            return Rectangle.Empty; //timeout
        }
        public Rectangle WaitImageSearch(Bitmap ImageBmp, Rectangle ScreenRect, int Variation = 0, double TimeoutSeconds = -1)
        {
            //TimeoutSeconds: How many seconds to wait before timing out
            //Omit or -1:   wait indefinitely
            //0:            wait a minimum of 0.5 seconds

            if (TimeoutSeconds < 0) { TimeoutSeconds = Int32.MaxValue; }
            if (TimeoutSeconds == 0) { TimeoutSeconds = 0.5; }

            DateTime waitTime = DateTime.Now.AddSeconds(TimeoutSeconds);

            Rectangle rect = Rectangle.Empty;

            while (DateTime.Now < waitTime)
            {
                DoDelay(10);

                try
                {
                    rect = ImageSearch(ImageBmp, ScreenRect, Variation);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ex=" + ex);
                    return Rectangle.Empty; //timeout
                }

                if (rect.X != 0 & rect.Y != 0)
                    return rect;               //no timeout
            }
            return Rectangle.Empty; //timeout
        }
        public Point WaitPixelSearch(string WinTitle, uint PixelColor, double TimeoutSeconds = -1)
        {
            //TimeoutSeconds: How many seconds to wait before timing out
            //Omit or -1:   wait indefinitely
            //0:            wait a minimum of 0.5 seconds

            if (TimeoutSeconds < 0) { TimeoutSeconds = Int32.MaxValue; }
            if (TimeoutSeconds == 0) { TimeoutSeconds = 0.5; }

            DateTime waitTime = DateTime.Now.AddSeconds(TimeoutSeconds);

            while (DateTime.Now < waitTime)
            {
                DoDelay(10);

                Rectangle rect = GetPos(WinTitle);

                Point p = (PixelSearch(rect, PixelColor));

                if (p.X != 0 & p.Y != 0)
                    return p;               //no timeout
            }
            return Point.Empty; //timeout
        }

        #endregion Public Functions

        #region Private Methods
        private void DoDelay(int milliSeconds)
        {
            if (milliSeconds < 0) return;
            DateTime waitTime = DateTime.Now.AddMilliseconds(milliSeconds);
            while (DateTime.Now < waitTime) { Application.DoEvents(); Thread.Sleep(1); }
        }
        public bool IsIconic(string WinTitle = "")
        {
            return Win32.IsIconic(GetID(WinTitle));
        }
        private bool IsMatch(string haystack, string needle)
        {
            bool result = false;

            StringComparison comparisonType = StringComparison.Ordinal;

            if (TitleMatchCase == MatchCase.InSensitive)
            {
                comparisonType = StringComparison.OrdinalIgnoreCase;
            }

            if (TitleMatchMode == MatchMode.StartsWith)
            {
                if (haystack.StartsWith(needle, comparisonType)) { result = true; }
            }
            else if (TitleMatchMode == MatchMode.Contains)
            {
                if (comparisonType == StringComparison.OrdinalIgnoreCase)
                {
                    haystack = haystack.ToUpper();
                    needle = needle.ToUpper();
                }
                if (haystack.Contains(needle)) { result = true; }
            }
            else if (TitleMatchMode == MatchMode.Exact)
            {
                if (haystack.Equals(needle, comparisonType)) { result = true; }
            }
            else if (TitleMatchMode == MatchMode.EndsWith)
            {
                if (haystack.EndsWith(needle, comparisonType)) { result = true; }
            }
            return result;
        }
        public bool IsVisible(string WinTitle = "")
        {
            return Win32.IsWindowVisible(GetID(WinTitle));
        }
        private void WriteLog(string Text, string LogFile = "", bool OverWrite = true)
        {
            if (!USE_LOG)
                return;

            if (LogFile == String.Empty) LogFile = this.GetType().Name + ".log";

            if (OverWrite) File.Delete(LogFile);

            const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

            using (StreamWriter writer = new StreamWriter(LogFile, true, Encoding.UTF8))
            {
                writer.WriteLine(DateTime.Now.ToString(dateTimeFormat) + ": " + Text);
            }
        }
        #endregion Private Methods

        #region Win32
        private class Win32
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool BlockInput([In, MarshalAs(UnmanagedType.Bool)] bool fBlockIt);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetCursorPos(out System.Drawing.Point lpPoint);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern IntPtr GetForegroundWindow();
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)] public static extern short GetKeyState(int vKey);
            [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetKeyboardState(byte[] lpKeyState);
            [DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] int nSize); 
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern IntPtr GetParent(IntPtr hWnd);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int GetWindowTextLength(IntPtr hWnd);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool IsIconic(IntPtr hWnd);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool IsWindowVisible(IntPtr hWnd);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInf);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern uint MapVirtualKey(uint uCode, uint uMapType); 
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool PostMessageA(IntPtr hWnd, uint Msg, int wParam, int lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool SetCursorPos(int x, int y);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool SetForegroundWindow(IntPtr hWnd);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize); 
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern IntPtr SendMessageTimeout(IntPtr windowHandle, uint Msg, IntPtr wParam, IntPtr lParam, int flags, uint timeout, out IntPtr result);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool SetWindowText(IntPtr hwnd, String lpString);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool UpdateWindow(IntPtr hWnd);

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int Left;        // x position of upper-left corner
                public int Top;         // y position of upper-left corner
                public int Right;       // x position of lower-right corner
                public int Bottom;      // y position of lower-right corner
            }
            [StructLayout(LayoutKind.Sequential)]
            public struct INPUT
            {
                internal uint type;
                internal InputUnion UNION;
            }

            // For use with the INPUT struct, see SendInput for an example
            public const int INPUT_MOUSE = 0;
            public const int INPUT_KEYBOARD = 1;
            public const int INPUT_HARDWARE = 2;

            [StructLayout(LayoutKind.Explicit)]
            internal struct InputUnion
            {
                [FieldOffset(0)]
                internal MOUSEINPUT mi;
                [FieldOffset(0)]
                internal KEYBDINPUT ki;
                [FieldOffset(0)]
                internal HARDWAREINPUT hi;
            }
            [StructLayout(LayoutKind.Sequential)]
            internal struct MOUSEINPUT
            {
                internal int dx;
                internal int dy;
                internal int mouseData;
                internal int dwFlags; // MOUSEEVENTF dwFlags;
                internal uint time;
                internal UIntPtr dwExtraInfo;
            }
            [StructLayout(LayoutKind.Sequential)]
            internal struct KEYBDINPUT
            {
                internal ushort wVk; // VirtualKeyShort wVk;
                internal ushort wScan;// ScanCodeShort wScan;
                internal uint dwFlags; // KEYEVENTF dwFlags;
                internal uint time;
                internal UIntPtr dwExtraInfo;
            }
            [StructLayout(LayoutKind.Sequential)]
            internal struct HARDWAREINPUT
            {
                internal int uMsg;
                internal short wParamL;
                internal short wParamH;
            }
            #region Win32 Constants

            //https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation?tabs=cmd
            public const int MAX_PATH = 260;

            #region GWL

            public const int GWL_WNDPROC = (-4);
            public const int GWL_HINSTANCE = (-6);
            public const int GWL_HWNDPARENT = (-8);
            public const int GWL_STYLE = (-16);
            public const int GWL_EXSTYLE = (-20);
            public const int GWL_USERDATA = (-21);
            public const int GWL_ID = (-12);

            #endregion

            #region HWND

            public const int HWND_TOP = 0;
            public const int HWND_BOTTOM = 1;
            public const int HWND_TOPMOST = -1;
            public const int HWND_NOTTOPMOST = -2;

            #endregion

            #region KEYEVENT
            public const int KEYEVENTF_KEYDOWN = 0; //custom
            public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
            public const int KEYEVENTF_KEYUP = 0x0002;
            public const int KEYEVENTF_UNICODE = 0x0004;
            public const int KEYEVENTF_SCANCODE = 0x0008;
            #endregion

            #region MOD
            public const uint MOD_NONE = 0x0000; //custom
            public const uint MOD_ALT = 0x0001;
            public const uint MOD_CONTROL = 0x0002;
            public const uint MOD_SHIFT = 0x0004;
            public const uint MOD_WIN = 0x0008;
            public const uint MOD_CONTROL_ALT = MOD_CONTROL + MOD_ALT; //custom
            #endregion

            #region MOUSE
            public const int MOUSEEVENTF_MOVE = 0x0001;
            public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
            public const int MOUSEEVENTF_LEFTUP = 0x0004;
            public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
            public const int MOUSEEVENTF_RIGHTUP = 0x0010;
            public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
            public const int MOUSEEVENTF_MIDDLEUP = 0x0040;
            public const int MOUSEEVENTF_XDOWN = 0x0080;
            public const int MOUSEEVENTF_XUP = 0x0100;
            public const int MOUSEEVENTF_WHEEL = 0x0800;
            public const int MOUSEEVENTF_VIRTUALDESK = 0x4000;
            public const int MOUSEEVENTF_ABSOLUTE = 0x8000;
            #endregion MOUSE

            #region SMTO
            public const int SMTO_NORMAL = 0;
            public const int SMTO_BLOCK = 1;
            public const int SMTO_ABORTIFHUNG = 2;
            public const int SMTO_NOTIMEOUTIFNOTHUNG = 8;
            public const int SMTO_ERRORONEXIT = 32;
            #endregion

            #region SW

            public const int SW_HIDE = 0;
            public const int SW_SHOWNORMAL = 1;
            public const int SW_NORMAL = 1;
            public const int SW_SHOWMINIMIZED = 2;
            public const int SW_SHOWMAXIMIZED = 3;
            public const int SW_MAXIMIZE = 3;
            public const int SW_SHOWNOACTIVATE = 4;
            public const int SW_SHOW = 5;
            public const int SW_MINIMIZE = 6;
            public const int SW_SHOWMINNOACTIVE = 7;
            public const int SW_SHOWNA = 8;
            public const int SW_RESTORE = 9;
            public const int SW_SHOWDEFAULT = 10;
            public const int SW_FORCEMINIMIZE = 11;
            public const int SW_MAX = 11;

            #endregion

            #region SWP

            public const int SWP_DRAWFRAME = 0x0020;
            public const int SWP_HIDEWINDOW = 0x0080;
            public const int SWP_NOACTIVATE = 0x0010;
            public const int SWP_NOMOVE = 0x0002;
            public const int SWP_NOREDRAW = 0x0008;
            public const int SWP_NOSIZE = 0x0001;
            public const int SWP_NOZORDER = 0x0004;
            public const int SWP_SHOWWINDOW = 0x0040;
            public const int SWP_FRAMECHANGED = 0x0020;

            #endregion

            #region WH
            public const int WH_MOUSE_LL = 14;
            public const int WH_KEYBOARD_LL = 13;
            public const int WH_MOUSE = 7;
            public const int WH_KEYBOARD = 2;
            #endregion

            #region WM
            
            public const int MIN_ALL = 419;
            public const int MIN_ALL_UNDO = 416;

            public const int WM_NULL = 0x0000;
            public const int WM_CREATE = 0x0001;
            public const int WM_DESTROY = 0x0002;
            public const int WM_MOVE = 0x0003;
            public const int WM_SIZE = 0x0005;
            public const int WM_ACTIVATE = 0x0006;
            public const int WM_SETFOCUS = 0x0007;
            public const int WM_KILLFOCUS = 0x0008;
            public const int WM_ENABLE = 0x000A;
            public const int WM_SETREDRAW = 0x000B;
            public const int WM_SETTEXT = 0x000C;
            public const int WM_GETTEXT = 0x000D;
            public const int WM_GETTEXTLENGTH = 0x000E;
            public const int WM_PAINT = 0x000F;
            public const int WM_CLOSE = 0x0010;
            public const int WM_QUERYENDSESSION = 0x0011;
            public const int WM_QUIT = 0x0012;
            public const int WM_QUERYOPEN = 0x0013;
            public const int WM_ERASEBKGND = 0x0014;
            public const int WM_SYSCOLORCHANGE = 0x0015;
            public const int WM_ENDSESSION = 0x0016;
            public const int WM_SYSTEMERROR = 0x0017;
            public const int WM_SHOWWINDOW = 0x0018;
            public const int WM_CTLCOLOR = 0x0019;
            public const int WM_WININICHANGE = 0x001A;
            public const int WM_SETTINGCHANGE = WM_WININICHANGE;
            public const int WM_DEVMODECHANGE = 0x001B;
            public const int WM_ACTIVATEAPP = 0x001C;
            public const int WM_FONTCHANGE = 0x001D;
            public const int WM_TIMECHANGE = 0x001E;
            public const int WM_CANCELMODE = 0x001F;
            public const int WM_SETCURSOR = 0x0020;
            public const int WM_MOUSEACTIVATE = 0x0021;
            public const int WM_CHILDACTIVATE = 0x0022;
            public const int WM_QUEUESYNC = 0x0023;
            public const int WM_GETMINMAXINFO = 0x0024;
            public const int WM_PAINTICON = 0x0026;
            public const int WM_ICONERASEBKGND = 0x0027;
            public const int WM_NEXTDLGCTL = 0x0028;
            public const int WM_SPOOLERSTATUS = 0x002A;
            public const int WM_DRAWITEM = 0x002B;
            public const int WM_MEASUREITEM = 0x002C;
            public const int WM_DELETEITEM = 0x002D;
            public const int WM_VKEYTOITEM = 0x002E;
            public const int WM_CHARTOITEM = 0x002F;
            public const int WM_SETFONT = 0x0030;
            public const int WM_GETFONT = 0x0031;
            public const int WM_SETHOTKEY = 0x0032;
            public const int WM_GETHOTKEY = 0x0033;
            public const int WM_QUERYDRAGICON = 0x0037;
            public const int WM_COMPAREITEM = 0x0039;
            public const int WM_GETOBJECT = 0x003D;
            public const int WM_COMPACTING = 0x0041;
            public const int WM_COMMNOTIFY = 0x0044;
            public const int WM_WINDOWPOSCHANGING = 0x0046;
            public const int WM_WINDOWPOSCHANGED = 0x0047;
            public const int WM_POWER = 0x0048;
            public const int WM_COPYDATA = 0x004A;
            public const int WM_CANCELJOURNAL = 0x004B;
            public const int WM_NOTIFY = 0x004E;
            public const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
            public const int WM_INPUTLANGCHANGE = 0x0051;
            public const int WM_TCARD = 0x0052;
            public const int WM_HELP = 0x0053;
            public const int WM_USERCHANGED = 0x0054;
            public const int WM_NOTIFYFORMAT = 0x0055;
            public const int WM_CONTEXTMENU = 0x007B;
            public const int WM_STYLECHANGING = 0x007C;
            public const int WM_STYLECHANGED = 0x007D;
            public const int WM_DISPLAYCHANGE = 0x007E;
            public const int WM_GETICON = 0x007F;
            public const int WM_SETICON = 0x0080;
            public const int WM_NCCREATE = 0x0081;
            public const int WM_NCDESTROY = 0x0082;
            public const int WM_NCCALCSIZE = 0x0083;
            public const int WM_NCHITTEST = 0x0084;
            public const int WM_NCPAINT = 0x0085;
            public const int WM_NCACTIVATE = 0x0086;
            public const int WM_GETDLGCODE = 0x0087;
            public const int WM_NCMOUSEMOVE = 0x00A0;
            public const int WM_NCLBUTTONDOWN = 0x00A1;
            public const int WM_NCLBUTTONUP = 0x00A2;
            public const int WM_NCLBUTTONDBLCLK = 0x00A3;
            public const int WM_NCRBUTTONDOWN = 0x00A4;
            public const int WM_NCRBUTTONUP = 0x00A5;
            public const int WM_NCRBUTTONDBLCLK = 0x00A6;
            public const int WM_NCMBUTTONDOWN = 0x00A7;
            public const int WM_NCMBUTTONUP = 0x00A8;
            public const int WM_NCMBUTTONDBLCLK = 0x00A9;
            public const int WM_NCXBUTTONDOWN = 0x00AB;
            public const int WM_NCXBUTTONUP = 0x00AC;
            public const int WM_NCXBUTTONDBLCLK = 0x00AD;
            public const int WM_INPUT = 0x00FF;
            public const int WM_KEYFIRST = 0x0100;
            public const int WM_KEYDOWN = 0x0100;
            public const int WM_KEYUP = 0x0101;
            public const int WM_CHAR = 0x0102;
            public const int WM_DEADCHAR = 0x0103;
            public const int WM_SYSKEYDOWN = 0x0104;
            public const int WM_SYSKEYUP = 0x0105;
            public const int WM_SYSCHAR = 0x0106;
            public const int WM_SYSDEADCHAR = 0x0107;
            public const int WM_UNICHAR = 0x0109;
            public const int WM_KEYLAST = 0x0109;
            public const int WM_INITDIALOG = 0x0110;
            public const int WM_COMMAND = 0x0111;
            public const int WM_SYSCOMMAND = 0x0112;
            public const int WM_TIMER = 0x0113;
            public const int WM_HSCROLL = 0x0114;
            public const int WM_VSCROLL = 0x0115;
            public const int WM_INITMENU = 0x0116;
            public const int WM_INITMENUPOPUP = 0x0117;
            public const int WM_MENUSELECT = 0x011F;
            public const int WM_MENUCHAR = 0x0120;
            public const int WM_ENTERIDLE = 0x0121;
            public const int WM_MENURBUTTONUP = 0x0122;
            public const int WM_MENUDRAG = 0x0123;
            public const int WM_MENUGETOBJECT = 0x0124;
            public const int WM_UNINITMENUPOPUP = 0x0125;
            public const int WM_MENUCOMMAND = 0x0126;
            public const int WM_CHANGEUISTATE = 0x0127;
            public const int WM_UPDATEUISTATE = 0x0128;
            public const int WM_QUERYUISTATE = 0x0129;
            public const int WM_CTLCOLORMSGBOX = 0x0132;
            public const int WM_CTLCOLOREDIT = 0x0133;
            public const int WM_CTLCOLORLISTBOX = 0x0134;
            public const int WM_CTLCOLORBTN = 0x0135;
            public const int WM_CTLCOLORDLG = 0x0136;
            public const int WM_CTLCOLORSCROLLBAR = 0x0137;
            public const int WM_CTLCOLORSTATIC = 0x0138;
            public const int WM_MOUSEFIRST = 0x0200;
            public const int WM_MOUSEMOVE = 0x0200;
            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_LBUTTONUP = 0x0202;
            public const int WM_LBUTTONDBLCLK = 0x0203;
            public const int WM_RBUTTONDOWN = 0x0204;
            public const int WM_RBUTTONUP = 0x0205;
            public const int WM_RBUTTONDBLCLK = 0x0206;
            public const int WM_MBUTTONDOWN = 0x0207;
            public const int WM_MBUTTONUP = 0x0208;
            public const int WM_MBUTTONDBLCLK = 0x0209;
            public const int WM_MOUSEWHEEL = 0x020A;
            public const int WM_MOUSELAST = 0x020A;
            public const int WM_PARENTNOTIFY = 0x0210;
            public const int WM_ENTERMENULOOP = 0x0211;
            public const int WM_EXITMENULOOP = 0x0212;
            public const int WM_NEXTMENU = 0x0213;
            public const int WM_SIZING = 532;
            public const int WM_CAPTURECHANGED = 533;
            public const int WM_MOVING = 534;
            public const int WM_POWERBROADCAST = 536;
            public const int WM_DEVICECHANGE = 537;
            public const int WM_IME_STARTCOMPOSITION = 0x010D;
            public const int WM_IME_ENDCOMPOSITION = 0x010E;
            public const int WM_IME_COMPOSITION = 0x010F;
            public const int WM_IME_KEYLAST = 0x010F;
            public const int WM_IME_SETCONTEXT = 0x0281;
            public const int WM_IME_NOTIFY = 0x0282;
            public const int WM_IME_CONTROL = 0x0283;
            public const int WM_IME_COMPOSITIONFULL = 0x0284;
            public const int WM_IME_SELECT = 0x0285;
            public const int WM_IME_CHAR = 0x0286;
            public const int WM_IME_REQUEST = 0x0288;
            public const int WM_IME_KEYDOWN = 0x0290;
            public const int WM_IME_KEYUP = 0x0291;
            public const int WM_MDICREATE = 0x0220;
            public const int WM_MDIDESTROY = 0x0221;
            public const int WM_MDIACTIVATE = 0x0222;
            public const int WM_MDIRESTORE = 0x0223;
            public const int WM_MDINEXT = 0x0224;
            public const int WM_MDIMAXIMIZE = 0x0225;
            public const int WM_MDITILE = 0x0226;
            public const int WM_MDICASCADE = 0x0227;
            public const int WM_MDIICONARRANGE = 0x0228;
            public const int WM_MDIGETACTIVE = 0x0229;
            public const int WM_MDISETMENU = 0x0230;
            public const int WM_ENTERSIZEMOVE = 0x0231;
            public const int WM_EXITSIZEMOVE = 0x0232;
            public const int WM_DROPFILES = 0x0233;
            public const int WM_MDIREFRESHMENU = 0x0234;
            public const int WM_MOUSEHOVER = 0x02A1;
            public const int WM_MOUSELEAVE = 0x02A3;
            public const int WM_NCMOUSEHOVER = 0x02A0;
            public const int WM_NCMOUSELEAVE = 0x02A2;
            public const int WM_WTSSESSION_CHANGE = 0x02B1;
            public const int WM_TABLET_FIRST = 0x02C0;
            public const int WM_TABLET_LAST = 0x02DF;
            public const int WM_CUT = 0x0300;
            public const int WM_COPY = 0x0301;
            public const int WM_PASTE = 0x0302;
            public const int WM_CLEAR = 0x0303;
            public const int WM_UNDO = 0x0304;
            public const int WM_RENDERFORMAT = 0x0305;
            public const int WM_RENDERALLFORMATS = 0x0306;
            public const int WM_DESTROYCLIPBOARD = 0x0307;
            public const int WM_DRAWCLIPBOARD = 0x0308;
            public const int WM_PAINTCLIPBOARD = 0x0309;
            public const int WM_VSCROLLCLIPBOARD = 0x030A;
            public const int WM_SIZECLIPBOARD = 0x030B;
            public const int WM_ASKCBFORMATNAME = 0x030C;
            public const int WM_CHANGECBCHAIN = 0x030D;
            public const int WM_HSCROLLCLIPBOARD = 0x030E;
            public const int WM_QUERYNEWPALETTE = 0x030F;
            public const int WM_PALETTEISCHANGING = 0x0310;
            public const int WM_PALETTECHANGED = 0x0311;
            public const int WM_HOTKEY = 0x0312;
            public const int WM_PRINT = 791;
            public const int WM_PRINTCLIENT = 792;
            public const int WM_APPCOMMAND = 0x0319;
            public const int WM_THEMECHANGED = 0x031A;
            public const int WM_HANDHELDFIRST = 856;
            public const int WM_HANDHELDLAST = 863;
            public const int WM_PENWINFIRST = 0x0380;
            public const int WM_PENWINLAST = 0x038F;
            public const int WM_COALESCE_FIRST = 0x0390;
            public const int WM_COALESCE_LAST = 0x039F;
            public const int WM_DDE_FIRST = 0x03E0;
            public const int WM_DWMCOMPOSITIONCHANGED = 0x031E;
            public const int WM_DWMNCRENDERINGCHANGED = 0x031F;
            public const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
            public const int WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321;
            public const int WM_APP = 0x8000;
            public const int WM_USER = 0x0400;

            #endregion

            #region WS

            public const int WS_OVERLAPPED = 0x0;
            public const uint WS_POPUP = 0x80000000;
            public const int WS_CHILD = 0x40000000;
            public const int WS_MINIMIZE = 0x20000000;
            public const int WS_VISIBLE = 0x10000000;
            public const int WS_DISABLED = 0x8000000;
            public const int WS_CLIPSIBLINGS = 0x4000000;
            public const int WS_CLIPCHILDREN = 0x2000000;
            public const int WS_MAXIMIZE = 0x1000000;
            public const int WS_CAPTION = 0xC00000;
            public const int WS_BORDER = 0x800000;
            public const int WS_DLGFRAME = 0x400000;
            public const int WS_VSCROLL = 0x200000;
            public const int WS_HSCROLL = 0x100000;
            public const int WS_SYSMENU = 0x80000;
            public const int WS_THICKFRAME = 0x40000;
            public const int WS_GROUP = 0x20000;
            public const int WS_TABSTOP = 0x10000;
            public const int WS_MINIMIZEBOX = 0x20000;
            public const int WS_MAXIMIZEBOX = 0x10000;
            public const int WS_TILED = WS_OVERLAPPED;
            public const int WS_ICONIC = WS_MINIMIZE;
            public const int WS_SIZEBOX = WS_THICKFRAME;
            public const int WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
            public const int WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;
            public const uint WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU);
            public const int WS_CHILDWINDOW = (WS_CHILD);

            public const int WS_EX_WINDOWEDGE = 0x100;
            public const int WS_EX_CLIENTEDGE = 0x200;
            public const int WS_EX_TOOLWINDOW = 0x80;
            public const int WS_EX_TOPMOST = 0x8;
            public const int WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
            public const int WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
            public const int WS_EX_DLGMODALFRAME = 0x1;
            public const int WS_EX_NOPARENTNOTIFY = 0x4;
            public const int WS_EX_TRANSPARENT = 0x20;
            public const int WS_EX_MDICHILD = 0x40;
            public const int WS_EX_CONTEXTHELP = 0x400;
            public const int WS_EX_RIGHT = 0x1000;
            public const int WS_EX_RTLREADING = 0x2000;
            public const int WS_EX_LEFTSCROLLBAR = 0x4000;
            public const int WS_EX_CONTROLPARENT = 0x10000;
            public const int WS_EX_STATICEDGE = 0x20000;
            public const int WS_EX_APPWINDOW = 0x40000;
            public const int WS_EX_LAYERED = 0x80000;
            public const int WS_EX_NOINHERITLAYOUT = 0x100000;
            public const int WS_EX_LAYOUTRTL = 0x400000;
            public const int WS_EX_NOACTIVATE = 0x8000000;
            public const int WS_EX_LEFT = 0x0;
            public const int WS_EX_LTRREADING = 0x0;
            public const int WS_EX_RIGHTSCROLLBAR = 0x0;
            public const int WS_EX_ACCEPTFILES = 0x10;
            public const int WS_EX_COMPOSITED = 0x2000000;

            #endregion

            #endregion Win32 Constants

        }
        #endregion Win32
    }

    class HotKey : IMessageFilter
    {
        /*
         * https://stackoverflow.com/questions/18685726/how-can-i-prevent-registerhotkey-from-blocking-the-key-for-other-applications
         * RegisterHotKey() registers global hotkeys.
         * Hotkeys are processed before regular keyboard input processing, 
         * meaning that if you register a hotkey successfully, 
         * pressing that key will result in you getting your hotkey message rather than 
         * the app with focus getting the normal WM_KEYDOWN/WM_CHAR messages. 
         * You have effectively blocked other apps from seeing that key press.
         * 
         * Note: Send(Keys Key) will NOT trigger InputHook() or HotKey(), but Send(string Keystrokes) will.
         */

        private class Win32
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, Keys vk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        }

        private const int WM_HOTKEY = 0x0312;

        private int HotKeyID { get; set; }

        private event Action<HotKey> onKeyAction;

        public HotKey() { } //empty constructor for option to call Register() separaately.
        public HotKey(Keys Modifiers, Keys Key = Keys.None, Action<HotKey> OnKeyAction = null)
        {
            Register(Modifiers, Key, OnKeyAction);
        }
        public bool Register(Keys Modifiers = Keys.None, Keys Key = Keys.None, Action<HotKey> OnKeyAction = null)
        {
            if (Key == Keys.None || OnKeyAction == null)
                return false;

            uint modifiers = 0;

            if (((int)Modifiers & (int)Keys.Alt) == (int)Keys.Alt)
                modifiers = modifiers | 1;
            if (((int)Modifiers & (int)Keys.Control) == (int)Keys.Control)
                modifiers = modifiers | 2;
            if (((int)Modifiers & (int)Keys.Shift) == (int)Keys.Shift)
                modifiers = modifiers | 4;

            this.onKeyAction = OnKeyAction;

            Application.AddMessageFilter(this);

            HotKeyID = Key.GetHashCode() + modifiers.GetHashCode();

            return Win32.RegisterHotKey(IntPtr.Zero, HotKeyID, modifiers, Key);
        }
        public bool UnRegister()
        {
            Application.RemoveMessageFilter(this);
            return Win32.UnregisterHotKey(IntPtr.Zero, HotKeyID);
        }
        ~HotKey()
        {
            UnRegister();
        }
        public bool PreFilterMessage(ref Message m)
        {
            //trigger if registered hotkey
            if (m.Msg == WM_HOTKEY && (int)m.WParam == HotKeyID)
            {
                //except ignore Sending.Key by Send()
                int sendingKeyID = Sending.Key.GetHashCode() + Keys.None.GetHashCode();


                if ((int)m.WParam == sendingKeyID) //(int)Sending.Key)
                {
                    //MessageBox.Show("HOTKEY Sending.Key=" + Sending.Key + ", key=" + m.WParam + ", SendingKeyID=" + sendingKeyID);

                    Sending.Key = Keys.None;
                    return false;
                }
    
                OnHotKeyAction();
            }
            return false;
        }
        private void OnHotKeyAction()
        {
            onKeyAction?.Invoke(this);
            return;

            //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new Action(() => HotKeyAction?.Invoke(this)));
        }
    }
    class InputHook
    {

        #region Public Properties
        public int vkCode { get; set; }
        public int scanCode { get; set; }
        public Point point { get; set; }
        public int data { get; set; }
        public enum Wheels : int { Wheel = WM_MOUSEWHEEL, HWheel = WM_MOUSEHWHEEL }
        #endregion

        #region Private Properties
        private Keys RegisteredModifiers { get; set; }
        private Keys RegisteredKey { get; set; }
        private Keys RegisteredKeyDownOrUp { get; set; }
        private Keys RegisteredSupressYN { get; set; }
        private int RegisteredMouseButton { get; set; }
        private int RegisteredMouseWheel { get; set; }
        private bool InProgressFlag { get; set; }
        private int HookType { get; set; }
        private IntPtr hookID { get; set; } = IntPtr.Zero;

        private delegate IntPtr HookHandler(int nCode, IntPtr wParam, IntPtr lParam);

        private event Action<InputHook> keyAction;

        private List<Keys> MouseButtonsList = new List<Keys>()
        {
            Keys.LButton, Keys.MButton, Keys.RButton, Keys.XButton1, Keys.XButton2
        };
        #endregion Private Properties

        #region Constructors
        public InputHook(Keys Modifiers, Keys Key, Keys DownOrUp, Keys SupressYN, Action<InputHook> OnKeyAction = null)
        {
            //Keys Modifiers    Alt, Control, None, Shift, LWin, RWin, or combination example: Keys.Alt | Keys.Control
            //Keys Key (kybd)   Keys.A through Keys.Zoom
            //Keys Key (mouse)  Keys.LButton, Keys.MButton, Keys.RButton, Keys.XButton1, Keys.XButton2
            //Keys DownOrUp     Keys.Up, Keys,Down
            //Keys SupressYN    Keys.Y, Keys.N
            //Action            OnKeyAction
            //
            //Note: Send(Keys Key) will NOT trigger InputHook() or HotKey(), but Send(string Keystrokes) will.

            if (DownOrUp != Keys.Up & DownOrUp != Keys.Down)
                DownOrUp = Keys.Down;

            if (SupressYN != Keys.Y & SupressYN != Keys.N)
                SupressYN = Keys.N;

            if (MouseButtonsList.Contains(Key))
            {
                if (Key == Keys.LButton & DownOrUp == Keys.Down) RegisteredMouseButton = WM_LBUTTONDOWN;
                else if (Key == Keys.LButton & DownOrUp == Keys.Up) RegisteredMouseButton = WM_LBUTTONUP;
                else if (Key == Keys.MButton & DownOrUp == Keys.Down) RegisteredMouseButton = WM_MBUTTONDOWN;
                else if (Key == Keys.MButton & DownOrUp == Keys.Up) RegisteredMouseButton = WM_MBUTTONUP;
                else if (Key == Keys.RButton & DownOrUp == Keys.Down) RegisteredMouseButton = WM_RBUTTONDOWN;
                else if (Key == Keys.RButton & DownOrUp == Keys.Up) RegisteredMouseButton = WM_RBUTTONUP;
                else if (Key == Keys.XButton1 & DownOrUp == Keys.Down) RegisteredMouseButton = WM_XBUTTONDOWN;
                else if (Key == Keys.XButton1 & DownOrUp == Keys.Up) RegisteredMouseButton = WM_XBUTTONUP;
                else if (Key == Keys.XButton2 & DownOrUp == Keys.Down) RegisteredMouseButton = WM_XBUTTONDOWN;
                else if (Key == Keys.XButton2 & DownOrUp == Keys.Up) RegisteredMouseButton = WM_XBUTTONUP;

                RegisteredModifiers = Modifiers;
                RegisteredKeyDownOrUp = DownOrUp;
                RegisteredSupressYN = SupressYN;
                keyAction += OnKeyAction;
                SetHook(WH_MOUSE_LL);
            }
            else
            {
                RegisteredModifiers = Modifiers;
                RegisteredKey = Key;
                RegisteredKeyDownOrUp = DownOrUp;
                RegisteredSupressYN = SupressYN;
                keyAction += OnKeyAction;
                SetHook(WH_KEYBOARD_LL);
            }
        }
        public InputHook(Keys Modifiers, Wheels MouseWheel, Keys SupressYN, Action<InputHook> OnKeyAction = null)
        {
            //Keys Modifiers    Alt, Control, None, Shift, LWin, RWin, or combo:  Keys.Alt | Keys.Control
            //Wheels MouseWheel Wheel, HWheel
            //Keys SupressYN    Keys.Y, Keys.N
            //Action            OnKeyAction

            RegisteredModifiers = Modifiers;
            RegisteredMouseWheel = (int)MouseWheel;
            RegisteredSupressYN = SupressYN;
            keyAction += OnKeyAction;
            SetHook(WH_MOUSE_LL);
        }
        #endregion Constructors

        ~InputHook()
        {
            Stop();
        }
        private void SetHook(int HookType)
        {
            this.HookType = HookType;
            HookHandler HOOKPROC = HookFunc;
            hookID = SetWindowsHookEx(HookType, HOOKPROC, IntPtr.Zero, 0);
            InProgressFlag = true;
        }
        public bool InProgress()
        {
            return InProgressFlag;
        }
        public void Stop()
        {
            UnhookWindowsHookEx(hookID);
            InProgressFlag = false;
        }
        public void OnKeyAction()
        {
            keyAction?.Invoke(this);
            //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new Action(() => HotKeyAction?.Invoke(this)));
        }
        private IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (HookType == WH_KEYBOARD_LL)
                {
                    int iwParam = wParam.ToInt32();

                    if ((iwParam == WM_KEYDOWN || iwParam == WM_SYSKEYDOWN) & RegisteredKeyDownOrUp == Keys.Down)
                    {
                        if (ProcessKeyboard(iwParam, wParam, lParam))
                            return (IntPtr)1;
                    }
                    else if ((iwParam == WM_KEYUP || iwParam == WM_SYSKEYUP) & RegisteredKeyDownOrUp == Keys.Up)
                    {

                        if (ProcessKeyboard(iwParam, wParam, lParam))
                            return (IntPtr)1;
                    }
                }
                else if (HookType == WH_MOUSE_LL)
                {
                    int iwParam = wParam.ToInt32();

                    if (iwParam == RegisteredMouseButton)
                    {
                        if (ProcessMouseButton(iwParam, wParam, lParam))
                            return (IntPtr)1;

                    }
                    else if (iwParam == RegisteredMouseWheel)
                    {
                        if (ProcessMouseWheel(iwParam, wParam, lParam))
                            return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }
        private bool ProcessKeyboard(int iwParam, IntPtr wParam, IntPtr lParam)
        {
            KeyboardHookStruct hookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

            if ((Keys)hookStruct.vkCode == RegisteredKey & Control.ModifierKeys == RegisteredModifiers)
            {
                //except ignore Sending.Key from Send()
                if (RegisteredKey == Sending.Key)
                {
                    Sending.Key = Keys.None;
                    return false;
                }

                this.vkCode = hookStruct.vkCode;
                this.scanCode = hookStruct.scanCode;

                OnKeyAction();

                if (RegisteredSupressYN == Keys.Y)
                    return true;
            }
            return false;
        }
        private bool ProcessMouseButton(int iwParam, IntPtr wParam, IntPtr lParam)
        {
            MouseHookStruct hookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));

            if (iwParam == RegisteredMouseButton & Control.ModifierKeys == RegisteredModifiers)
            {

                //WM_XBUTTON data: 0x02=FWD, 0x01=BACK
                if (RegisteredMouseButton == WM_XBUTTONDOWN | RegisteredMouseButton == WM_XBUTTONUP)
                    this.data = hookStruct.data >> 16;

                this.vkCode = RegisteredMouseButton;

                OnKeyAction();

                if (RegisteredSupressYN == Keys.Y)
                    return true;
            }
            return false;
        }
        private bool ProcessMouseWheel(int iwParam, IntPtr wParam, IntPtr lParam)
        {
            MouseHookStruct hookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));

            if (iwParam == RegisteredMouseWheel & Control.ModifierKeys == RegisteredModifiers)
            {

                this.point = hookStruct.point;

                //WM_MOUSEWHEEL data: 0x78=FWD (+120), 0x88=BACK (-120)
                if (RegisteredMouseWheel == WM_MOUSEWHEEL | RegisteredMouseWheel == WM_MOUSEHWHEEL)
                    this.data = hookStruct.data >> 16;

                this.vkCode = RegisteredMouseWheel;

                OnKeyAction();

                if (RegisteredSupressYN == Keys.Y)
                    return true;
            }
            return false;
        }
        //optional to combine keyboard and mouse then make public if user needs flags, time, dwExtraInfo
        [StructLayout(LayoutKind.Sequential)]
        public class InputHookStruct
        {
            public int vkCode;
            public int scanCode;
            public System.Drawing.Point point;
            public int data;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public System.Drawing.Point point;
            public int data;
            public int flags;
            public int time;
            public int dwExtraInfo;
            public int mouseDataHighWord;
        }

        #region WinAPI
        public const int WH_MOUSE_LL = 14;
        public const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x100;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYUP = 0x105;

        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_MBUTTONDOWN = 0x207;
        public const int WM_MBUTTONUP = 0x208;
        public const int WM_MOUSEWHEEL = 0x20A;
        public const int WM_XBUTTONDOWN = 0x20B;
        public const int WM_XBUTTONUP = 0x20C;
        public const int WM_MOUSEHWHEEL = 0x20E;

        public const int WHEEL_DELTA = 120;
        public const int XBUTTON1 = 0x1;
        public const int XBUTTON2 = 0x2;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookHandler lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        #endregion
    }
    class BlockInputHook
    {

        #region Private Properties
        private Keys RegisteredUnBlockKey { get; set; }
        private bool BlockKeyboardFlag { get; set; }
        private bool BlockMouseFlag { get; set; }
        private IntPtr HookKeyboardID { get; set; } = IntPtr.Zero;
        private IntPtr HookMouseID { get; set; } = IntPtr.Zero;

        private delegate IntPtr HookHandler(int nCode, IntPtr wParam, IntPtr lParam);

        private event Action<BlockInputHook> keyAction;

        #endregion Private Properties

        #region Constructor

        public BlockInputHook(bool Keyboard, bool Mouse, Keys UnBlockKey, Action<BlockInputHook> OnKeyAction = null)
        {
            //Keyboard          Block input True or False
            //Mouse             Block input True or False
            //EscapeKey         Key to abort and unblock input
            //CAUTION           BlockInputHook() will block keystrokes from Send()

            if (!Keyboard & !Mouse)
                return;

            RegisteredUnBlockKey = UnBlockKey;
            keyAction += OnKeyAction;

            HookHandler HOOKPROC = HookFunc;

            //set keyboard hook to catch abort key
            HookKeyboardID = SetWindowsHookEx(WH_KEYBOARD_LL, HOOKPROC, IntPtr.Zero, 0);

            if (Keyboard)
                BlockKeyboardFlag = true;

            if (Mouse)
            {
                HookMouseID = SetWindowsHookEx(WH_MOUSE_LL, HOOKPROC, IntPtr.Zero, 0);
                BlockMouseFlag = true;
            }
        }

        #endregion Constructors

        ~BlockInputHook()
        {
            Stop();
        }
        public bool InProgress()
        {
            return BlockKeyboardFlag | BlockMouseFlag;
        }
        public void Stop(bool Keyboard = true, bool Mouse = true)
        {
            if (Keyboard && HookKeyboardID != IntPtr.Zero)
                UnhookWindowsHookEx(HookKeyboardID);

            if (Mouse && HookMouseID != IntPtr.Zero)
                UnhookWindowsHookEx(HookMouseID);
        }
        public void OnKeyAction()
        {
            keyAction?.Invoke(this);
            //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new Action(() => HotKeyAction?.Invoke(this)));
        }
        private IntPtr HookFunc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int iwParam = wParam.ToInt32();

                if (iwParam >= WM_KEYDOWN && iwParam <= WM_SYSKEYUP)
                {
                    KeyboardHookStruct hookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

                    if ((Keys)hookStruct.vkCode == RegisteredUnBlockKey)
                    {
                        OnKeyAction();
                        return (IntPtr)1; //SUPRESS
                    }
                }
                else if (iwParam >= WM_MOUSEMOVE && iwParam <= WM_MOUSEHWHEEL && BlockMouseFlag)
                {
                    return (IntPtr)1; //SUPRESS
                }
            }
            if (BlockKeyboardFlag)
                return (IntPtr)1; // CallNextHookEx(hookID, nCode, wParam, lParam);
            else
                return CallNextHookEx(HookKeyboardID, nCode, wParam, lParam);
        }
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public System.Drawing.Point point;
            public int data;
            public int flags;
            public int time;
            public int dwExtraInfo;
            public int mouseDataHighWord;
        }

        #region WinAPI
        public const int WH_MOUSE_LL = 14;
        public const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x100;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYUP = 0x105;

        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_MBUTTONDOWN = 0x207;
        public const int WM_MBUTTONUP = 0x208;
        public const int WM_MOUSEWHEEL = 0x20A;
        public const int WM_XBUTTONDOWN = 0x20B;
        public const int WM_XBUTTONUP = 0x20C;
        public const int WM_MOUSEHWHEEL = 0x20E;

        public const int WHEEL_DELTA = 120;
        public const int XBUTTON1 = 0x1;
        public const int XBUTTON2 = 0x2;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookHandler lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        #endregion
    }


}
