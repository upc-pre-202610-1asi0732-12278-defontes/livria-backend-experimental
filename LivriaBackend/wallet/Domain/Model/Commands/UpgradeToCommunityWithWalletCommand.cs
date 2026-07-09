namespace LivriaBackend.wallet.Domain.Model.Commands
{
    /// <summary>
    /// Upgrade inicial freeplan → communityplan pagando con wallet.
    /// Distinto de <see cref="PaySubscriptionWithWalletCommand"/> (renovación mensual).
    /// </summary>
    public record UpgradeToCommunityWithWalletCommand(int UserClientId, decimal Amount);
}
