using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.Service.OperationsRepository.Contract.Cash;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Service.AlgoTrades.Core.Services
{
    public interface IAlgoInstanceTradesCountUpdater
    {
        Task IncreaseInstanceTradeCountAsync(ClientTradeDto clientTrade, AlgoInstanceTrade instanceTrade);
    }
}
