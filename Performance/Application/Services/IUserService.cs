using System.Collections.Generic;
using System.Threading.Tasks;
using Performance.Infrastructure.Entities;

namespace Performance.Application.Services
{
    public interface IUserService
    {
        Task<UserEntity?> AuthenticateAsync(string username, string password);
        Task EnsureSeedUsersAsync();
        Task<List<UserEntity>> GetAllUsersAsync();
        Task<UserEntity?> GetAsync(string userId); // Alias for GetUserByIdAsync
        Task<UserEntity?> GetUserByIdAsync(string userId);
        Task<UserEntity> CreateAsync(string username, string password, UserRole role);
        Task UpdateAsync(UserEntity user);
        Task UpdatePasswordAsync(string userId, string newPassword);
        Task DeleteAsync(string userId);
    }
}