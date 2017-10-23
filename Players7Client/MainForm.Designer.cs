namespace Players7Client
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.iDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gameCreatorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.betDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.playerCapacityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gameBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gameBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.iDDataGridViewTextBoxColumn,
            this.gameCreatorDataGridViewTextBoxColumn,
            this.betDataGridViewTextBoxColumn,
            this.playerCapacityDataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.gameBindingSource;
            this.dataGridView1.Location = new System.Drawing.Point(12, 56);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(536, 290);
            this.dataGridView1.TabIndex = 0;
            // 
            // iDDataGridViewTextBoxColumn
            // 
            this.iDDataGridViewTextBoxColumn.DataPropertyName = "ID";
            this.iDDataGridViewTextBoxColumn.HeaderText = "ID";
            this.iDDataGridViewTextBoxColumn.Name = "iDDataGridViewTextBoxColumn";
            // 
            // gameCreatorDataGridViewTextBoxColumn
            // 
            this.gameCreatorDataGridViewTextBoxColumn.DataPropertyName = "GameCreator";
            this.gameCreatorDataGridViewTextBoxColumn.HeaderText = "GameCreator";
            this.gameCreatorDataGridViewTextBoxColumn.Name = "gameCreatorDataGridViewTextBoxColumn";
            // 
            // betDataGridViewTextBoxColumn
            // 
            this.betDataGridViewTextBoxColumn.DataPropertyName = "Bet";
            this.betDataGridViewTextBoxColumn.HeaderText = "Bet";
            this.betDataGridViewTextBoxColumn.Name = "betDataGridViewTextBoxColumn";
            // 
            // playerCapacityDataGridViewTextBoxColumn
            // 
            this.playerCapacityDataGridViewTextBoxColumn.DataPropertyName = "PlayerCapacity";
            this.playerCapacityDataGridViewTextBoxColumn.HeaderText = "PlayerCapacity";
            this.playerCapacityDataGridViewTextBoxColumn.Name = "playerCapacityDataGridViewTextBoxColumn";
            // 
            // gameBindingSource
            // 
            this.gameBindingSource.DataSource = typeof(Players7Client.Game);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(635, 12);
            this.trackBar1.Maximum = 7;
            this.trackBar1.Minimum = 1;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(194, 45);
            this.trackBar1.TabIndex = 1;
            this.trackBar1.Value = 1;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(841, 498);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gameBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn iDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn gameCreatorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn betDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn playerCapacityDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource gameBindingSource;
        private System.Windows.Forms.TrackBar trackBar1;

    }
}