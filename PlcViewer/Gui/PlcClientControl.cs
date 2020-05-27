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
using PlcCommon.Util;

namespace PlcViewer.Gui
{
    public partial class PlcClientControl : UserControl
    {
        public PlcClientControl()
        {
            InitializeComponent();
        }

        public PlcClientControl(int did)
        {
            this.deviceId = did;
            InitializeComponent();
        }

        private int deviceId;

        public int DeviceId
        {
            get { return deviceId; }
            set { deviceId = value; }
        }

        private void PlcClientControl_Load(object sender, EventArgs e)
        {
            if (this.DesignMode == false && LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                LoadPins();
            }
        }

        private void LoadPins()
        {
            try
            {
                grpPlc.Controls.Clear();
                using (StackRedisManager rm = new StackRedisManager())
                {


                    using (NpgsqlProvider db = new NpgsqlProvider())
                    {
                        if (!db.Connect())
                        {
                            Logger.E("Veritabanına bağlanılamadı!");
                        }
                        var device = db.DeviceDetails(DeviceId.ToString()).FirstOrDefault();
                        if (device != null)
                        {
                            var hash = rm.GetHash(string.Concat("QTY:", device.DeviceHost, ":Status"));
                            if(hash != null)
                            {
                                grpPlc.Text = $"{device.DeviceHost} - {hash[0].Value} - {hash[1].Value}";
                                if (hash[0].Value != "online")
                                    grpPlc.BackColor = Color.Orange;
                            }
                            else
                            {
                                grpPlc.Text = device.DeviceHost;
                            }

                            int x = 6, y = 29;
                            for (int i = 0; i < device.DeviceDInfo.Count; i++)
                            {
                                int value = -1;
                                var date = new DateTime(1900, 1, 1);
                                var pin = rm.GetValue(string.Concat(StackRedisManager.RedisKeyPrefix, device.DeviceHost, ":", device.DeviceDInfo[i].Address, ":Pin"));
                                if (pin != null)
                                {
                                    value = pin.Count;
                                    date = Utility.UnixTimeToDateTime(pin.Time);
                                }

                                Label lbl = new Label();
                                lbl.Text = $"Id:{device.DeviceDInfo[i].WstationId},Kod:{device.DeviceDInfo[i].WstationCode},Adres:{device.DeviceDInfo[i].Address},Adet:{value},Tarih:{date.ToString("dd.MM.yyyy HH:mm:ss")}";
                                lbl.AutoSize = false;
                                lbl.Size = new Size(543, 29);
                                lbl.Location = new Point(x, y);
                                lbl.BorderStyle = BorderStyle.FixedSingle;
                                lbl.BackColor = Color.White;
                                y += 30;
                                grpPlc.Controls.Add(lbl);
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
