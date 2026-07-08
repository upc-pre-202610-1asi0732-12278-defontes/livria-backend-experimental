using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LivriaBackend.IAM.Domain.Model.Aggregates;
using LivriaBackend.IAM.Domain.Repositories;
using LivriaBackend.shared.Infrastructure.Persistence.EFC.Configuration; 

namespace LivriaBackend.IAM.Infrastructure.Persistence.Repositories
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly AppDbContext _context; 

        public IdentityRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task AddAsync(Identity identity)
        {
            await _context.Identities.AddAsync(identity);
        }

        public async Task<Identity> GetByIdAsync(int id)
        {
            return await _context.Identities
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Identities.AnyAsync(i => i.UserName == username);
        }
        public async Task<Identity> GetByUsernameAsync(string username)
        {
            return await _context.Identities
                .FirstOrDefaultAsync(i => i.UserName == username);
        }

        public async Task UpdateAsync(Identity identity)
        {
            _context.Entry(identity).State = EntityState.Modified;
            await Task.CompletedTask;
        }
        
        public async Task DeleteAsync(Identity identity)
        {
            _context.Set<Identity>().Remove(identity);
            await Task.CompletedTask;
        }

        public async Task<Identity> GetByUserIdAsync(int userId)
        {
            return await _context.Identities
                .FirstOrDefaultAsync(i => i.Id == userId);
        }
    }
}
