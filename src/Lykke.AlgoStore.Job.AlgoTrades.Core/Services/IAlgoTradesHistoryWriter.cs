using Lykke.Service.OperationsRepository.Contract.History;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.AlgoTrades.Core.Services
{
    public interface IAlgoTradesHistoryWriter
    {
        Task SaveAsync(OperationsHistoryMessage historyRecord);
    }
}
