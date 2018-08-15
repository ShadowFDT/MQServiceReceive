using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Receive
{
    public class RMQService : IDisposable
    {
        private string username { get; set; }
        private string password { get; set; }
        private string virtualhost { get; set; }
        private string hostname { get; set; }
        private string queuekey { get; set; }        
        private IConnection conn { get; set; }
        private IModel mod { get; set; }
        private string msg { get; set; }

        public void GetAppsettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            username = configuration["username"].ToString();
            password = configuration["password"].ToString();
            virtualhost = configuration["virtualhost"].ToString();
            hostname = configuration["hostname"].ToString();
            queuekey = configuration["queuekey"].ToString();
        }

        private bool RMQConnect()
        {
            ConnectionFactory connFact = new ConnectionFactory();
            connFact.UserName = username;
            connFact.Password = password;
            connFact.VirtualHost = virtualhost;
            connFact.HostName = hostname;
            connFact.RequestedHeartbeat = 60;

            int attempts = 0;
            while (attempts < 5)
            {
                attempts++;

                try
                {
                    conn = connFact.CreateConnection();
                    CreateModel();
                    return true;
                }
                catch (System.IO.EndOfStreamException e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(5000);
                    return false;
                }                
            }
            if (conn != null) conn.Dispose();
            
            return false;
        }

        private void CreateModel()
        {          
            mod = conn.CreateModel();
            mod.QueueDeclare(queue: queuekey, durable: false, exclusive: false, autoDelete: false, arguments: null);            
        }

        public string ReceiveMessage()
        {
            if (conn is null)
            {
                RMQConnect();
            }
            while (true)
            {
                ReceiveMsg();
                if (!string.IsNullOrEmpty(msg)) break;
            }

            string result = ValidateMessage(msg);
            return result;
        }

        private void ReceiveMsg()
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(mod);
                        
            consumer.Received += (model, ea) =>
            {
                byte[] msgbytes = ea.Body;
                string message = Encoding.UTF8.GetString(msgbytes);
                mod.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                msg = message;                
            };

            consumer.Shutdown += (model, ea) =>
            {
                RMQConnect();
                ReceiveMsg();
            };

            mod.BasicConsume(queue: queuekey, autoAck: false, consumer: consumer);
            Thread.Sleep(100);
        }
        
        public string ValidateMessage(string message)
        {
            string ValidMessage = "";
            if (message.Length > 0)
            {
                string[] Names = message.Split(",");
                if (Names.Length > 1)
                {
                    if (!string.IsNullOrWhiteSpace(Names[1]))
                    {
                        ValidMessage = Names[1].Trim();
                    }
                    else ValidMessage = "";
                }
                else ValidMessage = "";
            }
            return ValidMessage;
        }        

        void IDisposable.Dispose()
        {
            if (mod != null) { mod.Close(); }
            if (conn != null) { conn.Close(); conn.Dispose(); }
            GC.SuppressFinalize(this);
        }
    }
}
