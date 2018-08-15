using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Receive
{
    class ReceiveMQ
    {      
        static void Main(string[] args)
        {
            Console.Title = "Receive in microservice";            
            RMQService rmqService = new RMQService();
            rmqService.GetAppsettings();
            string name = rmqService.ReceiveMessage();

            Console.WriteLine("Received: Hello {0}, I am your father!", name);
            Console.WriteLine("Press[enter] to exit.");
            Console.ReadKey();
            Environment.Exit(-1);
        }        
    }
}
