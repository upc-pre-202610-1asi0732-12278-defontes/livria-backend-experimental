using System.ComponentModel.DataAnnotations;

namespace LivriaBackend.communities.Interfaces.REST.Resources
{
    /// <summary>
    /// Recurso de entrada para crear un nuevo comentario.
    /// </summary>
    public record CreateCommentResource(
        [Required] int PostId,
        [Required] int UserId,
        string Content,
        string Img
    );
}