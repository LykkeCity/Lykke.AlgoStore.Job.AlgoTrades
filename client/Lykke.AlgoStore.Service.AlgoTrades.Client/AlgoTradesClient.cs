using System;
using Common.Log;

namespace Lykke.AlgoStore.Service.AlgoTrades.Client
{
    public class AlgoTradesClient : IAlgoTradesClient, IDisposable
    {
        private readonly ILog _log;

        public AlgoTradesClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
