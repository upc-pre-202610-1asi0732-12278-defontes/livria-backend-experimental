using LivriaBackend.commerce.Domain.Model.Aggregates; 
using LivriaBackend.communities.Domain.Model.Aggregates;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LivriaBackend.users.Domain.Model.Aggregates
{
    /// <summary>
    /// Representa un cliente de usuario en el sistema.
    /// Hereda de la clase base <see cref="User"/> y añade propiedades específicas del cliente,
    /// como su icono, frase personal, suscripción, y colecciones relacionadas.
    /// </summary>
    public class UserClient : User
    {
        /// <summary>
        /// Obtiene el URL o identificador del icono/avatar del usuario.
        /// </summary>
        /// <remarks>Puede ser nulo en datos legados o si la columna en BD admite NULL.</remarks>
        public string? Icon { get; private set; }

        /// <summary>
        /// Obtiene una frase o estado personal del usuario.
        /// </summary>
        public string Phrase { get; private set; }

        /// <summary>
        /// Obtiene el plan de suscripción actual del usuario (ej. "freeplan", "communityplan").
        /// </summary>
        public string Subscription { get; private set; }
        
        /// <summary>
        /// Nuevos campos para la supervisión del pago mensual del usuario con plan comunidad
        /// </summary>
        public DateTime? PlanChangeDate { get; private set; }
        public bool HasPayed { get; private set; }

        /// <summary>
        /// Saldo disponible en la billetera virtual Livria.
        /// </summary>
        public decimal Wallet { get; private set; }
        

        /// <summary>
        /// Obtiene la colección de comunidades a las que este usuario se ha unido.
        /// </summary>
        public ICollection<UserCommunity> UserCommunities { get; private set; } = new List<UserCommunity>();

        /// <summary>
        /// Lista interna de libros favoritos del usuario.
        /// </summary>
        private readonly List<Book> _favoriteBooks = new List<Book>();

        /// <summary>
        /// Obtiene una colección de solo lectura de los libros favoritos del usuario.
        /// </summary>
        public IReadOnlyCollection<Book> FavoriteBooks => _favoriteBooks.AsReadOnly();
        
        /// <summary>
        /// Lista interna de libros excluidos por el usuario.
        /// </summary>
        private readonly List<Book> _exclusionBooks = new List<Book>();

        /// <summary>
        /// Obtiene una colección de solo lectura de los libros excluidos por el usuario.
        /// Esto permite al usuario marcar libros que NO desea ver o recibir recomendaciones.
        /// </summary>
        public IReadOnlyCollection<Book> ExclusionBooks => _exclusionBooks.AsReadOnly();

        private readonly List<Book> _readBooks = new List<Book>();

        /// <summary>
        /// Libros marcados como leídos por el usuario (Mis libros).
        /// </summary>
        public IReadOnlyCollection<Book> ReadBooks => _readBooks.AsReadOnly();


        /// <summary>
        /// Constructor protegido sin parámetros, típicamente utilizado por ORMs como Entity Framework Core.
        /// Inicializa las colecciones de navegación.
        /// </summary>
        protected UserClient() : base()
        {
            UserCommunities = new List<UserCommunity>();
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="UserClient"/> con las propiedades especificadas.
        /// Inicializa las colecciones de órdenes y comunidades.
        /// </summary>
        /// <param name="display">El nombre visible o alias del cliente.</param>
        /// <param name="username">El nombre de usuario único del cliente.</param>
        /// <param name="email">La dirección de correo electrónico del cliente.</param>
        /// <param name="icon">El URL o identificador del icono/avatar del cliente.</param>
        /// <param name="phrase">La frase o estado personal del cliente.</param>
        /// <param name="subscription">El plan de suscripción del cliente.</param>
        public UserClient(string display, string username, string email, string? icon, string phrase, string subscription)
            : base(display, username, email)
        {
            Icon = icon;
            Phrase = phrase;
            Subscription = subscription;
            HasPayed = false;
            PlanChangeDate = null;
            Wallet = 0m;
            UserCommunities = new List<UserCommunity>();
        }

        public bool HasSufficientWalletBalance(decimal amount) => Wallet >= amount;

        public void CreditWallet(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));
            Wallet += amount;
        }

        public void DebitWallet(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));
            if (Wallet < amount)
                throw new InvalidOperationException("Insufficient wallet balance.");
            Wallet -= amount;
        }

        /// <summary>
        /// Actualiza las propiedades de un cliente de usuario.
        /// Utiliza el método base <see cref="User.UpdateUserProperties"/> para actualizar las propiedades de usuario comunes.
        /// </summary>
        /// <param name="display">El nuevo nombre visible o alias.</param>
        /// <param name="username">El nuevo nombre de usuario.</param>
        /// <param name="email">La nueva dirección de correo electrónico.</param>
        /// <param name="icon">El nuevo URL o identificador del icono.</param>
        /// <param name="phrase">La nueva frase o estado personal.</param>
        public void Update(string display, string email, string? icon, string phrase)
        {
            base.UpdateUserProperties(display, email);
            Icon = icon;
            Phrase = phrase;
        }

        /// <summary>
        /// Actualiza el plan de suscripción del usuario cliente.
        /// </summary>
        /// <param name="newSubscriptionPlan">El nuevo plan de suscripción (ej. "freeplan", "communityplan").</param>
        /// <exception cref="ArgumentException">Se lanza si el nuevo plan de suscripción es nulo o vacío.</exception>
        public void UpdateSubscription(string newSubscriptionPlan)
        {
            if (string.IsNullOrWhiteSpace(newSubscriptionPlan))
                throw new ArgumentException("Subscription plan cannot be empty.", nameof(newSubscriptionPlan));
    
            Subscription = newSubscriptionPlan;
            PlanChangeDate = DateTime.UtcNow;
    
            if (newSubscriptionPlan == "communityplan")
                HasPayed = true;
            else if (newSubscriptionPlan == "freeplan")
                HasPayed = false;
        }
        public void SetHasPayed(bool hasPayed)
        {
            HasPayed = hasPayed;
            if (hasPayed)
                PlanChangeDate = DateTime.UtcNow; // reinicia el ciclo mensual
        }

        /// <summary>
        /// Método para verificar si venció del plazo
        /// </summary>
        /// <returns></returns>
        public bool IsPaymentOverdue()
        {
            if (Subscription != "communityplan" || !PlanChangeDate.HasValue)
                return false;
            return DateTime.UtcNow > PlanChangeDate.Value.AddDays(37); // 30 días del ciclo + 7 de gracia
        }
        
        /// <summary>
        /// Permite al usuario unirse a una comunidad específica.
        /// Añade una nueva entrada <see cref="UserCommunity"/> si el usuario no está ya en la comunidad.
        /// </summary>
        /// <param name="communityId">El ID de la comunidad a la que el usuario se unirá.</param>
        public void JoinCommunity(int communityId)
        {
            if (!UserCommunities.Any(uc => uc.CommunityId == communityId))
            {
                UserCommunities.Add(new UserCommunity(this.Id, communityId));
            }
        }

        /// <summary>
        /// Permite al usuario dejar una comunidad específica.
        /// Elimina la entrada <see cref="UserCommunity"/> correspondiente si el usuario está en la comunidad.
        /// </summary>
        /// <param name="communityId">El ID de la comunidad que el usuario dejará.</param>
        public void LeaveCommunity(int communityId)
        {
                var userCommunity = UserCommunities.FirstOrDefault(uc => uc.CommunityId == communityId);
                if (userCommunity != null)
                {
                    UserCommunities.Remove(userCommunity);
                }
        }
        
        /// <summary>
        /// Añade un libro a la colección de libros favoritos del usuario.
        /// El libro no se añade si ya está presente en la colección.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a añadir a favoritos.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el libro proporcionado es nulo.</exception>
        public void AddFavoriteBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (!_favoriteBooks.Contains(book))
            {
                _favoriteBooks.Add(book);
            }
            
            // Lógica adicional: Si se añade un libro a favoritos, se elimina de excluidos
            RemoveExclusionBook(book);
        }
        
        /// <summary>
        /// Elimina un libro de la colección de libros favoritos del usuario.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a eliminar de favoritos.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el libro proporcionado es nulo.</exception>
        public void RemoveFavoriteBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }
            _favoriteBooks.Remove(book);
        }

        /// <summary>
        /// Añade un libro a la colección de libros excluidos (no deseados) del usuario.
        /// El libro no se añade si ya está presente en la colección y se elimina de favoritos si estaba allí.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a añadir a la lista de exclusión.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el libro proporcionado es nulo.</exception>
        public void AddExclusionBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (!_exclusionBooks.Contains(book))
            {
                _exclusionBooks.Add(book);
            }
            RemoveFavoriteBook(book);
        }
        
        /// <summary>
        /// Elimina un libro de la colección de libros excluidos del usuario.
        /// </summary>
        /// <param name="book">El objeto <see cref="Book"/> a eliminar de la lista de exclusión.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el libro proporcionado es nulo.</exception>
        public void RemoveExclusionBook(Book book)
        {
            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }
            _exclusionBooks.Remove(book);
        }

        /// <summary>
        /// Alterna el estado leído de un libro. Devuelve true si quedó marcado como leído, false si se desmarcó.
        /// </summary>
        public bool ToggleReadBook(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            if (_readBooks.Contains(book))
            {
                _readBooks.Remove(book);
                return false;
            }

            _readBooks.Add(book);
            return true;
        }
    }
}