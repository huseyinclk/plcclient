using Newtonsoft.Json;
using PlcCommon.Logs;
using PlcCommon.Model;
using PlcCommon.Util;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlcCommon.RedisStore
{
    public class StackRedisManager : IDisposable
    {
        IDatabase client = null;
        ConnectionMultiplexer multiplexer;
        static readonly object LockForLogging = new object();
        public const string RedisKeyPrefix = "QTY:";
        public const string RedisAppKeyPrefix = "APP:";
        //string connectionStr = ConfigurationManager.AppSettings["RedisConnectionString"];
        bool isCacheZipEnabled = true;//bool.Parse(ConfigurationManager.AppSettings["IsCacheZipEnabled"]);

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        IDatabase Client
        {
            get
            {
                if (multiplexer == null || !multiplexer.IsConnected)
                {
                    if (multiplexer != null)
                    {
                        multiplexer.Close(false);
                        multiplexer.Dispose();
                        multiplexer = null;
                    }

                    var configurationOptions = new ConfigurationOptions
                    {
                        AbortOnConnectFail = false,
                        Ssl = false,
                        ConnectRetry = 3,
                        ConnectTimeout = 30000,
                        SyncTimeout = 30000,
                        DefaultDatabase = 0,
                        EndPoints = { System.Configuration.ConfigurationManager.AppSettings["RedisStore_HostName"].ToString() },
                        Password = System.Configuration.ConfigurationManager.AppSettings["RedisStore_Password"].ToString()
                    };

                    multiplexer = ConnectionMultiplexer.Connect(configurationOptions);
                    multiplexer.ConnectionFailed += multiplexer_ConnectionFailed;
                    multiplexer.ErrorMessage += multiplexer_ErrorMessage;
                    multiplexer.InternalError += multiplexer_InternalError;
                }

                client = multiplexer.GetDatabase();

                return client;
            }
        }

        /// <summary>
        /// Initializes the <see cref="StackRedisManager"/> class.
        /// </summary>
        public StackRedisManager()
        {
            //multiplexer = ConnectionMultiplexer.Connect(connectionStr);
            //client = multiplexer.GetDatabase();

            //multiplexer.ConnectionFailed += multiplexer_ConnectionFailed;
            //multiplexer.ErrorMessage += multiplexer_ErrorMessage;
            //multiplexer.InternalError += multiplexer_InternalError;
        }

        public PinValue GetValue(string _redisKey)
        {
            PinValue value = new PinValue();
            try
            {
                Logger.I(string.Format("Get from cache this key: {0}", _redisKey));
                var hashEntry = Client.HashGetAll(_redisKey);
                if (hashEntry != null && hashEntry.Length > 0)
                {
                    if (hashEntry.Length > 0)
                        value.Count = (int)hashEntry[0].Value;
                    if (hashEntry.Length > 1)
                        value.Time = (int)hashEntry[1].Value;
                    if (hashEntry.Length > 2)
                        value.IsBreak = hashEntry[2].Value == "1";
                    if (hashEntry.Length > 3)
                        value.SessionId = hashEntry[3].Value;
                    if (hashEntry.Length > 4)
                        value.Id = Convert.ToInt32(hashEntry[4].Value);
                    if (hashEntry.Length > 5)
                        value.WstationCode = hashEntry[5].Value;
                    if (hashEntry.Length > 6)
                        value.IsRework = hashEntry[6].Value == "1";
                    if (hashEntry.Length > 7)
                        value.RCount = Convert.ToInt32(hashEntry[7].Value);
                    if (hashEntry.Length > 8)
                        value.RTime = Convert.ToInt32(hashEntry[8].Value);
                    if (hashEntry.Length > 9)
                        value.Date = Convert.ToDateTime(hashEntry[9].Value.ToString());
                }
            }
            catch (Exception exc)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error in Get from cache this key: {0}, Err:{1}", _redisKey, exc.Message));
                }
            }
            return value;
        }

        public void SetValue(string _redisKey, PinValue value)
        {
            try
            {
                Logger.I(string.Format("Set to cache this key: {0}", _redisKey));
                Client.HashSet(_redisKey, new HashEntry[] {
                            new HashEntry("qty", value.Count),
                            new HashEntry("time", value.Time),
                            new HashEntry("break", value.IsBreak == true ? "1" : "0"),
                            new HashEntry("sid", Utility.ApplicationSessionId),
                            new HashEntry("did", value.IsBreak),
                            new HashEntry("code", value.WstationCode.StringNull()),
                            new HashEntry("isrework",value.IsRework == true ? "1":"0"),
                            new HashEntry("rcount",value.RCount),
                            new HashEntry("rtime",value.RTime),
                            new HashEntry("date",value.Date.ToString("yyyy-MM-dd HH:mm:ss"))});
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error in Get from cache this key: {0}, Err:{1}", _redisKey, ex.Message));
                }
            }
        }

        public AppInfo GetApp(string _redisKey)
        {
            AppInfo app = null;
            try
            {
                Logger.I(string.Format("Get from cache this key: {0}", _redisKey));
                var hashEntry = Client.HashGetAll(_redisKey);
                if (hashEntry != null && hashEntry.Length > 0)
                {
                    app = new AppInfo();
                    if (hashEntry.Length > 0)
                        app.Id = (int)hashEntry[0].Value;
                    if (hashEntry.Length > 1)
                        app.AppKey = hashEntry[1].Value;
                    if (hashEntry.Length > 2)
                        app.Statu = hashEntry[2].Value;
                    if (hashEntry.Length > 3)
                        app.Description = hashEntry[3].Value;
                    if (hashEntry.Length > 4)
                        app.StartDate = Convert.ToDateTime(hashEntry[4].Value);
                    if (hashEntry.Length > 5)
                        app.Duration = Convert.ToInt32(hashEntry[5].Value);
                }
            }
            catch (Exception exc)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error in Get from cache this key: {0}, Err:{1}", _redisKey, exc.Message));
                }
            }
            return app;
        }

        public void SetApp(string _redisKey, AppInfo value)
        {
            try
            {
                Logger.I(string.Format("Set to cache this key: {0}", _redisKey));
                Client.HashSet(_redisKey, new HashEntry[] {
                            new HashEntry("id", value.Id),
                            new HashEntry("key", value.AppKey.StringNull()),
                            new HashEntry("statu", value.Statu.StringNull()),
                            new HashEntry("desc", value.Description.StringNull()),
                            new HashEntry("start", value.StartDate.ToString("yyyy-MM-dd HH:mm:ss")),
                            new HashEntry("duration", value.Duration)});
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error in Get from cache this key: {0}, Err:{1}", _redisKey, ex.Message));
                }
            }
        }


        public void SetDevice(string _redisKey, AutomationDeviceInfo value)
        {
            try
            {
                Logger.I(string.Format("Set to cache this key: {0}", _redisKey));
                Client.HashSet(_redisKey, new HashEntry[] {
                            new HashEntry("id", value.AutomationDeviceId),
                            new HashEntry("host", value.DeviceHost),
                            new HashEntry("check", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                            new HashEntry("statu", value.IsConnected == true ? "online":"offline")});
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error in Get from cache this key: {0}, Err:{1}", _redisKey, ex.Message));
                }
            }
        }

        public void SetHash(string _redisKey, HashEntry[] hashFields)
        {
            try
            {
                Logger.I(string.Format("Set to cache this key: {0}", _redisKey));
                Client.HashSet(_redisKey, hashFields);
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error in Get from cache this key: {0}, Err:{1}", _redisKey, ex.Message));
                }
            }
        }

        public HashEntry[] GetHash(string _redisKey)
        {
            try
            {
                Logger.I(string.Format("Set to cache this key: {0}", _redisKey));
                return Client.HashGetAll(_redisKey);
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error in Get from cache this key: {0}, Err:{1}", _redisKey, ex.Message));
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the specified cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            var result = default(T);

            try
            {
                var jsonData = Client.StringGet(key);
                if (jsonData.HasValue)
                {
                    result = JsonConvert.DeserializeObject<T>(jsonData);
                }
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error in Get from cache this key: {0}", key));
                }
            }

            return result;
        }

        /// <summary>
        /// Gets from zip.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T GetFromZip<T>(string key)
        {
            var result = default(T);

            try
            {
                var jsonData = Client.StringGet(key);
                if (jsonData.HasValue)
                {
                    var data = (isCacheZipEnabled) ? DecompressString(jsonData) : jsonData.ToString();
                    result = JsonConvert.DeserializeObject<T>(data);
                }
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error in GetFromZip this key: {0}", key));
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the specified cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(string key, object value)
        {
            try
            {
                var jsonList = JsonConvert.SerializeObject(value);
                Client.StringSet(key, jsonList);
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error in Set to cache this key: {0}", key));
                }
            }
        }

        /// <summary>
        /// Sets to zip.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetToZip(string key, object value)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(value);
                var data = (isCacheZipEnabled) ? CompressString(jsonData) : jsonData;

                Client.StringSet(key, data);
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error in SetToZip this key: {0}", key));
                }
            }
        }

        /// <summary>
        /// Deletes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Del(string key)
        {
            try
            {
                Client.KeyDelete(key);
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Error deleting from cache this key: {0}", key));
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (client != null && client.Multiplexer != null)
            {
                client.Multiplexer.Close(true);
                client.Multiplexer.Dispose();
                client = null;
            }
        }

        /// <summary>
        /// Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        string CompressString(string text)
        {
            var result = string.Empty;

            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(text);
                var memoryStream = new MemoryStream();
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gZipStream.Write(buffer, 0, buffer.Length);
                }

                memoryStream.Position = 0;

                var compressedData = new byte[memoryStream.Length];
                memoryStream.Read(compressedData, 0, compressedData.Length);

                var gZipBuffer = new byte[compressedData.Length + 4];
                Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
                result = Convert.ToBase64String(gZipBuffer);
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Cache Exception Policy: {0}", ex.Message));
                }
            }

            return result;
        }

        /// <summary>
        /// Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns></returns>
        string DecompressString(string compressedText)
        {
            var result = string.Empty;

            try
            {
                byte[] gZipBuffer = Convert.FromBase64String(compressedText);
                using (var memoryStream = new MemoryStream())
                {
                    int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                    memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                    var buffer = new byte[dataLength];

                    memoryStream.Position = 0;
                    using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        gZipStream.Read(buffer, 0, buffer.Length);
                    }

                    result = Encoding.UTF8.GetString(buffer);
                }
            }
            catch (Exception ex)
            {
                lock (LockForLogging)
                {
                    Logger.W(string.Format("Cache Exception Policy: {0}", ex.Message));
                }
            }

            return result;
        }

        /// <summary>
        /// Handles the InternalError event of the multiplexer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="InternalErrorEventArgs"/> instance containing the event data.</param>
        void multiplexer_InternalError(object sender, InternalErrorEventArgs e)
        {
            var exc = e.Exception;
            Logger.W(string.Format("Error in Get from cache this key: {0}", exc));
        }

        /// <summary>
        /// Handles the ErrorMessage event of the multiplexer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RedisErrorEventArgs"/> instance containing the event data.</param>
        void multiplexer_ErrorMessage(object sender, RedisErrorEventArgs e)
        {
            var mssg = e.Message;
            Logger.W(string.Format("Error in Get from cache this key: {0}", mssg));
        }

        /// <summary>
        /// Handles the ConnectionFailed event of the multiplexer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ConnectionFailedEventArgs"/> instance containing the event data.</param>
        void multiplexer_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            var ex = e.Exception;
            var configurationOptions = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                Ssl = false,
                ConnectRetry = 3,
                ConnectTimeout = 30000,
                SyncTimeout = 30000,
                DefaultDatabase = 0,
                EndPoints = { System.Configuration.ConfigurationManager.AppSettings["RedisStore_HostName"].ToString() },
                Password = System.Configuration.ConfigurationManager.AppSettings["RedisStore_Password"].ToString()
            };
            multiplexer = ConnectionMultiplexer.Connect(configurationOptions);
            client = multiplexer.GetDatabase();
            Logger.W(string.Format("Error in Get from cache this key: {0}", ex.Message));
        }

        public List<string> GetAll(string pattern = null)
        {
            var result = new List<string>();
            var endpoints = Client.Multiplexer.GetEndPoints();
            var server = Client.Multiplexer.GetServer(endpoints.First());

            var keys = server.Keys();
            foreach (var key in keys)
            {
                if(!string.IsNullOrWhiteSpace(pattern))
                {
                    if (key.ToString().IndexOf(pattern) != -1)
                        result.Add(key);
                }
                else
                {
                    result.Add(key);
                }
                Logger.I(key.ToString());
            }
            return result;
        }
    }
}
