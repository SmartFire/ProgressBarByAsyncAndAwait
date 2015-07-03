using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProgressBarByAsyncAndAwait
{
    public partial class MainForm : Form
    {
        CancellationTokenSource cancelSource;
        public MainForm()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            cancelSource = new CancellationTokenSource();
            IProgress<int> progress = new Progress<int>((processValue) => { this.progressBar1.Value = processValue; });


            this.textBox1.AppendText("Starting work, please wait...");
            this.button1.Enabled = false;
            this.button2.Enabled = true;

            WasteTimeObject ad = new WasteTimeObject();
            ad.ShowProcess += UpdateProgress;

            try
            {
                Task<string> task = Task.Run(() => ad.GetSlowString(1, 10, progress, cancelSource.Token));

                DoingSomethings();

                this.textBox1.AppendText("\r\nDoingSomethings is over....");

                string result = await task;

                this.textBox1.AppendText(result);

                this.button2.Enabled = false;
            }
            catch (OperationCanceledException)
            {
                textBox1.AppendText("\r\nYou canceled the operation....");
            }
        }

        private void UpdateProgress(int i)
        {
            this.label1.Text = i * 10 + "%";
        }

        public void DoingSomethings()
        {
            this.textBox1.AppendText("\r\nDoingSomethings is running,please wait....");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (cancelSource != null)
                cancelSource.Cancel();

            this.button2.Enabled = false;
        }
    }

    public class WasteTimeObject
    {
        public Action<int> ShowProcess;

        public string GetSlowString(int begin, int length, IProgress<int> progress, CancellationToken cancel)
        {
            for (int i = begin; i < begin + length; i++)
            {
                if (ShowProcess != null)
                    ShowProcess(i);

                System.Threading.Thread.Sleep(1000);

                cancel.ThrowIfCancellationRequested();

                if (progress != null)
                {
                    progress.Report((int)((double)(i - begin + 1) * 100 / length));
                }
            }

            return "\r\nWasteTimeObject is over....";
        }
    }
}
