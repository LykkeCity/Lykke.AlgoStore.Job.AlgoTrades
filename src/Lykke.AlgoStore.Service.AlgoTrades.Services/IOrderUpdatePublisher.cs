using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;

namespace Lykke.AlgoStore.Service.AlgoTrades.Services
{
    public interface IOrderUpdatePublisher
    {
        Task Publish(AlgoInstanceTrade orderTrade);
    }
}
