using LivriaBackend.communities.Domain.Model.Aggregates;
using LivriaBackend.communities.Domain.Model.Commands;
using LivriaBackend.communities.Domain.Services;
using LivriaBackend.communities.Domain.Repositories;
using LivriaBackend.shared.Domain.Repositories;
using LivriaBackend.users.Domain.Model.Services; 
using LivriaBackend.users.Domain.Model.Queries;
using System;
using System.Threading.Tasks;
using LivriaBackend.communities.Domain.Model.Queries;

namespace LivriaBackend.communities.Application.Internal.CommandServices
{
    public class CommentCommandService : ICommentCommandService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserClientQueryService _userClientQueryService; 
        private readonly ICommunityQueryService _communityQueryService;
        private readonly IUnitOfWork _unitOfWork;

        public CommentCommandService(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            IUserClientQueryService userClientQueryService,
            ICommunityQueryService communityQueryService,
            IUnitOfWork unitOfWork)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _userClientQueryService = userClientQueryService;
            _communityQueryService = communityQueryService;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Comment> Handle(CreateCommentCommand command)
        {
            bool postExists = await _postRepository.ExistsAsync(command.PostId);
            if (!postExists)
            {
                throw new ArgumentException($"Post with ID {command.PostId} not found.");
            }
            
            var userClient = await _userClientQueryService.Handle(new GetUserClientByIdQuery(command.UserId));
            if (userClient == null)
            {
                throw new ArgumentException($"User with ID {command.UserId} not found.");
            }

            var newComment = new Comment(
                command.PostId,
                command.UserId,
                userClient.Username,
                command.Content,
                command.Img
            );

            await _commentRepository.AddAsync(newComment);
            await _unitOfWork.CompleteAsync();
            return newComment;
        }
        
        public async Task<bool> Handle(DeleteCommentCommand command)
        {
            var commentDetails = await _commentRepository.GetPostAndAuthorIdsByIdAsync(command.CommentId);

            if (commentDetails == null)
            {
                return false;
            }

            var (postId, authorUserId) = commentDetails.Value;
            
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                throw new InvalidOperationException($"Post ID {postId} associated with comment {command.CommentId} not found.");
            }
            int communityId = post.CommunityId;

            
            bool isAuthor = authorUserId == command.UserId;
            bool isCommunityOwner = await _communityQueryService.Handle(new CheckUserOwnerQuery(command.UserId, communityId));

            if (!isAuthor && !isCommunityOwner)
            {
                throw new ApplicationException($"User ID {command.UserId} is neither the author of Comment ID {command.CommentId} nor the owner of Community ID {communityId} and cannot delete it.");
            }
            
            var commentToDelete = await _commentRepository.GetByIdAsync(command.CommentId);
            await _commentRepository.DeleteAsync(commentToDelete);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}