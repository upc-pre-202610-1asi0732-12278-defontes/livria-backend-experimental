namespace LivriaBackend.wallet.Domain.Model.Commands
{
    public record PaySubscriptionWithWalletCommand(int UserClientId, decimal Amount);
}
