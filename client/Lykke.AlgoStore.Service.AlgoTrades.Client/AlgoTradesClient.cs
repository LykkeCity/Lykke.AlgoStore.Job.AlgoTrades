using Common.Log;
using Lykke.AlgoStore.Service.AlgoTrades.Client.AutorestClient;
using Lykke.AlgoStore.Service.AlgoTrades.Client.AutorestClient.Models;
using Lykke.AlgoStore.Service.AlgoTrades.Client.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.AlgoStore.Service.AlgoTrades.Client
{
    public class AlgoTradesClient : IAlgoTradesClient, IDisposable
    {
        private readonly ILog _log;
        private AlgoTradesAPI _apiClient;

        public AlgoTradesClient(string serviceUrl, ILog log)
        {
            _log = log;
            _apiClient = new AlgoTradesAPI(new Uri(serviceUrl));
        }

        public void Dispose()
        {
            if (_apiClient == null)
                return;
            _apiClient.Dispose();
            _apiClient = null;
        }

        private AlgoInstanceTradeResponse PrepareResponseMultiple(HttpOperationResponse<object> serviceResponse)
        {
            var error = serviceResponse.Body as ErrorResponse;
            var result = serviceResponse.Body as IList<AlgoInstanceTradeResponseModel>;

            if (error != null)
            {
                return new AlgoInstanceTradeResponse
                {
                    Error = new ErrorModel
                    {
                        Message = error.ErrorMessage
                    }
                };
            }

            if (result != null)
            {
                return new AlgoInstanceTradeResponse
                {
                    Records = result
                };
            }

            throw new ArgumentException("Unknown response object");
        }

        public async Task<AlgoInstanceTradeResponse> GetAlgoInstanceTradesByTradedAsset(string instanceId, string tradedAssetId, int maxNumberToFetch)
        {
            var response =
                await _apiClient.GetAlgoInstanceTradesWithHttpMessagesAsync(instanceId: instanceId, tradedAssetId: tradedAssetId, maxNumberToFetch: maxNumberToFetch);

            return PrepareResponseMultiple(response);
        }
    }
}
