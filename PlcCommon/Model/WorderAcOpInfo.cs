using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Model
{
    [Serializable]
    [DataContract(Name = "acopinf")]
    public class WorderAcOpInfo
    {
        public WorderAcOpInfo() { }

        [DataMember(Name = "id")]
        public int WorderAcOpId { get; set; }

        [DataMember(Name = "wstation_id")]
        public int WstationId { get; set; }

        [DataMember(Name = "wstation_code")]
        public string WstationCode { get; set; }

        [DataMember(Name = "qty")]
        public int Qty { get; set; }

        [DataMember(Name = "end")]
        public DateTime EndDate { get; set; }

        [DataMember(Name = "ip")]
        public string HostName { get; set; }

        [DataMember(Name = "address")]
        public string Address { get; set; }
    }
}
