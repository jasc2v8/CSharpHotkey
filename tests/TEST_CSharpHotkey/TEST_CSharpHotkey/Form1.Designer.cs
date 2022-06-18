namespace csHotKeyTest
{
    partial class Form1
    {
       
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelTitle = new System.Windows.Forms.Label();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonInputHook = new System.Windows.Forms.Button();
            this.buttonHotKey = new System.Windows.Forms.Button();
            this.buttonInputBox = new System.Windows.Forms.Button();
            this.buttonKeyLock = new System.Windows.Forms.Button();
            this.buttonKeyState = new System.Windows.Forms.Button();
            this.buttonKeyWait = new System.Windows.Forms.Button();
            this.buttonClick = new System.Windows.Forms.Button();
            this.buttonGet = new System.Windows.Forms.Button();
            this.buttonLogin = new System.Windows.Forms.Button();
            this.buttonSet = new System.Windows.Forms.Button();
            this.buttonMinMax = new System.Windows.Forms.Button();
            this.buttonMove = new System.Windows.Forms.Button();
            this.buttonWait = new System.Windows.Forms.Button();
            this.buttonTest = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonProcess = new System.Windows.Forms.Button();
            this.buttonWindowsList = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.comboBoxMatchCase = new System.Windows.Forms.ComboBox();
            this.comboBoxMatchMode = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonKill = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonSysGet = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(3, 7);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(49, 13);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "WinTitle:";
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxOutput.Location = new System.Drawing.Point(6, 12);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxOutput.Size = new System.Drawing.Size(776, 303);
            this.textBoxOutput.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.buttonInputHook);
            this.flowLayoutPanel1.Controls.Add(this.buttonHotKey);
            this.flowLayoutPanel1.Controls.Add(this.buttonInputBox);
            this.flowLayoutPanel1.Controls.Add(this.buttonKeyLock);
            this.flowLayoutPanel1.Controls.Add(this.buttonKeyState);
            this.flowLayoutPanel1.Controls.Add(this.buttonKeyWait);
            this.flowLayoutPanel1.Controls.Add(this.buttonClick);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(31, 19);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(652, 27);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // buttonInputHook
            // 
            this.buttonInputHook.Location = new System.Drawing.Point(3, 3);
            this.buttonInputHook.Name = "buttonInputHook";
            this.buttonInputHook.Size = new System.Drawing.Size(75, 23);
            this.buttonInputHook.TabIndex = 6;
            this.buttonInputHook.Text = "InputHook";
            this.buttonInputHook.UseVisualStyleBackColor = true;
            this.buttonInputHook.Click += new System.EventHandler(this.buttonHook_Click);
            // 
            // buttonHotKey
            // 
            this.buttonHotKey.Location = new System.Drawing.Point(84, 3);
            this.buttonHotKey.Name = "buttonHotKey";
            this.buttonHotKey.Size = new System.Drawing.Size(75, 23);
            this.buttonHotKey.TabIndex = 14;
            this.buttonHotKey.Text = "HotKey";
            this.buttonHotKey.UseVisualStyleBackColor = true;
            this.buttonHotKey.Click += new System.EventHandler(this.buttonHotKey_Click);
            // 
            // buttonInputBox
            // 
            this.buttonInputBox.Location = new System.Drawing.Point(165, 3);
            this.buttonInputBox.Name = "buttonInputBox";
            this.buttonInputBox.Size = new System.Drawing.Size(75, 23);
            this.buttonInputBox.TabIndex = 15;
            this.buttonInputBox.Text = "InputBox";
            this.buttonInputBox.UseVisualStyleBackColor = true;
            this.buttonInputBox.Click += new System.EventHandler(this.buttonInputBox_Click);
            // 
            // buttonKeyLock
            // 
            this.buttonKeyLock.Location = new System.Drawing.Point(246, 3);
            this.buttonKeyLock.Name = "buttonKeyLock";
            this.buttonKeyLock.Size = new System.Drawing.Size(75, 23);
            this.buttonKeyLock.TabIndex = 16;
            this.buttonKeyLock.Text = "KeyLock";
            this.buttonKeyLock.UseVisualStyleBackColor = true;
            this.buttonKeyLock.Click += new System.EventHandler(this.buttonKeyLock_Click);
            // 
            // buttonKeyState
            // 
            this.buttonKeyState.Location = new System.Drawing.Point(327, 3);
            this.buttonKeyState.Name = "buttonKeyState";
            this.buttonKeyState.Size = new System.Drawing.Size(75, 23);
            this.buttonKeyState.TabIndex = 7;
            this.buttonKeyState.Text = "KeyState";
            this.buttonKeyState.UseVisualStyleBackColor = true;
            this.buttonKeyState.Click += new System.EventHandler(this.buttonKeyState_Click);
            // 
            // buttonKeyWait
            // 
            this.buttonKeyWait.Location = new System.Drawing.Point(408, 3);
            this.buttonKeyWait.Name = "buttonKeyWait";
            this.buttonKeyWait.Size = new System.Drawing.Size(75, 23);
            this.buttonKeyWait.TabIndex = 0;
            this.buttonKeyWait.Text = "KeyWait";
            this.buttonKeyWait.UseVisualStyleBackColor = true;
            this.buttonKeyWait.Click += new System.EventHandler(this.buttonKeyWait_Click);
            // 
            // buttonClick
            // 
            this.buttonClick.Location = new System.Drawing.Point(489, 3);
            this.buttonClick.Name = "buttonClick";
            this.buttonClick.Size = new System.Drawing.Size(75, 23);
            this.buttonClick.TabIndex = 15;
            this.buttonClick.Text = "Click";
            this.buttonClick.UseVisualStyleBackColor = true;
            this.buttonClick.Click += new System.EventHandler(this.buttonClick_Click);
            // 
            // buttonGet
            // 
            this.buttonGet.Location = new System.Drawing.Point(3, 3);
            this.buttonGet.Name = "buttonGet";
            this.buttonGet.Size = new System.Drawing.Size(86, 23);
            this.buttonGet.TabIndex = 11;
            this.buttonGet.Text = "Get";
            this.buttonGet.UseVisualStyleBackColor = true;
            this.buttonGet.Click += new System.EventHandler(this.buttonGet_Click);
            // 
            // buttonLogin
            // 
            this.buttonLogin.Location = new System.Drawing.Point(176, 3);
            this.buttonLogin.Name = "buttonLogin";
            this.buttonLogin.Size = new System.Drawing.Size(75, 23);
            this.buttonLogin.TabIndex = 8;
            this.buttonLogin.Text = "Login";
            this.buttonLogin.UseVisualStyleBackColor = true;
            this.buttonLogin.Click += new System.EventHandler(this.buttonLogin_Click);
            // 
            // buttonSet
            // 
            this.buttonSet.Location = new System.Drawing.Point(419, 3);
            this.buttonSet.Name = "buttonSet";
            this.buttonSet.Size = new System.Drawing.Size(75, 23);
            this.buttonSet.TabIndex = 13;
            this.buttonSet.Text = "Set";
            this.buttonSet.UseVisualStyleBackColor = true;
            this.buttonSet.Click += new System.EventHandler(this.buttonSet_Click);
            // 
            // buttonMinMax
            // 
            this.buttonMinMax.Location = new System.Drawing.Point(257, 3);
            this.buttonMinMax.Name = "buttonMinMax";
            this.buttonMinMax.Size = new System.Drawing.Size(75, 23);
            this.buttonMinMax.TabIndex = 10;
            this.buttonMinMax.Text = "MinMax";
            this.buttonMinMax.UseVisualStyleBackColor = true;
            this.buttonMinMax.Click += new System.EventHandler(this.buttonMinMax_Click);
            // 
            // buttonMove
            // 
            this.buttonMove.Location = new System.Drawing.Point(338, 3);
            this.buttonMove.Name = "buttonMove";
            this.buttonMove.Size = new System.Drawing.Size(75, 23);
            this.buttonMove.TabIndex = 12;
            this.buttonMove.Text = "Move";
            this.buttonMove.UseVisualStyleBackColor = true;
            this.buttonMove.Click += new System.EventHandler(this.buttonMove_Click);
            // 
            // buttonWait
            // 
            this.buttonWait.Location = new System.Drawing.Point(500, 3);
            this.buttonWait.Name = "buttonWait";
            this.buttonWait.Size = new System.Drawing.Size(75, 23);
            this.buttonWait.TabIndex = 9;
            this.buttonWait.Text = "Wait";
            this.buttonWait.UseVisualStyleBackColor = true;
            this.buttonWait.Click += new System.EventHandler(this.buttonWait_Click);
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(3, 119);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(75, 23);
            this.buttonTest.TabIndex = 5;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(3, 3);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(75, 23);
            this.buttonClear.TabIndex = 3;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonProcess
            // 
            this.buttonProcess.Location = new System.Drawing.Point(3, 32);
            this.buttonProcess.Name = "buttonProcess";
            this.buttonProcess.Size = new System.Drawing.Size(75, 23);
            this.buttonProcess.TabIndex = 2;
            this.buttonProcess.Text = "Process List";
            this.buttonProcess.UseVisualStyleBackColor = true;
            this.buttonProcess.Click += new System.EventHandler(this.buttonProcess_Click);
            // 
            // buttonWindowsList
            // 
            this.buttonWindowsList.Location = new System.Drawing.Point(3, 61);
            this.buttonWindowsList.Name = "buttonWindowsList";
            this.buttonWindowsList.Size = new System.Drawing.Size(75, 23);
            this.buttonWindowsList.TabIndex = 4;
            this.buttonWindowsList.Text = "Win List";
            this.buttonWindowsList.UseVisualStyleBackColor = true;
            this.buttonWindowsList.Click += new System.EventHandler(this.buttonWindowsList_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 540);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(798, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.Location = new System.Drawing.Point(58, 3);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(364, 20);
            this.textBoxTitle.TabIndex = 4;
            // 
            // comboBoxMatchCase
            // 
            this.comboBoxMatchCase.FormattingEnabled = true;
            this.comboBoxMatchCase.Items.AddRange(new object[] {
            "Sensitive",
            "InSensitive"});
            this.comboBoxMatchCase.Location = new System.Drawing.Point(522, 3);
            this.comboBoxMatchCase.Name = "comboBoxMatchCase";
            this.comboBoxMatchCase.Size = new System.Drawing.Size(88, 21);
            this.comboBoxMatchCase.TabIndex = 8;
            this.comboBoxMatchCase.Text = "InSensitive";
            this.comboBoxMatchCase.SelectedIndexChanged += new System.EventHandler(this.comboBoxMatchCase_SelectedIndexChanged);
            // 
            // comboBoxMatchMode
            // 
            this.comboBoxMatchMode.FormattingEnabled = true;
            this.comboBoxMatchMode.Items.AddRange(new object[] {
            "Contains",
            "EndsWith",
            "Exact",
            "StartsWith"});
            this.comboBoxMatchMode.Location = new System.Drawing.Point(428, 3);
            this.comboBoxMatchMode.Name = "comboBoxMatchMode";
            this.comboBoxMatchMode.Size = new System.Drawing.Size(88, 21);
            this.comboBoxMatchMode.TabIndex = 9;
            this.comboBoxMatchMode.Text = "Contains";
            this.comboBoxMatchMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxMatchMode_SelectedIndexChanged);
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.labelTitle);
            this.flowLayoutPanel3.Controls.Add(this.textBoxTitle);
            this.flowLayoutPanel3.Controls.Add(this.comboBoxMatchMode);
            this.flowLayoutPanel3.Controls.Add(this.comboBoxMatchCase);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(27, 19);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(652, 34);
            this.flowLayoutPanel3.TabIndex = 10;
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.Controls.Add(this.buttonGet);
            this.flowLayoutPanel4.Controls.Add(this.buttonKill);
            this.flowLayoutPanel4.Controls.Add(this.buttonLogin);
            this.flowLayoutPanel4.Controls.Add(this.buttonMinMax);
            this.flowLayoutPanel4.Controls.Add(this.buttonMove);
            this.flowLayoutPanel4.Controls.Add(this.buttonSet);
            this.flowLayoutPanel4.Controls.Add(this.buttonWait);
            this.flowLayoutPanel4.Location = new System.Drawing.Point(27, 59);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(652, 30);
            this.flowLayoutPanel4.TabIndex = 11;
            // 
            // buttonKill
            // 
            this.buttonKill.Location = new System.Drawing.Point(95, 3);
            this.buttonKill.Name = "buttonKill";
            this.buttonKill.Size = new System.Drawing.Size(75, 23);
            this.buttonKill.TabIndex = 14;
            this.buttonKill.Text = "Kill";
            this.buttonKill.UseVisualStyleBackColor = true;
            this.buttonKill.Click += new System.EventHandler(this.buttonKill_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.flowLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(103, 436);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(685, 87);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Keyboard Tests";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.flowLayoutPanel4);
            this.groupBox2.Controls.Add(this.flowLayoutPanel3);
            this.groupBox2.Location = new System.Drawing.Point(103, 328);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(685, 102);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Windows Tests";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(3, 148);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 14;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.Controls.Add(this.buttonClear);
            this.flowLayoutPanel5.Controls.Add(this.buttonProcess);
            this.flowLayoutPanel5.Controls.Add(this.buttonWindowsList);
            this.flowLayoutPanel5.Controls.Add(this.buttonSysGet);
            this.flowLayoutPanel5.Controls.Add(this.buttonTest);
            this.flowLayoutPanel5.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel5.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel5.Location = new System.Drawing.Point(12, 328);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(85, 209);
            this.flowLayoutPanel5.TabIndex = 15;
            // 
            // buttonSysGet
            // 
            this.buttonSysGet.Location = new System.Drawing.Point(3, 90);
            this.buttonSysGet.Name = "buttonSysGet";
            this.buttonSysGet.Size = new System.Drawing.Size(75, 23);
            this.buttonSysGet.TabIndex = 16;
            this.buttonSysGet.Text = "SysGet";
            this.buttonSysGet.UseVisualStyleBackColor = true;
            this.buttonSysGet.Click += new System.EventHandler(this.buttonSysGet_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(798, 562);
            this.Controls.Add(this.flowLayoutPanel5);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.textBoxOutput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CSharpHotkey Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.flowLayoutPanel5.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button buttonKeyWait;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Button buttonProcess;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonWindowsList;
        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.Button buttonInputHook;
        private System.Windows.Forms.Button buttonKeyState;
        private System.Windows.Forms.Button buttonLogin;
        private System.Windows.Forms.Button buttonWait;
        private System.Windows.Forms.Button buttonMinMax;
        private System.Windows.Forms.Button buttonGet;
        private System.Windows.Forms.Button buttonMove;
        private System.Windows.Forms.Button buttonSet;
        private System.Windows.Forms.ComboBox comboBoxMatchCase;
        private System.Windows.Forms.ComboBox comboBoxMatchMode;
        private System.Windows.Forms.Button buttonHotKey;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonClick;
        private System.Windows.Forms.Button buttonKeyLock;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.Button buttonKill;
        private System.Windows.Forms.Button buttonInputBox;
        private System.Windows.Forms.Button buttonSysGet;
    }
}
