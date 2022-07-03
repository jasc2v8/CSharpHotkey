/*
 *  SwannViewControlTool is a tray app for the SwannView Link software
 *  The SwannView Link software controls the SwannView security camera DVR(s) on the network
 *  On start, only the tray icon is present
 *  To start the SwannView software,
 *      Click on tray icon, or
 *      Right-Click and select "SwannView Link"
 *  The SwannView software is started and this tray app does the following:
 *      The login button is pressed
 *      The split screen button is pressed
 *      The 4-way screen button is pressed
 *      The timer is started
 *  The User stops the SwannView software by:
 *      Pressing the X Close button on the window
 *          The SwannView software will close and the tray app stops the timer
 *      Pressing the Logout button
 *          The DVR window will close and the Login window will open
 *          The timer tick will check the Login window:
 *              If not present, do nothing
 *              Else stop the timer and Wait for Login window
 *              If found, Close the window
 *  To close the SwannView software and this tray app:
 *      Right-click the tray icon and select "Exit" to close.
 */

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

        private static CSharpHotkey Win = new CSharpHotkey();

        private const int timerInterval = 500;

        public Form1()
        {
            InitializeComponent();

            timer1.Interval = timerInterval;
            timer1.Tick += new EventHandler(TimerEventHandler);

            //keep this empty form hidden
            this.Opacity = 0;
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
            if (timer1.Enabled)
                timer1.Stop();

            timer1.Dispose();
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

            //The Search & Login window is open and the default button is Login so press it
            Win.Send(Keys.Enter);

            //wait for DVR window
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

            //start the timer
            timer1.Start();
        }

        private void TimerEventHandler(Object myObject, EventArgs myEventArgs)
        {
            //if DVR window present then do nothing
            if (Win.Exist(MYDVR_WINDOW_TITLE))
                return;

            //stop the timer
            timer1.Stop();

            //if login window doesn't exist (timeout) then do nothing
            if (Win.Wait(LOGIN_WINDOW_TITLE, 5))
                return;

            //else close it
            Win.Close(LOGIN_WINDOW_TITLE);
        }
    }
}
