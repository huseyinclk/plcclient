using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Model
{
    [Serializable]
    [DataContract(Name = "activity_time")]
    public class AutomationActivityTimeInfo
    {
        public AutomationActivityTimeInfo() { }

        [DataMember(Name = "id")]
        public int AutomationActivityTimeId { get; set; }

        [DataMember(Name = "wstation_id")]
        public int WstationId { get; set; }

        [DataMember(Name = "wstation_code")]
        public string WstationCode { get; set; }

        [DataMember(Name = "worderm_id")]
        public int WorderMId { get; set; }

        [DataMember(Name = "worder_no")]
        public string WorderNo { get; set; }

        [DataMember(Name = "item_id")]
        public int ItemId { get; set; }

        [DataMember(Name = "item_code")]
        public string ItemCode { get; set; }

        [DataMember(Name = "unit_id")]
        public int UnitId { get; set; }

        [DataMember(Name = "unit_code")]
        public string UnitCode { get; set; }

        [DataMember(Name = "qty")]
        public decimal Qty { get; set; }

        [DataMember(Name = "qty_diff")]
        public decimal QtyDifference { get; set; }

        [DataMember(Name = "date")]
        public DateTime Date { get; set; }

        [DataMember(Name = "time_diff")]
        public TimeSpan TimeDifference { get; set; }

        [DataMember(Name = "employee_id")]
        public int EmployeeId { get; set; }

        [DataMember(Name = "full_name")]
        public string FullName { get; set; }

        [DataMember(Name = "shift_id")]
        public int ShiftId { get; set; }

        [DataMember(Name = "shift_code")]
        public string ShiftCode { get; set; }

        [DataMember(Name = "worderacop_id")]
        public int WorderAcOpId { get; set; }

        [DataMember(Name = "automation_device_d_id")]
        public int AutomationDeviceDId { get; set; }

        [DataMember(Name = "automation_device_m_id")]
        public int AutomationDeviceMId { get; set; }

    }
}
