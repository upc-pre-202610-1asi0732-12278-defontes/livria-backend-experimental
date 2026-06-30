using LivriaBackend.wallet.Domain.Model.Aggregates;
using LivriaBackend.wallet.Domain.Model.Commands;

namespace LivriaBackend.wallet.Domain.Services
{
    public interface IWalletCommandService
    {
        Task<WalletTransaction> Handle(CreateRechargeRequestCommand command);
        Task<WalletTransaction?> Handle(ApproveRechargeRequestCommand command);
        Task<WalletTransaction?> Handle(RejectRechargeRequestCommand command);
        Task<WalletTransaction> Handle(DebitWalletForPurchaseCommand command);
    }
}
