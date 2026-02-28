namespace Web.Options
{
    public class RabbitMqOptions
    {
        public string Host { get; set; } = null!;
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public RabbitQueues Queues { get; set; } = null!;
    }

    public class RabbitQueues
    {
    }
}