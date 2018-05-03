using Lykke.AlgoStore.Job.AlgoTrades.Settings.JobSettings;
using Lykke.AlgoStore.Job.AlgoTrades.Settings.SlackNotifications;

namespace Lykke.AlgoStore.Job.AlgoTrades.Settings
{
    public class AppSettings
    {
        public AlgoTradesSettings AlgoTradesJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
