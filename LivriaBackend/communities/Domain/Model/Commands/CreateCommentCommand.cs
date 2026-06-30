namespace LivriaBackend.communities.Domain.Model.Commands
{
    /// <summary>
    /// Comando para crear un nuevo comentario en una publicación.
    /// </summary>
    public record CreateCommentCommand(
        int PostId,
        int UserId,
        string Content,
        string Img
    );
}