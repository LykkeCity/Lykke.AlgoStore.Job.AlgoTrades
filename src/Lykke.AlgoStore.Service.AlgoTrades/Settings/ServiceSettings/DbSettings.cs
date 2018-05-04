using Lykke.SettingsReader.Attributes;

namespace Lykke.AlgoStore.Service.AlgoTrades.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
