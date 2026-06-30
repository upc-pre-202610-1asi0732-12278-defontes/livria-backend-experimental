using System.Collections.Generic;
using System.Threading.Tasks;
using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.users.Domain.Model.Aggregates;
using LivriaBackend.users.Domain.Model.Queries;
using LivriaBackend.users.Domain.Model.Repositories;
using LivriaBackend.users.Domain.Model.Services;
using System.Linq;

namespace LivriaBackend.users.Application.Internal.QueryServices
{
    /// <summary>
    /// Implementa el servicio de consulta para las operaciones de la entidad <see cref="UserClient"/>.
    /// Encapsula la lógica de negocio para recuperar datos de clientes de usuario.
    /// </summary>
    public class UserClientQueryService : IUserClientQueryService
    {
        private readonly IUserClientRepository _userClientRepository;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UserClientQueryService"/>.
        /// </summary>
        /// <param name="userClientRepository">El repositorio para las operaciones de datos del cliente de usuario.</param>
        public UserClientQueryService(IUserClientRepository userClientRepository)
        {
            _userClientRepository = userClientRepository;
        }

        /// <summary>
        /// Maneja la consulta para obtener todos los clientes de usuario,
        /// excluyendo al usuario de sistema utilizado para anonimización.
        /// </summary>
        public async Task<IEnumerable<UserClient>> Handle(GetAllUserClientQuery query)
        {
            var allUsers = await _userClientRepository.GetAllAsync();
            return allUsers.Where(u => u.Id != UserConstants.DeletedUserId);
        }

        /// <summary>
        /// Maneja la consulta para obtener un cliente de usuario específico por su identificador único.
        /// </summary>
        /// <param name="query">La consulta <see cref="GetUserClientByIdQuery"/> que contiene el ID del cliente de usuario.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> encontrado, o <c>null</c> si no existe.
        /// </returns>
        public async Task<UserClient> Handle(GetUserClientByIdQuery query)
        {
            return await _userClientRepository.GetByIdAsync(query.UserClientId);
        }

        public async Task<bool> HasCommunityPlanAsync(int userClientId)
        {
            var userClient = await _userClientRepository.GetByIdAsync(userClientId);
            
            // Devuelve false si no existe O si el plan es incorrecto
            return userClient != null && userClient.Subscription == "communityplan";
        }

        /// <inheritdoc />
        public async Task<UserClientAvailability> GetRegistrationAvailabilityAsync(string? email, string? username)
        {
            bool? emailAvailable = null;
            if (email != null)
            {
                emailAvailable = !await _userClientRepository.ExistsByEmailAsync(email);
            }

            bool? usernameAvailable = null;
            if (username != null)
            {
                usernameAvailable = !await _userClientRepository.ExistsByUsernameAsync(username);
            }

            return new UserClientAvailability
            {
                EmailAvailable = emailAvailable,
                UsernameAvailable = usernameAvailable
            };
        }

        public async Task<IEnumerable<Book>> Handle(GetReadBooksByUserQuery query)
        {
            var userClient = await _userClientRepository.GetByIdAsync(query.UserClientId);
            if (userClient == null)
                throw new ArgumentException($"UserClient with ID {query.UserClientId} not found.");

            return userClient.ReadBooks;
        }
    }
}