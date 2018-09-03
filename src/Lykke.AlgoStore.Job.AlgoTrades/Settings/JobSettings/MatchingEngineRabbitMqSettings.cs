namespace Lykke.AlgoStore.Job.AlgoTrades.Settings.JobSettings
{
    public class MatchingEngineRabbitMqSettings
    {
        public string ConnectionString { get; set; }
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }
        public bool IsDurable { get; set; }
    }
}
