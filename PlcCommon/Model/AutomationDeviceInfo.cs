using PlcCommon.Logs;
using PlcCommon.RedisStore;
using PlcCommon.S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Model
{
    [Serializable]
    [DataContract(Name = "device")]
    public class AutomationDeviceInfo
    {
        public AutomationDeviceInfo()
        {
            this.DeviceDInfo = new List<AutomationDeviceDInfo>();
        }

        public Plc Device { get; set; }

        [DataMember(Name = "counter")]
        public bool IsConnected
        {
            get
            {
                return Device != null && Device.IsConnected;
            }
        }

        public bool IsOnline(StackRedisManager rm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.DeviceHost)) return false;

                string _redisKey = string.Concat(StackRedisManager.RedisKeyPrefix, this.DeviceHost, ":Status");
                var deviceStatus = rm.GetHash(_redisKey);
                if (deviceStatus != null && deviceStatus.Length > 0)
                {
                    return deviceStatus[0].Value == "online";
                }
               
            }
            catch (Exception exception)
            {
                Logger.E(exception);
            }
            return false;
        }

        [DataMember(Name = "id")]
        public int AutomationDeviceId { get; set; }

        [DataMember(Name = "group_id")]
        public int AutomationDeviceGroupId { get; set; }

        [DataMember(Name = "code")]
        public string DeviceCode { get; set; }

        [DataMember(Name = "name")]
        public string DeviceName { get; set; }

        [DataMember(Name = "host")]
        public string DeviceHost { get; set; }

        [DataMember(Name = "rack")]
        public int Rack { get; set; }

        [DataMember(Name = "slot")]
        public int Slot { get; set; }

        [DataMember(Name = "counter")]
        public bool IsCounter { get; set; }

        [DataMember(Name = "pins")]
        public System.Collections.Generic.List<AutomationDeviceDInfo> DeviceDInfo { get; set; }

    }
}
