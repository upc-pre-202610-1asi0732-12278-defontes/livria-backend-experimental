using LivriaBackend.wallet.Domain.Model.Aggregates;
using LivriaBackend.wallet.Domain.Model.ValueObjects;

namespace LivriaBackend.wallet.Domain.Repositories
{
    public interface IWalletTransactionRepository
    {
        Task<WalletTransaction?> GetByIdAsync(int id);
        Task<IEnumerable<WalletTransaction>> GetByUserClientIdAsync(int userClientId);
        Task<IEnumerable<WalletTransaction>> GetPendingRechargesAsync();
        Task AddAsync(WalletTransaction transaction);
        Task UpdateAsync(WalletTransaction transaction);
    }
}
