using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.AlgoStore.Service.Statistics.Client;
using Lykke.Service.OperationsRepository.Contract.Cash;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;

namespace Lykke.AlgoStore.Service.AlgoTrades.Services
{
    public class AlgoInstanceTradesCountUpdater : IAlgoInstanceTradesCountUpdater
    {
        private readonly IStatisticsClient _statisticsClient;
        private readonly IAlgoClientInstanceRepository _clientInstanceRepository;
        private readonly ILog _log;

        public AlgoInstanceTradesCountUpdater(
            IStatisticsClient statisticsClient,
            IAlgoClientInstanceRepository clientInstanceRepository,
            ILogFactory logFactory)
        {
            _statisticsClient = statisticsClient;
            _clientInstanceRepository = clientInstanceRepository;
            _log = logFactory.CreateLog(this);
        }

        public async Task IncreaseInstanceTradeCountAsync(
            ClientTradeDto clientTrade,
            AlgoInstanceTrade instanceTrade)
        {
            _log.Info($"IncreaseInstanceTradeCountAsync started. InstanceId: {instanceTrade.InstanceId}, WalletId: {instanceTrade.WalletId}");

            var instance = await _clientInstanceRepository
                .GetAlgoInstanceDataByWalletIdAsync(instanceTrade.WalletId, instanceTrade.InstanceId);

            _log.Info($"TradedAssetId: {instance.TradedAssetId}, AssetId: {clientTrade.AssetId}");

            if (instance.TradedAssetId != clientTrade.AssetId) return;

            _log.Info("_statisticsClient.IncreaseTotalTradesAsync started.");

            await _statisticsClient.IncreaseTotalTradesAsync(instance.AuthToken.ToBearerToken());

            _log.Info("_statisticsClient.IncreaseTotalTradesAsync finished.");
            _log.Info($"IncreaseInstanceTradeCountAsync finished.");
        }
    }
}
