﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Spider
{
    public partial class GUI : Form
    {
        public Action<string> ReportQueued { private set; get; }
        public Action<string> ReportProcessed { private set; get; }
        public Action<string> ReportStartProcessing { private set; get; }
        public Action<Dictionary<string, double>> ReportStatistics { private set; get; }
        public Action ReportFinished { private set; get; }
        public Action CancellationFinished { private set; get; }
        public Action ReportDBSaving { private set; get; }
        public Action ReportDBSaved { private set; get; }

        private bool closeready;
        private Controller controller;
        private int processedcount;

        public GUI()
        {
            InitializeComponent();
            
            ReportProcessed = new Action<string>(reportprocessed);
            ReportQueued = new Action<string>(reportqueued);
            ReportStartProcessing = new Action<string>(reportstartprocessing);
            ReportStatistics=new Action<Dictionary<string,double>>(reportstat);
            ReportFinished = new Action(reportfinished);
            CancellationFinished = new Action(closethis);
            ReportDBSaving = new Action(reportDBSaveStart);
            ReportDBSaved = new Action(reportDBSaveFinish);

            closeready = false;
            QueueCount.Text = "";
            ProcessCount.Text = "";
            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel2.Text = "";
            controller = new Controller(this);
            processedcount = controller.ProcessedCount;
        }

        private void reportfinished()
        {
            URLtextBox.Enabled = true; 
            button2.Enabled = true;
        }

        private void closethis()
        {
            closeready = true;
            Close();
        }

        private void reportstat(Dictionary<string,double> dictionary)
        {
            DataTable dt = new DataTable();
            DataRow dr;
            dt.Columns.Add("Keyword");
            dt.Columns.Add("Rank");

            foreach (var row in dictionary.OrderByDescending(pair => pair.Value))
            {
                dr = dt.NewRow();
                dr["Keyword"] = row.Key;
                dr["Rank"] = row.Value;
                dt.Rows.Add(dr);
            }
            dataGridView1.DataSource = dt;
        }

        private void reportstartprocessing(string link)
        {
            if (!closeready)
                URLtextBox.Text = link;
        }

        private void reportprocessed(string link)
        {
            processedcount++;
            textProcessed.AppendText(link + Environment.NewLine);
            ProcessCount.Text = processedcount.ToString();

            toolStripStatusLabel1.Text = "Data Received: ";
            
            long data = HttpDownloader.DataTransmitted;
            
            if (data < 1024)
                toolStripStatusLabel1.Text += data.ToString() + " B";
            else if (data < 1048576)
                toolStripStatusLabel1.Text += Math.Round((double)data / 1024, 1).ToString() + " KB";
            else if (data < 1073741824)
                toolStripStatusLabel1.Text += Math.Round((double)data / 1048576, 1).ToString() + " MB";
            else
                toolStripStatusLabel1.Text += Math.Round((double)data / 1073741824, 1).ToString() + " GB";

        }

        private void reportqueued(string link)
        {
            textQueued.AppendText(link);
            QueueCount.Text = controller.QueueCount.ToString();
        }

        private void reportDBSaveStart()
        {
            toolStripStatusLabel2.Text = "Saving Progress..";
            toolStripProgressBar1.Visible = true;
        }

        private void reportDBSaveFinish()
        {
            toolStripStatusLabel2.Text = "Database Saved at " + DateTime.Now.ToString();
            toolStripProgressBar1.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            controller.StartProcess();
            URLtextBox.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
        }

        private void GUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closeready)
            {
                Cursor = Cursors.WaitCursor;
                textProcessed.Cursor = textQueued.Cursor = dataGridView1.Cursor = URLtextBox.Cursor = MaxThreads.Cursor = button1.Cursor = Cursor;
                savingwork.Show();
                controller.AbortAll();
                e.Cancel = true;
            }
        }

        private void GUI_Load(object sender, EventArgs e)
        {
            if (controller.CheckPreviousWork())
            {
                var response = MessageBox.Show(this, "Resume previous session ?", "Spider", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (response == DialogResult.Yes)
                {
                    controller.ResumeWork();
                    URLtextBox.Enabled = false;
                    button1.Enabled = false;
                    button2.Enabled = false;
                }
                else
                {
                    controller.DiscardPreWork();
                }
            }
        }

        private void MaxThreads_ValueChanged(object sender, EventArgs e)
        {
            controller.MaxThreads = Convert.ToInt32(MaxThreads.Value);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Action EnableButton1 = new Action(() => button1.Enabled = true);
            Action DisableButton2 = new Action(() => button2.Enabled = false);
            Action EnableButton2 = new Action(() => button2.Enabled = true);
            Action ResetUrlText = new Action(() => { URLtextBox.Text = string.Empty; URLtextBox.Enabled = true; });
            Action DisplayFailure = new Action(() => MessageBox.Show(this, "Failed to seed with this link." + Environment.NewLine + "Either this link was already crawled before, or robots.txt does not permit crawling this page", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));

            if (!URLtextBox.Text.StartsWith("http://") && !URLtextBox.Text.StartsWith("https://"))
                URLtextBox.Text = "http://" + URLtextBox.Text;

            URLtextBox.Enabled = false;
            string URL = URLtextBox.Text;           
            
            new Task(() =>
            {
                Invoke(DisableButton2);
                if (controller.Seed(URL))
                {
                    Invoke(EnableButton1);
                }
                else
                {
                    Invoke(DisplayFailure);
                }
                Invoke(ResetUrlText);
                Invoke(EnableButton2);
            }).Start();
        }
    }
}
