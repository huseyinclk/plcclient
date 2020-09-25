using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Model
{
    public class PinValue
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public int Count { get; set; }
        public int VCount { get; set; }
        public int RCount { get; set; }
        public int Time { get; set; }
        public DateTime Date { get; set; }
        public int RTime { get; set; }
        public bool IsBreak { get; set; }
        public bool IsRework { get; set; }
        public string WstationCode { get; set; }
        public string SessionId { get; set; }
        public int Version { get; set; } // tembellik versionu
    }
}
