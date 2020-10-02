using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Logs
{
    public class LogWriter
    {
        public DateTime SatartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long TotalMemory { get; set; }
        public long Memory { get; set; }
        public StringBuilder LogText { get; set; }
        public Stopwatch Stopwatch { get; set; }

        public LogWriter()
        {
            this.SatartTime = DateTime.Now;
            this.TotalMemory = GC.GetTotalMemory(true);
            this.LogText = new StringBuilder();
            this.Stopwatch = Stopwatch.StartNew();
        }

        public long ElapsedMilliseconds
        {
            get { return Stopwatch != null ? Stopwatch.ElapsedMilliseconds : 0; }
        }

        public TimeSpan Elapsed
        {
            get { return Stopwatch != null ? Stopwatch.Elapsed : TimeSpan.FromSeconds(0); }
        }

        public bool IsRunning
        {
            get { return Stopwatch != null ? Stopwatch.IsRunning : false; }
        }

        public void Write(string log, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            this.LogText.Append(string.Concat("LOG :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Exception: ", log));
        }
        public void WriteLine(string log, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            this.LogText.AppendLine(string.Concat("LOG :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Exception: ", log));
        }
        public void Write(Exception ex, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            this.LogText.Append(string.Concat("LOG :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Exception: ", ex.Message, ", StackTrace:", ex.StackTrace));
        }
        public void WriteLine(Exception ex, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            this.LogText.AppendLine(string.Concat("LOG :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Exception: ", ex.Message, ", StackTrace:", ex.StackTrace));
        }

        public string EndLog()
        {
            this.Memory = GC.GetTotalMemory(true);
            this.Stopwatch.Stop();
            this.EndTime = DateTime.Now;
            LogText.AppendFormat("Memory-1: {0} Memory-2: {1}, ElapsedMillisecond:{2}", this.Memory, this.TotalMemory, this.Stopwatch.ElapsedMilliseconds);
            return LogText.ToString();
        }

        public override string ToString()
        {
            return LogText != null ? LogText.ToString() : base.ToString();
        }
    }
}
