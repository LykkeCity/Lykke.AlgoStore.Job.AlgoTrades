using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.AlgoStore.Service.Statistics.Client;
using Lykke.Service.OperationsRepository.Contract.Cash;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Service.AlgoTrades.Services
{
    public class AlgoInstanceTradesCountUpdater : IAlgoInstanceTradesCountUpdater
    {
        private readonly IStatisticsClient _statisticsClient;
        private readonly IAlgoClientInstanceRepository _clientInstanceRepository;

        public AlgoInstanceTradesCountUpdater(
            IStatisticsClient statisticsClient,
            IAlgoClientInstanceRepository clientInstanceRepository)
        {
            _statisticsClient = statisticsClient;
            _clientInstanceRepository = clientInstanceRepository;
        }

        public async Task IncreaseInstanceTradeCountAsync(
            ClientTradeDto clientTrade,
            AlgoInstanceTrade instanceTrade)
        {
            var instance = await _clientInstanceRepository
                .GetAlgoInstanceDataByWalletIdAsync(instanceTrade.WalletId, instanceTrade.InstanceId);

            if (instance.TradedAssetId != clientTrade.AssetId) return;

            await _statisticsClient.IncreaseTotalTradesAsync(instance.AuthToken.ToBearerToken());
        }
    }
}
