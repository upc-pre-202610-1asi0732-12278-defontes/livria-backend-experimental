using LivriaBackend.shared.Infrastructure.Persistence.EFC.Configuration;
using LivriaBackend.wallet.Domain.Model.Aggregates;
using LivriaBackend.wallet.Domain.Model.ValueObjects;
using LivriaBackend.wallet.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LivriaBackend.wallet.Infrastructure.Repositories
{
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        private readonly AppDbContext _context;

        public WalletTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WalletTransaction?> GetByIdAsync(int id)
        {
            return await _context.WalletTransactions.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<WalletTransaction>> GetByUserClientIdAsync(int userClientId)
        {
            return await _context.WalletTransactions
                .Where(t => t.UserClientId == userClientId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<WalletTransaction>> GetPendingRechargesAsync()
        {
            return await _context.WalletTransactions
                .Where(t => t.Type == EWalletTransactionType.Recharge
                            && t.Status == EWalletTransactionStatus.Pending)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(WalletTransaction transaction)
        {
            await _context.WalletTransactions.AddAsync(transaction);
        }

        public Task UpdateAsync(WalletTransaction transaction)
        {
            _context.WalletTransactions.Update(transaction);
            return Task.CompletedTask;
        }
    }
}
