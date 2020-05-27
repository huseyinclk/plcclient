using PlcCommon.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Util
{
    public static class Utility
    {
        private static string applicationSessionId = null;
        public static ShiftInfo CurrentShift = null;

        public static string ApplicationSessionId
        {
            get
            {

                if (string.IsNullOrEmpty(applicationSessionId))
                {
                    applicationSessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
                }
                return applicationSessionId;
            }
        }

        public static long ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (long)(datetime - sTime).TotalSeconds;
        }

        public static DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return sTime.AddSeconds(unixtime);
        }

        public static string Versiyon
        {
            get
            {
                Assembly entryPoint = Assembly.GetExecutingAssembly();
                AssemblyName entryPointName = entryPoint.GetName();
                Version entryPointVersion = entryPointName.Version;
                return string.Format("{0}", entryPointVersion);
            }
        }

        public static string GetBuildCode()
        {
            var assembly = Assembly.GetEntryAssembly();

            string appName = assembly.GetName().Name;

            var stream = assembly.GetManifestResourceStream($"{appName}.build.txt");

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
