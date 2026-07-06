using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Queries;
using LivriaBackend.commerce.Domain.Repositories;
using LivriaBackend.commerce.Domain.Model.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 

namespace LivriaBackend.commerce.Application.Internal.QueryServices
{
    /// <summary>
    /// Implementa el servicio de consultas para la entidad <see cref="Book"/>.
    /// Se encarga de procesar las solicitudes de lectura de datos de libros, interactuando con el repositorio.
    /// </summary>
    public class BookQueryService : IBookQueryService
    {
        private readonly IBookRepository _bookRepository;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="BookQueryService"/>.
        /// </summary>
        /// <param name="bookRepository">El repositorio de libros para acceder a los datos.</param>

        public BookQueryService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }
        /// <summary>
        /// Maneja el comando <see cref="GetBookByIdQuery"/> para obtener un libro específico por su identificador.
        /// </summary>
        /// <param name="query">La consulta que contiene el ID del libro a buscar.</param>
        /// <returns>El objeto <see cref="Book"/> si se encuentra; de lo contrario, null.</returns>
        /// <remarks>
        /// Este método delega la lógica de recuperación de datos al <see cref="IBookRepository"/>.
        /// </remarks>
        public async Task<Book?> Handle(GetBookByIdQuery query)
        {
            return await _bookRepository.GetByIdAsync(query.BookId);
        }
        
        /// <summary>
        /// Maneja el comando <see cref="GetAllBooksQuery"/> para obtener todos los libros disponibles.
        /// </summary>
        /// <param name="query">La consulta para obtener todos los libros (normalmente un objeto vacío).</param>
        /// <returns>Una colección de todos los objetos <see cref="Book"/>.</returns>
        /// <remarks>
        /// Este método delega la lógica de recuperación de datos al <see cref="IBookRepository"/>.
        /// </remarks>
        public async Task<IEnumerable<Book>> Handle(GetAllBooksQuery query)
        {
            return await _bookRepository.GetAllAsync();
        }
        
        public async Task<IEnumerable<Book>> Handle(GetDeletedBooksQuery query)
        {
            return await _bookRepository.GetAllDeletedAsync();
        }

        public async Task<IEnumerable<Book>> Handle(GetBooksByGenreQuery query)
        {
            return await _bookRepository.GetByGenreAsync(query.Genre);
        }

        public async Task<IEnumerable<Book>> Handle(GetRandomBooksQuery query)
        {
            var count = Math.Clamp(query.Count, 1, 20);
            var excludeIds = query.Exclude ?? new List<int>();
            return await _bookRepository.GetRandomAsync(count, excludeIds);
        }
    }
}