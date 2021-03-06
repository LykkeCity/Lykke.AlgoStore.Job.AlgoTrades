﻿using System.Collections.Generic;
using Lykke.AlgoStore.Service.AlgoTrades.Client.AutorestClient.Models;

namespace Lykke.AlgoStore.Service.AlgoTrades.Client.Models
{
    public  class AlgoInstanceTradeResponse
    {
        public ErrorModel Error { get; set; }
        public IList<AlgoInstanceTradeResponseModel> Records { get; set; }
    }
}
