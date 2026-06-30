using System;

namespace LivriaBackend.communities.Domain.Model.Aggregates
{
    /// <summary>
    /// Representa un comentario como una entidad agregada.
    /// Un comentario pertenece a una publicación (Post).
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Obtiene el identificador único del comentario.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Obtiene el identificador de la publicación a la que pertenece este comentario.
        /// </summary>
        public int PostId { get; private set; } 

        /// <summary>
        /// Obtiene el identificador del usuario que creó este comentario.
        /// </summary>
        public int UserId { get; private set; }      

        /// <summary>
        /// Obtiene el nombre de usuario de la persona que realizó el comentario (cached).
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Obtiene el contenido de texto del comentario.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Obtiene la URL de la imagen o GIF asociado al comentario, si lo hay.
        /// </summary>
        public string Img { get; private set; }

        /// <summary>
        /// Obtiene la fecha y hora de creación del comentario en formato UTC.
        /// </summary>
        public DateTime CreatedAt { get; private set; } 

        // Propiedades de navegación (Post, UserClient) si son necesarias...

        /// <summary>
        /// Constructor protegido sin parámetros para uso de Entity Framework Core.
        /// </summary>
        protected Comment() { }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="Comment"/>.
        /// </summary>
        /// <param name="postId">El identificador de la publicación.</param>
        /// <param name="userId">El identificador del usuario autor.</param>
        /// <param name="username">El nombre de usuario del autor.</param>
        /// <param name="content">El contenido de texto del comentario.</param>
        /// <param name="img">La URL de la imagen o GIF del comentario. Puede ser nula o vacía.</param>
        public Comment(int postId, int userId, string username, string content, string img)
        {
            var normalizedContent = content ?? string.Empty;
            var normalizedImg = img ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedContent) && string.IsNullOrWhiteSpace(normalizedImg))
            {
                throw new ArgumentException("Comment must include content or an image.", nameof(content));
            }

            PostId = postId;
            UserId = userId;
            Username = username;
            Content = normalizedContent;
            Img = normalizedImg;
            CreatedAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Actualiza el contenido de un comentario existente.
        /// </summary>
        /// <param name="content">El nuevo contenido de texto para el comentario.</param>
        public void Update(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Comment content cannot be empty.", nameof(content));
            }
            Content = content;
        }
        
        /// <summary>
        /// Anonimiza el comentario asignándolo al usuario eliminado.
        /// </summary>
        public void Anonymize()
        {
            UserId = UserConstants.DeletedUserId;
            Username = UserConstants.DeletedUsername;
        }
    }
}