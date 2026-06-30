namespace LivriaBackend.wallet.Domain.Model.Queries
{
    public record CanPayWithWalletQuery(int UserClientId, decimal Amount);
}
