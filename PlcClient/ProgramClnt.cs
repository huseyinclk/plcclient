using Newtonsoft.Json;
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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace PlcClient
{
    class ProgramClnt
    {
        const string EventLogSource = "PlcClient";
        private static RabbitMQManager counterConsumer;
        private static List<AutomationDeviceInfo> devices = null;
        static System.Timers.Timer timerCounter = null;
        static string applicationKey = "";

        static void Main(string[] args)
        {
            try
            {
                if (args != null && args.Length > 0)
                {
                    applicationKey = args[0];
                    InitialLogger();
                    while (true)
                    {
                        if (Environment.UserInteractive)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                        }
                        Stopwatch sw = Stopwatch.StartNew();
                        Logger.I($"Okuma başlıyor {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}");
                        using (RabbitMQManager activityQueue = new RabbitMQManager(RabbitMQManager.QueueNameActivity, 10))
                        using (RabbitMQManager breakQueue = new RabbitMQManager(RabbitMQManager.QueueNameBreak, 10))
                        using (StackRedisManager rm = new StackRedisManager())
                        {
                            using (NpgsqlProvider db = new NpgsqlProvider())
                            {
                                if (!db.Connect())
                                {
                                    Logger.E("Veritabanına bağlanılamadı!");
                                }

                                if (devices == null)
                                    devices = db.DeviceDetails(args[0]);

                                if (devices != null && devices.Count > 0)
                                {
                                    for (int i = 0; i < devices.Count; i++)
                                    {
                                        if (object.ReferenceEquals(devices[i].Device, null))
                                        {
                                            Logger.I($"PLC bağlanılıyor HOST:{devices[i].DeviceHost}");
                                            devices[i].Device = new PlcCommon.S7.Net.Plc(PlcCommon.S7.Net.CpuType.S71200, devices[i].DeviceHost, 0, 1);
                                            devices[i].Device.Open();
                                        }

                                        if (!devices[i].Device.IsConnected)
                                        {
                                            Logger.I($"PLC ulaşılamıyor HOST:{devices[i].DeviceHost}");
                                            string _redisKey = string.Concat(StackRedisManager.RedisKeyPrefix, devices[i].DeviceHost, ":Status");
                                            rm.SetHash(_redisKey, new HashEntry[] {
                                                                        new HashEntry("statu", "offline"),
                                                                        new HashEntry("check", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))});
                                            continue;
                                        }
                                        else
                                        {
                                            Logger.I($"PLC Okunuyor HOST:{devices[i].DeviceHost}");
                                            string _redisKey = string.Concat(StackRedisManager.RedisKeyPrefix, devices[i].DeviceHost, ":Status");
                                            rm.SetHash(_redisKey, new HashEntry[] {
                                                                        new HashEntry("statu", "online"),
                                                                        new HashEntry("check", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))});

                                            for (int loop = 0; loop < devices[i].DeviceDInfo.Count; loop++)
                                            {
                                                _redisKey = string.Concat(StackRedisManager.RedisKeyPrefix, devices[i].DeviceHost, ":", devices[i].DeviceDInfo[loop].Address, ":Pin");
                                                int value = 0;
                                                var xval = devices[i].Device.Read(devices[i].DeviceDInfo[loop].Address);
                                                if (xval != null)
                                                {
                                                    value = Convert.ToInt32(xval);
                                                }
                                                DateTime now = DateTime.Now;
                                                var pinVal = rm.GetValue(_redisKey);
                                                if (pinVal == null)
                                                {
                                                    pinVal = new PinValue();
                                                    pinVal.IsBreak = false;
                                                    pinVal.IsRework = false;
                                                    pinVal.Time = (int)Utility.ConvertToUnixTime(DateTime.Now);
                                                    pinVal.Date = DateTime.Now;
                                                    pinVal.WstationCode = devices[i].DeviceDInfo[loop].WstationCode;
                                                }

                                                if (pinVal.Date.Year < 2020) pinVal.Date = Utility.UnixTimeToDateTime(pinVal.Time);

                                                DateTime time = Utility.UnixTimeToDateTime(pinVal.Time);
                                                Logger.I($"Istasyon:{devices[i].DeviceDInfo[loop].WstationCode},HOST:{devices[i].DeviceHost}, Adres:{devices[i].DeviceDInfo[loop].Address}, PLC={value}");
                                                if (value == pinVal.Count && !pinVal.IsBreak && pinVal.SessionId == Utility.ApplicationSessionId)
                                                {
                                                    var times = (now - time).TotalSeconds; // toplam sure farki
                                                    if (times >= devices[i].DeviceDInfo[loop].BreakDuration) // son deger geldikten sonra, istasyondan gelen sure parametresine gore durus baslatilacak
                                                    {
                                                        Logger.I($"{devices[i].DeviceHost} Log kaydı, kod:{devices[i].DeviceDInfo[loop].WstationCode}, id:{devices[i].DeviceDInfo[loop].WstationId} adres:{devices[i].DeviceDInfo[loop].Address}");

                                                        pinVal.IsBreak = true; // durusta deger gelmiyor
                                                        AutomationBreakInfo automationBreak = new AutomationBreakInfo();
                                                        automationBreak.StartDate = pinVal.Date; // son deger alinan zaman olmali
                                                        automationBreak.AutomationDeviceDId = devices[i].DeviceDInfo[loop].AutomationDeviceDId;
                                                        automationBreak.AutomationDeviceMId = devices[i].DeviceDInfo[loop].AutomationDeviceId;
                                                        automationBreak.WstationId = devices[i].DeviceDInfo[loop].WstationId;
                                                        automationBreak.WstationCode = devices[i].DeviceDInfo[loop].WstationCode;
                                                        breakQueue.Publish(automationBreak);
                                                    }
                                                }
                                                else if (value == pinVal.Count && pinVal.IsBreak) //&& _sessionid == Utility.ApplicationSessionId)
                                                {
                                                    //hala durusta demektir
                                                }
                                                else
                                                {
                                                    pinVal.Id = devices[i].DeviceDInfo[loop].AutomationDeviceDId;
                                                    pinVal.IsBreak = false; // deger geliyor durus
                                                    pinVal.Time = (int)Utility.ConvertToUnixTime(now);
                                                    pinVal.Date = now;

                                                    Logger.I($"{devices[i].DeviceHost} Duruş kaydı, kod:{devices[i].DeviceDInfo[loop].WstationCode}, id:{devices[i].DeviceDInfo[loop].WstationId} adres:{devices[i].DeviceDInfo[loop].Address}");

                                                    AutomationActivityTimeInfo automationActivityTime = new AutomationActivityTimeInfo();
                                                    automationActivityTime.Date = now;
                                                    automationActivityTime.TimeDifference = now - time;
                                                    automationActivityTime.AutomationDeviceDId = devices[i].DeviceDInfo[loop].AutomationDeviceDId;
                                                    automationActivityTime.AutomationDeviceMId = devices[i].DeviceDInfo[loop].AutomationDeviceId;
                                                    automationActivityTime.WstationId = devices[i].DeviceDInfo[loop].WstationId;
                                                    automationActivityTime.WstationCode = devices[i].DeviceDInfo[loop].WstationCode;
                                                    automationActivityTime.Qty = value;
                                                    automationActivityTime.QtyDifference = value - pinVal.Count;
                                                    activityQueue.Publish(automationActivityTime);
                                                }

                                                if (pinVal.IsRework)
                                                {
                                                    pinVal.RCount = value - pinVal.Count;
                                                    value = pinVal.Count;
                                                }

                                                pinVal.Count = value;
                                                rm.SetValue(_redisKey, pinVal);
                                            }// okuma end

                                            Logger.I($"{devices[i].DeviceHost}\tOkundu.");
                                            // Here, the second application initializes what it needs to.
                                            // When it's done, it signals the wait handle:
                                        }
                                    }
                                }

                            }

                            AppInfo appinfo = new AppInfo();
                            appinfo.Statu = "live";
                            appinfo.StartDate = DateTime.Now;
                            appinfo.Id = Process.GetCurrentProcess().Id;
                            appinfo.Duration = (int)sw.Elapsed.TotalSeconds;
                            rm.SetApp(string.Concat(StackRedisManager.RedisAppKeyPrefix, applicationKey, ":Status"), appinfo);

                        }

                        sw.Stop();
                        if (Environment.UserInteractive)
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        Logger.I($"Okuma bitti {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Süre:{sw.Elapsed.TotalSeconds}");

                        int interval = 30000;
                        string Interval = "30000";
                        if (System.Configuration.ConfigurationManager.AppSettings["Interval"] != null)
                            Interval = System.Configuration.ConfigurationManager.AppSettings["Interval"].ToString();
                        int.TryParse(Interval, out interval);

                        Thread.Sleep(interval);

                        //bool xlive = true;
                        //Mutex singleMutex = new Mutex(true, "PLCCHECK", out xlive);
                        //singleMutex.ReleaseMutex();
                    }//end of while
                }

                Environment.Exit(1);

            }
            catch (NullReferenceException nullexc)
            {
                Logger.E(nullexc);
                Environment.Exit(-1);
            }
            catch (Exception exc)
            {
                Logger.E(exc);
                Environment.Exit(-1);
            }
        }


        private static void TimerCallback(object sender, ElapsedEventArgs e)
        {
            try
            {
                timerCounter.Enabled = false;
                if (devices != null && devices.Count > 0)
                {
                    foreach (AutomationDeviceInfo device in devices)
                    {
                        using (StackRedisManager rm = new StackRedisManager())
                        {
                            try
                            {
                                if (object.ReferenceEquals(device.Device, null))
                                {
                                    device.Device = new PlcCommon.S7.Net.Plc(PlcCommon.S7.Net.CpuType.S71200, device.DeviceHost, 0, 1);
                                    device.Device.Open();
                                    rm.SetDevice(string.Concat(StackRedisManager.RedisAppKeyPrefix, applicationKey, ":D:", device.DeviceHost, ":S"), device);
                                    if (!device.IsConnected)
                                    {
                                        continue;
                                    }
                                }

                                foreach (AutomationDeviceDInfo deviceDInfo in device.DeviceDInfo)
                                {
                                    int value = 0;
                                    object plcVal = device.Device.Read(deviceDInfo.Address);
                                    if (plcVal != null)
                                    {
                                        value = Convert.ToInt32(plcVal);
                                    }

                                    rm.SetHash(string.Concat(StackRedisManager.RedisAppKeyPrefix, applicationKey, ":D:", device.DeviceHost, ":", deviceDInfo.Address), new HashEntry[] { new HashEntry("val", value), new HashEntry("check", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) });
                                }

                            }
                            catch (Exception exc)
                            {
                                Logger.E(exc);
                            }
                        }

                    }
                }
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


        static void InitialLogger()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("tr-TR");
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ProgramClnt.CurrentDomain_UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(ProgramClnt.Application_ThreadException);

            Trace.Listeners.Add(new PlcCommon.Logs.TextWriterTraceListener());

            if (Environment.UserInteractive)
            {
                string tracelavel = "";
                if (System.Configuration.ConfigurationManager.AppSettings["tracelavel"] != null)
                    tracelavel = System.Configuration.ConfigurationManager.AppSettings["tracelavel"].ToString();

                Console.Title = $"PLC Client V:{Utility.Versiyon} Device:{applicationKey} Log:{tracelavel}";

                var consoleTracer = new ConsoleTraceListener(true);
                consoleTracer.Flush();
                Trace.Listeners.Add(consoleTracer);

                //Console.WriteLine("Press any key to stop...");
                //Console.ReadKey(true);
            }

            Application.ThreadException += Application_ThreadException;
        }


        static void CounterConsume()
        {
            try
            {
                counterConsumer = new RabbitMQManager(RabbitMQManager.QueueNameCounter, 10);
                counterConsumer.Received += CounterConsumer_Received;
                counterConsumer.Consume();
            }
            catch (Exception exc)
            {
                Logger.E("Application Error:" + exc.Message);
                Logger.E(exc);
            }
        }


        private static void CounterConsumer_Received(object sender, RabbitMQ.Client.Events.BasicDeliverEventArgs e)
        {
            Monitor.Enter(counterConsumer);
            if (e != null)
            {
                try
                {
                    Logger.I("Counter received");
                    var message = Encoding.UTF8.GetString(e.Body);
                    if (!string.IsNullOrEmpty(message))
                    {
                        WorderAcOpInfo worderInfo = JsonConvert.DeserializeObject<WorderAcOpInfo>(message);
                        if (worderInfo != null)
                        {
                            try
                            {
                                Logger.W($"Counter reset received:{worderInfo.WstationCode}");
                                //var plc = device.Where(p => p.IP == worderInfo.HostName).FirstOrDefault();
                                //if (plc != null)
                                //{
                                //    if (plc.CounterReset(worderInfo.Address, worderInfo.Qty))
                                //    {
                                //        Console.BackgroundColor = ConsoleColor.Blue;
                                //        counterConsumer.BasicAck(e.DeliveryTag, false);
                                //        Logger.I("ors.opcclient.counter-->" + worderInfo.WstationCode);
                                //        Console.WriteLine("ors.opcclient.counter-->" + worderInfo.WstationCode);
                                //    }
                                //    else
                                //    {
                                //        Logger.E("ors.opcclient.counter-->Error:" + worderInfo.WstationCode);
                                //        counterConsumer.BasicReject(e.DeliveryTag, false); // hata olduğunda siliyoruz şimdilik
                                //    }
                                //}
                                //else
                                //{
                                //    Logger.E("Counter plc bulunamadı:" + worderInfo.HostName);
                                //}
                            }
                            catch (Exception exc)
                            {
                                Logger.E("counter.Error-->" + worderInfo.WstationCode + ",Detay:" + exc.Message);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    counterConsumer.BasicReject(e.DeliveryTag, false); // hata olduğunda siliyoruz şimdilik
                    Logger.E("counter.Error-->CounterPublisher_Received,Detay:" + exception.Message);
                }
            }
            Monitor.Exit(counterConsumer);
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
    }
}

/*
1013,1022,985,986,987,988,989,990,991,992
993,1023,994,1024,1025,1026,1027,1028,1029,1030
1031,1015,1032,1033,1034,1035,1036,1037,1038,1039
1040,1016,1041,1042,1043,1044,1045,1047,1046,1048
1014,1017,1018,1019,1020,1021,1062,1067,1068,1069
1070,1071,1072,1073,1074,1075,1076,1063,1077,1078
1079,1050,1080,1081,1082,1083,1084,1085,1086,1064
1087,1088,1089,1090,1091,1092,1093,1094,1095,1096
1065,1097,1098,1099,1066,995,1004,1005,1006,1007
1008,1009,1010,996,997,998,999,1000,1001,1002,1003
     */
