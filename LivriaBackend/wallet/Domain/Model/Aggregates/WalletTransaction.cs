using System;
using LivriaBackend.wallet.Domain.Model.ValueObjects;

namespace LivriaBackend.wallet.Domain.Model.Aggregates
{
    public class WalletTransaction
    {
        public int Id { get; private set; }
        public int UserClientId { get; private set; }
        public decimal Amount { get; private set; }
        public EWalletTransactionType Type { get; private set; }
        public EWalletTransactionStatus Status { get; private set; }
        public string Reference { get; private set; }
        public string ProofUrl { get; private set; }
        public int? OrderId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public string? AdminNote { get; private set; }

        protected WalletTransaction()
        {
            Reference = string.Empty;
            ProofUrl = string.Empty;
        }

        public static WalletTransaction CreateRechargeRequest(
            int userClientId,
            decimal amount,
            string proofUrl,
            string? reference = null)
        {
            if (userClientId <= 0)
                throw new ArgumentException("UserClientId must be positive.", nameof(userClientId));
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));
            if (string.IsNullOrWhiteSpace(proofUrl))
                throw new ArgumentException("Proof URL is required for recharge requests.", nameof(proofUrl));

            var normalizedReference = string.IsNullOrWhiteSpace(reference)
                ? $"WALLET-{userClientId}-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : reference.Trim();

            return new WalletTransaction
            {
                UserClientId = userClientId,
                Amount = amount,
                Type = EWalletTransactionType.Recharge,
                Status = EWalletTransactionStatus.Pending,
                Reference = normalizedReference,
                ProofUrl = proofUrl.Trim(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public static WalletTransaction CreatePurchase(int userClientId, decimal amount, string orderCode)
        {
            if (userClientId <= 0)
                throw new ArgumentException("UserClientId must be positive.", nameof(userClientId));
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));
            if (string.IsNullOrWhiteSpace(orderCode))
                throw new ArgumentException("Order code is required.", nameof(orderCode));

            return new WalletTransaction
            {
                UserClientId = userClientId,
                Amount = amount,
                Type = EWalletTransactionType.Purchase,
                Status = EWalletTransactionStatus.Completed,
                Reference = orderCode,
                ProofUrl = string.Empty,
                OrderId = null,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };
        }

        public void Approve()
        {
            if (Type != EWalletTransactionType.Recharge)
                throw new InvalidOperationException("Only recharge requests can be approved.");
            if (Status != EWalletTransactionStatus.Pending)
                throw new InvalidOperationException("Only pending transactions can be approved.");

            Status = EWalletTransactionStatus.Completed;
            ProcessedAt = DateTime.UtcNow;
        }

        public void Reject(string? adminNote = null)
        {
            if (Type != EWalletTransactionType.Recharge)
                throw new InvalidOperationException("Only recharge requests can be rejected.");
            if (Status != EWalletTransactionStatus.Pending)
                throw new InvalidOperationException("Only pending transactions can be rejected.");

            Status = EWalletTransactionStatus.Rejected;
            ProcessedAt = DateTime.UtcNow;
            AdminNote = adminNote?.Trim();
        }
    }
}
