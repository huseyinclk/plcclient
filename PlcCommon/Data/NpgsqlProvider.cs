using Npgsql;
using PlcCommon.Logs;
using PlcCommon.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Data
{
    public class NpgsqlProvider : IDisposable
    {
        public NpgsqlProvider()
        {
        }

        private NpgsqlCommand command = null;
        private NpgsqlConnection connection = null;
        private string message = "";

        public string Message
        {
            get
            {
                return message;
            }
        }

        public bool IsConnected
        {
            get
            {
                return connection != null && connection.State == System.Data.ConnectionState.Open;
            }
        }

        public bool Connect()
        {
            try
            {
                if (connection == null)
                {
                    string connstr = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                    connection = new NpgsqlConnection(connstr);
                }
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                if (command == null)
                    command = connection.CreateCommand();

                return connection != null && connection.State == System.Data.ConnectionState.Open;
            }
            catch (Exception exception)
            {
                message = exception.Message;
                Logger.E(exception);
                return false;
            }
        }

        public bool Execute(string sql, NpgsqlParameter[] parameterArr)
        {
            try
            {
                Logger.V(sql);

                if (Connect())
                {
                    command.Parameters.Clear();
                    command.CommandText = sql;
                    if (parameterArr != null)
                        command.Parameters.AddRange(parameterArr);
                    var i = command.ExecuteScalar();
                    command.Parameters.Clear();
                    Logger.I($"Sonuc:{i},Sql:{sql}");
                    return i != null;
                }
                else
                {
                    message = "Bağlantı hatası!";
                }
            }
            catch (Exception exception)
            {
                message = exception.Message;
                Logger.E(exception);
            }
            finally
            {
                command.Parameters.Clear();
            }
            return false;
        }

        public List<AutomationDeviceInfo> GetDevicesForShift()
        {
            List<AutomationDeviceInfo> list = new List<AutomationDeviceInfo>();
            if (Connect())
            {
                command.CommandText = $@"SELECT d.automation_device_m_id,m.device_code,m.device_name,m.device_host,t.wstation_id,t.wstation_code,t.description,d.automation_device_d_id,d.address,d.break_duration,COALESCE(d.rack, 0) AS rack, COALESCE(d.slot, 1) AS slot
FROM prdd_wstation t LEFT JOIN prdd_automation_device_d d ON t.wstation_id = d.wstation_id LEFT JOIN prdd_automation_device_m m ON d.automation_device_m_id = m.automation_device_m_id
WHERE t.turn_of_work = 't'
ORDER BY m.automation_device_m_id";
                NpgsqlDataAdapter adater = new NpgsqlDataAdapter(command);
                DataTable dt = new DataTable();
                adater.Fill(dt);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int loop = 0; loop < dt.Rows.Count; loop++)
                    {
                        int automation_device_m_id = 0;
                        if (DBNull.Value != dt.Rows[loop]["automation_device_m_id"])
                            automation_device_m_id = Convert.ToInt32(dt.Rows[loop]["automation_device_m_id"]);
                        else
                            automation_device_m_id = -1011;

                        var deviceindex = list.FindIndex(p => p.AutomationDeviceId == automation_device_m_id);
                        if (deviceindex == -1)
                        {
                            var device = new AutomationDeviceInfo();
                            device.AutomationDeviceGroupId = 1;//Convert.ToInt32(dt.Rows[loop]["automation_device_group_m_id"]);
                            device.AutomationDeviceId = automation_device_m_id == -1011 ? automation_device_m_id : Convert.ToInt32(dt.Rows[loop]["automation_device_m_id"]);
                            device.DeviceCode = automation_device_m_id == -1011 ? "" : dt.Rows[loop]["device_code"].ToString();
                            device.DeviceHost = automation_device_m_id == -1011 ? "" : dt.Rows[loop]["device_host"].ToString();
                            device.Rack = automation_device_m_id == -1011 ? 0 : Convert.ToInt32(dt.Rows[loop]["rack"]);
                            device.Slot = automation_device_m_id == -1011 ? 0 : Convert.ToInt32(dt.Rows[loop]["slot"]);
                            device.DeviceName = "";
                            list.Add(device);
                            deviceindex = list.Count - 1;
                        }

                        var deviceItem = new AutomationDeviceDInfo();
                        deviceItem.AutomationDeviceId = list[deviceindex].AutomationDeviceId;
                        deviceItem.AutomationDeviceDId = automation_device_m_id == -1011 ? 0 : Convert.ToInt32(dt.Rows[loop]["automation_device_d_id"]);
                        deviceItem.Name = "";
                        deviceItem.Address = automation_device_m_id == -1011 ? "" : dt.Rows[loop]["address"].ToString();
                        deviceItem.WstationId = Convert.ToInt32(dt.Rows[loop]["wstation_id"]);
                        deviceItem.WstationCode = dt.Rows[loop]["wstation_code"].ToString();
                        deviceItem.BreakDuration = automation_device_m_id == -1011 ? 0 : Convert.ToInt32(dt.Rows[loop]["break_duration"]);
                        deviceItem.IsCounter = true;
                        list[deviceindex].DeviceDInfo.Add(deviceItem);
                    }
                }
            }
            return list;

        }

        public List<AutomationDeviceInfo> Devices(int groupnameId = 1)
        {
            List<AutomationDeviceInfo> list = new List<AutomationDeviceInfo>();
            if (Connect())
            {
                command.CommandText = $@"SELECT m.automation_device_group_m_id, d.automation_device_m_id, dm.device_code, dm.device_host  
FROM prdd_automation_device_group_d d INNER JOIN 
prdd_automation_device_group_m m ON m.automation_device_group_m_id = d.automation_device_group_m_id INNER JOIN 
prdd_automation_device_m dm ON d.automation_device_m_id = dm.automation_device_m_id 
WHERE m.automation_device_group_m_id = {groupnameId}
ORDER BY dm.device_host";
                NpgsqlDataAdapter adater = new NpgsqlDataAdapter(command);
                using (DataTable dt = new DataTable())
                {
                    adater.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int loop = 0; loop < dt.Rows.Count; loop++)
                        {
                            var device = new AutomationDeviceInfo();
                            device.AutomationDeviceGroupId = Convert.ToInt32(dt.Rows[loop]["automation_device_group_m_id"]);
                            device.AutomationDeviceId = Convert.ToInt32(dt.Rows[loop]["automation_device_m_id"]);
                            device.DeviceCode = dt.Rows[loop]["device_code"].ToString();
                            device.DeviceHost = dt.Rows[loop]["device_host"].ToString();
                            device.DeviceName = "";
                            list.Add(device);
                        }
                    }
                }
            }
            list.TrimExcess();
            return list;

        }

        public List<AutomationDeviceInfo> DeviceDetails(string devices)
        {
            List<AutomationDeviceInfo> list = new List<AutomationDeviceInfo>();
            if (Connect())
            {
                command.CommandText = $@"SELECT m.automation_device_group_m_id, d.automation_device_m_id, dd.automation_device_d_id, dm.device_code, dm.device_host, dd.address, dd.wstation_id, w.wstation_code, dd.break_duration,COALESCE(dd.rack, 0) AS rack, COALESCE(dd.slot, 1) AS slot  
FROM prdd_automation_device_group_d d INNER JOIN 
prdd_automation_device_group_m m ON m.automation_device_group_m_id = d.automation_device_group_m_id INNER JOIN 
prdd_automation_device_m dm ON d.automation_device_m_id = dm.automation_device_m_id INNER JOIN 
prdd_automation_device_d dd ON dm.automation_device_m_id = dd.automation_device_m_id INNER JOIN 
prdd_wstation w ON dd.wstation_id = w.wstation_id
WHERE d.automation_device_m_id IN ({devices})
ORDER BY m.automation_device_group_m_id, d.automation_device_m_id, dd.automation_device_d_id, dm.device_code, dm.device_host";
                NpgsqlDataAdapter adater = new NpgsqlDataAdapter(command);
                DataTable dt = new DataTable();
                adater.Fill(dt);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int loop = 0; loop < dt.Rows.Count; loop++)
                    {
                        var deviceindex = list.FindIndex(p => p.AutomationDeviceId == Convert.ToInt32(dt.Rows[loop]["automation_device_m_id"]));
                        if (deviceindex == -1)
                        {
                            var device = new AutomationDeviceInfo();
                            device.AutomationDeviceGroupId = Convert.ToInt32(dt.Rows[loop]["automation_device_group_m_id"]);
                            device.AutomationDeviceId = Convert.ToInt32(dt.Rows[loop]["automation_device_m_id"]);
                            device.Rack = Convert.ToInt32(dt.Rows[loop]["rack"]);
                            device.Slot = Convert.ToInt32(dt.Rows[loop]["slot"]);
                            device.DeviceCode = dt.Rows[loop]["device_code"].ToString();
                            device.DeviceHost = dt.Rows[loop]["device_host"].ToString();
                            device.DeviceName = "";
                            list.Add(device);
                            deviceindex = list.Count - 1;
                        }

                        var deviceItem = new AutomationDeviceDInfo();
                        deviceItem.AutomationDeviceId = list[deviceindex].AutomationDeviceId;
                        deviceItem.AutomationDeviceDId = Convert.ToInt32(dt.Rows[loop]["automation_device_d_id"]);
                        deviceItem.Name = "";
                        deviceItem.Address = dt.Rows[loop]["address"].ToString();
                        deviceItem.WstationId = Convert.ToInt32(dt.Rows[loop]["wstation_id"]);
                        deviceItem.WstationCode = dt.Rows[loop]["wstation_code"].ToString();
                        deviceItem.BreakDuration = Convert.ToInt32(dt.Rows[loop]["break_duration"]);
                        deviceItem.IsCounter = true;
                        list[deviceindex].DeviceDInfo.Add(deviceItem);
                    }
                }
            }
            return list;

        }

        public List<AutomationDeviceInfo> GroupItems(string groupname)
        {
            List<AutomationDeviceInfo> list = new List<AutomationDeviceInfo>();
            if (Connect())
            {
                command.CommandText = $@"SELECT m.automation_device_group_m_id, d.automation_device_m_id, dd.automation_device_d_id, dm.device_code, dm.device_host, dd.address, dd.wstation_id, w.wstation_code, dd.break_duration,COALESCE(dd.rack, 0) AS rack, COALESCE(dd.slot, 1) AS slot 
FROM prdd_automation_device_group_d d INNER JOIN 
prdd_automation_device_group_m m ON m.automation_device_group_m_id = d.automation_device_group_m_id INNER JOIN 
prdd_automation_device_m dm ON d.automation_device_m_id = dm.automation_device_m_id INNER JOIN 
prdd_automation_device_d dd ON dm.automation_device_m_id = dd.automation_device_m_id INNER JOIN 
prdd_wstation w ON dd.wstation_id = w.wstation_id
WHERE m.group_code = '{groupname}'
ORDER BY m.automation_device_group_m_id, d.automation_device_m_id, dd.automation_device_d_id, dm.device_code, dm.device_host";
                NpgsqlDataAdapter adater = new NpgsqlDataAdapter(command);
                DataTable dt = new DataTable();
                adater.Fill(dt);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int loop = 0; loop < dt.Rows.Count; loop++)
                    {
                        var deviceindex = list.FindIndex(p => p.AutomationDeviceId == Convert.ToInt32(dt.Rows[loop]["automation_device_m_id"]));
                        if (deviceindex == -1)
                        {
                            var device = new AutomationDeviceInfo();
                            device.AutomationDeviceGroupId = Convert.ToInt32(dt.Rows[loop]["automation_device_group_m_id"]);
                            device.AutomationDeviceId = Convert.ToInt32(dt.Rows[loop]["automation_device_m_id"]);
                            device.DeviceCode = dt.Rows[loop]["device_code"].ToString();
                            device.DeviceHost = dt.Rows[loop]["device_host"].ToString();
                            device.DeviceName = "";
                            list.Add(device);
                            deviceindex = list.Count - 1;
                        }

                        var deviceItem = new AutomationDeviceDInfo();
                        deviceItem.AutomationDeviceId = list[deviceindex].AutomationDeviceId;
                        deviceItem.AutomationDeviceDId = Convert.ToInt32(dt.Rows[loop]["automation_device_d_id"]);
                        deviceItem.Name = "";
                        deviceItem.Address = dt.Rows[loop]["address"].ToString();
                        deviceItem.WstationId = Convert.ToInt32(dt.Rows[loop]["wstation_id"]);
                        deviceItem.WstationCode = dt.Rows[loop]["wstation_code"].ToString();
                        deviceItem.BreakDuration = Convert.ToInt32(dt.Rows[loop]["break_duration"]);
                        deviceItem.IsCounter = true;
                        list[deviceindex].DeviceDInfo.Add(deviceItem);
                    }
                }
            }
            return list;

            /*
             SELECT m.automation_device_group_m_id, d.automation_device_m_id, dd.automation_device_d_id, dm.device_code, dm.device_host, dd.address, dd.wstation_id, w.wstation_code, dd.break_duration 
FROM prdd_automation_device_group_d d INNER JOIN 
prdd_automation_device_group_m m ON m.automation_device_group_m_id = d.automation_device_group_m_id INNER JOIN 
prdd_automation_device_m dm ON d.automation_device_m_id = dm.automation_device_m_id INNER JOIN 
prdd_automation_device_d dd ON dm.automation_device_m_id = dd.automation_device_m_id INNER JOIN 
prdd_wstation w ON dd.wstation_id = w.wstation_id
WHERE m.group_code = 'FAB-4'
ORDER BY m.automation_device_group_m_id, d.automation_device_m_id, dd.automation_device_d_id, dm.device_code, dm.device_host
             */

            /*
             SELECT m.automation_device_group_m_id, d.automation_device_m_id, dm.device_code, dm.device_host 
FROM prdd_automation_device_group_d d INNER JOIN 
prdd_automation_device_group_m m ON m.automation_device_group_m_id = d.automation_device_group_m_id INNER JOIN 
prdd_automation_device_m dm ON d.automation_device_m_id = dm.automation_device_m_id
WHERE m.group_code = 'FAB-4'
             */

            /*
             SELECT m.automation_device_m_id, d.automation_device_d_id, d.address, d.wstation_id, w.wstation_code, d.break_duration, d.is_break, d.is_counter 
FROM prdd_automation_device_m m INNER JOIN prdd_automation_device_d d ON m.automation_device_m_id = d.automation_device_m_id INNER JOIN prdd_wstation w ON d.wstation_id = w.wstation_id
WHERE m.automation_device_m_id = 1062
             */

            /*
             
            List<AutomationDeviceInfo> devices = (from q in new XPQuery<AutomationDeviceGroupD>(DevExpress.Xpo.XpoDefault.Session)
                                                  where q.AutomationDeviceGroupM.GroupCode == groupname &&
                                                  q.AutomationDeviceGroupM.Status == WarehouseManagement.Enums.RecordStatus.New &&
                                                  q.AutomationDeviceM.Status == WarehouseManagement.Enums.RecordStatus.New
                                                  select new AutomationDeviceInfo()
                                                  {
                                                      AutomationDeviceId = q.AutomationDeviceM.AutomationDeviceMId,
                                                      AutomationDeviceGroupId = q.AutomationDeviceGroupM.AutomationDeviceGroupMId,
                                                      DeviceCode = q.AutomationDeviceM.DeviceCode,
                                                      DeviceName = q.AutomationDeviceM.DeviceCode,
                                                      DeviceHost = q.AutomationDeviceM.DeviceHost,
                                                      DeviceDInfo = AutomationDeviceDList(q.AutomationDeviceM.AutomationDeviceMId)
                                                  }).ToList();
             */
        }

        public ShiftInfo GetShift(string shiftcode)
        {
            ShiftInfo shift = null;
            if (Connect())
            {
                try
                {
                    command.CommandText = $"SELECT s.shift_id, s.shift_code, s.start, s.end, s.description FROM prdd_shift s WHERE s.shift_code = '{shiftcode}'";
                    NpgsqlDataAdapter adater = new NpgsqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adater.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        shift = new ShiftInfo();
                        shift.ShiftId = Convert.ToInt32(dt.Rows[0]["shift_id"]);
                        shift.ShiftDesc = dt.Rows[0]["description"].ToString();
                        shift.ShiftCode = dt.Rows[0]["shift_code"].ToString();
                        shift.Start = dt.Rows[0]["start"].ToString();
                        shift.End = dt.Rows[0]["end"].ToString();
                    }

                }
                catch (Exception exception)
                {
                    Logger.E(exception);
                }
            }
            return shift;
        }

        #region IDisposable
        ~NpgsqlProvider()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (command != null)
                {
                    command.Dispose();
                }

                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }

                command = null;
                connection = null;
                NpgsqlConnection.ClearAllPools();
            }

            disposed = true;
        }
        #endregion
    }
}
