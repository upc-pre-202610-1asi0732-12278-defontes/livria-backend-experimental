using System.Threading.Tasks;
using LivriaBackend.users.Domain.Model.Aggregates;
using LivriaBackend.users.Domain.Model.Commands;

namespace LivriaBackend.users.Domain.Model.Services
{
    /// <summary>
    /// Define el contrato para el servicio de comandos de clientes de usuario.
    /// Este servicio es responsable de orquestar las operaciones que modifican el estado de los clientes de usuario.
    /// </summary>
    public interface IUserClientCommandService
    {
        /// <summary>
        /// Maneja el comando para crear un nuevo cliente de usuario.
        /// </summary>
        /// <param name="command">El comando <see cref="CreateUserClientCommand"/> que contiene los datos para el nuevo cliente de usuario.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> recién creado.
        /// </returns>
        Task<UserClient> Handle(CreateUserClientCommand command);

        /// <summary>
        /// Maneja el comando para actualizar un cliente de usuario existente.
        /// </summary>
        /// <param name="command">El comando <see cref="UpdateUserClientCommand"/> que contiene los datos actualizados del cliente de usuario.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> actualizado.
        /// </returns>
        Task<UserClient> Handle(UpdateUserClientCommand command);

        /// <summary>
        /// Maneja el comando para eliminar un cliente de usuario existente.
        /// </summary>
        /// <param name="command">El comando <see cref="DeleteUserClientCommand"/> que contiene el ID del cliente de usuario a eliminar.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es <c>true</c> si el cliente de usuario fue eliminado exitosamente; de lo contrario, <c>false</c>.
        /// </returns>
        Task<bool> Handle(DeleteUserClientCommand command);
        
        /// <summary>
        /// Maneja el comando para añadir un libro a la lista de favoritos de un cliente de usuario.
        /// </summary>
        /// <param name="command">El comando <see cref="AddFavoriteBookCommand"/> que contiene los IDs del usuario y del libro.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> con el libro añadido a favoritos.
        /// </returns>
        Task<UserClient> Handle(AddFavoriteBookCommand command);   
        
        /// <summary>
        /// Maneja el comando para eliminar un libro de la lista de favoritos de un cliente de usuario.
        /// </summary>
        /// <param name="command">El comando <see cref="RemoveFavoriteBookCommand"/> que contiene los IDs del usuario y del libro.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> con el libro eliminado de favoritos.
        /// </returns>
        Task<UserClient> Handle(RemoveFavoriteBookCommand command); 
        
        /// <summary>
        /// Maneja el comando para añadir un libro a la lista de exclusión de un cliente de usuario.
        /// </summary>
        /// <param name="command">El comando <see cref="AddExclusionBookCommand"/> que contiene los IDs del usuario y del libro.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> con el libro añadido a la lista de exclusión.
        /// </returns>
        Task<UserClient> Handle(AddExclusionBookCommand command);

        /// <summary>
        /// Maneja el comando para eliminar un libro de la lista de exclusión de un cliente de usuario.
        /// </summary>
        /// <param name="command">El comando <see cref="RemoveExclusionBookCommand"/> que contiene los IDs del usuario y del libro.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> con el libro eliminado de la lista de exclusión.
        /// </returns>
        Task<UserClient> Handle(RemoveExclusionBookCommand command);

        /// <summary>
        /// Marca o desmarca un libro como leído (toggle).
        /// </summary>
        Task<ToggleReadBookResult> Handle(ToggleReadBookCommand command);

        /// <summary>
        /// Maneja el comando para actualizar el plan de suscripción de un cliente de usuario.
        /// </summary>
        /// <param name="command">El comando <see cref="UpdateUserClientSubscriptionCommand"/> que contiene el ID del usuario y el nuevo plan de suscripción.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> con el plan de suscripción actualizado.
        /// </returns>
        Task<UserClient> Handle(UpdateUserClientSubscriptionCommand command);
        
        /// <summary>
        /// Maneja el comando para actualizar si el cliente ha pagado
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task<UserClient> Handle(UpdateUserClientHasPayedCommand command);
    }
}