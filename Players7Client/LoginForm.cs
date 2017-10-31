using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Players7Client
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();

            this.Load += LoginForm_Load;
            this.KeyPreview = true;
            this.KeyDown += LoginForm_KeyDown;
        }

        void LoginForm_Load(object sender, EventArgs e)
        {
            this.nameBox.Text = Program.GlobalSettings.Username;
            this.ipBox.Text = Program.GlobalSettings.PreferredIP;
        }

        /// <summary>
        /// Login button click
        /// </summary>
        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string username = this.nameBox.Text;
                string ipaddress = this.ipBox.Text;
                string passCode;
                if (checkBox1.Checked)
                {
                    // we must read from the encrypted file
                    using (var reader = new StreamReader("bin.pkf"))
                    {
                        StringBuilder constructedPasscode = new StringBuilder(200);
                        string content = await reader.ReadToEndAsync();
                        for (int i = 17; i < content.Length; i++)
                        {
                            int readMore = (int)content[i++];
                            int key = (int)content[i];
                            i += readMore;
                            char brick = (char)((int)content[i] ^ key);
                            constructedPasscode.Append(brick);
                        }
                        passCode = constructedPasscode.ToString();
                    }
                }
                else
                {
                    passCode = this.ipBox.Text;
                }

                NetworkHelper netcom = new NetworkHelper(ipaddress, 15432, username, passCode, (s, o) => { MessageBox.Show(string.Format(s, o)); });
                if (netcom.Connect())
                {
                    //MessageBox.Show("Successfully connected");
                    Program.Callback = () => Application.Run(new MainForm(netcom));
                    await Task.Delay(500);
                    this.Close();
                }
                else {
                    MessageBox.Show("Error while connecting");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                button1_Click(sender, e);
            }
        }
    }
}