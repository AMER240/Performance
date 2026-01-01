using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Performance.Application.Interfaces;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.Infrastructure.Data;

namespace Performance.Infrastructure.Repositories
{
    /// <summary>
    /// User repository implementation
    /// </summary>
    public class UserRepository : Repository<UserEntity>, IUserRepository
    {
        public UserRepository(PerformanceDbContext context) : base(context)
        {
        }

        public async Task<UserEntity?> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<List<UserEntity>> GetUsersByRoleAsync(UserRole role, int maxCount = 3)
        {
            return await _dbSet
                .Where(u => u.Role == role)
                .Take(maxCount)
                .ToListAsync();
        }

        public async Task<List<UserEntity>> GetAllUsersAsync()
        {
            return await _dbSet
                .OrderBy(u => u.UserName)
                .ToListAsync();
        }
    }
}
