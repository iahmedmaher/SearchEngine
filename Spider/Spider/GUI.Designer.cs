namespace Spider
{
    partial class GUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GUI));
            this.URLtextBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textQueued = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textProcessed = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.MaxThreads = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.savingwork = new System.Windows.Forms.GroupBox();
            this.QueueCount = new System.Windows.Forms.Label();
            this.ProcessCount = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.dummylabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxThreads)).BeginInit();
            this.savingwork.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // URLtextBox
            // 
            this.URLtextBox.Location = new System.Drawing.Point(12, 13);
            this.URLtextBox.Name = "URLtextBox";
            this.URLtextBox.Size = new System.Drawing.Size(715, 20);
            this.URLtextBox.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(814, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textQueued
            // 
            this.textQueued.Location = new System.Drawing.Point(12, 58);
            this.textQueued.Multiline = true;
            this.textQueued.Name = "textQueued";
            this.textQueued.ReadOnly = true;
            this.textQueued.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textQueued.Size = new System.Drawing.Size(327, 391);
            this.textQueued.TabIndex = 2;
            this.textQueued.WordWrap = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Queued URLs";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(356, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Processed URLs";
            // 
            // textProcessed
            // 
            this.textProcessed.Location = new System.Drawing.Point(359, 58);
            this.textProcessed.Multiline = true;
            this.textProcessed.Name = "textProcessed";
            this.textProcessed.ReadOnly = true;
            this.textProcessed.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textProcessed.Size = new System.Drawing.Size(327, 391);
            this.textProcessed.TabIndex = 5;
            this.textProcessed.WordWrap = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(704, 58);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(268, 391);
            this.dataGridView1.TabIndex = 7;
            // 
            // MaxThreads
            // 
            this.MaxThreads.Location = new System.Drawing.Point(895, 13);
            this.MaxThreads.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.MaxThreads.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MaxThreads.Name = "MaxThreads";
            this.MaxThreads.Size = new System.Drawing.Size(77, 20);
            this.MaxThreads.TabIndex = 8;
            this.MaxThreads.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.MaxThreads.ValueChanged += new System.EventHandler(this.MaxThreads_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(27, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(188, 33);
            this.label1.TabIndex = 9;
            this.label1.Text = "Saving work...";
            // 
            // savingwork
            // 
            this.savingwork.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.savingwork.Controls.Add(this.label1);
            this.savingwork.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.savingwork.Font = new System.Drawing.Font("Microsoft Sans Serif", 1F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.savingwork.Location = new System.Drawing.Point(380, 200);
            this.savingwork.Name = "savingwork";
            this.savingwork.Size = new System.Drawing.Size(240, 100);
            this.savingwork.TabIndex = 10;
            this.savingwork.TabStop = false;
            this.savingwork.Visible = false;
            // 
            // QueueCount
            // 
            this.QueueCount.Location = new System.Drawing.Point(239, 43);
            this.QueueCount.Name = "QueueCount";
            this.QueueCount.Size = new System.Drawing.Size(100, 11);
            this.QueueCount.TabIndex = 11;
            this.QueueCount.Text = "QueueCount";
            this.QueueCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ProcessCount
            // 
            this.ProcessCount.Location = new System.Drawing.Point(586, 42);
            this.ProcessCount.Name = "ProcessCount";
            this.ProcessCount.Size = new System.Drawing.Size(100, 11);
            this.ProcessCount.TabIndex = 12;
            this.ProcessCount.Text = "ProcessCount";
            this.ProcessCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(733, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 13;
            this.button2.Text = "Seed";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.dummylabel,
            this.toolStripStatusLabel2,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 459);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(984, 22);
            this.statusStrip1.TabIndex = 14;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(84, 17);
            this.toolStripStatusLabel1.Text = "Data Received:";
            // 
            // dummylabel
            // 
            this.dummylabel.Name = "dummylabel";
            this.dummylabel.Size = new System.Drawing.Size(777, 17);
            this.dummylabel.Spring = true;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(108, 17);
            this.toolStripStatusLabel2.Text = "Database Saved at  ";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.MarqueeAnimationSpeed = 150;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.toolStripProgressBar1.Visible = false;
            // 
            // GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 481);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.ProcessCount);
            this.Controls.Add(this.QueueCount);
            this.Controls.Add(this.savingwork);
            this.Controls.Add(this.MaxThreads);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textProcessed);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textQueued);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.URLtextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(1000, 520);
            this.MinimumSize = new System.Drawing.Size(1000, 520);
            this.Name = "GUI";
            this.Text = "Spider";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GUI_FormClosing);
            this.Load += new System.EventHandler(this.GUI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaxThreads)).EndInit();
            this.savingwork.ResumeLayout(false);
            this.savingwork.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox URLtextBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textQueued;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textProcessed;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.NumericUpDown MaxThreads;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox savingwork;
        private System.Windows.Forms.Label QueueCount;
        private System.Windows.Forms.Label ProcessCount;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel dummylabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
    }
}

