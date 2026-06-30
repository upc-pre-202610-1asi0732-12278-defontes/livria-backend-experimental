namespace LivriaBackend.wallet.Interfaces.REST.Resources
{
    public record WalletBalanceResource(
        int UserClientId,
        decimal Balance
    );
}
