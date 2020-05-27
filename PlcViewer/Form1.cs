using PlcCommon.RedisStore;
using PlcViewer.Gui;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tabControl1.TabPages.Clear();
            using (StackRedisManager rm = new StackRedisManager())
            {
                var keys = rm.GetAll(StackRedisManager.RedisAppKeyPrefix);
                if (keys != null && keys.Count > 0)
                {
                    int x = 0, y = 0;
                    for (int t = 0; t < keys.Count; t++)
                    {
                        ClientAppControl cntlr = new ClientAppControl();
                        cntlr.Name = $"App{t}";
                        cntlr.AppPath = keys[t];
                        cntlr.Size = new Size(550, 250);
                        cntlr.Location = new Point(x, y);
                        cntlr.Dock = DockStyle.Fill;
                        TabPage p = new TabPage();
                        p.Text = $"App{t}";
                        p.Controls.Add(cntlr);
                        tabControl1.TabPages.Add(p);

                        if (x + 550 >= this.Width)
                        {
                            x = 0;
                            y += 250;
                        }
                        else
                        {
                            x += 550;
                        }

                    }
                }
                Application.DoEvents();
            }
        }
    }
}
