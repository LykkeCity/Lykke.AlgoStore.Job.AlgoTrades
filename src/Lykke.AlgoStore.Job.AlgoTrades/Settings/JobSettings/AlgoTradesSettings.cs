namespace Lykke.AlgoStore.Job.AlgoTrades.Settings.JobSettings
{
    public class AlgoTradesSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings Rabbit { get; set; }
        public MatchingEngineRabbitMqSettings MatchingEngineRabbitMq { get; set; }
    }
}
