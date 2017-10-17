using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Players7Client
{
    public sealed class PasswordBox : TextBox
    {
        int passwordChars = -1;
        private char[] password = new char[16];

        protected override void OnTextChanged(EventArgs e)
        {
            int length = this.TextLength;
            var chars = new char[16];
            for (int i = 0; i < length; i++)
            {
                chars[i] = '*';
                if (i == length)
                    password[++passwordChars] = this.Text[i];
            }
            this.Text = new string(chars);
        }
    }
}
