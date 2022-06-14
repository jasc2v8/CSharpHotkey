using System;
using System.Windows.Forms;
using UTIL;

namespace AutoCredentialsTool
{
    public partial class FormEdit : Form
    {
        private bool isEncoded = false;
        private IniFile ini = null;
        string iniFilename = "act.ini";

        public FormEdit()
        {
            InitializeComponent();
        }
        private void FormEdit_Load(object sender, EventArgs e)
        {
            ini = new IniFile(iniFilename);
            textBoxUsername.Text = Base64.Decode(ini.GetValue("USERNAME"));
            textBoxPassword.Text = Base64.Decode(ini.GetValue("PASSWORD"));
        }

        private void buttonEnter_Click(object sender, EventArgs e)
        {
            string encodedValue = Base64.Encode(textBoxUsername.Text);
            if (encodedValue != String.Empty)
                ini.SetValue("USERNAME", encodedValue);

            encodedValue = Base64.Encode(textBoxPassword.Text);
            if (encodedValue != String.Empty)
                ini.SetValue("PASSWORD", encodedValue);

            this.Close();
        }
    }
}
