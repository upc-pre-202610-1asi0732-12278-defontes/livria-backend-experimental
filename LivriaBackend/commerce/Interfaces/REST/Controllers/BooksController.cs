using AutoMapper;
using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Commands;
using LivriaBackend.commerce.Domain.Model.Queries;
using LivriaBackend.commerce.Domain.Model.Services;
using LivriaBackend.commerce.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using LivriaBackend.users.Domain.Model.Commands;
using LivriaBackend.users.Domain.Model.Queries;
using LivriaBackend.users.Domain.Model.Services;

namespace LivriaBackend.commerce.Interfaces.REST.Controllers
{
    /// <summary>
    /// Controlador RESTful para gestionar las operaciones relacionadas con los libros.
    /// </summary>
    [ApiController]
    [Route("api/v1/books")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)] 

    public class BooksController : ControllerBase
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookCommandService _bookCommandService;
        private readonly IUserClientCommandService _userClientCommandService;
        private readonly IUserClientQueryService _userClientQueryService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="BooksController"/>.
        /// </summary>
        /// <param name="bookQueryService">El servicio de consulta de libros.</param>
        /// <param name="bookCommandService">El servicio de comandos de libros.</param>
        /// <param name="userClientCommandService">Servicio de comandos de usuario (libros leídos).</param>
        /// <param name="userClientQueryService">Servicio de consulta de usuario (libros leídos).</param>
        /// <param name="mapper">La instancia de AutoMapper para la transformación de objetos.</param>
        public BooksController(
            IBookQueryService bookQueryService,
            IBookCommandService bookCommandService,
            IUserClientCommandService userClientCommandService,
            IUserClientQueryService userClientQueryService,
            IMapper mapper)
        {
            _bookQueryService = bookQueryService;
            _bookCommandService = bookCommandService;
            _userClientCommandService = userClientCommandService;
            _userClientQueryService = userClientQueryService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene los datos de todos los libros disponibles en el sistema.
        /// </summary>
        /// <returns>
        /// Una acción de resultado HTTP que contiene una colección de <see cref="BookResource"/>
        /// si la operación es exitosa (código 200 OK).
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        [SwaggerOperation(
            Summary= "Obtener los datos de todos los libros.",
            Description= "Te muestra los datos de los libros."
            
        )]
        public async Task<ActionResult<IEnumerable<BookResource>>> GetAllBooks()
        {
            var query = new GetAllBooksQuery();
            var books = await _bookQueryService.Handle(query);
            var bookResources = _mapper.Map<IEnumerable<BookResource>>(books);
            return Ok(bookResources);
        }
        
        [AllowAnonymous]
        [HttpGet("random")]
        [SwaggerOperation(
            Summary = "Obtener libros aleatorios.",
            Description = "Devuelve una cantidad específica de libros aleatorios, con opción de excluir IDs."
        )]
        public async Task<ActionResult<IEnumerable<BookResource>>> GetRandomBooks(
            [FromQuery] int count = 10,
            [FromQuery] string? exclude = null)
        {
            var excludeIds = string.IsNullOrWhiteSpace(exclude)
                ? null
                : exclude.Split(',').Select(int.Parse).ToList();

            var query = new GetRandomBooksQuery(count, excludeIds);
            var books = await _bookQueryService.Handle(query);
            var bookResources = _mapper.Map<IEnumerable<BookResource>>(books);
            return Ok(bookResources);
        }

        [AllowAnonymous]
        [HttpGet("genre/{genre}")]
        [SwaggerOperation(
            Summary = "Obtener libros por género.",
            Description = "Devuelve todos los libros que pertenecen a un género específico."
        )]
        public async Task<ActionResult<IEnumerable<BookResource>>> GetBooksByGenre(string genre)
        {
            var query = new GetBooksByGenreQuery(genre);
            var books = await _bookQueryService.Handle(query);
            var bookResources = _mapper.Map<IEnumerable<BookResource>>(books);
            return Ok(bookResources);
        }

        /// <summary>
        /// Obtiene los datos de un libro específico por su identificador único.
        /// </summary>
        /// <param name="id">El identificador único del libro.</param>
        /// <returns>
        /// Una acción de resultado HTTP que contiene un <see cref="BookResource"/> si el libro es encontrado (código 200 OK),
        /// o un resultado NotFound (código 404) si el libro no existe.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary= "Obtener los datos de un libro en específico.",
            Description= "Te muestra los datos del libro que buscaste."
        )]
        public async Task<ActionResult<BookResource>> GetBookById(int id)
        {
            var query = new GetBookByIdQuery(id);
            var book = await _bookQueryService.Handle(query);

            if (book == null)
            {
                return NotFound();
            }

            var bookResource = _mapper.Map<BookResource>(book);
            return Ok(bookResource);
        }

        /// <summary>
        /// Crea un nuevo libro en el sistema.
        /// </summary>
        /// <param name="resource">Los datos del nuevo libro a crear.</param>
        /// <returns>
        /// Una acción de resultado HTTP que contiene el <see cref="BookResource"/> del libro creado
        /// con un código 201 CreatedAtAction si la operación es exitosa.
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "UserClient,Admin")]
        [SwaggerOperation(
            Summary= "Crear un nuevo libro.",
            Description= "Crea un nuevo libro en el sistema."
        )]
        public async Task<ActionResult<BookResource>> CreateBook([FromBody] CreateBookResource resource)
        {
            var createCommand = _mapper.Map<CreateBookCommand>(resource);
            try
            {
                var book = await _bookCommandService.Handle(createCommand); 

                var bookResource = _mapper.Map<BookResource>(book);
                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, bookResource);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (LivriaBackend.shared.Domain.Exceptions.DuplicateEntityException ex) 
            {
                return Conflict(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An internal server error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Aumenta el stock de un libro existente por su ID.
        /// </summary>
        /// <param name="id">El identificador único del libro cuyo stock se aumentará.</param>
        /// <param name="resource">El recurso que contiene la cantidad a añadir al stock.</param>
        /// <returns>
        /// Una acción de resultado HTTP que contiene el <see cref="BookResource"/> actualizado
        /// si la operación fue exitosa (código 200 OK).
        /// Retorna 400 Bad Request si la validación de la cantidad falla o los datos son inválidos.
        /// Retorna 404 Not Found si el libro no existe.
        /// Retorna 500 Internal Server Error si ocurre un error inesperado.
        /// </returns>
        [Authorize(Roles = "UserClient,Admin")]
        [HttpPut("{id}/stock")] 
        [SwaggerOperation(
            Summary = "Aumentar el stock de un libro.",
            Description = "Permite añadir una cantidad específica al stock disponible de un libro."
        )]
        [ProducesResponseType(typeof(BookResource), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddBookStock(int id, [FromBody] UpdateBookStockResource resource) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var command = new UpdateBookStockCommand(id, resource.QuantityToAdd); 

            try
            {
                var updatedBook = await _bookCommandService.Handle(command);
                
                if (updatedBook == null)
                {
                    return NotFound(new { message = $"Book with ID {id} not found." });
                }
                
                var bookResource = _mapper.Map<BookResource>(updatedBook);
                return Ok(bookResource);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while updating book stock: " + ex.Message });
            }
        }

        [Authorize(Roles = "UserClient,Admin")]
        [HttpPut("{id}/update")]
        [SwaggerOperation(
            Summary = "Actualizar la información de un libro.",
            Description = "Permite cambiar la información de los atributos de un libro en específico."
        )]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookResource resource)
        {
            var command = new UpdateBookCommand(id, resource.Title, resource.Description, 
                resource.Author, resource.PurchasePrice, resource.Cover, 
                resource.Genre, resource.Language);
            try
            {
                var result = await _bookCommandService.Handle(command);
                if (result == null) return NotFound();
            
                var response = _mapper.Map<BookResource>(result);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "UserClient,Admin")]
        [HttpPatch("{id}/deactivate")]
        [SwaggerOperation(
            Summary = "Desactivar un libro (Soft Delete).",
            Description = "Cambia el estado del libro a inactivo. El libro permanecerá en el sistema pero no será visible en el catálogo general."
        )]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var command = new DeleteBookCommand(id);
            var result = await _bookCommandService.Handle(command);

            if (!result) return NotFound(new { message = "Book not found." });
            
            return Ok(new { message = "Book deactivated successfully." });
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet("deactivated")]
        [SwaggerOperation(
            Summary = "Obtener lista de libros desactivados.",
            Description = "Permite visualizar todos los libros con el estado 'desactivado'."
        )]
        public async Task<IActionResult> GetDeletedBooks()
        {
            var query = new GetDeletedBooksQuery();
            var books = await _bookQueryService.Handle(query);
            var resources = _mapper.Map<IEnumerable<BookResource>>(books);
            return Ok(resources);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/reactivate")]
        public async Task<IActionResult> ReactivateBook(int id)
        {
            var command = new ReactivateBookCommand(id);
            var result = await _bookCommandService.Handle(command);
    
            if (result == null) return NotFound();
    
            var resource = _mapper.Map<BookResource>(result);
            return Ok(resource);
        }

        /// <summary>
        /// Obtiene los libros marcados como leídos por un usuario (Mis libros).
        /// </summary>
        [Authorize(Roles = "UserClient,Admin")]
        [HttpGet("read/{userClientId:int}")]
        [SwaggerOperation(
            Summary = "Obtener libros leídos de un usuario.",
            Description = "Devuelve la lista de libros que el usuario marcó como leídos."
        )]
        [ProducesResponseType(typeof(IEnumerable<BookResource>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BookResource>>> GetReadBooks(int userClientId)
        {
            try
            {
                var books = await _userClientQueryService.Handle(new GetReadBooksByUserQuery(userClientId));
                return Ok(_mapper.Map<IEnumerable<BookResource>>(books));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Marca o desmarca un libro como leído (toggle).
        /// </summary>
        [Authorize(Roles = "UserClient,Admin")]
        [HttpPatch("{bookId:int}/read/toggle")]
        [SwaggerOperation(
            Summary = "Toggle libro leído.",
            Description = "Si el libro no está leído lo marca; si ya está leído lo desmarca."
        )]
        [ProducesResponseType(typeof(ToggleReadBookResponseResource), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ToggleReadBookResponseResource>> ToggleReadBook(
            int bookId,
            [FromBody] ToggleReadBookResource resource)
        {
            try
            {
                var result = await _userClientCommandService.Handle(
                    new ToggleReadBookCommand(resource.UserClientId, bookId));
                return Ok(new ToggleReadBookResponseResource(result.BookId, result.IsRead));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}