//
//  https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.keys
//
using System;
using System.Windows.Forms;
using System.Reflection;
using CSharpHotkeyLib;
using System.Threading;
using System.Diagnostics;

namespace Test_Hotkey
{
    public partial class Form1 : Form
    {
        private CSharpHotkey Win = new CSharpHotkey();

        private int DebugKeyDelay { get; set; }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = Assembly.GetEntryAssembly().GetName().Name + " v" + Assembly.GetEntryAssembly().GetName().Version;

            //textBoxInput.Text = "Password{ENTER}+(e)c%(FO)"; //notepad Alt-F,O = File, Open
            textBoxInput.Text = "^a^(biu)PASSword{ENTER}{+}{^}{%} {{} {}} {[} {]} {h 10}, +(ec), +ec^+{home}^(biu)^{END}"; //wordpad
            //textBoxInput.Text = textBoxInput.Text.ToUpper();
            toolStripStatusLabel1.Text = "Ready.";

            //WriteLine("SystemInformation.KeyboardSpeed for repeat=" + SystemInformation.KeyboardSpeed);
            Win.SetTitleMatchMode(MATCH_MODE.Contains, MATCH_CASE.InSensitive);

            //textBoxKeyDelay.Text = "300";
        }

        private void WriteLine(string text)
        {
            textBoxOutput.AppendText(text + Environment.NewLine);
        }
        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxOutput.Clear();
            toolStripStatusLabel1.Text = "Ready.";
        }
        private void buttonOK_Click(object sender, EventArgs e)
        {
            //WriteLine("OK clicked...");
            toolStripStatusLabel1.Text = "OK!";
            textBoxOutput.Clear();

            string WinTitle = "wordpad";

            WriteLine("CapsLock: " + Control.IsKeyLocked(Keys.CapsLock));

            WriteLine("Manually activate window: " + WinTitle);

            if (Win.WaitActive(WinTitle, 5))
            {
                WriteLine("TIMEOUT waiting for window: " + WinTitle);
                return;

            }
            Int32.TryParse(textBoxKeyDelay.Text, out int i);
            DebugKeyDelay = i;
            //WriteLine("DebugKeyDelay="+ DebugKeyDelay);
            Win.SetKeyDelay(i);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //SendTest(textBoxInput.Text, false);
            Win.Send(textBoxInput.Text, false);

            stopWatch.Stop();
            WriteLine("Elapsed: " + stopWatch.ElapsedMilliseconds + " ms.");
            WriteLine("Test complete.");

        //very limited testing:
        //  SendKeys.SendWait = Elapsed: 3010 ms.
        //  SendKeys.Send     = Elapsed: 3016 ms.
        //  SendInput         = Elapsed: 3020 ms. (a bit slower but apparently more reliable)
        }
        public void SendTest(string KeyStrokes, bool SendWait = true)
        {
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
 
            string _DoSend(string Buffer)
            {
                //debug return buffer
                //WriteLine("Send: " + Buffer);
                //Win.Sleep(DebugKeyDelay);
                //return String.Empty; ;

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

                Win.Sleep(DebugKeyDelay);
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
    }
}
