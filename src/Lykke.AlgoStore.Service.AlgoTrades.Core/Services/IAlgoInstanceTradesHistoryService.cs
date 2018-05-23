using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Domain;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Service.AlgoTrades.Core.Services
{
    public interface IHistoryService<TSource, TDest>
    {
        Task<TDest> ExecuteAsync(TSource src);
    }

    public interface IAlgoInstanceTradesHistoryService : IHistoryService<AlgoInstanceTrade, AlgoInstanceTradeResponseModel>
    {

    }
}
