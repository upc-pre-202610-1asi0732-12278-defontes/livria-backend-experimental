using AutoMapper;
using LivriaBackend.wallet.Domain.Model.Aggregates;
using LivriaBackend.wallet.Domain.Model.Commands;
using LivriaBackend.wallet.Interfaces.REST.Resources;

namespace LivriaBackend.wallet.Interfaces.REST.Transform
{
    public class MappingWallet : Profile
    {
        public MappingWallet()
        {
            CreateMap<WalletTransaction, WalletTransactionResource>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateRechargeRequestResource, CreateRechargeRequestCommand>();
        }
    }
}
