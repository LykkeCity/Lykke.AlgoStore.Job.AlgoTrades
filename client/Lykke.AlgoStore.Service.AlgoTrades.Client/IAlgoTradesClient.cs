
using System.Threading.Tasks;
using Lykke.AlgoStore.Service.AlgoTrades.Client.Models;

namespace Lykke.AlgoStore.Service.AlgoTrades.Client
{
    public interface IAlgoTradesClient
    {
        Task<AlgoInstanceTradeResponse> GetAlgoInstanceTradesByTradedAsset(string instanceId, string tradedAssetId,
            int maxNumberToFetch);
    }
}
