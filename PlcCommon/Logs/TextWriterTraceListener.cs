using PlcCommon.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Logs
{
    public class TextWriterTraceListener : TraceListener
    {
        private readonly StreamWriter Writer;

        public TextWriterTraceListener()
        {
            try
            {
                string tracelavel = "";
                if (System.Configuration.ConfigurationManager.AppSettings["tracelavel"] != null)
                    tracelavel = System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString();

                string appName = Assembly.GetCallingAssembly().GetName().Name;

                string dir = $"{System.Windows.Forms.Application.StartupPath}\\Trace\\{appName}\\";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string trace = $"{dir}{DateTime.Now.ToString("yyMMddHH")}_{Process.GetCurrentProcess().Id}.log";
                Writer = new StreamWriter(trace, false, Encoding.GetEncoding("windows-1254"));
                Writer.AutoFlush = true;
                Writer.WriteLine($"AppStart:{DateTime.Now.ToString()}, Log:{tracelavel}, Build:{Utility.GetBuildCode()}");
            }
            catch
            {
            }
        }


        ~TextWriterTraceListener()
        {
            try
            {
                if (Writer != null)
                    Writer.Close();
            }
            catch
            {
                ;
            }
        }


        public override void Write(string message)
        {
            try
            {
                if (Writer != null)
                    Writer.Write(message);
            }
            catch
            {
            }
        }

        public override void WriteLine(string message)
        {
            try
            {
                if (Writer != null)
                    Writer.WriteLine(message);
            }
            catch
            {
            }
        }
    }
}
