using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Commands;
using LivriaBackend.commerce.Domain.Model.Entities;
using LivriaBackend.commerce.Domain.Repositories;
using LivriaBackend.commerce.Domain.Model.Services;
using LivriaBackend.shared.Domain.Repositories;
using LivriaBackend.users.Domain.Model.Repositories;
using LivriaBackend.commerce.Domain.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LivriaBackend.notifications.Domain.Model.Services;
using LivriaBackend.notifications.Domain.Model.Commands;
using LivriaBackend.notifications.Domain.Model.ValueObjects;
using LivriaBackend.users.Domain.Model.Services;
using LivriaBackend.wallet.Domain.Model.Aggregates;
using LivriaBackend.wallet.Domain.Repositories;


namespace LivriaBackend.commerce.Application.Internal.CommandServices
{
    /// <summary>
    /// Implementa el servicio de comandos para la entidad <see cref="Order"/>.
    /// Procesa comandos relacionados con la creación y gestión de órdenes de compra.
    /// </summary>
    public class OrderCommandService : IOrderCommandService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUserClientRepository _userClientRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationCommandService _notificationCommandService;
        private readonly IUserAdminCommandService _userAdminCommandService;
        private readonly IWalletTransactionRepository _walletTransactionRepository;


        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="OrderCommandService"/>.
        /// </summary>
        /// <param name="orderRepository">El repositorio de órdenes.</param>
        /// <param name="cartItemRepository">El repositorio de ítems del carrito.</param>
        /// <param name="bookRepository">El repositorio de libros.</param>
        /// <param name="userClientRepository">El repositorio de clientes de usuario.</param>
        /// <param name="unitOfWork">La unidad de trabajo para gestionar transacciones.</param>
        /// <param name="notificationCommandService">El servicio de comandos de notificación para enviar alertas al usuario.</param>
        /// <param name="userAdminCommandService">El servicio de comandos de UserAdmin para actualizar su capital.</param>
        public OrderCommandService(
            IOrderRepository orderRepository,
            ICartItemRepository cartItemRepository,
            IBookRepository bookRepository,
            IUserClientRepository userClientRepository,
            IUnitOfWork unitOfWork,
            INotificationCommandService notificationCommandService,
            IUserAdminCommandService userAdminCommandService,
            IWalletTransactionRepository walletTransactionRepository)
        {
            _orderRepository = orderRepository;
            _cartItemRepository = cartItemRepository;
            _bookRepository = bookRepository;
            _userClientRepository = userClientRepository;
            _unitOfWork = unitOfWork;
            _notificationCommandService = notificationCommandService;
            _userAdminCommandService = userAdminCommandService;
            _walletTransactionRepository = walletTransactionRepository;
        }

        /// <summary>
        /// Maneja el comando <see cref="CreateOrderCommand"/> para crear una nueva orden de compra,
        /// obteniendo los ítems directamente del carrito del usuario especificado por UserClientId.
        /// </summary>
        /// <param name="command">El comando que contiene los detalles de la orden, incluyendo el UserClientId.</param>
        /// <returns>El objeto <see cref="Order"/> creado.</returns>
        /// <exception cref="ArgumentException">
        /// Se lanza si el cliente de usuario no se encuentra, si el carrito está vacío,
        /// si los detalles de envío son inválidos para el tipo de entrega, o si un libro no se encuentra.
        /// </exception>
        /// <exception cref="InvalidOperationException">Se lanza si no hay suficiente stock para un libro.</exception>
        /// <remarks>
        /// Este método:
        /// 1. Valida la existencia del cliente de usuario y obtiene sus datos.
        /// 2. Recupera **todos** los <see cref="CartItem"/>s asociados al <see cref="UserClient"/>.
        /// 3. Valida que el carrito no esté vacío.
        /// 4. Valida los detalles de envío según si es `IsDelivery`.
        /// 5. Itera sobre los ítems del carrito para:
        ///    a. Validar la existencia y stock de cada libro.
        ///    b. Crear <see cref="OrderItem"/>s.
        ///    c. Disminuir el stock del libro.
        /// 6. Crea la nueva <see cref="Order"/> con todos sus <see cref="OrderItem"/>s.
        /// 7. Elimina los ítems del carrito una vez que la orden ha sido creada.
        /// 8. Persiste todos los cambios utilizando la unidad de trabajo.
        /// 9. Envía una notificación de "Orden Recibida" al cliente de usuario.
        /// 10. ¡Actualiza el capital del UserAdmin!
        /// </remarks>
        public async Task<Order> Handle(CreateOrderCommand command)
        {

            var userClient = await _userClientRepository.GetByIdAsync(command.UserClientId);
            if (userClient == null)
            {
                throw new ArgumentException($"UserClient with ID {command.UserClientId} not found.", nameof(command.UserClientId));
            }


            var cartItems = (await _cartItemRepository.GetCartItemsByUserIdAsync(command.UserClientId)).ToList();

            if (!cartItems.Any())
            {
                throw new ArgumentException($"Cart for UserClient with ID {command.UserClientId} is empty. Cannot create an order.", nameof(command.UserClientId));
            }


            Shipping? shippingDetails = null;
            if (command.IsDelivery)
            {
                if (command.ShippingDetails == null)
                {
                    throw new ArgumentException("Shipping details are required for delivery orders.", nameof(command.ShippingDetails));
                }

                shippingDetails = new Shipping(
                    command.ShippingDetails.Address,
                    command.ShippingDetails.City,
                    command.ShippingDetails.District,
                    command.ShippingDetails.Reference
                );
            }
            else
            {
                if (command.ShippingDetails != null)
                {
                    throw new ArgumentException("Shipping details should not be provided for non-delivery orders.", nameof(command.IsDelivery));
                }
            }

            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cartItems)
            {
                var book = await _bookRepository.GetByIdAsync(cartItem.BookId);
                if (book == null || !book.IsActive)
                {
                    throw new InvalidOperationException($"The book '{book?.Title ?? "Unknown"}' is no longer available for sale.");
                }

                if (book.Stock < cartItem.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for book '{book.Title}'. Available: {book.Stock}, Requested: {cartItem.Quantity}.");
                }

                var orderItem = new OrderItem(
                    book.Id,
                    book.Title,
                    book.Author,
                    book.SalePrice,
                    book.Cover,
                    cartItem.Quantity
                );
                orderItems.Add(orderItem);

                book.DecreaseStock(cartItem.Quantity);
                await _bookRepository.UpdateAsync(book);
            }

            var order = new Order(
                command.UserClientId,
                command.UserEmail,
                command.UserPhone,
                command.UserFullName,
                command.RecipientName,
                command.IsDelivery,
                shippingDetails,
                orderItems,
                command.Status,
                command.PaymentMethod
            );

            if (string.Equals(order.PaymentMethod, "wallet", StringComparison.OrdinalIgnoreCase))
            {
                if (!userClient.HasSufficientWalletBalance(order.Total))
                {
                    throw new InvalidOperationException(
                        $"Insufficient wallet balance. Required: {order.Total:F2}, Available: {userClient.Wallet:F2}.");
                }

                userClient.DebitWallet(order.Total);
                await _userClientRepository.UpdateAsync(userClient);

                var purchaseTransaction = WalletTransaction.CreatePurchase(
                    command.UserClientId,
                    order.Total,
                    order.Code);
                await _walletTransactionRepository.AddAsync(purchaseTransaction);
            }

            await _orderRepository.AddAsync(order);

            foreach (var cartItem in cartItems)
            {
                await _cartItemRepository.DeleteAsync(cartItem);
            }

            await _unitOfWork.CompleteAsync();

          
            int userAdminToUpdateId = 1; 
            await _userAdminCommandService.UpdateUserAdminCapitalAsync(userAdminToUpdateId, order.Total);


            await _notificationCommandService.Handle(new CreateNotificationCommand(
                command.UserClientId,
                ENotificationType.Order,
                DateTime.UtcNow
            ));

            return order;
        }

        /// <summary>
        /// Maneja el comando <see cref="UpdateOrderStatusCommand"/> para actualizar el estado de una orden existente.
        /// </summary>
        /// <param name="command">El comando que contiene el ID de la orden y el nuevo estado.</param>
        /// <returns>El objeto <see cref="Order"/> actualizado, o <c>null</c> si la orden no se encuentra.</returns>
        /// <exception cref="ArgumentException">Se lanza si el nuevo estado no es 'pending', 'in progress' o 'delivered'.</exception>
        public async Task<Order?> Handle(UpdateOrderStatusCommand command)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId);
            if (order == null)
            {
                return null;
            }

            order.UpdateStatus(command.Status);

            await _unitOfWork.CompleteAsync();
            return order;
        }
    }
}