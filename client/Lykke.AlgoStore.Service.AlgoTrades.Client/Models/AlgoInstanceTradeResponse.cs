using Lykke.AlgoStore.Service.AlgoTrades.Client.AutorestClient.Models;
using System.Collections.Generic;

namespace Lykke.AlgoStore.Service.AlgoTrades.Client.Models
{
    public  class AlgoInstanceTradeResponse
    {
        public ErrorModel Error { get; set; }
        public IList<AlgoInstanceTradeResponseModel> Records { get; set; }
    }
}
