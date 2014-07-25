using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtLib;

namespace gui3
{
    public partial class Form1 : Form
    {

        private string format = "{0}-{1}-赤鼻岛.jpg";
        private string tim = "20141122";
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (file.ShowDialog() == DialogResult.OK)
            {
                filename.Text = file.FileName; 
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(source.ShowDialog() == DialogResult.OK)
            {
                source_dir.Text = source.SelectedPath;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (des.ShowDialog() == DialogResult.OK)
            {
                target_dir.Text =des.SelectedPath;
            }
        }

        private void time_CheckedChanged(object sender, EventArgs e)
        {
            if (time.Checked)
            {
                tim += "-112233";
            }
            else
            {
                tim = "20141122";
            }

            flush();
        }

        void flush() 
        { 
            label5.Text = string.Format(format,textBox4.Text,tim);
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            flush();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.f1 = this;
            f2.Show();
            this.Hide();

            var bar = f2.GetProcess();

            ReName.Init(textBox4.Text, filename.Text, source_dir.Text, target_dir.Text, time.Checked);
            bar.Minimum = 1;
            bar.Maximum = ReName.GetTaskCount();
            bar.Value = 1;
            bar.Step = 1;

            ReName.onRenameCompleted += (x,y)=> bar.PerformStep();
            ReName.onCompleted += () =>
            {
                f2.Close();
                this.Show();
            };

            ReName.Run();
        }
       
    }
}
