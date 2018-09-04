using Lykke.AlgoStore.Service.AlgoTrades.Client.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.AlgoStore.Service.AlgoTrades.Client.AutorestClient;
using Lykke.AlgoStore.Service.AlgoTrades.Client.AutorestClient.Models;

namespace Lykke.AlgoStore.Service.AlgoTrades.Client
{
    public class AlgoTradesClient : IAlgoTradesClient, IDisposable
    {
        private AlgoTradesAPI _apiClient;

        public AlgoTradesClient(string serviceUrl)
        {
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
                        Message = error.ErrorMessage,
                        ModelErrors = error.ModelErrors,
                        StatusCode = serviceResponse.Response.StatusCode
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

        public async Task<AlgoInstanceTradeResponse> GetAlgoInstanceTradesByPeriod(string instanceId, string tradedAssetId, DateTime from, DateTime to)
        {
            var response = await _apiClient.GetAlgoInstanceTradesByPeriodWithHttpMessagesAsync(instanceId: instanceId, tradedAssetId: tradedAssetId, fromMoment: from, toMoment: to);

            return PrepareResponseMultiple(response);
        }
    }
}
