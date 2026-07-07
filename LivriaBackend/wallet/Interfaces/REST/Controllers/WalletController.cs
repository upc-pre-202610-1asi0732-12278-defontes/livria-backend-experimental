using AutoMapper;
using LivriaBackend.wallet.Domain.Model.Commands;
using LivriaBackend.wallet.Domain.Model.Queries;
using LivriaBackend.wallet.Interfaces.REST.Resources;
using LivriaBackend.wallet.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace LivriaBackend.wallet.Interfaces.REST.Controllers
{
    [Authorize(Roles = "UserClient,Admin")]
    [ApiController]
    [Route("api/v1/wallet")]
    [Produces(MediaTypeNames.Application.Json)]
    public class WalletController : ControllerBase
    {
        private readonly IWalletCommandService _walletCommandService;
        private readonly IWalletQueryService _walletQueryService;
        private readonly IMapper _mapper;

        public WalletController(
            IWalletCommandService walletCommandService,
            IWalletQueryService walletQueryService,
            IMapper mapper)
        {
            _walletCommandService = walletCommandService;
            _walletQueryService = walletQueryService;
            _mapper = mapper;
        }

        [HttpGet("{userClientId:int}")]
        [SwaggerOperation(Summary = "Obtener saldo de billetera.")]
        [ProducesResponseType(typeof(WalletBalanceResource), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<WalletBalanceResource>> GetBalance(int userClientId)
        {
            try
            {
                var balance = await _walletQueryService.Handle(new GetWalletBalanceQuery(userClientId));
                return Ok(new WalletBalanceResource(userClientId, balance));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{userClientId:int}/can-pay")]
        [SwaggerOperation(Summary = "Verificar si el usuario puede pagar un monto con su billetera.")]
        [ProducesResponseType(typeof(WalletCanPayResource), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<WalletCanPayResource>> CanPay(int userClientId, [FromQuery] decimal amount)
        {
            if (amount <= 0)
                return BadRequest(new { message = "Amount must be positive." });

            try
            {
                var result = await _walletQueryService.Handle(new CanPayWithWalletQuery(userClientId, amount));
                return Ok(new WalletCanPayResource(userClientId, result.Required, result.Balance, result.CanPay));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{userClientId:int}/transactions")]
        [SwaggerOperation(Summary = "Historial de movimientos de billetera.")]
        [ProducesResponseType(typeof(IEnumerable<WalletTransactionResource>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<WalletTransactionResource>>> GetTransactions(int userClientId)
        {
            try
            {
                var transactions = await _walletQueryService.Handle(new GetWalletTransactionsQuery(userClientId));
                return Ok(_mapper.Map<IEnumerable<WalletTransactionResource>>(transactions));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("recharge-requests")]
        [SwaggerOperation(Summary = "Solicitar recarga de billetera (comprobante de transferencia bancaria).")]
        [ProducesResponseType(typeof(WalletTransactionResource), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<WalletTransactionResource>> CreateRechargeRequest(
            [FromBody] CreateRechargeRequestResource resource)
        {
            var command = _mapper.Map<CreateRechargeRequestCommand>(resource);
            try
            {
                var transaction = await _walletCommandService.Handle(command);
                var result = _mapper.Map<WalletTransactionResource>(transaction);
                return CreatedAtAction(nameof(GetRechargeRequestById), new { id = transaction.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("recharge-requests")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Listar solicitudes de recarga pendientes (admin).")]
        [ProducesResponseType(typeof(IEnumerable<WalletTransactionResource>), 200)]
        public async Task<ActionResult<IEnumerable<WalletTransactionResource>>> GetPendingRechargeRequests(
            [FromQuery] string? status = "pending")
        {
            if (!string.Equals(status, "pending", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Only status=pending is supported." });

            var transactions = await _walletQueryService.Handle(new GetPendingRechargeRequestsQuery());
            return Ok(_mapper.Map<IEnumerable<WalletTransactionResource>>(transactions));
        }

        [HttpGet("recharge-requests/{id:int}")]
        [SwaggerOperation(Summary = "Obtener una solicitud de recarga por ID.")]
        [ProducesResponseType(typeof(WalletTransactionResource), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<WalletTransactionResource>> GetRechargeRequestById(int id)
        {
            var transaction = await _walletQueryService.Handle(new GetWalletTransactionByIdQuery(id));
            if (transaction == null)
                return NotFound(new { message = "Recharge request not found." });

            return Ok(_mapper.Map<WalletTransactionResource>(transaction));
        }

        [HttpPatch("recharge-requests/{id:int}/approve")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Aprobar recarga y acreditar saldo (admin).")]
        [ProducesResponseType(typeof(WalletTransactionResource), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<WalletTransactionResource>> ApproveRechargeRequest(int id)
        {
            try
            {
                var transaction = await _walletCommandService.Handle(new ApproveRechargeRequestCommand(id));
                if (transaction == null)
                    return NotFound(new { message = "Recharge request not found." });

                return Ok(_mapper.Map<WalletTransactionResource>(transaction));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{userClientId:int}/pay-subscription")]
        [SwaggerOperation(
            Summary = "Pagar suscripción community con wallet.",
            Description = "Descuesta el monto de la wallet, renueva HasPayed=true y acredita al admin.")]
        [ProducesResponseType(typeof(WalletTransactionResource), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<WalletTransactionResource>> PaySubscription(
            int userClientId,
            [FromBody] PaySubscriptionResource resource)
        {
            try
            {
                var transaction = await _walletCommandService.Handle(
                    new PaySubscriptionWithWalletCommand(userClientId, resource.Amount));
                return Ok(_mapper.Map<WalletTransactionResource>(transaction));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("recharge-requests/{id:int}/reject")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Rechazar solicitud de recarga (admin).")]
        [ProducesResponseType(typeof(WalletTransactionResource), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<WalletTransactionResource>> RejectRechargeRequest(
            int id,
            [FromBody] RejectRechargeRequestResource? resource)
        {
            try
            {
                var transaction = await _walletCommandService.Handle(
                    new RejectRechargeRequestCommand(id, resource?.AdminNote));
                if (transaction == null)
                    return NotFound(new { message = "Recharge request not found." });

                return Ok(_mapper.Map<WalletTransactionResource>(transaction));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
