namespace LivriaBackend.users.Domain.Model.Commands
{
    public record ToggleReadBookResult(int BookId, bool IsRead);
}
