using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Model
{
    public partial class ShiftInfo
    {
        public ShiftInfo() { }

        public int ShiftId { get; set; }

        public string ShiftCode { get; set; }

        public string ShiftDesc { get; set; }

        public string Start { get; set; }

        public string End { get; set; }

        public static TimeSpan StrToTime(string time)
        {
            int days = 0, hours = 0, minutes = 0, seconds = 0;
            if (!string.IsNullOrEmpty(time))
            {
                if (time.IndexOf(":") != -1)
                {
                    string[] arg = time.Split(':');
                    if (arg != null && arg.Length > 0)
                        hours = Convert.ToInt32(arg[0]);
                    if (arg != null && arg.Length > 1)
                        minutes = Convert.ToInt32(arg[1]);
                }
                else
                {
                    if (time.Length > 2)
                        hours = Convert.ToInt32(time.Substring(0, 2));
                    if (time.Length > 3)
                        minutes = Convert.ToInt32(time.Substring(2, 2));
                }
            }
            if (hours < 12 && DateTime.Now.Hour == 23) days = 1;
            return new TimeSpan(days, hours, minutes, seconds);
        }

    }
}
