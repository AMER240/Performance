using System.Collections.Generic;
using System.Threading.Tasks;
using Performance.Domain.Entities;
using Performance.Domain.Enums;

namespace Performance.Application.Interfaces
{
    /// <summary>
    /// User-specific repository interface
    /// </summary>
    public interface IUserRepository : IRepository<UserEntity>
    {
        /// <summary>
        /// Get user by username
        /// </summary>
        Task<UserEntity?> GetByUsernameAsync(string username);
        
        /// <summary>
        /// Get users by role
        /// </summary>
        Task<List<UserEntity>> GetUsersByRoleAsync(UserRole role, int maxCount = 3);
        
        /// <summary>
        /// Get all users (for dropdowns, etc.)
        /// </summary>
        Task<List<UserEntity>> GetAllUsersAsync();
    }
}
