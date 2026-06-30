namespace LivriaBackend.notifications.Domain.Model.ValueObjects
{
    /// <summary>
    /// Define los tipos predefinidos de notificaciones que pueden ser generadas en el sistema.
    /// Cada miembro de la enumeración representa una categoría distinta de evento que merece una notificación.
    /// </summary>
    public enum ENotificationType
    {
        /// <summary>
        /// Notificación enviada cuando un nuevo usuario se registra o es bienvenido al sistema.
        /// </summary>
        Welcome,
        /// <summary>
        /// Notificación enviada para confirmar un inicio de sesión o dar la bienvenida de vuelta a un usuario.
        /// </summary>
        Login,
        /// <summary>
        /// Notificación relacionada con el estado de un pedido (ej. "pedido recibido", "pedido enviado").
        /// </summary>
        Order,
        /// <summary>
        /// Notificación relacionada con la activación, cambio o eventos del plan de suscripción de un usuario.
        /// </summary>
        Plan,
        /// <summary>
        /// Notificación enviada cuando el contenido de un usuario (ej. una publicación) recibe un "me gusta".
        /// </summary>
        Like,
        /// <summary>
        /// Notificación relacionada con la billetera virtual (recarga aprobada, etc.).
        /// </summary>
        Wallet,
        /// <summary>
        /// Tipo de notificación por defecto o genérica, utilizada cuando no aplica un tipo específico.
        /// </summary>
        Default 
    }
}