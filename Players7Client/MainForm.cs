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
            CheckForIllegalCrossThreadCalls = false;
            this.KeyPreview = true;
            this.KeyDown += Handle_KeyDown;
            this.Load += MainForm_Load;
        }

        public MainForm(NetworkHelper helper) : this()
        {
            this.helper = helper;
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            Player.Me.MyLeverage.ValueChanged += MyLeverage_ValueChanged;

            //var bindingSource = new BindingSource();
            //bindingSource.DataSource = GameManager.Games;
            //this.dataGridView1.DataSource = bindingSource;
            //this.dataGridView1.AutoGenerateColumns = true;
            //GameManager.Games.Add(new Game() { ID = 1, Bet = 1, GameCreator = "admin", PlayerCapacity = 5 });
        }

        void MyLeverage_ValueChanged(string modif)
        {
            this.trackBar1.Value = (int)Player.Me.MyLeverage;
        }

        public void FreezeLeverageScroller()
        {
            this.trackBar1.Value = 1;
            this.trackBar1.Enabled = false;
        }

        public void UnfreezeLeverageScroller()
        {
            this.trackBar1.Value = (int)Player.Me.MyLeverage;
            this.trackBar1.Enabled = true;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.helper.SendSetLeverageRequest(this.trackBar1.Value);
        }
    }
}
