using LivriaBackend.wallet.Domain.Model.Aggregates;
using LivriaBackend.wallet.Domain.Model.Queries;

namespace LivriaBackend.wallet.Domain.Services
{
    public interface IWalletQueryService
    {
        Task<decimal> Handle(GetWalletBalanceQuery query);
        Task<IEnumerable<WalletTransaction>> Handle(GetWalletTransactionsQuery query);
        Task<IEnumerable<WalletTransaction>> Handle(GetPendingRechargeRequestsQuery query);
        Task<(bool CanPay, decimal Balance, decimal Required)> Handle(CanPayWithWalletQuery query);
        Task<WalletTransaction?> Handle(GetWalletTransactionByIdQuery query);
    }
}
