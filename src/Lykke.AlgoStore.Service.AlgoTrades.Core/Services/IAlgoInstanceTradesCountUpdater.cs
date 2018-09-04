using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Service.AlgoTrades.Core.Services
{
    public interface IAlgoInstanceTradesCountUpdater
    {
        Task IncreaseInstanceTradeCountAsync(AlgoInstanceTrade instanceTrade);
    }
}
