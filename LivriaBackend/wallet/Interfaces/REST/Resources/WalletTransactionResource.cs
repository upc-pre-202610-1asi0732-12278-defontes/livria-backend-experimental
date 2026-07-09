using System;
using System.ComponentModel.DataAnnotations;

namespace LivriaBackend.wallet.Interfaces.REST.Resources
{
    public record WalletTransactionResource(
        int Id,
        int UserClientId,
        decimal Amount,
        string Type,
        string Status,
        string Reference,
        string ProofUrl,
        int? OrderId,
        DateTime CreatedAt,
        DateTime? ProcessedAt,
        string? AdminNote
    );

    public record CreateRechargeRequestResource(
        [Required][Range(1, int.MaxValue)] int UserClientId,
        [Required][Range(0.01, double.MaxValue)] decimal Amount,
        [Required][Url][StringLength(500, MinimumLength = 10)] string ProofUrl,
        [StringLength(100, MinimumLength = 3)] string? Reference
    );

    public record RejectRechargeRequestResource(
        [StringLength(500)] string? AdminNote
    );

    public record PaySubscriptionResource(
        [Required][Range(0.01, double.MaxValue)] decimal Amount
    );

    /// <summary>
    /// Body para upgrade inicial free → community pagando con wallet.
    /// </summary>
    public record UpgradeToCommunityResource(
        [Required][Range(0.01, double.MaxValue)] decimal Amount
    );
}
