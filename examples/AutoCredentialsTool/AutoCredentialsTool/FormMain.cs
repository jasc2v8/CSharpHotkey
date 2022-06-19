//1.0.7.0   CSharpHotkey v0.0.7

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Principal;
using System.Windows.Forms;
using Cursors = System.Windows.Forms.Cursors;

using UTIL;
using CSharpHotkeyLib;

namespace AutoCredentialsTool
{
    public partial class FormMain : Form
    {
        private bool USE_LOG_FILE = true;

        private const int KEYDELAY = 175; //default=10, type slower to avoid security detection

        CSharpHotkey Win = new CSharpHotkey();

        //iniFile set for R/W access by Inno setup
        private static string iniPath = "act.ini";

        IniFile ini = new IniFile(iniPath);

        //TODO modify for your configuration
        string VPN_EXE = @"C:\Program Files\Aruba Networks\Virtual Internet Agent\anuacui.exe";
        string OUTLOOK_EXE = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Microsoft Office 2013\Outlook 2013.lnk";

        //option to load external image
        private string ImageBmpFilename = String.Empty; //@"..\..\Resources\vpn_cancel.png";

        //log file not installed by Inno, therefore no R/W access in Program Files, Use My Documents instead
        private static string logPath = 
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\act.log";

        private LogFile log = new LogFile(logPath);
        private class Win32
        {
            [DllImport("user32.dll")]
            public static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

            public const int EWX_LOGOFF = 0x00000000;
            public const int EWX_SHUTDOWN = 0x00000001;
            public const int EWX_REBOOT = 0x00000002;
            public const int EWX_FORCE = 0x00000004;
            public const int EWX_POWEROFF = 0x00000008;
            public const int EWX_FORCEIFHUNG = 0x00000010;
            public const int SHTDN_REASON_MAJOR_OTHER = 0x00000000;
        }

        private bool RestartingAsAdmin = false;
        private bool CloseForm = false;

        Form formEdit = new FormEdit();
        public FormMain()
        {
            InitializeComponent();
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //allow form to close
            if ( RestartingAsAdmin | CloseForm )
            {
                SaveSettings();
                return;
            }

            //hide instead of close
            e.Cancel = true;
            Hide();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            //TODO remove this when ready
            string msg = "IMPORTANT NOTICE" + "\r\n\r\n" +
                "This example will NOT run on your PC!" + "\r\n\r\n" +
                "You must mofify the code for your VPN and Outlook configurations";

            MessageBox.Show(msg, "AutoCredentialTool Example");

            //TODO enable start after you've modified this code to run on your PC
            buttonStart.Enabled = false;
            
            this.Text += " v" + typeof(FormMain).Assembly.GetName().Version;

            Win.SetTitleMatchMode(MatchMode.Contains, MatchCase.InSensitive);

            notifyIcon1.Visible = true;

            if (ini.GetValue("START_VPN") == "True")
                checkBoxVPN.Checked = true;

            if (ini.GetValue("START_OUTLOOK") == "True")
                checkBoxOutlook.Checked = true;

            if (ini.GetValue("AUTO_HIDE") == "True")
                checkBoxAutoHide.Checked = true;

            if (ini.GetValue("LOG_FILE") == "True")
                USE_LOG_FILE = true;

            HotKey hotkeyControlAltU = new HotKey(Keys.Control | Keys.Alt, Keys.U, (param) =>
            {
                Win.SetKeyDelay(KEYDELAY);
                Win.KeyWait(Keys.P, Keys.Up, 5);
                Win.Sleep(500); //debounce
                Win.Send(Base64.Decode(ini.GetValue("USERNAME")));
                Win.Send("{ENTER}");
            });

            HotKey hotkeyControlAltP = new HotKey(Keys.Control | Keys.Alt, Keys.P, (param) =>
            {
                Win.SetKeyDelay(KEYDELAY);
                Win.KeyWait(Keys.P, Keys.Up, 5);
                Win.Sleep(500); //debounce
                Win.Send(Base64.Decode(ini.GetValue("PASSWORD")));
                Win.Send("{ENTER}");
            });

            //TODO uncomment this once you are ready
            //if (ini.GetValue("USERNAME") == String.Empty | ini.GetValue("PASSWORD") == String.Empty)
            //{
            //    formEdit.ShowDialog();
            //    ini.Reload();
            //}

        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
            buttonStart.Enabled = true;
            SaveSettings();
            Hide();
        }
        private void buttonDisplayOff_Click(object sender, EventArgs e)
        {
            SaveSettings();

            int WM_SYSCOMMAND = 0x0112;
            int SC_MONITORPOWER = 0xF170;
            int MonitorState = 2; //On = -1, Off = 2, StandBy = 1

            var choice = MessageBox.Show("Display Off - Are you sure?" +
                "\n\n(3 second delay to settle mouse)",
                FormMain.ActiveForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choice == DialogResult.Yes)
            {
                Thread.Sleep(3000);
                Win32.SendMessage(this.Handle.ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, MonitorState);
            }
        }
        private void buttonRestart_Click(object sender, EventArgs e)
        {
            SaveSettings();

            var choice = MessageBox.Show("Restart - Are you sure?",
                FormMain.ActiveForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choice == DialogResult.Yes)
            {
                Process.Start("shutdown", "/r /t 0");
            }
        }
        private void buttonRestartUEFI_Click(object sender, EventArgs e)
        {
            SaveSettings();

            if (UserIsAdmin())
            {
                var choice = MessageBox.Show("Restart to UEFI - Are you sure?",
                 FormMain.ActiveForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (choice == DialogResult.Yes)
                {
                    Process.Start("shutdown", "/r /t 0 /fw");
                }
            }
            else
            {
                var choice = MessageBox.Show("Press OK to Restart as Admin\n\nThen Press the [Restart to UEFI] Button again.",
                    FormMain.ActiveForm.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                if (choice == DialogResult.OK)
                {
                    RestartWithElevatedPrivileges();
                }
            }
        }
        private void buttonRestartRE_Click(object sender, EventArgs e)
        {
            SaveSettings();

            var choice = MessageBox.Show("Restart to RE - Are you sure?",
                FormMain.ActiveForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choice == DialogResult.Yes)
            {
                Process.Start("shutdown", "/r /o /t 0");
            }
        }
        private void buttonSettings_Click(object sender, EventArgs e)
        {
            SaveSettings();

            Process.Start("ms-settings:powersleep");
        }
        private void buttonShutdown_Click(object sender, EventArgs e)
        {
            SaveSettings();

            var choice = MessageBox.Show("Shutdown - Are you sure?",
                FormMain.ActiveForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choice == DialogResult.Yes)
            {
                Process.Start("shutdown", "/s /t 0");
            }
        }
        private void buttonSignOut_Click(object sender, EventArgs e)
        {
            SaveSettings();

            var choice = MessageBox.Show("Sign Out - Are you sure?",
                FormMain.ActiveForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choice == DialogResult.Yes)
            {
                Win32.ExitWindowsEx(Win32.EWX_LOGOFF + Win32.EWX_FORCE, Win32.SHTDN_REASON_MAJOR_OTHER);
            }
        }
        private void buttonSleep_Click(object sender, EventArgs e)
        {
            SaveSettings();

            var choice = MessageBox.Show("Sleep - Are you sure?",
                FormMain.ActiveForm.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choice == DialogResult.Yes)
            {
                const bool force = true, disableWakeEvent = true;

                Application.SetSuspendState(PowerState.Suspend,
                    force, disableWakeEvent);
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {

            SaveSettings();

            if (USE_LOG_FILE)
            {
                log.Clear();
                log.Debug("Log cleared.");
            }

            WaitCursor(true);

            bool startedVPN = false;
            bool startedOutlook = false;

            if (checkBoxVPN.Checked)
            {
                string WinTitle = "Virtual Intranet Access";

                toolStripStatusLabel.Text = "Starting VPN...";

                if (!Win.Exist(WinTitle))
                {
                    if (File.Exists(VPN_EXE))
                    {
                        Process.Start(VPN_EXE);
                    }
                    else
                    {
                        WaitCursor(false);
                        toolStripStatusLabel.Text = "Error starting VPN executable.";
                        return;
                    }
                }

                toolStripStatusLabel.Text = "Waiting for VPN window to open....";

                Win.Wait(WinTitle, 10);

                Win.Activate();

                toolStripStatusLabel.Text = "Waiting for VPN window active...";

                if (Win.WaitActive("", 10))
                {
                    WaitCursor(false);
                    toolStripStatusLabel.Text = "TIMEOUT Waiting for VPN window active...";
                    return;
                }

                toolStripStatusLabel.Text = "Click VPN button to connect...";

                Rectangle screenRect = Win.GetPos(WinTitle);

                Win.Click(MouseButton.Left, screenRect.X + (screenRect.Width / 2), screenRect.Y + (screenRect.Height / 2));

                toolStripStatusLabel.Text = "Waiting for VPN PIN prompt...";

                Bitmap ImageBmp;
                if (ImageBmpFilename == String.Empty)
                    ImageBmp = new Bitmap(Properties.Resources.vpn_PIN);
                else
                    ImageBmp = new Bitmap(ImageBmpFilename);
                Rectangle imageRect = Win.WaitImageSearch(ImageBmp, screenRect, 0, 30);
                ImageBmp.Dispose();

                Rectangle winRect = Win.GetPos(WinTitle);

                //convert image to screen coordinates for mouse move/click
                int mouseX = imageRect.X + winRect.X;
                int mouseY = imageRect.Y + winRect.Y;

                if (imageRect == Rectangle.Empty)
                {
                    WaitCursor(false);
                    toolStripStatusLabel.Text = "TIMEOUT Waiting for VPN PIN prompt...";
                    return;
                }
                else
                {
                    Win.Click(MouseButton.Left, mouseX, mouseY);
                }

                toolStripStatusLabel.Text = "Sending VPN PIN...";

                string vpnPIN = ini.GetValue("PASSWORD");
                
                vpnPIN = Base64.Decode(vpnPIN);

                Win.Sleep(100);
                Win.SetKeyDelay(175);
                Win.Send(vpnPIN);
                Win.Send("{ENTER}");

                toolStripStatusLabel.Text = "Waiting for VPN window to close...";

                if (Win.WaitClose(WinTitle, 30))
                {
                    WaitCursor(false);
                    toolStripStatusLabel.Text = "TIMEOUT Waiting for VPN window to close...";
                    return;
                }
                startedVPN = true;
            }

            if (checkBoxOutlook.Checked)
            {
                toolStripStatusLabel.Text = "Starting Outlook...";

                if (File.Exists(OUTLOOK_EXE))
                {
                    Process.Start(OUTLOOK_EXE);
                    startedOutlook = true;
                }
                else
                {
                    WaitCursor(false);
                    toolStripStatusLabel.Text = "Error starting Outlook executable.";
                }
            }

            WaitCursor(false);

            toolStripStatusLabel.Text = "VPN Started: " + startedVPN + ", Outlook Started: " + startedOutlook;

            if (checkBoxAutoHide.Checked)
                Hide();

        }
        private void ContextMenuItemEdit_Click(object sender, EventArgs e)
        {
            //formEdit.Show();      //non-modal, can click on the parent form.
            formEdit.ShowDialog();  //model, cannot click on the parent form.
            ini.Reload();
        }
        private void ContextMenuItemExit_Click(object sender, EventArgs e)
        {
            CloseForm = true;
            Application.Exit();
            return;
        }
        private void ContextMenuItemOpen_Click(object sender, EventArgs e)
        {
            Show();
            Win.Activate(this.Text);
        }
        private void notifyIcon1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //handle left click only. right click is handled by the context menu
            if (e.Button == MouseButtons.Left)
            {
                Show();
                Win.Activate(this.Text);
            }
        }
        private void RestartWithElevatedPrivileges()
        {
            //Console:
            //string programpath = new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            //Windows Forms:
            string programpath = Application.ExecutablePath;

            ProcessStartInfo startinfo = new ProcessStartInfo
            {
                FileName = programpath,
                UseShellExecute = true,
                Verb = "runas",
                Arguments = " "
            };

            Process.Start(startinfo);
            //Console:
            //System.Environment.Exit(0); // return code 0, change if required
            //Windows Forms:
            RestartingAsAdmin = true;
            Application.Exit( );
        }
        public static bool UserIsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public void SaveSettings()
        {
            ini.SetValue("START_VPN", checkBoxVPN.Checked.ToString());
            ini.SetValue("START_OUTLOOK", checkBoxOutlook.Checked.ToString());
            ini.SetValue("AUTO_HIDE", checkBoxAutoHide.Checked.ToString());
        }
        public void WaitCursor(bool WaitCursorOn)
        {
            if (WaitCursorOn)
            {
                Cursor = Cursors.WaitCursor;
                buttonStart.Enabled = false;
            }
            else
            {
                Cursor = Cursors.Default;
                buttonStart.Enabled = true;
            }
        }
        public void WriteLog(string text)
        {
            if (USE_LOG_FILE)
                log.WriteLine(text);
        }

    }
}
