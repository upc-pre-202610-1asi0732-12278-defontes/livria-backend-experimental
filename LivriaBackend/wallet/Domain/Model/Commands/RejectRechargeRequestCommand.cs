namespace LivriaBackend.wallet.Domain.Model.Commands
{
    public record RejectRechargeRequestCommand(int TransactionId, string? AdminNote);
}
