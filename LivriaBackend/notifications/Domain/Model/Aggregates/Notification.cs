using System;
using LivriaBackend.notifications.Domain.Model.ValueObjects; 

namespace LivriaBackend.notifications.Domain.Model.Aggregates
{
    /// <summary>
    /// Representa una notificación como un agregado dentro del dominio de notificaciones.
    /// Una notificación es un mensaje dirigido a un usuario con un tipo, título, contenido y estado.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Obtiene el identificador único de la notificación.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Obtiene el identificador del cliente de usuario al que está dirigida esta notificación.
        /// </summary>
        public int UserClientId { get; private set; } 
        
        /// <summary>
        /// Obtiene la fecha y hora de creación de la notificación en formato UTC.
        /// </summary>
        public DateTime CreatedAt { get; private set; } 

        /// <summary>
        /// Obtiene el tipo de la notificación, definido por la enumeración <see cref="ENotificationType"/>.
        /// </summary>
        public ENotificationType Type { get; private set; } 

        /// <summary>
        /// Obtiene el título de la notificación, que se establece en función de su <see cref="Type"/>.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Obtiene el contenido detallado de la notificación, que se establece en función de su <see cref="Type"/>.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Obtiene un valor que indica si la notificación ha sido leída por el usuario.
        /// Falso por defecto.
        /// </summary>
        public bool IsRead { get; private set; } 

        /// <summary>
        /// Obtiene un valor que indica si la notificación ha sido oculta por el usuario.
        /// Falso por defecto. Las notificaciones ocultas pueden no mostrarse en la interfaz de usuario.
        /// </summary>
        public bool IsHidden { get; private set; } 

        /// <summary>
        /// Constructor protegido sin parámetros para uso de Entity Framework Core.
        /// </summary>
        protected Notification() { } 

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="Notification"/> con los detalles especificados.
        /// El título y el contenido se establecen automáticamente en función del <paramref name="type"/> de la notificación.
        /// Las notificaciones se crean como no leídas y no ocultas por defecto.
        /// </summary>
        /// <param name="userClientId">El identificador del cliente de usuario al que se dirige la notificación.</param>
        /// <param name="type">El <see cref="ENotificationType"/> de la notificación.</param>
        /// <param name="createdAt">La fecha y hora de creación de la notificación (se recomienda usar <see cref="DateTime.UtcNow"/>).</param>
        public Notification(int userClientId, ENotificationType type, DateTime createdAt)
        {
            UserClientId = userClientId; 
            CreatedAt = createdAt;
            Type = type;
            IsRead = false; 
            IsHidden = false; 
            SetTitleAndContentByType(type); 
        }

        /// <summary>
        /// Establece el <see cref="Title"/> y <see cref="Content"/> de la notificación
        /// basándose en su <see cref="ENotificationType"/>.
        /// </summary>
        /// <param name="type">El tipo de notificación que determina el título y contenido.</param>
        private void SetTitleAndContentByType(ENotificationType type)
        {
            switch (type)
            {
                case ENotificationType.Welcome:
                    Title = "Welcome to Livria!";
                    Content = "We're thrilled to have you here.";
                    break;
                case ENotificationType.Login:
                    Title = "Welcome back!";
                    Content = "Nice to see you again. Let’s continue exploring.";
                    break;
                case ENotificationType.Order:
                    Title = "Order Received";
                    Content = "Thanks for your order! It's being processed.";
                    break;
                case ENotificationType.Plan:
                    Title = "You just subscribed to a community plan.";
                    Content = "Enjoy the perks!";
                    break;
                case ENotificationType.Like:
                    Title = "Recent Likes";
                    Content = "Wow! Your recent post got a lot of likes! Wanna see?";
                    break;
                case ENotificationType.Wallet:
                    Title = "Wallet Updated";
                    Content = "Your wallet recharge was approved. Funds are now available.";
                    break;
                case ENotificationType.Default:
                default:
                    Title = "Notification";
                    Content = "An event just occurred.";
                    break;
            }
        }
        
        /// <summary>
        /// Marca la notificación como leída.
        /// </summary>
        public void MarkAsRead()
        {
            IsRead = true;
        }
        
        /// <summary>
        /// Oculta la notificación. Las notificaciones ocultas pueden no ser visibles para el usuario en la interfaz.
        /// </summary>
        public void Hide()
        {
            IsHidden = true;
        }
        
        /// <summary>
        /// Revela la notificación previamente oculta.
        /// </summary>
        public void Unhide()
        {
            IsHidden = false;
        }
    }
}