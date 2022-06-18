//
//	This is the basic framework for an AutoWalk function for a game that uses the following keys:
//		W = go forward, S= go backwards, A = go left, D = go right, Space = AutoWalk start/stop
//	When the Space bar is pressed, it will hold down the W key
//	When the W or S key is pressed, that key will be held down
//	When the Space bar is pressed again, the AutoWalk will be stopped. Press Space to resume AutoWalk.
//  The S and D keys are not held down.
//  The AutoWalk function will only be active when the WinTile window is active.
//  Enter WinTitle = "autowalk" to see the keystrokes in this window.
//  Enter TimeInterval = milliseconds between AutoWalk keydown commands.
//  Adjust TimeInterval for your system and game.
//  Minimum of 33 ms recommended but your mileage may vary

using System;
using System.Windows.Forms;
using System.Reflection;
using System.Timers;
using CSharpHotkeyLib;

namespace AutoWalk
{
	public partial class Form1 : Form
	{
		private string WinTitle = string.Empty;

		InputHook hookW = null;
		InputHook hookS = null;
		HotKey hotkeySpace = null;

		System.Timers.Timer walkTimer = null;
		
		Keys WalkKey { get; set; }
		Keys WalkKeyCurrent { get; set; }

		bool IsLooping { get; set; }
		string WalkWindow { get; set; }

		CSharpHotkey Win = new CSharpHotkey();

		public Form1()
		{
			InitializeComponent();

			this.Text = Assembly.GetEntryAssembly().GetName().Name + " v" + Assembly.GetEntryAssembly().GetName().Version;

			textBoxInput.Text = "notepad _class notepad";

		}
		private void Form1_Load(object sender, EventArgs e)
		{
			//textBoxInput.Text = "Input...";
			toolStripStatusLabel1.Text = "Ready.";
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			//MessageBox.Show("timer1=" + timer1.Enabled);
		}

		private void WriteLine(string text)
		{
			textBoxOutput.AppendText(text + Environment.NewLine);
		}
		private void buttonClear_Click(object sender, EventArgs e)
		{
			textBoxOutput.Clear();
			textBoxInput.Clear();
			toolStripStatusLabel1.Text = "Ready.";
		}

		private void StartHooks()
        {
			hookW = new InputHook(Keys.None, Keys.W, Keys.Down, Keys.N, (hook) => { WalkKey = Keys.W; });
			//hookA = new InputHook(Keys.None, Keys.A, Keys.Down, Keys.N, (hook) => { WalkKey = Keys.A; });
			hookS = new InputHook(Keys.None, Keys.S, Keys.Down, Keys.N, (hook) => { WalkKey = Keys.S; });
			//hookD = new InputHook(Keys.None, Keys.D, Keys.Down, Keys.N, (hook) => { WalkKey = Keys.D; });
		}
		private void StopHooks()
		{
			hookW.Stop();
			//hookA.Stop();
			hookS.Stop();
			//hookD.Stop();
		}
		private void buttonSTART_Click(object sender, EventArgs e)
		{

			textBoxOutput.Focus();

			//WriteLine("IN 3 SECONDS...");
			//Win.Sleep(3000);PresentationCore

			hotkeySpace = new HotKey(Keys.None, Keys.Space, (hotkey) =>
            {
                IsLooping = !IsLooping;
				//WriteLine("IsLooping=" + IsLooping);

				if (IsLooping)
                {
					StopHooks();
					walkTimer.Start();
				}
				else
                {
					walkTimer.Stop();
                    Win.Send(WalkKey, Keys.Up);
					StartHooks();
				}
			});

            WalkKey = Keys.W;

			walkTimer = new System.Timers.Timer();
			walkTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            walkTimer.Interval = (double)numericUpDownTimerInterval.Value; //recommend 33
            walkTimer.SynchronizingObject = this;

            numericUpDownTimerInterval.Enabled = false;
			textBoxInput.Enabled = false;

			IsLooping = false;
			StartHooks();

			WinTitle = textBoxInput.Text;
			Win.SetTitleMatchMode(MatchMode.Contains, MatchCase.InSensitive);

			//WriteLine("AutoWalk stared for WinTitle: " + WinTitle);
			WriteLine("Press W, A, S, D to Auto Walk or SPACE to Start/Stop.");

			toolStripStatusLabel1.Text = "AutoWalk STARTED.";

		}

		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			if (!Win.Active(WinTitle))
				return;

			if (WalkKeyCurrent != Keys.W & Win.GetKeyState(Keys.W)) { WalkKey = Keys.W; }
			if (WalkKeyCurrent != Keys.S & Win.GetKeyState(Keys.S)) { WalkKey = Keys.S; }

			if (WalkKeyCurrent != WalkKey)
            {
				Win.Send(WalkKeyCurrent, Keys.Up);

				WalkKeyCurrent = WalkKey;
				//return;
            }

			Win.Send(WalkKeyCurrent, Keys.Down);
		}
        private void buttonStop_Click(object sender, EventArgs e)
        {

			//WriteLine("AutoWalk STOPPED.");
			toolStripStatusLabel1.Text = "AutoWalk STOPPED.";

			if (walkTimer == null || hookW == null || hotkeySpace == null)
				return;

			IsLooping = false;
			walkTimer.Stop();
			walkTimer.Dispose();

			StopHooks();
			hookW = null;
			hookS = null;

			hotkeySpace.UnRegister();
			hotkeySpace = null;

			Win.Send(WalkKeyCurrent, Keys.Up);

			numericUpDownTimerInterval.Enabled = true;
			textBoxInput.Enabled = true;

		}
	}

}
