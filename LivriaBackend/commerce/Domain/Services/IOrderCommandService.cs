using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Commands;
using System.Threading.Tasks;

namespace LivriaBackend.commerce.Domain.Model.Services
{
    /// <summary>
    /// Define el contrato para el servicio de comandos de órdenes.
    /// Este servicio es responsable de orquestar las operaciones que modifican el estado de las órdenes de compra.
    /// </summary>
    public interface IOrderCommandService
    {
        /// <summary>
        /// Maneja el comando para crear una nueva orden de compra.
        /// </summary>
        /// <param name="command">El comando <see cref="CreateOrderCommand"/> que contiene los detalles para la nueva orden.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="Order"/> creado.
        /// </returns>
        Task<Order> Handle(CreateOrderCommand command);

        /// <summary>
        /// Maneja el comando para actualizar el estado de una orden existente.
        /// </summary>
        /// <param name="command">El comando <see cref="UpdateOrderStatusCommand"/> que contiene el ID de la orden y el nuevo estado.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="Order"/> actualizado, o <c>null</c> si la orden no se encuentra.
        /// </returns>
        Task<Order?> Handle(UpdateOrderStatusCommand command);

        /// <summary>
        /// Maneja el comando para pagar una orden con wallet.
        /// </summary>
        /// <param name="command">El comando <see cref="PayOrderWithWalletCommand"/> que contiene el ID de la orden.</param>
        /// <returns>
        /// Una tarea que representa la operación asíncrona.
        /// El resultado de la tarea es el objeto <see cref="Order"/> actualizado, o <c>null</c> si la orden no se encuentra.
        /// </returns>
        Task<Order?> Handle(PayOrderWithWalletCommand command);
    }
}