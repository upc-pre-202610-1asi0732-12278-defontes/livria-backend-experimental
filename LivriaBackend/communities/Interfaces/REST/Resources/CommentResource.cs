using System;

namespace LivriaBackend.communities.Interfaces.REST.Resources
{
    /// <summary>
    /// Recurso de respuesta para representar un comentario.
    /// </summary>
    public record CommentResource(
        int Id,
        int PostId,
        int UserId,
        string Username,
        string Content,
        string Img,
        DateTime CreatedAt
    );
}