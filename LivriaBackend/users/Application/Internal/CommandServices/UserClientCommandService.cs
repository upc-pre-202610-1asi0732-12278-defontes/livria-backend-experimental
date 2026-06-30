using LivriaBackend.shared.Domain.Repositories;
using System;
using System.Threading.Tasks;
using LivriaBackend.commerce.Domain.Repositories; 
using System.Linq;
using LivriaBackend.communities.Domain.Repositories;
using LivriaBackend.IAM.Domain.Repositories;
using LivriaBackend.notifications.Domain.Model.Services; 
using LivriaBackend.notifications.Domain.Model.Commands; 
using LivriaBackend.notifications.Domain.Model.ValueObjects;
using LivriaBackend.users.Domain.Model.Aggregates;
using LivriaBackend.users.Domain.Model.Commands;
using LivriaBackend.users.Domain.Model.Repositories;
using LivriaBackend.users.Domain.Model.Services;


namespace LivriaBackend.users.Application.Internal.CommandServices
{
    /// <summary>
    /// Implementa el servicio de comandos para las operaciones de la entidad <see cref="UserClient"/>.
    /// Encapsula la lógica de negocio para la gestión de clientes de usuario, incluyendo
    /// la interacción con repositorios de libros y servicios de notificación.
    /// </summary>
    public class UserClientCommandService : IUserClientCommandService
    {
        private readonly IUserClientRepository _userClientRepository;
        private readonly IBookRepository _bookRepository; 
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationCommandService _notificationCommandService;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IIdentityRepository _identityRepository;
        private readonly ICommunityRepository _communityRepository;
        private readonly IUserAdminRepository _userAdminRepository;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UserClientCommandService"/>.
        /// </summary>
        /// <param name="userClientRepository">El repositorio para las operaciones de datos del cliente de usuario.</param>
        /// <param name="bookRepository">El repositorio para las operaciones de datos de libros.</param>
        /// <param name="unitOfWork">La unidad de trabajo para gestionar las transacciones de base de datos.</param>
        /// <param name="notificationCommandService">El servicio de comandos para la creación de notificaciones.</param>
        public UserClientCommandService(
            IUserClientRepository userClientRepository,
            IBookRepository bookRepository,
            IUnitOfWork unitOfWork,
            INotificationCommandService notificationCommandService,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IReviewRepository reviewRepository,
            IIdentityRepository identityRepository,
            ICommunityRepository communityRepository,
            IUserAdminRepository userAdminRepository)
             
        {
            _userClientRepository = userClientRepository;
            _bookRepository = bookRepository;
            _unitOfWork = unitOfWork;
            _notificationCommandService = notificationCommandService;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _reviewRepository = reviewRepository;
            _identityRepository = identityRepository;
            _communityRepository = communityRepository;
            _userAdminRepository = userAdminRepository;
        }

        /// <summary>
        /// Maneja el comando para crear un nuevo cliente de usuario.
        /// Realiza validaciones de unicidad para el nombre de usuario y el correo electrónico.
        /// Tras la creación exitosa, genera una notificación de bienvenida.
        /// </summary>
        /// <param name="command">El comando <see cref="CreateUserClientCommand"/> que contiene los datos para el nuevo cliente de usuario.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> recién creado.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Se lanza si ya existe un usuario con el mismo nombre de usuario o correo electrónico.
        /// </exception>
        public async Task<UserClient> Handle(CreateUserClientCommand command)
        {
            
            if (await _userClientRepository.ExistsByUsernameAsync(command.Username))
            {
                throw new ArgumentException($"User with username '{command.Username}' already exists.");
            }
            
            if (await _userClientRepository.ExistsByEmailAsync(command.Email))
            {
                throw new ArgumentException($"User with email '{command.Email}' already exists.");
            }
            
            var userClient = new UserClient(
                command.Display,
                command.Username,
                command.Email,
                command.Icon, 
                command.Phrase, 
                "freeplan"
            );
            
            await _userClientRepository.AddAsync(userClient);
            await _unitOfWork.CompleteAsync();

            
            await _notificationCommandService.Handle(new CreateNotificationCommand(
                userClient.Id,           
                ENotificationType.Welcome, 
                DateTime.UtcNow          
            ));

            return userClient;
        }

        /// <summary>
        /// Maneja el comando para actualizar un cliente de usuario existente.
        /// Si el plan de suscripción cambia a "communityplan", se genera una notificación.
        /// </summary>
        /// <param name="command">El comando <see cref="UpdateUserClientCommand"/> que contiene los datos actualizados del cliente de usuario.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> actualizado.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Se lanza si el cliente de usuario con el ID especificado no se encuentra.
        /// </exception>
        public async Task<UserClient> Handle(UpdateUserClientCommand command)
        {
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
            {
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.");
            }
            
            var oldSubscription = userClient.Subscription;
            
            userClient.Update(
                command.Display,
                command.Email,
                command.Icon,
                command.Phrase
                );

            
            await _userClientRepository.UpdateAsync(userClient);
            await _unitOfWork.CompleteAsync();
            
            if (oldSubscription != "communityplan" && userClient.Subscription == "communityplan")
            {
                await _notificationCommandService.Handle(new CreateNotificationCommand(
                    userClient.Id,       
                    ENotificationType.Plan, 
                    DateTime.UtcNow      
                ));
            }

            return userClient;
        }

        /// <summary>
        /// Maneja el comando para eliminar un cliente de usuario existente.
        /// </summary>
        /// <param name="command">El comando <see cref="DeleteUserClientCommand"/> que contiene el ID del cliente de usuario a eliminar.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es <c>true</c> si el cliente de usuario fue eliminado exitosamente; de lo contrario, <c>false</c>.
        /// </returns>
        public async Task<bool> Handle(DeleteUserClientCommand command)
        {
            
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
            {
                return false; 
            }
            
            // Anonimizar Posts
            var posts = await _postRepository.GetPostsByUserIdAsync(command.UserClientId);
            foreach (var post in posts)
            {
                post.Anonymize();
                await _postRepository.UpdateAsync(post);
            }

            // Anonimizar Comentarios
            var comments = await _commentRepository.GetCommentsByUserIdAsync(command.UserClientId); 
            foreach (var comment in comments)
            {
                comment.Anonymize();
                await _commentRepository.UpdateAsync(comment);
            }
    
            // Anonimizar Reseñas
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(command.UserClientId);
            foreach (var review in reviews)
            {
                review.Anonymize();
                await _reviewRepository.UpdateAsync(review);
            }
            
            var communities = await _communityRepository.GetCommunitiesByOwnerIdAsync(command.UserClientId);
            foreach (var community in communities)
            {
                community.AnonymizeOwner(UserConstants.DeletedUserId);
                await _communityRepository.UpdateAsync(community);
            }
            
            var identity = await _identityRepository.GetByUserIdAsync(command.UserClientId);
            if (identity != null)
            {
                await _identityRepository.DeleteAsync(identity);
            }
            
            await _userClientRepository.DeleteAsync(userClient);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>
        /// Maneja el comando para añadir un libro a la lista de favoritos de un cliente de usuario.
        /// </summary>
        /// <param name="command">El comando <see cref="AddFavoriteBookCommand"/> que contiene los IDs del usuario y del libro.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> con el libro añadido a favoritos.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Se lanza si el cliente de usuario o el libro no se encuentran.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Se lanza si el libro ya está en la lista de favoritos del usuario.
        /// </exception>
        public async Task<UserClient> Handle(AddFavoriteBookCommand command)
        {
            
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
            {
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.");
            }
            
            var book = await _bookRepository.GetByIdAsync(command.BookId);
            if (book == null)
            {
                throw new ArgumentException($"Book with ID {command.BookId} not found.");
            }
            
            if (userClient.FavoriteBooks.Any(fb => fb.Id == book.Id))
            {
                throw new InvalidOperationException($"Book with ID {book.Id} is already in favorites for UserClient {userClient.Id}.");
            }
            
            userClient.AddFavoriteBook(book); 
            await _userClientRepository.UpdateAsync(userClient); 
            await _unitOfWork.CompleteAsync(); 

            return userClient;
        }

        /// <summary>
        /// Maneja el comando para eliminar un libro de la lista de favoritos de un cliente de usuario.
        /// </summary>
        /// <param name="command">El comando <see cref="RemoveFavoriteBookCommand"/> que contiene los IDs del usuario y del libro.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> con el libro eliminado de favoritos.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Se lanza si el cliente de usuario o el libro no se encuentran.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Se lanza si el libro no está en la lista de favoritos del usuario.
        /// </exception>
        public async Task<UserClient> Handle(RemoveFavoriteBookCommand command)
        {
            
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
            {
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.");
            }
            
            var book = await _bookRepository.GetByIdAsync(command.BookId);
            if (book == null)
            {
                throw new ArgumentException($"Book with ID {command.BookId} not found.");
            }
            
            if (!userClient.FavoriteBooks.Any(fb => fb.Id == book.Id))
            {
                throw new InvalidOperationException($"Book with ID {book.Id} is not in favorites for UserClient {userClient.Id}.");
            }
            
            userClient.RemoveFavoriteBook(book); 
            await _userClientRepository.UpdateAsync(userClient); 
            await _unitOfWork.CompleteAsync(); 

            return userClient;
        }

        public async Task<ToggleReadBookResult> Handle(ToggleReadBookCommand command)
        {
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.");

            var book = await _bookRepository.GetByIdAsync(command.BookId);
            if (book == null)
                throw new ArgumentException($"Book with ID {command.BookId} not found.");

            var isRead = userClient.ToggleReadBook(book);
            await _userClientRepository.UpdateAsync(userClient);
            await _unitOfWork.CompleteAsync();

            return new ToggleReadBookResult(book.Id, isRead);
        }
        
        /// <summary>
        /// Maneja el comando para actualizar el plan de suscripción de un cliente de usuario.
        /// Realiza validación del plan y genera una notificación si el plan cambia a "communityplan".
        /// </summary>
        /// <param name="command">El comando <see cref="UpdateUserClientSubscriptionCommand"/> que contiene el ID del usuario y el nuevo plan de suscripción.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> con el plan de suscripción actualizado.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Se lanza si el cliente de usuario no se encuentra o si el nuevo plan de suscripción es inválido.
        /// </exception>
        public async Task<UserClient> Handle(UpdateUserClientSubscriptionCommand command)
        {
            
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
            {
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.");
            }

            var oldSubscription = userClient.Subscription;
            
            
            if (command.NewSubscriptionPlan != "freeplan" && command.NewSubscriptionPlan != "communityplan")
            {
                throw new ArgumentException("Invalid subscription plan. Must be 'freeplan' or 'communityplan'.");
            }
            
            userClient.UpdateSubscription(command.NewSubscriptionPlan);

            await _userClientRepository.UpdateAsync(userClient);
            await _unitOfWork.CompleteAsync();
            
            if (oldSubscription != "communityplan" && userClient.Subscription == "communityplan")
            {
                await _notificationCommandService.Handle(new CreateNotificationCommand(
                    userClient.Id,       
                    ENotificationType.Plan, 
                    DateTime.UtcNow      
                ));
                
                var userAdmins = await _userAdminRepository.GetAllAsync();
                var admin = userAdmins.FirstOrDefault();

                if (admin != null)
                {
                    admin.AddCapital(19.90m);
                    await _userAdminRepository.UpdateAsync(admin);
                }
            }

            return userClient;
        }

        /// <summary>
        /// Maneja el comando para añadir un libro a la lista de exclusión de un cliente de usuario.
        /// </summary>
        /// <param name="command">El comando <see cref="AddExclusionBookCommand"/> que contiene los IDs del usuario y del libro.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> con el libro añadido a la lista de exclusión.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Se lanza si el cliente de usuario o el libro no se encuentran.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Se lanza si el libro ya está en la lista de exclusión del usuario.
        /// </exception>
        public async Task<UserClient> Handle(AddExclusionBookCommand command)
        {
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
            {
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.");
            }
            
            var book = await _bookRepository.GetByIdAsync(command.BookId);
            if (book == null)
            {
                throw new ArgumentException($"Book with ID {command.BookId} not found.");
            }
            
            if (userClient.ExclusionBooks.Any(eb => eb.Id == book.Id))
            {
                throw new InvalidOperationException($"Book with ID {book.Id} is already in exclusions for UserClient {userClient.Id}.");
            }
            
            userClient.AddExclusionBook(book); 
            await _userClientRepository.UpdateAsync(userClient); 
            await _unitOfWork.CompleteAsync(); 

            return userClient;
        }
        
        /// <summary>
        /// Maneja el comando para eliminar un libro de la lista de exclusión de un cliente de usuario.
        /// </summary>
        /// <param name="command">El comando <see cref="RemoveExclusionBookCommand"/> que contiene los IDs del usuario y del libro.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="UserClient"/> con el libro eliminado de la lista de exclusión.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Se lanza si el cliente de usuario o el libro no se encuentran.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Se lanza si el libro no está en la lista de exclusión del usuario.
        /// </exception>
        public async Task<UserClient> Handle(RemoveExclusionBookCommand command)
        {
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
            {
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.");
            }
            
            var book = await _bookRepository.GetByIdAsync(command.BookId);
            if (book == null)
            {
                throw new ArgumentException($"Book with ID {command.BookId} not found.");
            }
            
            if (!userClient.ExclusionBooks.Any(eb => eb.Id == book.Id))
            {
                throw new InvalidOperationException($"Book with ID {book.Id} is not in exclusions for UserClient {userClient.Id}.");
            }
            
            userClient.RemoveExclusionBook(book); 
            await _userClientRepository.UpdateAsync(userClient); 
            await _unitOfWork.CompleteAsync(); 

            return userClient;
        }
        
        /// <summary>
        /// Maneja el comando para actualizar si el cliente ha pagado.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<UserClient> Handle(UpdateUserClientHasPayedCommand command)
        {
            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.");

            userClient.SetHasPayed(command.HasPayed);
            await _userClientRepository.UpdateAsync(userClient);
            await _unitOfWork.CompleteAsync();
            return userClient;
        }
    }
}