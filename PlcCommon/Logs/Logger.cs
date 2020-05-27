using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.Logs
{
    public class Logger
    {
        #region Write
        public static void Write(Exception exception, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.Trace.WriteLine(string.Concat("Trace :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Exception: ", exception.Message, ", StackTrace:", exception.StackTrace));
        }

        public static void Write(string str, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.Trace.WriteLine(string.Concat("Trace :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Mesaj: ", str));
        }

        public static void Write(object obj, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.Trace.WriteLine(string.Concat("Trace :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Object: ", obj != null ? obj.ToString() : "null"));
        }
        #endregion

        #region Verbose
        public static void V(Exception exception, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 4)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Verbose :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Exception: ", exception.Message, ", StackTrace:", exception.StackTrace));
            }
        }

        public static void V(string str, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 4)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Verbose :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Mesaj: ", str));
            }
        }

        public static void V(object obj, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 4)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Verbose :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Object: ", obj != null ? obj.ToString() : "null"));
            }
        }
        #endregion

        #region Info
        public static void I(Exception exception, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 3)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Info :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Exception: ", exception.Message, ", StackTrace:", exception.StackTrace));
            }
        }

        public static void I(string str, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 3)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Info :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Mesaj: ", str));
            }
        }

        public static void I(object obj, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 3)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Info :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Object: ", obj != null ? obj.ToString() : "null"));
            }
        }
        #endregion

        #region Warning
        public static void W(Exception exception, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 2)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Warning :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Exception: ", exception.Message, ", StackTrace:", exception.StackTrace));
            }
        }

        public static void W(string str, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 2)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Warning :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Mesaj: ", str));
            }
        }

        public static void W(object obj, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 2)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Warning :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Object: ", obj != null ? obj.ToString() : "null"));
            }
        }
        #endregion

        #region Error
        public static void E(Exception exception, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 1)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Error :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Exception: ", exception.Message, ", StackTrace:", exception.StackTrace));
            }
        }

        public static void E(string str, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 1)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Error :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Mesaj: ", str));
            }
        }

        public static void E(object obj, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            System.Diagnostics.TraceLevel level = System.Diagnostics.TraceLevel.Off;
            Enum.TryParse<System.Diagnostics.TraceLevel>(System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString(), out level);
            if (level.GetHashCode() >= 1)
            {
                System.Diagnostics.Trace.WriteLine(string.Concat("Error :: ", DateTime.Now.ToString(), ", Caller: ", callerName, ", lineNumber : ", lineNumber, ", Object: ", obj != null ? obj.ToString() : "null"));
            }
        }
        #endregion
    }
}
