
using System;
using System.Threading.Tasks;
using Lykke.AlgoStore.Service.AlgoTrades.Client.Models;

namespace Lykke.AlgoStore.Service.AlgoTrades.Client
{
    public interface IAlgoTradesClient
    {
        Task<AlgoInstanceTradeResponse> GetAlgoInstanceTradesByTradedAsset(string instanceId, string tradedAssetId,
            int maxNumberToFetch);

        Task<AlgoInstanceTradeResponse> GetAlgoInstanceTradesByPeriod(string instanceId, string tradedAssetId,
            DateTime from, DateTime to);
    }
}
