namespace LivriaBackend.wallet.Domain.Model.Commands
{
    public record DebitWalletForPurchaseCommand(
        int UserClientId,
        decimal Amount,
        string OrderCode
    );
}
