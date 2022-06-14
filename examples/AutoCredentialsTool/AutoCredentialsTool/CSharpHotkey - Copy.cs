/* 2022-06-03-0800 Rectangle GetPos, DoDelay
 
    TODO
        GetWindowsList and/or GetDesktopWindowsList?
        InputBox

        Cleanup Win32 by removing unused DLL imports
*/

/*
    OVERVIEW
    --------
    CSharpHotkey implements many of the AutoHotkey_L_v1 automation functions in C# .NET code
    The user's C# .NET code provides all the gui, structure, work, and flow.
    CSharpHotkey provides many of the Pinvoke Win32 native functions as AHK_L_v1 Win functions
    CSharpHotkey is a C# class compiled with the exe (user could compile as external dyamic dll if preferred)

    CSharpHotkey is NOT;
        1. A complete implementation or replacement of AHK_L
        2. Cannot load or execute .ahk scripts
        3. No plans to update for AHK_L_v2

    DIFFERENCES
    -----------
    WinTitle        Most modern desktop and web apps don't use controls anymore,
                    Therefore there is no need to match classname nor text in controls.
                    We can still use the WinTitle for basic Hotkey automation

    Unimplemented   Most functions may be implemented with C# .NET code:
                        File, I/O, flow, Group, Obj, Send, Sounds, String, etc.
                    Control functions not implemented because modern desktop and web apps don't use controls

    BUILD TOOLS
    -----------
    Visual Studio 2022 with .NET 3.5 (for compatability)
    Can easily be converted to other Visual Studio, .NET Core, and .NET Framework versions.
    Can easily be converted a static class if preferred (remove Constructor and change methods to static).

    CREDITS
    -----------
    ImageSearch:        See ImageSearch()
    Everything Else:    The many contributors to StackOverflow, CodeProject, MSDN, and many others.
    
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

namespace CSharpHotkeyLib
{
 
    #region Public Struct

    public struct GET
    {
        public const int ID = 1;
        public const int MinMax = 2;
        public const int PID = 3;
        public const int ProcessName = 4;
        public const int ProcessPath = 5;
        public const int Style = 6;

        public const int Disabled = 7;
        public const int Exist = 8;
        public const int Maximize = 9;
        public const int Minimize = 10;
        public const int Normal = 11;
        public const int Visible = 12;
    }
    public struct MATCH_MODE
    {
        public const int StartsWith = 1;
        public const int Contains = 2;
        public const int Exact = 3;
        public const int EndsWith = 4;
    }
    public struct MATCH_CASE
    {
        public const int InSensitive = 1;
        public const int Sensitive = 2;
    }
    public struct MOUSE_BUTTON
    {
        //Mouse - System.Windows.Input.MouseButton Enum
        public const int Left = 0;
        public const int Middle = 1;
        public const int Right = 2;
    }
    public struct SET
    {
        public const int AlwaysOnTop = 1;
        public const int Bottom = 2;
        public const int Disable = 3;
        public const int Enable = 4;
        public const int NotTop = 5;
        public const int Top = 6;
    }
    public struct MODKEY
    {
        //Hotkey modifiers
        public const uint None = 0x0000; //custom
        public const uint Alt = 0x0001;
        public const uint Control = 0x0002;
        public const uint Shift = 0x0004;
        public const uint Win = 0x0008;
        public const uint ControlAlt = MODKEY.Control + MODKEY.Alt; //custom
    }
    public struct PIXEL_MODE
    {
        public const int BGR = 1;
        public const int RGB = 2;
    }
    public struct WINAPI
    {
        public const int WM_HOTKEY = 0x0312;
    }

    #endregion Public Struct
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
        private class POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class MouseLLHookStruct
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        #endregion
        
        #region Properties

        public string DebugString { get; set; }

        //LastFoundWindow
        //In AHK, this is the window most recently found by
        //  IfWinExist, IfWinNotExist, WinExist(), IfWinActive, IfWinNotActive, WinActive(),
        //  WinWaitActive, WinWaitNotActive, or WinWait.
        //Here it's the window found by GetID(), which is used in most functions
        private static IntPtr LastFoundWindowHandle { get; set; }
        private static int TitleMatchMode { get; set; }
        private static int TitleMatchModeCase { get; set; }
        private static int KeyDelay { get; set; }
        private static int MouseDelay { get; set; }
        private static int WinDelay { get; set; }
        public bool MouseEventHandledFlag { get; set; }
        private List<string> MinimizeAllList { get; set; }
        #endregion Properties

        #region Variables

        private static Dictionary<string, IntPtr> windowHandleDict = new Dictionary<string, IntPtr>();

        private int hMouseHook = 0;
        private int hKeyboardHook = 0;
        private static HookProc MouseHookProcedure;
        private static HookProc KeyboardHookProcedure;

        private static IntPtr hThis = Process.GetCurrentProcess().MainWindowHandle;

        /*
         * Individual keys are registered as hook keys
         * Each key is saved with a bool flag to suppress or not suppress the key back to the OS
         */
        private static Dictionary<Keys, bool> hookedKeysDict = new Dictionary<Keys, bool>();

        private static List<string> hotKeyList = new List<string>();
        private static int hotKeyNumber = 0;

        #endregion Variables

        #region Delegates
        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        #endregion

        #region Events
        public event MouseEventHandler OnMouseActivity;
        public event KeyEventHandler KeyDown;
        public event KeyPressEventHandler KeyPress;
        public event KeyEventHandler KeyUp;
        #endregion

        #region Constructor
        public CSharpHotkey()
        {
            LastFoundWindowHandle = IntPtr.Zero;

            TitleMatchModeCase = MATCH_MODE.Exact;
            TitleMatchModeCase = MATCH_CASE.Sensitive;

            KeyDelay = MouseDelay = 10;
            WinDelay = 100;

            MinimizeAllList = new List<string>();

            MouseEventHandledFlag = false; //not used. never supress mouse activity

        }
        #endregion Constructor

        #region Public Methods
        public bool Activate(string WinTitle = "")
        {
            bool result = Win32.SetForegroundWindow(GetID(WinTitle));
            DoDelay(WinDelay);
            return result;
        }
        public bool ActivateBottom_UNIMPLEMENTED(string WinTitle = "")
        {
            return false;
        }
        public bool Active(string WinTitle = "")
        {
            if (GetID(WinTitle) == Win32.GetForegroundWindow())
                return true;

            return false;
        }
        //public void Click(int MOUSE_BUTTON_PARAM = MOUSE_BUTTON.Left, Point MousePoint )
        //{ how to have defalut Pont of (-1,-1)?
        //    Click(MOUSE_BUTTON_PARAM, MousePoint.X, MousePoint.Y);
        //}

        public void Click(int MOUSE_BUTTON_PARAM = MOUSE_BUTTON.Left, int x = -1, int y = -1)
        {
            //yes, I know mouse_event is depreciated
            //if a more robust solution is needed, there are several on Github and Nuget:
            //https://github.com/search?q=InputSimulator+language%3AC%23&type=Repositories&ref=advsearch&l=C%23&l=
            //https://www.nuget.org/packages?q=InputSimulator

            uint EVENT_DOWN = 0;
            uint EVENT_UP = 0;

            switch (MOUSE_BUTTON_PARAM)
            {
                case MOUSE_BUTTON.Left:
                    EVENT_DOWN = Win32.MOUSEEVENTF_LEFTDOWN;
                    EVENT_UP = Win32.MOUSEEVENTF_LEFTUP;
                    break;
                case MOUSE_BUTTON.Middle:
                    EVENT_DOWN = Win32.MOUSEEVENTF_MIDDLEDOWN;
                    EVENT_UP = Win32.MOUSEEVENTF_MIDDLEUP;
                    break;
                case MOUSE_BUTTON.Right:
                    EVENT_DOWN = Win32.MOUSEEVENTF_RIGHTDOWN;
                    EVENT_UP = Win32.MOUSEEVENTF_RIGHTUP;
                    break;
                default:
                    EVENT_DOWN = Win32.MOUSEEVENTF_LEFTDOWN;
                    EVENT_UP = Win32.MOUSEEVENTF_LEFTUP;
                    break;
            }

            if (x >= 0 && y >= 0)
                Cursor.Position = new Point(x, y);

            Win32.mouse_event(EVENT_DOWN, 0, 0, 0, 0);
            Thread.Sleep(10);
            Win32.mouse_event(EVENT_UP, 0, 0, 0, 0);

            DoDelay(MouseDelay);
        }
        public bool Close(string WinTitle = "")
        {
            IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
            return Win32.PostMessage(new HandleRef(hWnd, GetID(WinTitle)), Win32.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
        public bool Exist(string WinTitle = "")
        {
            if (GetID(WinTitle) != IntPtr.Zero) { return true; } else { return false; }
        }
        //public IntPtr Find(string caption)
        //{
        //    return Win32.FindWindowByCaption(IntPtr.Zero, caption);
        //}
        //public IntPtr Find(string className = null, string winTitle = null)
        //{
        //    if (className == String.Empty)
        //        className = null;

        //    return Win32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, className, winTitle);
        //}
        public string Get(int GET_PARAM, string WinTitle = "")
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

            switch (GET_PARAM)
            {
                case GET.ID:
                    break;
            }

            if (GET_PARAM == GET.ID)
            {
                result = hWnd.ToString("X");
            } 
            else if (GET_PARAM == GET.PID)
            {
                result = pid.ToString("X");
            }
            else if (GET_PARAM == GET.ProcessName)
            {
                Win32.GetWindowThreadProcessId(hWnd, out pid);
                Process p = Process.GetProcessById(pid);
                result = p.ProcessName;
            }
            else if (GET_PARAM == GET.ProcessPath)
            {
                const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
                IntPtr hproc = Win32.OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
                StringBuilder buffer = new StringBuilder(Win32.MAX_PATH);
                Win32.GetModuleFileNameEx(hproc, IntPtr.Zero, buffer, buffer.Capacity);
                string processPath = buffer.ToString();
                result = processPath;
            }
            else if (GET_PARAM == GET.Style)
            {
                int style = Win32.GetWindowLong(hWnd, Win32.GWL_STYLE);
                result = style.ToString("X");
            }
            else if (GET_PARAM == GET.Disabled)
            {
                if (isDisabled) result = "1";
            }
            else if (GET_PARAM == GET.Exist)
            {
                if (isExist) result = "1";
            }
            else if (GET_PARAM == GET.Maximize)
            {
                if (isMax) result = "1";
            }
            else if (GET_PARAM == GET.Minimize)
            {
                if (isMin) result = "1";
            }
            else if (GET_PARAM == GET.Normal)
            {
                if (isNormal) result = "1";
            }
            else if (GET_PARAM == GET.Visible)
            {
                if (isVisible) result = "1";
            }
            return result;
        }
        public void GetActiveStats(string WinTitle, out int Width, out int Height, out int X, out int Y)
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;

            GetPos(out X, out Y, out Width, out Height, GetActiveTitle());
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
                _group	Window Group (use _list with less features instead)
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

        public Rectangle GetPos(string WinTitle)
        {
            //System.Drawing.Rectangle:
            //  R/W={X, Y, Width, Height, Size, Location}
            //  R/O={Left, Top, Right, Bottom, IsEmpty}
            //  Right:  The x-coordinate that is the sum of X and Width of this Rectangle.
            //  Bottom: The y-coordinate that is the sum of Y and Height of this Rectangle.
            //  can't return Right  = Width  - X (Right is R/O)
            //  can't return Bottom = Height - Y (Bottom is R/O)
            //  Right and Bottom are R/O, so convert Width and Height which are R/W:
            //  return Width  = Width  - X
            //  return Height = Height - Y

            IntPtr hWnd = GetID(WinTitle);

            Rectangle rect = new Rectangle();

            if (!Win32.GetWindowRect(hWnd, out rect))
                return Rectangle.Empty;

            Point point = new Point(rect.X, rect.Y);
            Size size = new Size(rect.Width - rect.X, rect.Height - rect.Y);
            rect = new Rectangle(point, size);

            return rect;
        }
        public void GetPos(out int X, out int Y, out int Width, out int Height, string WinTitle = "")
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;

            //System.Drawing.Rectangle:
            //  R/W={X, Y, Width, Height, Size, Location}
            //  R/O={Left, Top, Right, Bottom, IsEmpty}
            //  Right:  The x-coordinate that is the sum of X and Width of this Rectangle.
            //  Bottom: The y-coordinate that is the sum of Y and Height of this Rectangle.

            Rectangle rect = new Rectangle();

            IntPtr hWnd = GetID(WinTitle);

            if (!Win32.GetWindowRect(hWnd, out rect))
                return;
            
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width - rect.Left;
            Height = rect.Height - rect.Top;
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
        //public Rectangle GetRect(string WinTitle = "")
        //{
        //    //System.Drawing.Rectangle:
        //    //  R/W={X, Y, Width, Height, Size, Location}
        //    //  R/O={Left, Top, Right, Bottom, IsEmpty}
        //    //  Right:  The x-coordinate that is the sum of X and Width of this Rectangle.
        //    //  Bottom: The y-coordinate that is the sum of Y and Height of this Rectangle.

        //    IntPtr hWnd = GetID(WinTitle);

        //    Rectangle rect = new Rectangle();

        //    if (!Win32.GetWindowRect(hWnd, out rect))
        //        return Rectangle.Empty;

        //    return rect;
        //}
        public string GetText_UNIMPLEMENTED(string WinTitle)
        {
            return String.Empty;
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
        public void GetScreen(out int Left, out int Right, out int Top, out int Bottom, out int Width, out int Height)
        {
            Left = SystemInformation.VirtualScreen.Left;
            Right = SystemInformation.VirtualScreen.Right;
            Top = SystemInformation.VirtualScreen.Top;
            Bottom = SystemInformation.VirtualScreen.Bottom;
            Width = SystemInformation.VirtualScreen.Width;
            Height = SystemInformation.VirtualScreen.Height;
        }
        public Rectangle GetScreenRect()
        {
            int X = SystemInformation.VirtualScreen.Left;
            int Y = SystemInformation.VirtualScreen.Bottom;
            int W = SystemInformation.VirtualScreen.Width;
            int H = SystemInformation.VirtualScreen.Height;
            Rectangle rect = new Rectangle(X, Y, W, H);
            return rect;
        }
        public void Hide(string WinTitle = "")
        {
            Win32.ShowWindow(GetID(WinTitle), Win32.SW_HIDE);
            DoDelay(WinDelay);
        }
        public void HookAddKey(Keys key, bool suppress = false)
        {
            if (hookedKeysDict.ContainsKey(key))
                hookedKeysDict.Remove(key);

            hookedKeysDict.Add(key, suppress);
        }
        public void HookStart()
        {
            this.HookStart(true, true);
        }
        public void HookStart(bool InstallMouseHook, bool InstallKeyboardHook)
        {
            if (hMouseHook == 0 && InstallMouseHook)
            {
                MouseHookProcedure = new HookProc(MouseHookProc);
                hMouseHook = Win32.SetWindowsHookEx(
                    Win32.WH_MOUSE_LL,
                    MouseHookProcedure,
                    Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]), 0);
                if (hMouseHook == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    HookStop(true, false, false);
                    throw new System.ComponentModel.Win32Exception(errorCode);
                }
            }

            if (hKeyboardHook == 0 && InstallKeyboardHook)
            {
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                hKeyboardHook = Win32.SetWindowsHookEx(
                    Win32.WH_KEYBOARD_LL,
                    KeyboardHookProcedure,
                    Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]), 0);
                if (hKeyboardHook == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    HookStop(false, true, false);
                    throw new System.ComponentModel.Win32Exception(errorCode);
                }
            }
        }
        public void HookStop()
        {
            this.HookStop(true, true, true);
        }
        public void HookStop(bool UninstallMouseHook, bool UninstallKeyboardHook, bool ThrowExceptions)
        {
            if (hMouseHook != 0 && UninstallMouseHook)
            {
                int retMouse = Win32.UnhookWindowsHookEx(hMouseHook);
                hMouseHook = 0;
                if (retMouse == 0 && ThrowExceptions)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(errorCode);
                }
            }

            if (hKeyboardHook != 0 && UninstallKeyboardHook)
            {
                int retKeyboard = Win32.UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
                if (retKeyboard == 0 && ThrowExceptions)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(errorCode);
                }
            }
        }
        public string HotkeyMethod(int index)
        {
            try
            {
                return hotKeyList[index];
            }
            catch (Exception ex)
            {
                //throw new System.ComponentModel.Win32Exception(ex.Message.ToString());
            }
            return String.Empty;
        }
        public bool HotkeyRegister(IntPtr hWnd, Keys hotKey, uint modifiers, string hotKeyMethod)
        {
            try
            {
                Win32.RegisterHotKey(hWnd, hotKeyNumber, modifiers, (uint)hotKey);
                hotKeyList.Add(hotKeyMethod);
                hotKeyNumber++;
                return true;
            }
            catch (Exception ex)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(errorCode);

                //MessageBox.Show("ERROR HotKeyRegister: " + hotKey.ToString() + ", ex=" + ex.Message);
                //return false;
            }
        }
        
        public bool HotkeyUnRegisterAll(IntPtr hWnd)
        {
            bool result = true;

            for (int i = 0; i < hotKeyList.Count; i++)
            {
                try
                {
                    Win32.UnregisterHotKey(hWnd, i);
                }
                catch (Exception ex)
                {
                    result = false;
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(errorCode);
                    //MessageBox.Show("ERROR HotKeyUnregister Item: " + i);
                }
            }
            return result;
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
        public string[] InputBox(string Title, string Prompt, bool Hide, Point Location, 
            int TimeoutMilliSeconds = int.MaxValue, string Default = "", string IconFile = "")
        {
            //AHK_L_v1 ErrorLevel: 0=OK, 1=CANCEL, 2=Timeout
            //AHK_L_v2 Object IB: IB.Result, IB.Value

            //Result: "OK", "Cancel", "Timeout"
            //Value : User input, even if cancel or timeout
            string[] result = { "Result", "Value" };

            if (Title == String.Empty)
                Title = this.GetType().Name;// "CSharpHotkey";

            var startPos = FormStartPosition.Manual;
            if (Location.X < 0 || Location.Y < 0)
                startPos = FormStartPosition.CenterScreen;

            //AHK_L_v1 max timout = 2147483(24.8 days), int.MaxValue = 2147483647
            if (TimeoutMilliSeconds > int.MaxValue / 1000) TimeoutMilliSeconds = int.MaxValue / 1000;

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
            if (System.IO.File.Exists(IconFile)) form.Icon = new Icon(@"..\..\face-smile.ico");
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

            result[0] = dialogResult.ToString();
            result[1] = textBox.Text;

            if (timeout)
            {
                result[0] = "Timeout";
            }
            else
            {
                timer.Stop();
                timerHandle.Free();
                timer.Dispose();
            }
            return result;
        }
        public void KeyLock(Keys key, bool lockOn = true)
        {
            if (lockOn && Control.IsKeyLocked(key) || (!lockOn && !Control.IsKeyLocked(key)))
                return;

            Win32.keybd_event((byte)key, 0, Win32.KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            Win32.keybd_event((byte)key, 0, Win32.KEYEVENTF_EXTENDEDKEY | Win32.KEYEVENTF_KEYUP, (UIntPtr)0);
        }
        public bool KeyWait(Keys Key, string Options = "")
        {
            //Options:
            //  D     =  wait for key down else wait for key up (default). D or d case insensitive
            //  U     =  wait for key up
            //  Tn    =  timeoutSeconds, 0 = no wait, -1 = indefinite. T or t case insensitive
            //  Empty =  wait indefinitely for the specified key or mouse/joystick button to be physically released by the user.

            string KeyUpDown = "U";
            int timeoutSeconds = -1;

            const uint LSB = 1 << 0;    // least significant bit = 1
            const uint MSB = 1 << 15;   // most  significant bit = 32768

            Options = Options.ToUpper();

            if (Options.Contains("D"))
                KeyUpDown = "D";

            if (Options.Contains("T"))
            {
                int pos = Options.IndexOf("T");
                string timeString = Options.Substring(pos+1);
                Int32.TryParse(timeString, out timeoutSeconds);
            }

            //MessageBox.Show("timeoutSeconds=" + timeoutSeconds);

            uint start_time = Win32.GetTickCount();

            while (true)
            {
                DoDelay(10);

                short x = Win32.GetAsyncKeyState(Key); //must be Async

                if ((KeyUpDown == "D") && (x & MSB) == MSB)
                    return false;   //no timeout

                if ((KeyUpDown == "U") && (x & MSB) == 0)
                    return false;   //no timeout

                if (timeoutSeconds >= 0)
                {
                    if ((timeoutSeconds * 1000) - (Win32.GetTickCount() - start_time) <= 0)
                        return true;    //timeout
                }
            }
        }
        //public void Kill(string WinTitle = "")
        //{
        //    if (Exist(WinTitle))
        //    {
        //        Win32.SendMessage((IntPtr)GetID(WinTitle), Win32.WM_CLOSE, (IntPtr)null, (IntPtr)null);
        //    }
        //}
        public bool Kill(string WinTitle = "", int MilliSecondsToWait = 500)
        {
            if (MilliSecondsToWait < 1)
                MilliSecondsToWait = 500;

            bool result = false;

            if (!Exist(WinTitle))
                return result;

            Close(WinTitle);

            if (WaitClose(WinTitle, MilliSecondsToWait))
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
        public bool Minimize(string WinTitle = "")
        {
            bool result = Win32.ShowWindow(GetID(WinTitle), Win32.SW_MINIMIZE);
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
        public void MouseGetPos(out Point pt, out IntPtr hWnd)
        {
            Win32.GetCursorPos(out pt);

            Cursor cursor = new Cursor(Cursor.Current.Handle);
            hWnd = cursor.Handle;
        }
        public void MouseMove(Point pt)
        {
            Win32.SetCursorPos(pt.X, pt.Y);
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
        public Point PixelSearch(Rectangle ScreenRect,
            uint PixelColor, int PIXEL_MODE_PARAM = PIXEL_MODE.BGR,
            int Variation = 0)
        {
            //ScreenRect:   area of screen to search
            //PixelColor    if CSHK_PIXEL_MODE.RGB then PixelColor is RGB else BGR
            //Variation:    the allowable shades of color +/- 0 to 255 (false match if too high)

            if (ScreenRect == Rectangle.Empty)
                ScreenRect = Screen.PrimaryScreen.Bounds;

            //bitmap bytes are BGR. if CSHK_PIXEL_MODE.RGB then convert PixelColor from RGB to BGR
            if (PIXEL_MODE_PARAM == PIXEL_MODE.RGB)
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
        public void Send(string Keys, bool SendWait = true)
        {
            // See https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys
            // Difference is that all MOD keys must be in parenthesis
            // Example: Instead of %F   use %(F)    output is Alt-F
            // Example: Instead of +ec  use +(e)c   output is Ec

            void _DoSend(string _Keys, bool _SendWait = true)
            {
                if (SendWait)
                    SendKeys.SendWait(_Keys);
                else
                    SendKeys.Send(_Keys);
                DoDelay(KeyDelay);
            }

            if (Keys.Equals("flush", StringComparison.OrdinalIgnoreCase))
            {
                SendKeys.Flush();
                DoDelay(100);
                return;
            }

            bool actionKey = false;
            string actionString = String.Empty;

            bool modKey = false;
            string modString = String.Empty;

            bool openParanthesis = false;

            foreach (char ch in Keys)
            {

                if (ch == '}')
                {
                    actionKey = false;
                    actionString += ch;
                    _DoSend(actionString);
                    actionString = String.Empty;
                    continue;
                }

                if (ch == '{')
                {
                    actionKey = true;
                }

                if (actionKey)
                {
                    actionString += ch;
                    continue;
                }

                if (openParanthesis & ch == ')')
                {
                    openParanthesis = false;
                    modKey = false;
                    modString += ch;
                    _DoSend(modString);
                    modString = String.Empty;
                    continue;
                }

                if (modKey & openParanthesis)
                {
                    modString += ch;
                    continue;
                }

                if (ch == '+' | ch == '^' | ch == '%') //+shift|^control|%alt
                {
                    modKey = true;
                    modString += ch;
                    continue;
                }

                if (modKey & ch == '(')
                {
                    openParanthesis = true;
                    modString += ch;
                    continue;
                }
                _DoSend(ch.ToString());
            }
        }
        public bool Set(int SET_PARAM, string WinTitle = "")
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

            switch (SET_PARAM)
            {
                case SET.AlwaysOnTop:
                    hWndInsertAfter = (IntPtr)Win32.HWND_TOPMOST;
                    break;
                case SET.Bottom:
                    hWndInsertAfter = (IntPtr)Win32.HWND_BOTTOM;
                    break;
                case SET.Top:
                    hWndInsertAfter = (IntPtr)Win32.HWND_TOP;
                    break;
                case SET.NotTop:
                    hWndInsertAfter = (IntPtr)Win32.HWND_NOTTOPMOST;
                    break;
                case SET.Disable:
                    Win32.EnableWindow(hWnd, false);
                    Win32.UpdateWindow(hWnd);
                    break;
                case SET.Enable:
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
        public bool SetTitle(string NewTitle, string WinTitle = "")
        {
            //return Win32.SetWindowText(GetID(WinTitle), NewTitle);
            IntPtr hWnd = GetID(WinTitle);
            bool result = Win32.SetWindowText(hWnd, NewTitle);
            Win32.UpdateWindow(hWnd);
            DoDelay(WinDelay);
            return result;
        }
        public void SetTitleMatchMode(int MATCH_MODE_PARAM = 3, int MATCH_CASE_PARAM = 2) //default Exact, Sensitive
        {
            TitleMatchMode = MATCH_MODE_PARAM;
            TitleMatchModeCase = MATCH_CASE_PARAM;
        }
        public void SetTitleMatchMode(string mode = "", string modeCase = "")
        {
            switch (mode)
            {
                case "Contains":
                    TitleMatchMode = MATCH_MODE.Contains;
                    break;
                case "EndsWith":
                    TitleMatchMode = MATCH_MODE.EndsWith;
                    break;
                case "Exact":
                    TitleMatchMode = MATCH_MODE.Exact;
                    break;
                case "StartsWith":
                    TitleMatchMode = MATCH_MODE.StartsWith;
                    break;
                default:
                    TitleMatchMode = MATCH_MODE.Exact;
                    break;
            }
            switch (modeCase)
            {
                case "Sensitive":
                    TitleMatchModeCase = MATCH_CASE.Sensitive;
                    break;
                case "InSensitive":
                    TitleMatchModeCase = MATCH_CASE.InSensitive;
                    break;
                default:
                    TitleMatchModeCase = MATCH_CASE.Sensitive;
                    break;
            }
        }
        public void SetKeyDelay(int milliSeconds = 100)
        {
            //Time in milliSeconds: -1 for no delay, 0 for the smallest possible delay, 100 is the default.

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
        public void Sleep(int timeMilliseconds)
        {
            if (timeMilliseconds < 0) return;
            DoDelay(timeMilliseconds);
            //System.DateTime waitTime = System.DateTime.Now.AddMilliseconds(timeMilliseconds);
            //while (System.DateTime.Now < waitTime) { System.Windows.Forms.Application.DoEvents(); }
        }
        public void SoundBeep(int frequency = 523, int duration = 150)
        {
            Console.Beep(frequency, duration);
        }
        public void WaitCancel_TODO()
        {
            /* Cancel Wait loops
             * CancelWait = true;
             * if (CancelWait) { CancelWait = false; return true; }
            */
            

        }
        public bool Wait(string WinTitle, double timeoutSeconds = -1)
        {
            //timeoutSeconds: How many seconds to wait before timing out
            //Omit to allow the command to wait indefinitely
            //Specifying 0 is the same as specifying 0.5

            if (timeoutSeconds == 0) { timeoutSeconds = 0.5; }

            uint start_time = Win32.GetTickCount();

            while (true)
            {
                DoDelay(10);

                if (Exist(WinTitle))
                    return false;   //no timeout

                if (timeoutSeconds >= 0)
                {
                    if ((timeoutSeconds * 1000) - (Win32.GetTickCount() - start_time) <= 0)
                        return true;    //timeout
                }
            }
        }
        public bool WaitActive(string WinTitle, double timeoutSeconds = -1)
        {
            //timeoutSeconds: How many seconds to wait before timing out
            //Omit to allow the command to wait indefinitely
            //Specifying 0 is the same as specifying 0.5

            if (timeoutSeconds == 0) { timeoutSeconds = 0.5; }
            
            uint start_time = Win32.GetTickCount();

            while (true)
            {
                DoDelay(10);

                if (GetID(WinTitle) == Win32.GetForegroundWindow())
                    return false;   //no timeout

                if (timeoutSeconds >= 0)
                {
                    if ((timeoutSeconds * 1000) - (Win32.GetTickCount() - start_time) <= 0)
                        return true;    //timeout
                }
            }
        }
        //public bool WaitNotActive_TODO(string WinTitle, double timeoutSeconds = -1)
        //{
        //    //timeoutSeconds: How many seconds to wait before timing out
        //    //Omit to allow the command to wait indefinitely
        //    //Specifying 0 is the same as specifying 0.5

        //    if (timeoutSeconds == 0) { timeoutSeconds = 0.5; }

        //    uint start_time = Win32.GetTickCount();

        //    while (true)
        //    {
        //        DoDelay(10);

        //        if (GetID(WinTitle) != Win32.GetForegroundWindow())
        //            return false;   //no timeout

        //        if (timeoutSeconds >= 0)
        //        {
        //            if ((timeoutSeconds * 1000) - (Win32.GetTickCount() - start_time) <= 0)
        //                return true;    //timeout
        //        }
        //    }
        //}
        public bool WaitClose(string WinTitle, double timeoutSeconds = -1)
        {
            //timeoutSeconds: How many seconds to wait before timing out
            //Omit to allow the command to wait indefinitely
            //Specifying 0 is the same as specifying 0.5

            if (timeoutSeconds == 0) { timeoutSeconds = 0.5; }
            
            uint start_time = Win32.GetTickCount();

            while (true)
            {
                DoDelay(10);

                if (!IsWindow(WinTitle))   // It's closed, so we're done.
                    return false;       //no timeout

                if (timeoutSeconds >= 0)
                {
                    if ((timeoutSeconds * 1000) - (Win32.GetTickCount() - start_time) <= 0)
                        return true;    //timeout
                }
            }
        }
        public bool WaitNotActive(string WinTitle, double timeoutSeconds = -1)
        {
            //timeoutSeconds: How many seconds to wait before timing out
            //Omit to allow the command to wait indefinitely
            //Specifying 0 is the same as specifying 0.5

            if (timeoutSeconds == 0) { timeoutSeconds = 0.5; }

            uint start_time = Win32.GetTickCount();

            while (true)
            {
                DoDelay(10);

                if (GetID(WinTitle) != Win32.GetForegroundWindow())
                    return false; //no timeout

                if (timeoutSeconds >= 0)
                {
                    if ((timeoutSeconds * 1000) - (Win32.GetTickCount() - start_time) <= 0)
                        return true;    //timeout
                }
            }
        }
        public Rectangle WaitImageSearch(string ImageFile, Rectangle ScreenRect, int Variation = 0, double TimeoutSeconds = -1)
        {
            //timeoutSeconds: omit=indefinite, 0=0.5
            //Return: Timeout=Rectangle.Empty

            if (TimeoutSeconds == 0) { TimeoutSeconds = 0.5; }

            uint start_time = Win32.GetTickCount();

            while (true)
            {
                DoDelay(10);

                Rectangle rect = ImageSearch(ImageFile, ScreenRect, Variation);

                if (rect.X != 0 & rect.Y != 0)
                    return rect;               //no timeout

                if (TimeoutSeconds >= 0)
                {
                    if ((TimeoutSeconds * 1000) - (Win32.GetTickCount() - start_time) <= 0)
                        return Rectangle.Empty; //timeout
                }
            }
        }
        public Rectangle WaitImageSearch(Bitmap ImageBmp, Rectangle ScreenRect, int Variation = 0, double TimeoutSeconds = -1)
        {
            //timeoutSeconds: omit=indefinite, 0=0.5
            //Return: Timeout=Rectangle.Empty

            if (TimeoutSeconds == 0) { TimeoutSeconds = 0.5; }

            uint start_time = Win32.GetTickCount();

            Rectangle rect = Rectangle.Empty;

            while (true)
            {
                DoDelay(100);

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

                if (TimeoutSeconds >= 0)
                {
                    if ((TimeoutSeconds * 1000) - (Win32.GetTickCount() - start_time) <= 0)
                        return Rectangle.Empty; //timeout
                }
            }
        }
        public Point WaitPixelSearch(string WinTitle, uint PixelColor, double TimeoutSeconds = -1)
        {
            //timeoutSeconds: omit=indefinite, 0=0.5
            //Return: Timeout=Point.Empty

            if (TimeoutSeconds == 0) { TimeoutSeconds = 0.5; }

            uint start_time = Win32.GetTickCount();

            while (true)
            {
                DoDelay(10);

                Rectangle rect = GetPos(WinTitle);

                Point p = (PixelSearch(rect, PixelColor));

                if (p.X != 0 & p.Y != 0)
                    return p;               //no timeout

                if (TimeoutSeconds >= 0)
                {
                    if ((TimeoutSeconds * 1000) - (Win32.GetTickCount() - start_time) <= 0)
                        return Point.Empty; //timeout
                }
            }
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

            if (TitleMatchModeCase == MATCH_CASE.InSensitive)
            {
                comparisonType = StringComparison.OrdinalIgnoreCase;
            }

            if (TitleMatchMode == MATCH_MODE.StartsWith)
            {
                if (haystack.StartsWith(needle, comparisonType)) { result = true; }
            }
            else if (TitleMatchMode == MATCH_MODE.Contains)
            {
                if (comparisonType == StringComparison.OrdinalIgnoreCase)
                {
                    haystack = haystack.ToUpper();
                    needle = needle.ToUpper();
                }
                if (haystack.Contains(needle)) { result = true; }
            }
            else if (TitleMatchMode == MATCH_MODE.Exact)
            {
                if (haystack.Equals(needle, comparisonType)) { result = true; }
            }
            else if (TitleMatchMode == MATCH_MODE.EndsWith)
            {
                if (haystack.EndsWith(needle, comparisonType)) { result = true; }
            }
            return result;
        }
        private bool IsParent(IntPtr hWnd)
        {
            IntPtr foundHWND = Win32.FindWindowEx(hWnd, IntPtr.Zero, null, null);
            return foundHWND != IntPtr.Zero;
        }
        private bool IsWindow(string WinTitle = "")
        {
            return Win32.IsWindow(GetID(WinTitle));
        }
        public bool IsVisible(string WinTitle = "")
        {
            return Win32.IsWindowVisible(GetID(WinTitle));
        }
        private void WriteLog(string Text, string LogFile = "", bool OverWrite = true)
        {
            return; //uncomment to disable

            if (LogFile == String.Empty) LogFile = this.GetType().Name + ".log";

            if (OverWrite) File.Delete(LogFile);

            const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

            using (StreamWriter writer = new StreamWriter(LogFile, true, Encoding.UTF8))
            {
                writer.WriteLine(DateTime.Now.ToString(dateTimeFormat) + ": " + Text);
            }
        }
        #endregion Private Methods

        #region Private Hook Methods
        private int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (OnMouseActivity != null))
            {
                MouseLLHookStruct mouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));

                MouseButtons button = MouseButtons.None;
                short mouseDelta = 0;
                bool isButtonClick = false;

                switch (wParam)
                {
                    case Win32.WM_LBUTTONDOWN:
                        //case WM_LBUTTONUP: 
                        //case WM_LBUTTONDBLCLK: 
                        button = MouseButtons.Left;
                        isButtonClick = true;
                        break;
                    case Win32.WM_RBUTTONDOWN:
                        //case WM_RBUTTONUP: 
                        //case WM_RBUTTONDBLCLK: 
                        button = MouseButtons.Right;
                        isButtonClick = true;
                        break;
                    case Win32.WM_MBUTTONDOWN:
                        //case WM_RBUTTONUP: 
                        //case WM_RBUTTONDBLCLK: 
                        button = MouseButtons.Middle;
                        isButtonClick = true;
                        break;
                    case Win32.WM_MOUSEWHEEL:
                        //If the message is WM_MOUSEWHEEL, the high-order word of mouseData member is the wheel delta. 
                        //One wheel click is defined as WHEEL_DELTA, which is 120. 
                        //(value >> 16) & 0xffff; retrieves the high-order word from the given 32-bit value
                        mouseDelta = (short)((mouseHookStruct.mouseData >> 16) & 0xffff);
                        //X BUTTONS (I havent them so was unable to test)
                        //If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP, 
                        //or WM_NCXBUTTONDBLCLK, the high-order word specifies which X button was pressed or released, 
                        //and the low-order word is reserved. This value can be one or more of the following values. 
                        //Otherwise, mouseData is not used. 
                        button = MouseButtons.None;
                        isButtonClick = false;
                        break;
                    case Win32.WM_MOUSEMOVE:
                        //pass the mouseHookStruct to the code below
                        break;
                    default:
                        break;
                }

                //tempString = ""; //"wParam: " + wParam.ToString("X") + ", isButtonClick: " + isButtonClick;

                //check for single or double button click
                if (isButtonClick)
                {
                    TimeSpan ts = DateTime.Now - DBLCLK.clickedTime;

                    if (ts.TotalMilliseconds <= SystemInformation.DoubleClickTime)
                    {
                        if ((button == DBLCLK.mouseButton) &&
                            (mouseHookStruct.pt.x == DBLCLK.x) & (mouseHookStruct.pt.y == DBLCLK.y))
                            DBLCLK.clickCount = 2;
                    }
                    else
                    {
                        DBLCLK.mouseButton = button;
                        DBLCLK.clickCount = 1;
                        DBLCLK.clickedTime = DateTime.Now;
                        DBLCLK.x = mouseHookStruct.pt.x;
                        DBLCLK.y = mouseHookStruct.pt.y;
                    }
                    //tempString += ", clickCount: " + clickCount.ToString();
                }

                MouseEventArgs e = new MouseEventArgs(
                                                   button,
                                                   DBLCLK.clickCount,
                                                   mouseHookStruct.pt.x,
                                                   mouseHookStruct.pt.y,
                                                   mouseDelta);
                OnMouseActivity(this, e);

                if (MouseEventHandledFlag)
                    return 1;
            }
            return Win32.CallNextHookEx(hMouseHook, nCode, wParam, lParam);
        }
        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (KeyDown != null || KeyUp != null || KeyPress != null))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                Keys keyData = key | Control.ModifierKeys;

                DebugString = ""; //"KeyData: " + keyData + ", ModifierKeys: " + Control.ModifierKeys.ToString("X");

                KeyEventArgs e = new KeyEventArgs(keyData);
                //ok DebugString = "KeyData: " + keyData + ", hookedKeysDict: " + hookedKeysDict[keyData];                

                if (hookedKeysDict.ContainsKey(key))
                {
                    //return Keys value e.g. Keys.Escape
                    if (KeyDown != null && (wParam == Win32.WM_KEYDOWN || wParam == Win32.WM_SYSKEYDOWN))
                    {
                        KeyDown(this, e);
                    }
                    //return ASCII key value including lower/upper case
                    if (KeyPress != null && wParam == Win32.WM_KEYDOWN)
                    {
                        if ((vkCode >= (int)Keys.D0 && vkCode <= (int)Keys.Z)
                            || (vkCode == (int)Keys.Space | vkCode == (int)Keys.Back))
                        {
                            char keyChar = (char)vkCode;

                            bool lockCaps = Control.IsKeyLocked(Keys.CapsLock);
                            bool modShift = Control.ModifierKeys == Keys.Shift;

                            if (!lockCaps && !modShift)
                                keyChar = Char.ToLower(keyChar);

                            KeyPressEventArgs kpArg = new KeyPressEventArgs(keyChar);
                            KeyPress(this, kpArg);
                        }
                    }
                    if (KeyUp != null && (wParam == Win32.WM_KEYUP || wParam == Win32.WM_SYSKEYUP))
                    {
                        KeyUp(this, e);
                    }

                    if (hookedKeysDict[key])
                        return 1; //1 = handled = supress
                }
            }
            return Win32.CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }

        #endregion


        #region Win32
        private class Win32
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true)] public static extern bool CloseWindow(IntPtr hWnd);
            [DllImport("user32.dll")] public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

            [DllImport("user32.dll", SetLastError = true)] public static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);
            [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)] public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
            [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);
            [DllImport("user32.dll")] public static extern IntPtr GetActiveWindow();
            [DllImport("user32.dll")] public static extern IntPtr GetAncestor(IntPtr hwnd, int flags);
            [DllImport("user32.dll")] public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);
            [DllImport("user32.dll", SetLastError = true)] public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
            [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetCursorPos(out System.Drawing.Point lpPoint); [DllImport("user32.dll")] public static extern IntPtr GetDesktopWindow(); 
            [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
            [DllImport("user32")] public static extern int GetKeyboardState(byte[] pbKeyState);
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)] public static extern short GetKeyState(int vKey);
            [DllImport("psapi.dll")] public static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, uint nSize);
            [DllImport("psapi.dll")] public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] int nSize); 
            [DllImport("user32.dll")] public static extern IntPtr GetParent(IntPtr hWnd);
            [DllImport("USER32.DLL")] public static extern IntPtr GetShellWindow();
            [DllImport("kernel32.dll")] public static extern uint GetTickCount();
            [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);
            [DllImport("user32.dll", SetLastError = true)] public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
            [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetWindowRect(IntPtr hWnd, out Rectangle lpRect);
            [DllImport("USER32.DLL")] public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
            [DllImport("USER32.DLL")] public static extern int GetWindowTextLength(IntPtr hWnd);
            [DllImport("user32.dll", SetLastError = true)] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId); [DllImport("user32.dll")] public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase); 
            [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr hWnd);
            [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr hWnd);
            [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
            [DllImport("user32.dll")] public static extern bool IsZoomed(IntPtr hWnd);
            [DllImport("user32.dll")] public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo); [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInf);
            [DllImport("User32.dll")] public static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);
            [DllImport("kernel32.dll", SetLastError = true)] public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId); 
            [DllImport("user32.dll", SetLastError = true)] public static extern bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
            [DllImport("user32.dll")] public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, int flags);
            [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
            [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
            [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
            [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [Out] StringBuilder lParam);
            [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool SetCursorPos(int x, int y);
            [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessageTimeout(
                IntPtr windowHandle,
                uint Msg,
                IntPtr wParam,
                IntPtr lParam,
                int flags,
                uint timeout,
                out IntPtr result);

            [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr SetActiveWindow(IntPtr hWnd);
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)] public static extern bool SetWindowText(IntPtr hwnd, String lpString);
            [DllImport("user32.dll", SetLastError = true)] public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
            [DllImport("user32.dll", SetLastError = true)] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
            
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

            [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
            [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern int UnhookWindowsHookEx(int idHook);

            [DllImport("user32.dll")] public static extern bool UpdateWindow(IntPtr hWnd);

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
        private class Win32
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, Modifiers fsModifiers, Keys vk);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        }
        public enum Modifiers
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8,
            ControlAlt = Control + Alt //custom
        }

        private const int WM_HOTKEY = 0x0312;

        private int HotKeyID { get; set; }

        public event Action<HotKey> HotKeyAction;

        private event Action<HotKey> onKeyAction;

        public HotKey() { }
        public HotKey(Modifiers Modifiers, Keys Key = Keys.None, Action<HotKey> OnKeyAction = null)
        {
            Register(Modifiers, Key, OnKeyAction);
        }
        public bool Register(Modifiers Modifiers = Modifiers.None, Keys Key = Keys.None, Action<HotKey> OnKeyAction = null)
        {
            if (Key == Keys.None || OnKeyAction == null)
                return false;

            HotKeyAction += OnKeyAction;

            this.onKeyAction = OnKeyAction;

            Application.AddMessageFilter(this);

            HotKeyID = Key.GetHashCode() + Modifiers.GetHashCode();

            return Win32.RegisterHotKey(IntPtr.Zero, HotKeyID, Modifiers, Key);
        }
        public bool UnRegister()
        {
            if (onKeyAction != null)
                HotKeyAction -= onKeyAction;

            Application.RemoveMessageFilter(this);
            return Win32.UnregisterHotKey(IntPtr.Zero, HotKeyID);
        }
        ~HotKey()
        {
            UnRegister();
        }
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && (int)m.WParam == HotKeyID)
            {
                OnHotKeyAction();
                return true;
            }
            return false;
        }
        private void OnHotKeyAction()
        {
            HotKeyAction?.Invoke(this);
            return;

            //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new Action(() => HotKeyAction?.Invoke(this)));
        }
    }

}