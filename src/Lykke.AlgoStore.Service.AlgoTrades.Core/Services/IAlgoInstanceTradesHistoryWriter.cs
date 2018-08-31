using Lykke.Service.OperationsRepository.Contract.History;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Models.Events;

namespace Lykke.AlgoStore.Service.AlgoTrades.Core.Services
{
    public interface IAlgoInstanceTradesHistoryWriter
    {
        Task SaveAsync(OperationsHistoryMessage historyRecord);
        Task SaveAsync(Order order);
    }
}
