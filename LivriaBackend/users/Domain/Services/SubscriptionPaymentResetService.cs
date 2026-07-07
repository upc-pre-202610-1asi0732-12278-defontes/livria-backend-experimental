using LivriaBackend.shared.Domain.Repositories;
using LivriaBackend.users.Domain.Model.Repositories;
using Microsoft.Extensions.Logging;

public class SubscriptionPaymentResetService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionPaymentResetService> _logger;

    public SubscriptionPaymentResetService(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionPaymentResetService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var userClientRepository = scope.ServiceProvider.GetRequiredService<IUserClientRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var allUsers = await userClientRepository.GetAllAsync();
                foreach (var user in allUsers)
                {
                    if (user.Subscription != "communityplan")
                        continue;

                    if (!user.PlanChangeDate.HasValue)
                        continue;

                    if (DateTime.UtcNow > user.PlanChangeDate.Value.AddDays(30))
                    {
                        user.SetHasPayed(false);
                        await userClientRepository.UpdateAsync(user);
                    }
                }

                await unitOfWork.CompleteAsync();

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo ejecutar la revisión de pagos de suscripción. Reintento en 30 s.");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
