using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.Service.OperationsRepository.Contract.Cash;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Models.Events;

namespace Lykke.AlgoStore.Service.AlgoTrades.Core.Services
{
    public interface IAlgoInstanceTradesHistoryWriter
    {
        Task SaveAsync(ClientTradeDto clientTrade, AlgoInstanceTrade algoInstanceOrder);
        Task SaveAsync(Order order);
    }
}
