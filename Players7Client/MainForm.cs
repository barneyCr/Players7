using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Players7Client
{
    public partial class MainForm : Form
    {
        #region Constructors
        MainForm()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += Handle_KeyDown;
        }

        public MainForm(NetworkHelper helper) : this()
        {

        }
		#endregion

        Properties.Settings settings = global::Players7Client.Properties.Settings.Default;

		readonly NetworkHelper helper;
		readonly Action<String> SystemMessage;

		void Handle_KeyDown(object sender, KeyEventArgs e)
		{
			//if (e.KeyCode == Keys.Enter)
				//this.button1.PerformClick();
        }
    }
}
