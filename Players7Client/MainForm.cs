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

        public void OnGameAdded(Game g) {
            DataGridViewRow row = new DataGridViewRow();
            row.SetValues(g.Name, g.GameCreator, g.PlayerCapacity, g.Bet);
            this.dataGridView1.Rows.Add(row);
        }

        static DataTable ConvertListToDataTable(List<Game> gameList)
        {
            // New table.
            DataTable table = new DataTable();

            // Get max columns.
            int columns = 0;
            object[][] list = gameList.Select(g => new object[] { g.Name, g.GameCreator, g.PlayerCapacity.ToString(), g.Bet.ToString(), g.Btn }).ToArray();
            foreach (var array in list) 
            {
                if (array.Length > columns)
                {
                    columns = array.Length;
                }
            }
            
            // Add columns.
            for (int i = 0; i < columns; i++)
            {
                table.Columns.Add();
            }
            

            // Add rows.
            foreach (object[] array in list)
            {
                DataRow row = table.Rows.Add(array);
                if (row.ItemArray.Count() == 0)
                    MessageBox.Show("");
            }

            return table;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Player.Me.MyLeverage.ValueChanged += MyLeverage_ValueChanged;
            List<Game> games = new List<Game>() {
                new Game () { Name = "game 1", GameCreator= "eu1", Bet = 1, PlayerCapacity = 2, ID=1, Btn = new Button()},
                new Game () { Name = "game 2", GameCreator= "eu2", Bet = 1, PlayerCapacity = 2, ID=2, Btn = new Button()},
                new Game () { Name = "game 3", GameCreator= "eu3", Bet = 1, PlayerCapacity = 2, ID=3, Btn = new Button()},
                new Game () { Name = "game 4", GameCreator= "eu4", Bet = 1, PlayerCapacity = 2, ID=4, Btn = new Button()},
            };
            DataTable table = ConvertListToDataTable(games);
            this.dataGridView1.DataSource=table;
            dataGridView1.Update();
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
