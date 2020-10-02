using PlcCommon.S7.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlcViewer
{
    public partial class FormTest : Form
    {
        public FormTest()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var obj = plc.Read("DB1.DBD0");
            if(obj != null)
            {
                label1.Text = obj.ToString();
            }
            else
            {
                label1.Text = "null";
            }
        }
        Plc plc = null;
        private void button1_Click(object sender, EventArgs e)
        {
            plc = new PlcCommon.S7.Net.Plc(PlcCommon.S7.Net.CpuType.S71200, "192.168.144.62", 0, 1);
            plc.Open();
            timer1.Start();
        }
    }
}
