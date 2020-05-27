using Newtonsoft.Json;
using PlcCommon.Logs;
using PlcCommon.Util;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlcCommon.RabbitMQ
{
    public class RabbitMQManager : IDisposable
    {
        public readonly object lockQueue = new object();
        private IConnection connection = null;
        private IModel channel = null;
        public event EventHandler<BasicDeliverEventArgs> Received;
        string QueueName = "ors.opcclient.com";
        ushort PrefetchCount = 10;

        public static readonly string QueueNameBreak = "ors.opcclient.break";
        public static readonly string QueueNameActivity = "ors.opcclient.activity";
        public static readonly string QueueNameCounter = "ors.opcclient.counter";
        public static readonly string RedisKeyPrefix = "QTY:";

        public RabbitMQManager()
        {
        }

        public RabbitMQManager(string queuename, ushort prefetchCount)
        {
            QueueName = queuename;
            PrefetchCount = prefetchCount;
            CreateConnection();
        }

        public bool IsConnected
        {
            get
            {
                return ((connection != null && connection.IsOpen) && (channel != null && channel.IsOpen));
            }
        }

        public void CreateConnection()
        {
            Monitor.Enter(lockQueue);
            try
            {
                if (channel != null)
                {
                    if (channel.IsOpen) channel.Close();
                    channel.Dispose();
                }
                if (connection != null)
                {
                    if (connection.IsOpen) connection.Close();
                    connection.Dispose();
                }

                Logger.I("Rabbit bağlantısı kuruluyor.");
                var factory = new ConnectionFactory()
                {
                    HostName = System.Configuration.ConfigurationManager.AppSettings["RabbitMQ_HostName"].ToString(),
                    UserName = System.Configuration.ConfigurationManager.AppSettings["RabbitMQ_UserName"].ToString(),
                    Password = System.Configuration.ConfigurationManager.AppSettings["RabbitMQ_Password"].ToString(),
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = false
                };
                connection = factory.CreateConnection();
                connection.AutoClose = false;
                factory.AutomaticRecoveryEnabled = true;
                factory.TopologyRecoveryEnabled = false;
                factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);

                #region Connection
                connection.CallbackException += (object sender, CallbackExceptionEventArgs e) =>
                {
                    Logger.E("Rabbit bağlantısı koptu!");
                    if (e.Exception != null)
                        Logger.E(e.Exception);

                    //MailHelper.SendMail(MailHelper.Adresler, MailHelper.MailBaslik, "Rabbit bağlantısı koptu! (Activity) ");
                    if (connection != null)
                    {
                        if (connection.IsOpen)
                            connection.Close();
                        connection.Dispose();
                    }
                    connection = null;
                };
                connection.ConnectionBlocked += (object sender, ConnectionBlockedEventArgs e) =>
                {
                    Logger.E("Rabbit bağlantısı engellendi!");
                    Logger.E(e.Reason);

                    //MailHelper.SendMail(MailHelper.Adresler, MailHelper.MailBaslik, "Rabbit bağlantısı engellendi! (Activity)");
                    if (connection != null)
                    {
                        if (connection.IsOpen)
                            connection.Close();
                        connection.Dispose();
                    }
                    connection = null;
                };
                /*connection.ConnectionShutdown += (object sender, ShutdownEventArgs e) =>
                {
                    Logger.E("Rabbit bağlantısı kapandi! (Activity)");

                    //MailHelper.SendMail(MailHelper.Adresler, MailHelper.MailBaslik, "Rabbit bağlantısı kapandi! (Activity)");
                    if (connection != null)
                    {
                        if (connection.IsOpen)
                            connection.Close();
                        connection.Dispose();
                    }
                    connection = null;
                };*/
                connection.ConnectionUnblocked += (object sender, EventArgs e) =>
                {
                    Logger.E("Rabbit bağlantısı engel kalkti!");
                    //MailHelper.SendMail(MailHelper.Adresler, MailHelper.MailBaslik, "Rabbit bağlantısı engel kalkti!");
                };
                #endregion
                #region Channel
                channel = connection.CreateModel();
                channel.CallbackException += (object sender, CallbackExceptionEventArgs e) =>
                {
                    Logger.E("Rabbit callback hatası!");
                    if (e.Exception != null)
                        Logger.E(e.Exception);

                    //MailHelper.SendMail(MailHelper.Adresler, MailHelper.MailBaslik, "Rabbit callback hatası! (Activity)");
                };
                /*channel.ModelShutdown += (object sender, ShutdownEventArgs e) =>
                {
                    Logger.E("Rabbit model hatası!");
                    //MailHelper.SendMail(MailHelper.Adresler, MailHelper.MailBaslik, "Rabbit break model hatası! (Activity)");

                    //if (e.Initiator != ShutdownInitiator.Application)
                    //{
                    //    Task.Run(() => ((AutorecoveringModel)channelActivity).AutomaticallyRecover((AutorecoveringConnection)connectionActivity, null));
                    //}
                };*/
                channel.QueueDeclare(queue: QueueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                #endregion

                if (Received != null)
                {
                    Consume();
                }

                Logger.I("Rabbit bağlandı.");
            }
            catch (Exception exception)
            {
                Logger.E(exception);
            }

            Monitor.Exit(lockQueue);

        }

        public void Publish(object publishObject)
        {
            try
            {
                Logger.I("Rabbit Publish.");

                if (!IsConnected) CreateConnection();
                string message = JsonConvert.SerializeObject(publishObject);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "",
                                     routingKey: QueueName,
                                     basicProperties: null,
                                     body: body);

                Logger.I("Rabbit Publish başarılı.");
            }
            catch (Exception exception)
            {
                Logger.E(exception);
            }
        }

        public void Consume(bool _noAck = false)
        {
            try
            {
                Logger.I("Rabbit Consume.");

                if (!IsConnected) CreateConnection();
                var consumer = new EventingBasicConsumer(channel);
                if (Received != null)
                    consumer.Received += Received;
                channel.BasicQos(0, PrefetchCount, false);
                channel.BasicConsume(queue: QueueName,
                                     noAck: _noAck, // true olursa mesaj okunduğunda silinir.
                                     consumer: consumer);

                Logger.I("Rabbit Consume başarılı.");
            }
            catch (Exception exception)
            {
                Logger.E(exception);
            }
        }

        public void BasicAck(ulong deliveryTag, bool multiple = false)
        {
            try
            {
                Logger.I("Rabbit BasicAck.");

                if (!IsConnected) CreateConnection();
                channel.BasicAck(deliveryTag, multiple);

                Logger.I("Rabbit BasicAck başarılı.");
            }
            catch (Exception exception)
            {
                Logger.E(exception);
            }
        }

        public void BasicNack(ulong deliveryTag, bool multiple = false, bool requeue = true)
        {
            try
            {
                Logger.I("Rabbit BasicNack.");

                if (!IsConnected) CreateConnection();
                channel.BasicNack(deliveryTag, multiple, requeue);

                Logger.I("Rabbit BasicNack başarılı.");
            }
            catch (Exception exception)
            {
                Logger.E(exception);
            }
        }

        public void BasicReject(ulong deliveryTag, bool multiple = false)
        {
            try
            {
                Logger.I("Rabbit BasicReject.");

                if (!IsConnected) CreateConnection();
                channel.BasicReject(deliveryTag, multiple);

                Logger.I("Rabbit BasicReject başarılı.");
            }
            catch (Exception exception)
            {
                Logger.E(exception);
            }
        }

        #region IDisposable
        ~RabbitMQManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (channel != null)
                {
                    channel.Close();
                    channel.Dispose();
                }
                if (connection != null)
                {
                    if (connection.IsOpen)
                        connection.Close();
                    connection.Dispose();
                }
                channel = null;
                connection = null;
            }

            disposed = true;
        }
        #endregion
    }
}
