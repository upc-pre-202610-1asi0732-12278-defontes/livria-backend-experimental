using System.Linq;
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
        private readonly IUserAdminRepository _userAdminRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationCommandService _notificationCommandService;

        public WalletCommandService(
            IWalletTransactionRepository walletTransactionRepository,
            IUserClientRepository userClientRepository,
            IUserAdminRepository userAdminRepository,
            IUnitOfWork unitOfWork,
            INotificationCommandService notificationCommandService)
        {
            _walletTransactionRepository = walletTransactionRepository;
            _userClientRepository = userClientRepository;
            _userAdminRepository = userAdminRepository;
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
            userClient.SetHasPayed(true);
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

        public async Task<WalletTransaction> Handle(PaySubscriptionWithWalletCommand command)
        {
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.", nameof(command.UserClientId));

            if (userClient.Subscription != "communityplan")
                throw new InvalidOperationException("User does not have a community plan subscription.");

            if (!userClient.HasSufficientWalletBalance(command.Amount))
                throw new InvalidOperationException("Insufficient wallet balance to pay subscription.");

            userClient.DebitWallet(command.Amount);
            userClient.SetHasPayed(true);

            var reference = $"SUB-{command.UserClientId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var transaction = WalletTransaction.CreateSubscriptionPayment(
                command.UserClientId,
                command.Amount,
                reference);

            var userAdmins = await _userAdminRepository.GetAllAsync();
            var admin = userAdmins.FirstOrDefault();
            if (admin != null)
            {
                admin.AddCapital(command.Amount);
                await _userAdminRepository.UpdateAsync(admin);
            }

            await _userClientRepository.UpdateAsync(userClient);
            await _walletTransactionRepository.AddAsync(transaction);
            await _unitOfWork.CompleteAsync();

            await _notificationCommandService.Handle(new CreateNotificationCommand(
                command.UserClientId,
                ENotificationType.Plan,
                DateTime.UtcNow));

            return transaction;
        }
    }
}
