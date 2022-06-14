namespace DEMO_CSharpHotkey
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.buttonOpenLog = new System.Windows.Forms.Button();
            this.buttonDEMO = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.listBoxDemos = new System.Windows.Forms.ListBox();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonClear = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxWinTitle = new System.Windows.Forms.TextBox();
            this.comboBoxMatchMode = new System.Windows.Forms.ComboBox();
            this.comboBoxMatchCase = new System.Windows.Forms.ComboBox();
            this.statusStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOpenLog
            // 
            this.buttonOpenLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOpenLog.Location = new System.Drawing.Point(18, 478);
            this.buttonOpenLog.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOpenLog.Name = "buttonOpenLog";
            this.buttonOpenLog.Size = new System.Drawing.Size(89, 26);
            this.buttonOpenLog.TabIndex = 1;
            this.buttonOpenLog.Text = "Open Log";
            this.buttonOpenLog.UseVisualStyleBackColor = true;
            this.buttonOpenLog.Click += new System.EventHandler(this.buttonOpenLog_Click);
            // 
            // buttonDEMO
            // 
            this.buttonDEMO.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDEMO.Location = new System.Drawing.Point(138, 478);
            this.buttonDEMO.Margin = new System.Windows.Forms.Padding(4);
            this.buttonDEMO.Name = "buttonDEMO";
            this.buttonDEMO.Size = new System.Drawing.Size(82, 26);
            this.buttonDEMO.TabIndex = 6;
            this.buttonDEMO.Text = "DEMO";
            this.buttonDEMO.UseVisualStyleBackColor = true;
            this.buttonDEMO.Click += new System.EventHandler(this.buttonDEMO_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 46);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Select:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(225, 46);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Results:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 520);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(881, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(42, 17);
            this.toolStripStatusLabel1.Text = "Ready.";
            // 
            // listBoxDemos
            // 
            this.listBoxDemos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxDemos.ForeColor = System.Drawing.SystemColors.WindowText;
            this.listBoxDemos.FormattingEnabled = true;
            this.listBoxDemos.ItemHeight = 15;
            this.listBoxDemos.Location = new System.Drawing.Point(4, 4);
            this.listBoxDemos.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxDemos.Name = "listBoxDemos";
            this.listBoxDemos.Size = new System.Drawing.Size(198, 402);
            this.listBoxDemos.TabIndex = 2;
            this.listBoxDemos.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxOutput.Location = new System.Drawing.Point(210, 4);
            this.textBoxOutput.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxOutput.Size = new System.Drawing.Size(638, 402);
            this.textBoxOutput.TabIndex = 0;
            this.textBoxOutput.WordWrap = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.26471F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75.73529F));
            this.tableLayoutPanel1.Controls.Add(this.listBoxDemos, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxOutput, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(18, 64);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(852, 410);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Location = new System.Drawing.Point(782, 478);
            this.buttonClear.Margin = new System.Windows.Forms.Padding(4);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(88, 26);
            this.buttonClear.TabIndex = 0;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 15);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 15);
            this.label3.TabIndex = 9;
            this.label3.Text = "WinTitle:";
            // 
            // textBoxWinTitle
            // 
            this.textBoxWinTitle.Location = new System.Drawing.Point(114, 12);
            this.textBoxWinTitle.Name = "textBoxWinTitle";
            this.textBoxWinTitle.Size = new System.Drawing.Size(499, 22);
            this.textBoxWinTitle.TabIndex = 10;
            // 
            // comboBoxMatchMode
            // 
            this.comboBoxMatchMode.FormattingEnabled = true;
            this.comboBoxMatchMode.Items.AddRange(new object[] {
            "Contains",
            "EndsWith",
            "Exact",
            "StartsWith"});
            this.comboBoxMatchMode.Location = new System.Drawing.Point(619, 12);
            this.comboBoxMatchMode.Name = "comboBoxMatchMode";
            this.comboBoxMatchMode.Size = new System.Drawing.Size(122, 23);
            this.comboBoxMatchMode.TabIndex = 11;
            this.comboBoxMatchMode.Text = "Contains";
            // 
            // comboBoxMatchCase
            // 
            this.comboBoxMatchCase.FormattingEnabled = true;
            this.comboBoxMatchCase.Items.AddRange(new object[] {
            "Sensitive",
            "InSensitive"});
            this.comboBoxMatchCase.Location = new System.Drawing.Point(747, 12);
            this.comboBoxMatchCase.Name = "comboBoxMatchCase";
            this.comboBoxMatchCase.Size = new System.Drawing.Size(122, 23);
            this.comboBoxMatchCase.TabIndex = 12;
            this.comboBoxMatchCase.Text = "InSensitive";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(881, 542);
            this.Controls.Add(this.comboBoxMatchCase);
            this.Controls.Add(this.comboBoxMatchMode);
            this.Controls.Add(this.textBoxWinTitle);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonDEMO);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOpenLog);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Lucida Console", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Utilities Tester";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOpenLog;
        private System.Windows.Forms.Button buttonDEMO;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ListBox listBoxDemos;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxWinTitle;
        private System.Windows.Forms.ComboBox comboBoxMatchMode;
        private System.Windows.Forms.ComboBox comboBoxMatchCase;
    }
}

