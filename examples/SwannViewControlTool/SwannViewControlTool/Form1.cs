using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using CSharpHotkeyLib;

namespace SwannViewControlTool
{
    public partial class Form1 : Form
    {

        private const string LOGIN_WINDOW_TITLE = "Search & Login";
        private const string MYDVR_WINDOW_TITLE ="SwannView Link";
        private const string EXE_NAME = @"C:\Program Files (x86)\SwannView Link\MyDVR.exe";

        private CSharpHotkey Win = new CSharpHotkey();

        public Form1()
        {
            InitializeComponent();

            this.Hide();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = Assembly.GetEntryAssembly().GetName().Name + " v" + Assembly.GetEntryAssembly().GetName().Version;
            notifyIcon1.Visible = true;
            Win.SetTitleMatchMode(MatchMode.Contains, MatchCase.InSensitive);

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        private void ContextMenuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void ContextMenuItemOpen_Click(object sender, EventArgs e)
        {
            DoStart();
        }
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            //handle left click only. right click is handled by the context menu
            if (e.Button == MouseButtons.Left)
            {
                DoStart();
            }
        }
        private void DoStart()
        {
            Process.Start(EXE_NAME);

            if (Win.Wait(LOGIN_WINDOW_TITLE, 15))
            {
                this.Show();
                MessageBox.Show("Timeout waiting for: " + LOGIN_WINDOW_TITLE);
                return;
            }

            //The Search & Login window is open and the default button is Login
            //press the Login button
            Win.Send(Keys.Enter);

            if (Win.Wait(MYDVR_WINDOW_TITLE, 15))
            {
                this.Show();
                MessageBox.Show("Timeout waiting for: " + MYDVR_WINDOW_TITLE);
                return;
            }

            Win.Sleep(250);

            //the DVR window is open and maximized
            //press the window split button (my display is 1920x1080)
            Win.Click(MouseButton.Left, 1598,1060);

            Win.Sleep(250);

            //press the 4 window button (my display is 1920x1080)
            Win.Click(MouseButton.Left, 1660, 995);

            Win.WaitClose(MYDVR_WINDOW_TITLE);


            //don't wait for the login window as it's not reliable
            return;

            //if (Win.Wait(LOGIN_WINDOW_TITLE, 15))
            //{
            //    //If timeout, user pressed X to close window so do nothing
            //    return;
            //}

            ////if no timeout, user pressed logout so close the login window
            //Win.Sleep(250);
            //Win.Click(MouseButton.Left, 1280, 740);
        }
    }
}
