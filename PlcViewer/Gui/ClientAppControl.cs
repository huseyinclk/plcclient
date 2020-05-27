using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PlcCommon.RedisStore;
using PlcCommon.Data;
using PlcCommon.Logs;

namespace PlcViewer.Gui
{
    public partial class ClientAppControl : UserControl
    {
        public ClientAppControl()
        {
            InitializeComponent();
        }

        public string AppPath { get; set; }

        private void ClientAppControl_Load(object sender, EventArgs e)
        {
            if (this.DesignMode == false && LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                LoadDevices();
            }
        }

        private void LoadDevices()
        {
            try
            {
                grpApp.Controls.Clear();
                grpApp.Text = this.AppPath.Replace("APP:", "").Replace(":Status", "");
                using (StackRedisManager rm = new StackRedisManager())
                {
                    var app = rm.GetApp(AppPath);
                    if(app != null)
                    {
                        lblId.Text = app.Id.ToString();
                        lblDurum.Text = app.Statu.ToString();
                        lblStart.Text = app.StartDate.ToString();
                        lblSure.Text = app.Description.ToString();
                    }
                    using (NpgsqlProvider db = new NpgsqlProvider())
                    {
                        if (!db.Connect())
                        {
                            Logger.E("Veritabanına bağlanılamadı!");
                        }
                        var devices = db.DeviceDetails(this.AppPath.Replace("APP:", "").Replace(":Status", ""));
                        if (devices != null && devices.Count > 0)
                        {
                            int x = 355, y = 0;
                            for (int t = 0; t < devices.Count; t++)
                            {
                                PlcClientControl cntrl = new PlcClientControl();
                                cntrl.Name = $"PLC{t}";
                                cntrl.DeviceId = devices[t].AutomationDeviceId;
                                cntrl.Location = new Point(x, y);
                                cntrl.Size = new Size(550, 250);

                                if (x + 550 >= 1240)
                                {
                                    x = 0;
                                    y += 250;
                                }
                                else
                                {
                                    x += 550;
                                }

                                grpApp.Controls.Add(cntrl);
                                Application.DoEvents();
                            }


                        }





                    }
                }
            }
            catch (Exception exception)
            {
                Logger.E(exception);
            }
        }
    }
}
