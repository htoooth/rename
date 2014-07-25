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
    public partial class Form2 : Form
    {
        public Form1 f1;
        public Form2()
        {
            InitializeComponent();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Hide();
            f1.Show();
            ReName.Running = false;
            this.Close();
            
        }

        public ProgressBar GetProcess()
        {
            return progressBar1;
        }

    }
}
