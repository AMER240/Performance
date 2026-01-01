using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Performance.Domain.Entities;
using Performance.Domain.Enums;
using Performance.Application.Interfaces;

namespace Performance.Application.Services
{
    /// <summary>
    /// User service implementation
    /// Clean Architecture compliant - uses repositories instead of direct DbContext
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserEntity?> AuthenticateAsync(string username, string password)
        {
            var hash = ComputeHash(password);
            var user = await _userRepository.GetByUsernameAsync(username);
            return user != null && user.PasswordHash == hash ? user : null;
        }

        public async Task EnsureSeedUsersAsync()
        {
            var allUsers = await _userRepository.GetAllUsersAsync();
            if (allUsers.Any()) return;
            
            var manager = new UserEntity { Id = System.Guid.NewGuid().ToString(), UserName = "manager", PasswordHash = ComputeHash("manager123"), Role = UserRole.Manager };
            var emp = new UserEntity { Id = System.Guid.NewGuid().ToString(), UserName = "employee", PasswordHash = ComputeHash("employee123"), Role = UserRole.Employee };
            
            await _userRepository.AddAsync(manager);
            await _userRepository.AddAsync(emp);
        }

        public async Task<List<UserEntity>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<UserEntity?> GetAsync(string userId)
        {
            return await GetUserByIdAsync(userId);
        }

        public async Task<UserEntity?> GetUserByIdAsync(string userId)
        {
            var allUsers = await _userRepository.GetAllUsersAsync();
            return allUsers.FirstOrDefault(u => u.Id == userId);
        }

        public async Task<UserEntity> CreateAsync(string username, string password, UserRole role)
        {
            var existing = await _userRepository.GetByUsernameAsync(username);
            if (existing != null)
            {
                throw new InvalidOperationException($"Username '{username}' already exists.");
            }

            var user = new UserEntity
            {
                Id = System.Guid.NewGuid().ToString(),
                UserName = username,
                PasswordHash = ComputeHash(password),
                Role = role
            };

            return await _userRepository.AddAsync(user);
        }

        public async Task UpdateAsync(UserEntity user)
        {
            await _userRepository.UpdateAsync(user);
        }

        public async Task UpdatePasswordAsync(string userId, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found.");
            }

            user.PasswordHash = ComputeHash(newPassword);
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found.");
            }

            await _userRepository.DeleteAsync(user);
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return System.Convert.ToBase64String(bytes);
        }
    }
}
