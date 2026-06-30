namespace LivriaBackend.wallet.Interfaces.REST.Resources
{
    public record WalletCanPayResource(
        int UserClientId,
        decimal RequiredAmount,
        decimal Balance,
        bool CanPay
    );
}
