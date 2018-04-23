using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Repositories;
using Lykke.AlgoStore.Job.AlgoTrades.Core.Services;
using Lykke.Service.OperationsRepository.Contract.History;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Job.AlgoTrades.Services
{
    public class AlgoTradesHistoryWriter : IAlgoTradesHistoryWriter
    {
        private readonly IAlgoInstanceTradeRepository _algoInstanceTradeRepository;

        public AlgoTradesHistoryWriter(IAlgoInstanceTradeRepository algoInstanceTradeRepository)
        {
            _algoInstanceTradeRepository = algoInstanceTradeRepository;
        }

        public async Task SaveAsync(OperationsHistoryMessage historyRecord)
        {
            //Save trade history to algo db table...when IAlgoInstanceTradeRepository is ready
        }
    }
}
