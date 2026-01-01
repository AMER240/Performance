using System.Collections.Generic;
using System.Threading.Tasks;
using Performance.Domain.Entities;
using Performance.Domain.Enums;

namespace Performance.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserEntity?> AuthenticateAsync(string username, string password);
        Task EnsureSeedUsersAsync();
        Task<List<UserEntity>> GetAllUsersAsync();
        Task<UserEntity?> GetAsync(string userId);
        Task<UserEntity?> GetUserByIdAsync(string userId);
        Task<UserEntity> CreateAsync(string username, string password, UserRole role);
        Task UpdateAsync(UserEntity user);
        Task UpdatePasswordAsync(string userId, string newPassword);
        Task DeleteAsync(string userId);
    }
}
