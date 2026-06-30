using System.ComponentModel.DataAnnotations;

namespace LivriaBackend.commerce.Interfaces.REST.Resources
{
    public record ToggleReadBookResource(
        [Required][Range(1, int.MaxValue)] int UserClientId
    );

    public record ToggleReadBookResponseResource(
        int BookId,
        bool IsRead
    );
}
