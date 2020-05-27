using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Model
{
    [Serializable]
    [DataContract(Name = "breaks")]
    public class AutomationBreakInfo
    {
        public AutomationBreakInfo() { }

        [DataMember(Name = "id")]
        public int AutomationBreakId { get; set; }

        [DataMember(Name = "wstation_id")]
        public int WstationId { get; set; }

        [DataMember(Name = "wstation_code")]
        public string WstationCode { get; set; }

        [DataMember(Name = "reason_code")]
        public string BreakReasonCode { get; set; }

        [DataMember(Name = "reason_id")]
        public int BreakReasonId { get; set; }

        [DataMember(Name = "start")]
        public DateTime StartDate { get; set; }

        [DataMember(Name = "end")]
        public DateTime EndDate { get; set; }

        [DataMember(Name = "duration")]
        public decimal BreakDuration { get; set; }

        [DataMember(Name = "automation_device_d_id")]
        public int AutomationDeviceDId { get; set; }

        [DataMember(Name = "automation_device_m_id")]
        public int AutomationDeviceMId { get; set; }

    }
}
