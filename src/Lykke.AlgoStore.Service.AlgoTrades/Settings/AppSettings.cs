using Lykke.AlgoStore.Service.AlgoTrades.Settings.ServiceSettings;
using Lykke.AlgoStore.Service.AlgoTrades.Settings.SlackNotifications;

namespace Lykke.AlgoStore.Service.AlgoTrades.Settings
{
    public class AppSettings
    {
        public AlgoTradesSettings AlgoTradesService { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
