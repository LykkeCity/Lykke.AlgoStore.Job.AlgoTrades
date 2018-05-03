using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Domain;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Services;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.OperationsRepository.Contract;

namespace Lykke.AlgoStore.Service.AlgoTrades.Services
{
    public class AlgoInstanceTradesHistoryService : IAlgoInstanceTradesHistoryService
    {
        private readonly CachedDataDictionary<string, Asset> _assetsCache;
        private readonly CachedDataDictionary<string, AssetPair> _assetPairsCache;
        private readonly ILog _log;

        public AlgoInstanceTradesHistoryService(
            CachedDataDictionary<string, Asset> assetsCache,
            CachedDataDictionary<string, AssetPair> assetPairsCache,
            ILog log)
        {
            _assetsCache = assetsCache ?? throw new ArgumentNullException(nameof(assetsCache));
            _assetPairsCache = assetPairsCache ?? throw new ArgumentNullException(nameof(assetPairsCache));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<AlgoInstanceTradeResponseModel> ExecuteAsync(AlgoInstanceTrade algoInstanceTrade)
        {
            var asset = await GetAssetByIdAsync(algoInstanceTrade.AssetId);
            var assetPair = await GetAssetPairByIdAsync(algoInstanceTrade.AssetPairId);

            var result = AutoMapper.Mapper.Map<AlgoInstanceTradeResponseModel>(algoInstanceTrade);
            result.AssetPair = assetPair?.Name;
            result.TradedAssetName = asset?.Name;

            return result;
        }

        private async Task<Asset> GetAssetByIdAsync(string assetId)
        {
            if (string.IsNullOrEmpty(assetId)) return null;

            var cachedValues = await _assetsCache.Values();

            return cachedValues.FirstOrDefault(x => x.Id == assetId);
        }

        private async Task<AssetPair> GetAssetPairByIdAsync(string assetPairId)
        {
            if (string.IsNullOrEmpty(assetPairId)) return null;

            var cachedValues = await _assetPairsCache.Values();

            return cachedValues.FirstOrDefault(x => x.Id == assetPairId);
        }
    }
}
