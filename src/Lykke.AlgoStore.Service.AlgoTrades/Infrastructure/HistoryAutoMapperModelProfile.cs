using AutoMapper;
using Lykke.AlgoStore.CSharp.AlgoTemplate.Models.Models;
using Lykke.AlgoStore.Service.AlgoTrades.Core.Domain;

namespace Lykke.AlgoStore.Service.AlgoTrades.Infrastructure
{
    public class HistoryAutoMapperModelProfile : Profile
    {
        public HistoryAutoMapperModelProfile()
        {
            CreateMap<AlgoInstanceTrade, AlgoInstanceTradeResponseModel>()
                .ForMember(dest => dest.AssetPair, opt => opt.Ignore())
                .ForMember(dest => dest.TradedAssetName, opt => opt.Ignore())
                .ForSourceMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
