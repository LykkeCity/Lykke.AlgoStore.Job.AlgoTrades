using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.Service.OperationsRepository.Contract.Cash;
using Lykke.Service.OperationsRepository.Contract.History;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Service.AlgoTrades.Core.Services
{
    public interface IAlgoInstanceTradesHistoryWriter
    {
        Task SaveAsync(ClientTradeDto clientTrade, AlgoInstanceTrade algoInstanceOrder);
    }
}
