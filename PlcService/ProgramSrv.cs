using Newtonsoft.Json;
using Npgsql;
using PlcCommon.Data;
using PlcCommon.Logs;
using PlcCommon.Model;
using PlcCommon.RabbitMQ;
using PlcCommon.RedisStore;
using PlcCommon.Util;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace PlcService
{
    class ProgramSrv
    {

        const string EventLogSource = "PlcService";
        static bool showApplication = true;
        private static RabbitMQManager counterConsumer;
        private static RabbitMQManager breakConsumer;
        private static RabbitMQManager activityConsumer;
        static System.Timers.Timer timerCounter = null;
        private static List<int[]> listDevice;

        static void Main(string[] args)
        {
            try
            {
                InitialLogger();

                if (args != null && args.Length > 0)
                {
                    SetShift(args[0]);
                }
                else
                {
                    Start();
                }

                activityConsumer = new RabbitMQManager(RabbitMQManager.QueueNameActivity, 10);
                activityConsumer.Received += ActivityConsumer_Received;
                activityConsumer.Consume();

                breakConsumer = new RabbitMQManager(RabbitMQManager.QueueNameBreak, 10);
                breakConsumer.Received += BreakConsumer_Received;
                breakConsumer.Consume();

                counterConsumer = new RabbitMQManager(RabbitMQManager.QueueNameCounter, 10);
                counterConsumer.Received += CounterConsumer_Received;
                counterConsumer.Consume();

                while (true)
                {
                    int interval = 460000;
                    if (System.Configuration.ConfigurationManager.AppSettings["Interval"] != null)
                        interval = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Interval"].ToString()) * 60 * 1000;

                    if (System.Configuration.ConfigurationManager.AppSettings["ShowApp"] != null)
                        showApplication = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["ShowApp"].ToString());

                    Thread.Sleep(interval);

                    if (Environment.UserInteractive)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                    }


                    CheckApp();

                    if (Environment.UserInteractive)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                }

            }
            catch (Exception exc)
            {
                Logger.E(exc);
                Environment.Exit(-1);
            }
        }

        private static readonly object counterLock = new object();
        private static void CounterConsumer_Received(object sender, RabbitMQ.Client.Events.BasicDeliverEventArgs e)
        {
            Monitor.Enter(counterLock);
            if (e != null)
            {
                try
                {
                    Logger.I("Counter received");
                    var message = Encoding.UTF8.GetString(e.Body);
                    if (!string.IsNullOrEmpty(message))
                    {
                        using (NpgsqlProvider db = new NpgsqlProvider())
                        {
                            if (!db.Connect())
                            {
                                Logger.E("Veritabanına bağlanılamadı!");
                            }
                            WorderAcOpInfo worderInfo = JsonConvert.DeserializeObject<WorderAcOpInfo>(message);
                            if (worderInfo != null)
                            {
                                try
                                {
                                    Logger.W($"Counter reset received:{worderInfo.WstationCode}");

                                    PlcCommon.S7.Net.Plc plc = new PlcCommon.S7.Net.Plc(PlcCommon.S7.Net.CpuType.S71200, worderInfo.HostName, 0, 1);
                                    plc.Open();
                                    if (plc != null && plc.IsConnected)
                                    {
                                        int val = 0;
                                        var plcvar = plc.Read(worderInfo.Address);
                                        if (plcvar != null)
                                        {
                                            val = Convert.ToInt32(plcvar) - worderInfo.Qty;
                                            plc.Write(worderInfo.Address, val);
                                            counterConsumer.BasicAck(e.DeliveryTag, false);
                                            Logger.I("ors.opcclient.counter-->" + worderInfo.WstationCode);
                                        }
                                        else
                                        {
                                            Logger.E("ors.opcclient.counter-->Error:" + worderInfo.WstationCode);
                                            counterConsumer.BasicReject(e.DeliveryTag, false); // hata olduğunda siliyoruz şimdilik
                                        }
                                    }
                                    else
                                    {
                                        Logger.E("Counter PLC bağlanılamadı:" + worderInfo.HostName);
                                    }
                                }
                                catch (Exception exc)
                                {
                                    Logger.E("counter.Error-->" + worderInfo.WstationCode + ",Detay:" + exc.Message);
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.E(exception);
                    counterConsumer.BasicReject(e.DeliveryTag, false); // hata olduğunda siliyoruz şimdilik
                }
            }
            Monitor.Exit(counterLock);
        }

        private static readonly object breakLock = new object();
        private static void BreakConsumer_Received(object sender, RabbitMQ.Client.Events.BasicDeliverEventArgs e)
        {
            if (e != null)
            {
                Monitor.Enter(breakLock);
                Logger.V("Break received");
                var message = Encoding.UTF8.GetString(e.Body);
                if (!string.IsNullOrEmpty(message))
                {
                    try
                    {
                        using (NpgsqlProvider db = new NpgsqlProvider())
                        {
                            if (!db.Connect())
                            {
                                Logger.E("Veritabanına bağlanılamadı!");
                            }

                            AutomationBreakInfo automationBreak = JsonConvert.DeserializeObject<AutomationBreakInfo>(message);
                            if (automationBreak != null)
                            {

                                Logger.I("ors.opcclient.break-->" + automationBreak.WstationCode);

                                if (db.Execute("SELECT \"sp_prdt_automation_break\"(@automationdevicedid, @wstationid, @wstation_code, @startdate)",
                                     new NpgsqlParameter[]
                                     {
                                            new NpgsqlParameter("automationdevicedid",automationBreak.AutomationDeviceDId),
                                            new NpgsqlParameter("wstationid",automationBreak.WstationId),
                                            new NpgsqlParameter("wstation_code",automationBreak.WstationCode),
                                            new NpgsqlParameter("startdate",automationBreak.StartDate)
                                     }))
                                {
                                    breakConsumer.BasicAck(e.DeliveryTag, false);
                                    Logger.I($"ors.opcclient.break-->{automationBreak.WstationCode}");
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Logger.E(exc);
                    }
                    Monitor.Exit(breakLock);
                }
            }
        }

        private static readonly object activityLock = new object();
        private static void ActivityConsumer_Received(object sender, RabbitMQ.Client.Events.BasicDeliverEventArgs e)
        {
            if (e != null)
            {
                Monitor.Enter(activityLock);
                Logger.V("Activity received");
                var message = Encoding.UTF8.GetString(e.Body);
                if (!string.IsNullOrEmpty(message))
                {
                    try
                    {
                        using (NpgsqlProvider db = new NpgsqlProvider())
                        {
                            if (!db.Connect())
                            {
                                Logger.E("Veritabanına bağlanılamadı!");
                            }

                            AutomationActivityTimeInfo automationActivityTime = JsonConvert.DeserializeObject<AutomationActivityTimeInfo>(message);
                            if (automationActivityTime != null)
                            {

                                Logger.I($"ors.opcclient.activity-->{automationActivityTime.WstationCode},{automationActivityTime.Qty}");

                                if (db.Execute("SELECT  \"sp_prdt_automation_ac_time\"(@automationdevicedid, @wstationid, @wstation_code, @cnt, @cntdiff, @ac_time, @ac_time_diff)",
                                      new NpgsqlParameter[]
                                     {
                                            new NpgsqlParameter("automationdevicedid",automationActivityTime.AutomationDeviceDId),
                                            new NpgsqlParameter("wstationid",automationActivityTime.WstationId),
                                            new NpgsqlParameter("wstation_code",automationActivityTime.WstationCode),
                                            new NpgsqlParameter("cnt",automationActivityTime.Qty),
                                            new NpgsqlParameter("cntdiff",automationActivityTime.QtyDifference),
                                            new NpgsqlParameter("ac_time",automationActivityTime.Date),
                                            new NpgsqlParameter("ac_time_diff",automationActivityTime.TimeDifference.TotalSeconds)
                                     }))
                                {
                                    activityConsumer.BasicAck(e.DeliveryTag, false);
                                    Logger.I($"ors.opcclient.activity-->{automationActivityTime.WstationCode},{automationActivityTime.Qty}");
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Logger.E(exc);
                    }
                }
                Monitor.Exit(activityLock);
            }
        }

        /// <summary>
        /// Her bir plc için bir exe başlatılacak. Plc okumayı bitirdikten sonra kapanacak
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TimerCallback(object sender, ElapsedEventArgs e)
        {
            try
            {
                timerCounter.Enabled = false;
                using (NpgsqlProvider db = new NpgsqlProvider())
                {
                    if (!db.Connect())
                    {
                        Logger.E("Veritabanına bağlanılamadı!");
                    }

                    using (StackRedisManager rm = new StackRedisManager())
                    {
                        int grp = 0;
                        var devices = db.Devices();
                        listDevice = new List<int[]>();
                        if (devices != null && devices.Count > 0)
                        {
                            List<int> deviceList = new List<int>();
                            grp = devices.Count / 10;
                            while (devices.Count > 0)
                            {
                                var device = devices[0];
                                if (deviceList.Count < grp || devices.Count < grp)
                                {
                                    deviceList.Add(device.AutomationDeviceId);
                                    devices.Remove(device);
                                }
                                else
                                {
                                    listDevice.Add(deviceList.ToArray());
                                    deviceList.Clear();
                                }
                            }
                            if (deviceList.Count > 0)
                            {
                                listDevice.Add(deviceList.ToArray());
                                deviceList.Clear();
                            }

                            try
                            {
                                var processes = Process.GetProcesses();
                                for (int p = 0; p < processes.Length; p++)
                                {
                                    if (processes[p].ProcessName == "PlcClient")
                                    {
                                        processes[p].Kill();
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                Logger.I(exception);
                            }

                            for (int i = 0; i < listDevice.Count; i++)
                            {
                                Trace.WriteLine(string.Join(",", listDevice[i]));
                                StartApp(string.Join(",", listDevice[i]), rm);
                            }
                        }
                    }
                }
            }
            catch (NullReferenceException nullexception)
            {
                Logger.E(nullexception);
            }
            catch (Exception exception)
            {
                Logger.E(exception);
            }
            finally
            {
                timerCounter.Enabled = true;
            }

        }

        static void Start()
        {
            try
            {
                CloseAllApp();

                using (NpgsqlProvider db = new NpgsqlProvider())
                {
                    if (!db.Connect())
                    {
                        Logger.E("Veritabanına bağlanılamadı!");
                    }

                    using (StackRedisManager rm = new StackRedisManager())
                    {
                        int grp = 0;
                        var devices = db.Devices();
                        listDevice = new List<int[]>();
                        if (devices != null && devices.Count > 0)
                        {
                            List<int> deviceList = new List<int>();
                            grp = devices.Count / 10;
                            while (devices.Count > 0)
                            {
                                var device = devices[0];
                                if (deviceList.Count < grp || devices.Count < grp)
                                {
                                    deviceList.Add(device.AutomationDeviceId);
                                    devices.Remove(device);
                                }
                                else
                                {
                                    listDevice.Add(deviceList.ToArray());
                                    deviceList.Clear();
                                }
                            }
                            if (deviceList.Count > 0)
                            {
                                listDevice.Add(deviceList.ToArray());
                                deviceList.Clear();
                            }



                            for (int i = 0; i < listDevice.Count; i++)
                            {
                                Trace.WriteLine("Cihaz:", string.Join(",", listDevice[i]));
                                StartApp(string.Join(",", listDevice[i]), rm);
                            }

                        }
                    }
                }
            }
            catch (NullReferenceException nullexception)
            {
                Logger.E(nullexception);
            }
            catch (Exception exception)
            {
                Logger.E(exception);
            }
        }

        static void SetShift(string shiftCode)
        {
            try
            {
                CloseAllApp();

                string trace = "";
                StreamWriter writer = null;
                LogWriter logwriter = new LogWriter();
                var offlineCount = 0;
                bool isonline = false;

                string appName = Assembly.GetCallingAssembly().GetName().Name;

                string dir = $"{System.Windows.Forms.Application.StartupPath}\\Trace\\{appName}\\";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (StackRedisManager rm = new StackRedisManager())
                using (NpgsqlProvider db = new NpgsqlProvider())
                {
                    DateTime dt = DateTime.Now.Date;

                    Utility.CurrentShift = db.GetShift(shiftCode);

                    trace = $"{dir}{DateTime.Now.ToString("yyMMddHH")}_{Utility.CurrentShift.ShiftCode}.log";
                    writer = new StreamWriter(trace, false, Encoding.GetEncoding("windows-1254"));
                    writer.AutoFlush = true;

                    TimeSpan time = ShiftInfo.StrToTime(Utility.CurrentShift.Start);
                    var shifttime = new DateTime(dt.Year, dt.Month, dt.Day, time.Hours, time.Minutes, 0);

                    Logger.I($"Vardiya degistir {Utility.CurrentShift}");

                    List<AutomationDeviceInfo> device = db.GetDevicesForShift();
                    if (device != null && device.Count > 0)
                    {
                        for (int i = 0; i < device.Count; i++)
                        {
                            if (device[i].AutomationDeviceId != -1011)
                            {
                                device[i].Device = new PlcCommon.S7.Net.Plc(PlcCommon.S7.Net.CpuType.S71200, device[i].DeviceHost, 0, 1);

                                if (device[i].IsOnline(rm))
                                    device[i].Device.Open();

                                isonline = device[i].Device.IsConnected;

                                if (!isonline)
                                {
                                    writer.WriteLine($"PLC:{device[i].DeviceHost},Ulaşılamıyor!");
                                    offlineCount++;
                                }
                            }

                            for (int loop = 0; loop < device[i].DeviceDInfo.Count; loop++)
                            {
                                int count = 0;
                                Logger.I($"{device[i].DeviceHost} Vardiya değiştir, kod:{device[i].DeviceDInfo[loop].WstationCode}, id:{device[i].DeviceDInfo[loop].WstationId} adres:{device[i].DeviceDInfo[loop].Address}");

                                string _redisKey = string.Concat(StackRedisManager.RedisKeyPrefix, device[i].DeviceHost, ":", device[i].DeviceDInfo[loop].Address, ":Pin");
                                var rval = rm.GetValue(_redisKey);

                                object val = null;

                                if (device[i].AutomationDeviceId != -1011 && isonline)
                                    val = val = device[i].Device.Read(device[i].DeviceDInfo[loop].Address);

                                #region PLC okunamiyorsa Redis oku
                                if (val == null && rval != null)
                                {
                                    val = rval.Count;
                                    if (!isonline) rval.VCount = rval.Count;//plcye erisilemiyorsa vardiya miktari tutulacak
                                    else rval.VCount = 0;
                                }
                                #endregion

                                int.TryParse(val.ToString(), out count);

                                if (device[i].AutomationDeviceId != -1011 && isonline)
                                    device[i].Device.Write(device[i].DeviceDInfo[loop].Address, 0);

                                #region Redis Update

                                rval.Count = 0;
                                rval.RCount = 0;
                                rval.IsRework = false;
                                rm.SetValue(_redisKey, rval);

                                #endregion

                                Logger.I($"ors.opcclient.setshift-->{device[i].DeviceDInfo[loop].Address}");
                                var newid = db.Execute("SELECT \"sp_prdt_setshift\"(@automationdevicedid, @wstationid, @wstationcode, @cnt, @shiftid, @shifttime)",
                                      new NpgsqlParameter[]
                                         {
                                            new NpgsqlParameter("automationdevicedid",device[i].DeviceDInfo[loop].AutomationDeviceDId),
                                            new NpgsqlParameter("wstationid",device[i].DeviceDInfo[loop].WstationId),
                                            new NpgsqlParameter("wstationcode",device[i].DeviceDInfo[loop].WstationCode),
                                            new NpgsqlParameter("cnt",count),
                                            new NpgsqlParameter("shiftid",Utility.CurrentShift.ShiftId),
                                            new NpgsqlParameter("shifttime",shifttime)
                                         });
                                if (!newid)
                                {
                                    writer.WriteLine($"SELECT \"sp_prdt_setshift\"(@automationdevicedid='{device[i].DeviceDInfo[loop].AutomationDeviceDId}', @wstationid='{device[i].DeviceDInfo[loop].WstationId}', @wstationcode='{device[i].DeviceDInfo[loop].WstationCode}', @cnt='{count}', @shiftid='{Utility.CurrentShift.ShiftId}', @shifttime='{shifttime}')");
                                    writer.WriteLine($"SetShift-->{device[i].DeviceDInfo[loop].WstationCode},{Utility.CurrentShift.ShiftId}\tHata:{db.Message}");
                                    Logger.W($"SetShift-->{device[i].DeviceDInfo[loop].WstationCode},{Utility.CurrentShift.ShiftId}\tHata:{db.Message}");
                                }
                            }
                        }
                    }
                }

                System.Diagnostics.Trace.WriteLine(logwriter.EndLog());

                //writer.WriteLine($"`{Utility.CurrentShift.ShiftCode}` Vardiya başarılı bir şekilde değiştirildi.Memory-1: {b2} Memory-2: {totalMemory}, ElapsedMillisecond:{stopwatch.ElapsedMilliseconds}, Başlangıç:{satartTime.ToString("HH:mm:ss")}, Bitiş:{endTime.ToString("HH:mm:ss")}");
                writer.Close();
                writer.Dispose();

                MailHelper.SendMail(offlineCount > 0 ? MailHelper.Adresler : null, MailHelper.MailBaslik, $"`{Utility.CurrentShift.ShiftCode}` Vardiya başarılı bir şekilde değiştirildi, ancak bazı PLC lere işlem yapılamadı. PLC adresleri ektedir. Memory-1: {logwriter.Memory} Memory-2: {logwriter.TotalMemory}, ElapsedMillisecond:{logwriter.ElapsedMilliseconds}, Başlangıç:{logwriter.SatartTime.ToString("HH:mm:ss")}, Bitiş:{logwriter.EndTime.ToString("HH:mm:ss")}", trace);

            }
            catch (Exception exception)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(exception.Message);
                Logger.E(exception);
            }
            finally
            {
                Start();
            }

        }

        /// <summary>
        /// Açık uygulama varsa kapatacak yeni exe basşlatılacak
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="rm"></param>
        private static void StartApp(string arguments, StackRedisManager rm)
        {
            ProcessStartInfo p = new ProcessStartInfo();
            p.FileName = $"{Application.StartupPath}\\PlcClient.exe";
            p.Arguments = arguments;
            if (!showApplication)// (!Environment.UserInteractive)
            {
                p.ErrorDialog = false;
                p.CreateNoWindow = true;
                p.WindowStyle = ProcessWindowStyle.Hidden;
            }
            //p.UseShellExecute = false;
            //p.RedirectStandardOutput = true;
            //p.RedirectStandardInput = true;
            p.WorkingDirectory = Application.StartupPath;
            Process xp = Process.Start(p);
            AppInfo appinfo = new AppInfo();
            appinfo.Statu = "first";
            appinfo.StartDate = DateTime.Now;
            appinfo.Id = Process.GetCurrentProcess().Id;
            appinfo.Duration = 0;
            rm.SetApp(string.Concat(StackRedisManager.RedisAppKeyPrefix, arguments, ":Status"), appinfo);
            //Process process = new Process();
            //process.StartInfo = p;
            //process.OutputDataReceived += Process_OutputDataReceived;
            //process.Start();
            //process.BeginOutputReadLine();
        }

        private static void CheckApp()
        {
            Logger.I("Check applications");
            for (int i = 0; i < listDevice.Count; i++)
            {
                var arguments = string.Join(",", listDevice[i]);
                using (StackRedisManager rm = new StackRedisManager())
                {
                    try
                    {
                        AppInfo appinfo = rm.GetApp(string.Concat(StackRedisManager.RedisAppKeyPrefix, arguments, ":Status"));
                        if (appinfo != null)
                        {
                            if (appinfo.Statu == "check")
                            {
                                try
                                {
                                    var proc = Process.GetProcessById(appinfo.Id);
                                    if (proc != null) proc.Kill();
                                }
                                catch (Exception exc)
                                {
                                    Logger.E(exc);
                                }
                                StartApp(string.Join(",", listDevice[i]), rm);
                                MailHelper.SendMail(null, MailHelper.MailBaslik, $"`{arguments}` Uygulama offline olduğu için kapatılıp açıldı");
                            }
                            else
                            {
                                appinfo.Statu = "check";
                                appinfo.StartDate = DateTime.Now;
                                rm.SetApp(string.Concat(StackRedisManager.RedisAppKeyPrefix, arguments, ":Status"), appinfo);
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Logger.E(exc);
                    }
                }
            }
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        static void InitialLogger()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("tr-TR");
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ProgramSrv.CurrentDomain_UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(ProgramSrv.Application_ThreadException);

            string tracelavel = "";
            if (System.Configuration.ConfigurationManager.AppSettings["tracelavel"] != null)
                tracelavel = System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString();

            Trace.Listeners.Add(new PlcCommon.Logs.TextWriterTraceListener());

            if (Environment.UserInteractive)
            {
                Console.Title = $"PLC Server V:{Utility.Versiyon}";

                //var consoleTracer = new ConsoleTraceListener(true);
                //consoleTracer.Flush();
                //Trace.Listeners.Add(consoleTracer);
            }

        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            WriteExceptionMessage(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteExceptionMessage(e.ExceptionObject as Exception);
        }

        private static void WriteExceptionMessage(Exception e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (e != null)
            {
                stringBuilder.AppendLine("DFS uygulamasında hata oluştu.");
                stringBuilder.AppendLine("Message : " + e.Message);
                if (e.Data != null && e.Data.Count > 0)
                {
                    foreach (object key in (IEnumerable)e.Data.Keys)
                        stringBuilder.AppendLine(key.ToString());
                }
                stringBuilder.AppendLine("TargetSite : " + (object)e.TargetSite);
                if (e.InnerException != null)
                    stringBuilder.AppendLine("Inner Exception : " + e.InnerException.Message);
                stringBuilder.AppendLine("Source  : " + e.Source);
                stringBuilder.AppendLine("Stack Trace : " + e.StackTrace);
            }
            Logger.E(stringBuilder.ToString());
            EventLog.WriteEntry(EventLogSource, stringBuilder.ToString(), EventLogEntryType.Error, 58, 1);
            MailHelper.SendMail(MailHelper.Adresler, MailHelper.MailBaslik, stringBuilder.ToString());
        }

        static void CloseAllApp()
        {
            try
            {
                using (StackRedisManager rm = new StackRedisManager())
                {
                    var keys = rm.GetAll(StackRedisManager.RedisAppKeyPrefix);
                    if (keys != null && keys.Count > 0)
                    {
                        for (int t = 0; t < keys.Count; t++)
                        {
                            try
                            {
                                var app = rm.GetApp(keys[t]);
                                if (app != null)
                                {
                                    Process p = Process.GetProcessById(app.Id);
                                    if (p != null) p.Kill();
                                }
                            }
                            catch (Exception excep)
                            {
                                Logger.I(excep);
                            }
                            //rm.Del(keys[t]);
                        }
                        rm.Del("APP:*");
                    }
                }

                var processes = Process.GetProcesses();
                for (int p = 0; p < processes.Length; p++)
                {
                    if (processes[p].ProcessName == "PlcClient")
                    {
                        processes[p].Kill();
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.I(exception);
            }
        }
    }
}
