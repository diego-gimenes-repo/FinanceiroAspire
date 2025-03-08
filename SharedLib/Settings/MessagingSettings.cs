using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.Settings
{
    public class MessagingSettings
    {
        //  RabbitMQ, Kafka
        public string Provider { get; set; } = "RabbitMQ";
        public string Prefix { get; set; } = "amqp://";
        public string HostUri { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Additional fields 
        public int Port { get; set; } = 5672;
        
    }
}
