using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Model
{
    [Serializable]
    [DataContract(Name = "app_inf")]
    public class AppInfo
    {
        public AppInfo() { }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "key")]
        public string AppKey { get; set; }

        [DataMember(Name = "statu")]
        public string Statu { get; set; }

        [DataMember(Name = "desc")]
        public string Description { get; set; }

        [DataMember(Name = "start")]
        public DateTime StartDate { get; set; }

        [DataMember(Name = "duration")]
        public int Duration { get; set; }

    }
}
