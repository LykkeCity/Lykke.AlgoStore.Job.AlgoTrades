using JetBrains.Annotations;

namespace Lykke.AlgoStore.Service.AlgoTrades.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AlgoTradesSettings
    {
        public DbSettings Db { get; set; }
    }
}
