using System.ComponentModel.DataAnnotations;

namespace LivriaBackend.users.Domain.Model.Commands
{
    public record ToggleReadBookCommand(
        [Required] int UserClientId,
        [Required] int BookId
    );
}
