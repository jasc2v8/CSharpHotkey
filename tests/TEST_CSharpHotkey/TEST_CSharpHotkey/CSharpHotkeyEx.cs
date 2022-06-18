//
//Add Extended or Custom functions here
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CSharpHotkeyLib
{
    public partial class CSharpHotkey
    {

        public List<string> GetWindowsList() //not AHK
        {
            List<string> windowsList = new List<string>();

            string windowTitle = String.Empty;

            Win32.EnumWindows(delegate (IntPtr hWnd, int lParam)
            {

                if (!Win32.IsWindowVisible(hWnd))
                    return true;

                int length = Win32.GetWindowTextLength(hWnd);

                if (length != 0)
                {
                    StringBuilder sb = new StringBuilder(length);

                    if (Win32.GetWindowText(hWnd, sb, length + 1) != 0)
                        windowTitle = sb.ToString();

                    if (!windowsList.Contains(windowTitle) &&
                        windowTitle != "Program Manager" &&
                        windowTitle != "Windows Shell Experience Host")
                        windowsList.Add(windowTitle);
                }
                return true; //true=continue enum loop
            }, 0);

            return windowsList;
        }
        public List<string> GetDesktopWindowsList() //not AHK
        {

            List<string> windowsList = new List<string>();

            string windowTitle = String.Empty;

            Win32.EnumWindows(delegate (IntPtr hWnd, int lParam)
            {

                //skip if not visible
                int style = Win32.GetWindowLong(hWnd, Win32.GWL_STYLE);
                if ((style & Win32.WS_VISIBLE) == 0)
                    return true;

                int length = Win32.GetWindowTextLength(hWnd);

                if (length++ > 0 && Win32.IsWindowVisible(hWnd))
                {

                    StringBuilder sb = new StringBuilder(length);

                    Win32.GetWindowText(hWnd, sb, length);
                    windowTitle = sb.ToString();

                    if (windowTitle != "Program Manager" && windowTitle != "Windows Shell Experience Host")
                    {
                        int pid;
                        Win32.GetWindowThreadProcessId(hWnd, out pid);

                        var parent = Win32.GetParent(hWnd);
                        if (parent == IntPtr.Zero)
                        {
                            windowsList.Add(windowTitle);
                        }
                        else
                        {
                            var proc = Process.GetProcessById((int)pid);
                            if (proc.MainWindowTitle == "")
                            {
                                windowsList.Add(windowTitle);
                            }
                        }
                    }

                    //if ((windowTitle != String.Empty) & (!windowsList.Contains(windowTitle)))                    windowsList.Add(windowTitle);
                }
                return true; //true=continue enum loop
            }, 0);

            return windowsList;
        }   
    }
}