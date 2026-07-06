using LivriaBackend.commerce.Domain.Model.Aggregates;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LivriaBackend.commerce.Domain.Repositories
{
    /// <summary>
    /// Define el contrato para un repositorio de <see cref="Book"/>.
    /// Proporciona métodos para acceder y manipular datos de libros.
    /// </summary>
    public interface IBookRepository
    {
        /// <summary>
        /// Obtiene un libro de forma asíncrona por su identificador único.
        /// </summary>
        /// <param name="id">El identificador único del libro.</param>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado de la tarea es el <see cref="Book"/> encontrado, o null si no existe.</returns>
        Task<Book> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todos los libros de forma asíncrona.
        /// </summary>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado de la tarea es una colección de todos los <see cref="Book"/>.</returns>
        Task<IEnumerable<Book>> GetAllAsync();

        /// <summary>
        /// Añade un nuevo libro de forma asíncrona al repositorio.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a añadir.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task AddAsync(Book book);

        /// <summary>
        /// Actualiza un libro existente de forma asíncrona en el repositorio.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a actualizar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task UpdateAsync(Book book); 

        /// <summary>
        /// Verifica si existe un libro con el mismo título (ignorando mayúsculas/minúsculas).
        /// </summary>
        /// <param name="title">El título del libro.</param>
        /// <returns>True si existe un libro con el mismo título, de lo contrario False.</returns>
        Task<bool> ExistsByTitleAsync(string title);
        
        Task DeleteAsync(Book book);
        
        Task<bool> ExistsByTitleAndAuthorAsync(string title, string author);

        Task<IEnumerable<Book>> GetAllDeletedAsync();

        Task<IEnumerable<Book>> GetByGenreAsync(string genre);

        Task<IEnumerable<Book>> GetRandomAsync(int count, List<int> excludeIds);
    }
}