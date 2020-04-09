using System;
using System.Text;
using System.Threading;
using System.Xml;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Test
{
    class Program
    {
        private static readonly AutoResetEvent _closingEvent = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "guest";
            factory.Password = "guest";
            factory.HostName = "10.3.56.9";
            factory.Port = 5672;
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare("heartbeat-exchange", ExchangeType.Direct);
                    channel.QueueDeclare("heartbeat-queue", true, false, false, null);
                    channel.QueueBind("heartbeat-queue", "heartbeat-exchange", "heartbeat", null);

                    var consumer = new EventingBasicConsumer(channel);
                    Console.WriteLine("Before");

                    consumer.Received += (sender, args) =>
                    {
                        var msg = Encoding.UTF8.GetString(args.Body);
                        Console.WriteLine(msg);
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(msg);
                        Console.WriteLine(xmlDoc.OuterXml);
                    };

                    channel.BasicConsume("heartbeat-queue", true, consumer);
                    Console.WriteLine("Press Ctrl + C to cancel!");
                    Console.CancelKeyPress += ((s, a) =>
                    {
                        Console.WriteLine("Bye!");
                        _closingEvent.Set();
                    });

                    _closingEvent.WaitOne();
                }
            }
        }
    }
}
