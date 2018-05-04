using Lykke.SettingsReader.Attributes;

namespace Lykke.AlgoStore.Job.AlgoTrades.Settings.JobSettings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public string ExchangeOperationsHistory { get; set; }

        public string QueueAlgoTradesUpdater { get; set; }
    }
}
