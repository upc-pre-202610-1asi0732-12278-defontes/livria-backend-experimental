using LivriaBackend.users.Domain.Model.Aggregates;
using LivriaBackend.users.Interfaces.REST.Resources;

namespace LivriaBackend.users.Interfaces.REST.Transform
{
    public static class UserClientResourceFromEntityAssembler
    {
        public static UserClientResource ToResourceFromEntity(UserClient entity)
        {
            if (entity == null)
            {
                return null;
            }
            
            return new UserClientResource(
                entity.Id,
                entity.Display,
                entity.Username,
                entity.Email,
                entity.Icon ?? string.Empty,
                entity.Phrase,
                entity.Subscription,
                entity.PlanChangeDate,
                entity.HasPayed,
                entity.Wallet
            );
        }
    }
}