using LivriaBackend.shared.Infrastructure.Persistence.EFC.Configuration;
using LivriaBackend.shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LivriaBackend.users.Domain.Model.Aggregates;
using LivriaBackend.users.Domain.Model.Repositories;

namespace LivriaBackend.users.Infrastructure.Repositories
{
    /// <summary>
    /// Implementa el repositorio para la entidad agregada <see cref="UserClient"/>.
    /// Extiende de <see cref="BaseRepository{TEntity}"/> para heredar funcionalidades básicas
    /// y añade métodos específicos para operaciones de clientes de usuario, incluyendo
    /// la carga ansiosa de sus relaciones.
    /// </summary>
    public class UserClientRepository : BaseRepository<UserClient>, IUserClientRepository
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UserClientRepository"/>.
        /// </summary>
        /// <param name="context">El contexto de la base de datos de la aplicación (<see cref="AppDbContext"/>).</param>
        public UserClientRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene un cliente de usuario por su identificador único de forma asíncrona.
        /// Incluye ansiosamente sus colecciones de comunidades, libros favoritos y libros excluidos.
        /// </summary>
        /// <param name="id">El identificador único del cliente de usuario.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. 
        /// El resultado de la tarea es el <see cref="UserClient"/> encontrado, o <c>null</c> si no existe.
        /// </returns>
        public new async Task<UserClient> GetByIdAsync(int id)
        {
            return await this.Context.UserClients
                .Include(uc => uc.UserCommunities)
                .Include(uc => uc.FavoriteBooks)
                .Include(uc => uc.ExclusionBooks)
                .Include(uc => uc.ReadBooks)
                .FirstOrDefaultAsync(uc => uc.Id == id);
        }

        /// <summary>
        /// Obtiene una colección de todos los clientes de usuario de forma asíncrona.
        /// Incluye ansiosamente sus colecciones de comunidades, libros favoritos y libros excluidos.
        /// </summary>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. 
        /// El resultado de la tarea es una colección de todos los <see cref="UserClient"/>.
        /// Retorna una colección vacía si no hay clientes de usuario.
        /// </returns>
        public new async Task<IEnumerable<UserClient>> GetAllAsync()
        {
            return await this.Context.UserClients
                .Include(uc => uc.UserCommunities)
                .Include(uc => uc.FavoriteBooks)
                .Include(uc => uc.ExclusionBooks)
                .Include(uc => uc.ReadBooks)
                .ToListAsync();
        }

        /// <summary>
        /// Añade un nuevo cliente de usuario al contexto de la base de datos de forma asíncrona.
        /// Los cambios se persistirán en la base de datos cuando se llame a <c>_unitOfWork.CompleteAsync()</c>.
        /// </summary>
        /// <param name="userClient">La instancia de <see cref="UserClient"/> a añadir.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task AddAsync(UserClient userClient)
        {
            await this.Context.UserClients.AddAsync(userClient);
        }

        /// <summary>
        /// Marca un cliente de usuario existente para ser actualizado en el contexto de la base de datos.
        /// Los cambios se persistirán en la base de datos cuando se llame a <c>_unitOfWork.CompleteAsync()</c>.
        /// </summary>
        /// <param name="userClient">La instancia de <see cref="UserClient"/> con los datos actualizados.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task UpdateAsync(UserClient userClient)
        {
            this.Context.Entry(userClient).State = EntityState.Modified;
            await Task.CompletedTask; 
        }

        /// <summary>
        /// Marca un cliente de usuario para ser eliminado del contexto de la base de datos.
        /// Los cambios se persistirán en la base de datos cuando se llame a <c>_unitOfWork.CompleteAsync()</c>.
        /// </summary>
        /// <param name="userClient">La instancia de <see cref="UserClient"/> a eliminar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task DeleteAsync(UserClient userClient)
        {
            this.Context.UserClients.Remove(userClient);
            await Task.CompletedTask; 
        }

        /// <summary>
        /// Obtiene un cliente de usuario por su nombre de usuario de forma asíncrona.
        /// Incluye ansiosamente sus colecciones de comunidades, libros favoritos y libros excluidos.
        /// </summary>
        /// <param name="username">El nombre de usuario del cliente a buscar.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. 
        /// El resultado de la tarea es el <see cref="UserClient"/> encontrado, o <c>null</c> si no existe.
        /// </returns>
        public async Task<UserClient> GetByUsernameAsync(string username)
        {
            return await this.Context.UserClients
                .Include(uc => uc.UserCommunities)
                .Include(uc => uc.FavoriteBooks)
                .Include(uc => uc.ExclusionBooks)
                .Include(uc => uc.ReadBooks)
                .FirstOrDefaultAsync(uc => uc.Username == username);
        }

        /// <summary>
        /// Obtiene un cliente de usuario por su dirección de correo electrónico de forma asíncrona.
        /// Incluye ansiosamente sus colecciones de comunidades, libros favoritos y libros excluidos.
        /// </summary>
        /// <param name="email">La dirección de correo electrónico del cliente a buscar.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. 
        /// El resultado de la tarea es el <see cref="UserClient"/> encontrado, o <c>null</c> si no existe.
        /// </returns>
        public async Task<UserClient> GetByEmailAsync(string email)
        {
            return await this.Context.UserClients
                .Include(uc => uc.UserCommunities)
                .Include(uc => uc.FavoriteBooks) 
                .Include(uc => uc.ExclusionBooks)
                .FirstOrDefaultAsync(uc => uc.Email == email);
        }

        /// <summary>
        /// Verifica de forma asíncrona si un cliente de usuario con el nombre de usuario especificado ya existe.
        /// </summary>
        /// <param name="username">El nombre de usuario a verificar.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. 
        /// El resultado de la tarea es <c>true</c> si el nombre de usuario ya existe; de lo contrario, <c>false</c>.
        /// </returns>
        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await this.Context.UserClients.AnyAsync(uc => uc.Username == username);
        }

        /// <summary>
        /// Verifica de forma asíncrona si un cliente de usuario con la dirección de correo electrónico especificada ya existe.
        /// </summary>
        /// <param name="email">La dirección de correo electrónico a verificar.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona. 
        /// El resultado de la tarea es <c>true</c> si la dirección de correo electrónico ya existe; de lo contrario, <c>false</c>.
        /// </returns>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await this.Context.UserClients.AnyAsync(uc => uc.Email == email);
        }
    }
}