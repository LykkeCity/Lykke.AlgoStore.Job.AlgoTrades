using Lykke.SettingsReader.Attributes;

namespace Lykke.AlgoStore.Service.AlgoTrades.Settings
{
    public class AssetsServiceClientSettings
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
