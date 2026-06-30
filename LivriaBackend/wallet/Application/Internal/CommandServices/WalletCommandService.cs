using LivriaBackend.notifications.Domain.Model.Commands;
using LivriaBackend.notifications.Domain.Model.Services;
using LivriaBackend.notifications.Domain.Model.ValueObjects;
using LivriaBackend.shared.Domain.Repositories;
using LivriaBackend.users.Domain.Model.Repositories;
using LivriaBackend.wallet.Domain.Model.Aggregates;
using LivriaBackend.wallet.Domain.Model.Commands;
using LivriaBackend.wallet.Domain.Repositories;
using LivriaBackend.wallet.Domain.Services;

namespace LivriaBackend.wallet.Application.Internal.CommandServices
{
    public class WalletCommandService : IWalletCommandService
    {
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IUserClientRepository _userClientRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationCommandService _notificationCommandService;

        public WalletCommandService(
            IWalletTransactionRepository walletTransactionRepository,
            IUserClientRepository userClientRepository,
            IUnitOfWork unitOfWork,
            INotificationCommandService notificationCommandService)
        {
            _walletTransactionRepository = walletTransactionRepository;
            _userClientRepository = userClientRepository;
            _unitOfWork = unitOfWork;
            _notificationCommandService = notificationCommandService;
        }

        public async Task<WalletTransaction> Handle(CreateRechargeRequestCommand command)
        {
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.", nameof(command.UserClientId));

            var transaction = WalletTransaction.CreateRechargeRequest(
                command.UserClientId,
                command.Amount,
                command.ProofUrl,
                command.Reference);

            await _walletTransactionRepository.AddAsync(transaction);
            await _unitOfWork.CompleteAsync();
            return transaction;
        }

        public async Task<WalletTransaction?> Handle(ApproveRechargeRequestCommand command)
        {
            var transaction = await _walletTransactionRepository.GetByIdAsync(command.TransactionId);
            if (transaction == null)
                return null;

            var userClient = await _userClientRepository.GetByIdAsync(transaction.UserClientId);
            if (userClient == null)
                throw new InvalidOperationException($"UserClient with ID {transaction.UserClientId} not found.");

            transaction.Approve();
            userClient.CreditWallet(transaction.Amount);
            await _userClientRepository.UpdateAsync(userClient);
            await _walletTransactionRepository.UpdateAsync(transaction);
            await _unitOfWork.CompleteAsync();

            await _notificationCommandService.Handle(new CreateNotificationCommand(
                transaction.UserClientId,
                ENotificationType.Wallet,
                DateTime.UtcNow));

            return transaction;
        }

        public async Task<WalletTransaction?> Handle(RejectRechargeRequestCommand command)
        {
            var transaction = await _walletTransactionRepository.GetByIdAsync(command.TransactionId);
            if (transaction == null)
                return null;

            transaction.Reject(command.AdminNote);
            await _walletTransactionRepository.UpdateAsync(transaction);
            await _unitOfWork.CompleteAsync();
            return transaction;
        }

        public async Task<WalletTransaction> Handle(DebitWalletForPurchaseCommand command)
        {
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.", nameof(command.UserClientId));

            userClient.DebitWallet(command.Amount);
            var transaction = WalletTransaction.CreatePurchase(
                command.UserClientId,
                command.Amount,
                command.OrderCode);

            await _userClientRepository.UpdateAsync(userClient);
            await _walletTransactionRepository.AddAsync(transaction);
            await _unitOfWork.CompleteAsync();
            return transaction;
        }
    }
}
