using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Model
{
    [Serializable]
    [DataContract(Name = "device_pin")]
    public class AutomationDeviceDInfo
    {
        public AutomationDeviceDInfo() { }

        [DataMember(Name = "id")]
        public int AutomationDeviceDId { get; set; }

        [DataMember(Name = "device_id")]
        public int AutomationDeviceId { get; set; }

        [DataMember(Name = "wstation_id")]
        public int WstationId { get; set; }

        [DataMember(Name = "wstation_code")]
        public string WstationCode { get; set; }

        [DataMember(Name = "address")]
        public string Address { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "duration")]
        public int BreakDuration { get; set; }

        [DataMember(Name = "counter")]
        public bool IsCounter { get; set; }
    }
}
