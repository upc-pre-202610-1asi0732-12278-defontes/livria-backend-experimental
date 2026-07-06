using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Repositories;
using LivriaBackend.shared.Infrastructure.Persistence.EFC.Configuration;
using LivriaBackend.shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;

namespace LivriaBackend.commerce.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación concreta del repositorio de <see cref="Book"/> utilizando Entity Framework Core.
    /// Hereda de <see cref="BaseRepository{TEntity}"/> y <see cref="IBookRepository"/>.
    /// </summary>
    public class BookRepository : BaseRepository<Book>, IBookRepository
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="BookRepository"/>.
        /// </summary>
        /// <param name="context">El contexto de la base de datos de la aplicación.</param>
        public BookRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene un libro de forma asíncrona por su identificador único, incluyendo sus reseñas.
        /// Sobrescribe el método base para incluir la carga ansiosa de la colección <see cref="Book.Reviews"/>.
        /// </summary>
        /// <param name="id">El identificador único del libro.</param>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado de la tarea es el <see cref="Book"/> encontrado con sus reseñas, o null si no existe.</returns>
        public new async Task<Book> GetByIdAsync(int id)
        {
            return await this.Context.Books
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        /// <summary>
        /// Obtiene todos los libros de forma asíncrona, incluyendo sus reseñas.
        /// Sobrescribe el método base para incluir la carga ansiosa de la colección <see cref="Book.Reviews"/>.
        /// </summary>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado de la tarea es una colección de todos los <see cref="Book"/> con sus reseñas.</returns>
        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await this.Context.Books
                .ToListAsync();
        }

        /// <summary>
        /// Añade un nuevo libro de forma asíncrona al repositorio.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a añadir.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public new async Task AddAsync(Book book)
        {
            await this.Context.Books.AddAsync(book);
        }

        /// <summary>
        /// Actualiza un libro existente de forma asíncrona en el repositorio.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a actualizar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        /// <remarks>
        /// Este método establece el estado de la entidad a <see cref="EntityState.Modified"/>,
        /// lo que indica a Entity Framework Core que la entidad ha sido modificada y debe ser guardada.
        /// No realiza una operación asíncrona de base de datos directa, sino que prepara el contexto.
        /// La persistencia se realiza con <c>UnitOfWork</c>.
        /// </remarks>
        public new async Task UpdateAsync(Book book) 
        {
            this.Context.Entry(book).State = EntityState.Modified;
            await Task.CompletedTask; 
        }

        /// <summary>
        /// Verifica si existe un libro con el mismo título (ignorando mayúsculas/minúsculas).
        /// </summary>
        /// <param name="title">El título del libro.</param>
        /// <returns>True si existe un libro con el mismo título, de lo contrario False.</returns>
        public new async Task<bool> ExistsByTitleAsync(string title) 
        {
            return await Context.Books.AnyAsync(b =>
                b.Title.ToLower() == title.ToLower());
        }
        
        public async Task<bool> ExistsByTitleAndAuthorAsync(string title, string author)
        {
            return await Context.Books.AnyAsync(b => 
                b.Title.ToLower() == title.ToLower() && 
                b.Author.ToLower() == author.ToLower() &&
                b.IsActive);
        }

        public async Task DeleteAsync(Book book)
        {
            this.Context.Entry(book).State = EntityState.Modified;
            await Task.CompletedTask;
        }
        
        public async Task<IEnumerable<Book>> GetAllDeletedAsync()
        {
            return await this.Context.Books
                .IgnoreQueryFilters()
                .Where(b => !b.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetByGenreAsync(string genre)
        {
            return await this.Context.Books
                .Where(b => b.Genre == genre)
                .ToListAsync();
        }
    }
}