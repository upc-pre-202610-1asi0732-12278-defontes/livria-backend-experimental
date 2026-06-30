using System.ComponentModel.DataAnnotations;

namespace LivriaBackend.wallet.Domain.Model.Commands
{
    public record CreateRechargeRequestCommand(
        [Required] int UserClientId,
        [Required][Range(0.01, double.MaxValue)] decimal Amount,
        [Required][Url][StringLength(500, MinimumLength = 10)] string ProofUrl,
        [StringLength(100, MinimumLength = 3)] string? Reference = null
    );
}
