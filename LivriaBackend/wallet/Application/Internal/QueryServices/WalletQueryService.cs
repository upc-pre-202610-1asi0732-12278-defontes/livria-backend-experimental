using LivriaBackend.users.Domain.Model.Repositories;
using LivriaBackend.wallet.Domain.Model.Aggregates;
using LivriaBackend.wallet.Domain.Model.Queries;
using LivriaBackend.wallet.Domain.Repositories;
using LivriaBackend.wallet.Domain.Services;

namespace LivriaBackend.wallet.Application.Internal.QueryServices
{
    public class WalletQueryService : IWalletQueryService
    {
        private readonly IUserClientRepository _userClientRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;

        public WalletQueryService(
            IUserClientRepository userClientRepository,
            IWalletTransactionRepository walletTransactionRepository)
        {
            _userClientRepository = userClientRepository;
            _walletTransactionRepository = walletTransactionRepository;
        }

        public async Task<decimal> Handle(GetWalletBalanceQuery query)
        {
            var userClient = await _userClientRepository.GetByIdAsync(query.UserClientId);
            if (userClient == null)
                throw new ArgumentException($"UserClient with ID {query.UserClientId} not found.", nameof(query.UserClientId));

            return userClient.Wallet;
        }

        public async Task<IEnumerable<WalletTransaction>> Handle(GetWalletTransactionsQuery query)
        {
            var userClient = await _userClientRepository.GetByIdAsync(query.UserClientId);
            if (userClient == null)
                throw new ArgumentException($"UserClient with ID {query.UserClientId} not found.", nameof(query.UserClientId));

            return await _walletTransactionRepository.GetByUserClientIdAsync(query.UserClientId);
        }

        public async Task<IEnumerable<WalletTransaction>> Handle(GetPendingRechargeRequestsQuery query)
        {
            return await _walletTransactionRepository.GetPendingRechargesAsync();
        }

        public async Task<(bool CanPay, decimal Balance, decimal Required)> Handle(CanPayWithWalletQuery query)
        {
            var userClient = await _userClientRepository.GetByIdAsync(query.UserClientId);
            if (userClient == null)
                throw new ArgumentException($"UserClient with ID {query.UserClientId} not found.", nameof(query.UserClientId));

            return (userClient.HasSufficientWalletBalance(query.Amount), userClient.Wallet, query.Amount);
        }

        public async Task<WalletTransaction?> Handle(GetWalletTransactionByIdQuery query)
        {
            return await _walletTransactionRepository.GetByIdAsync(query.TransactionId);
        }
    }
}
