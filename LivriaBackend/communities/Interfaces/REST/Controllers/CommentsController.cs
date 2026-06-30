using AutoMapper;
using LivriaBackend.communities.Domain.Model.Commands;
using LivriaBackend.communities.Domain.Model.Queries;
using LivriaBackend.communities.Domain.Services;
using LivriaBackend.communities.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace LivriaBackend.communities.Interfaces.REST.Controllers
{
    [ApiController]
    [Route("api/v1/comments")]
    [Produces(MediaTypeNames.Application.Json)]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentCommandService _commentCommandService;
        private readonly ICommentQueryService _commentQueryService;
        private readonly IMapper _mapper;

        public CommentsController(
            ICommentCommandService commentCommandService,
            ICommentQueryService commentQueryService,
            IMapper mapper)
        {
            _commentCommandService = commentCommandService;
            _commentQueryService = commentQueryService;
            _mapper = mapper;
        }
        
        [HttpPost]
        [SwaggerOperation(Summary = "Crear un nuevo comentario.")]
        [ProducesResponseType(typeof(CommentResource), 201)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<ActionResult<CommentResource>> CreateComment(
            [FromBody] CreateCommentResource resource)
        {
            var command = new CreateCommentCommand(
                resource.PostId,
                resource.UserId,
                resource.Content ?? string.Empty,
                resource.Img ?? string.Empty
            );
            
            var comment = await _commentCommandService.Handle(command);
            
            var commentResource = _mapper.Map<CommentResource>(comment);
            return StatusCode(201, commentResource);
        }
        
        [HttpDelete("{commentId}")]
        [SwaggerOperation(Summary = "Eliminar un comentario por ID (solo el autor o dueño de la comunidad).")]
        [ProducesResponseType(204)] // No Content
        [ProducesResponseType(typeof(string), 403)] // Forbidden (Permisos)
        [ProducesResponseType(typeof(string), 404)] // Not Found
        public async Task<ActionResult> DeleteComment(int commentId, [FromQuery] int userId)
        {
            var command = new DeleteCommentCommand(commentId, userId);
            
            try
            {
                var result = await _commentCommandService.Handle(command);
                if (!result)
                {
                    return NotFound($"Comment with ID {commentId} not found.");
                }
                return NoContent();
            }
            catch (ApplicationException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }
        
        [HttpGet("/api/v1/posts/{postId}/comments")]
        [SwaggerOperation(Summary = "Listar todos los comentarios de una publicación.")]
        [ProducesResponseType(typeof(IEnumerable<CommentResource>), 200)]
        public async Task<ActionResult<IEnumerable<CommentResource>>> GetCommentsByPostId(int postId)
        {
            var query = new GetCommentsByPostIdQuery(postId);
            var comments = await _commentQueryService.Handle(query);
            
            var resources = _mapper.Map<IEnumerable<CommentResource>>(comments);
            return Ok(resources);
        }
    }
}