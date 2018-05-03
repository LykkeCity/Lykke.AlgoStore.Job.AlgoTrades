using Lykke.Service.OperationsRepository.Contract.History;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Service.AlgoTrades.Core.Services
{
    public interface IAlgoInstanceTradesHistoryWriter
    {
        Task SaveAsync(OperationsHistoryMessage historyRecord);
    }
}
