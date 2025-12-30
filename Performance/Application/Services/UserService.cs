using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Performance.Infrastructure.Entities;
using Performance.Infrastructure.Application;

namespace Performance.Application.Services
{
    public class UserService : IUserService
    {
        private readonly PerformanceDbContext _db;

        public UserService(PerformanceDbContext db)
        {
            _db = db;
        }

        public async Task<UserEntity?> AuthenticateAsync(string username, string password)
        {
            var hash = ComputeHash(password);
            return await _db.Users.FirstOrDefaultAsync(u => u.UserName == username && u.PasswordHash == hash);
        }

        public async Task EnsureSeedUsersAsync()
        {
            if (await _db.Users.AnyAsync()) return;
            var manager = new UserEntity { Id = System.Guid.NewGuid().ToString(), UserName = "manager", PasswordHash = ComputeHash("manager123"), Role = UserRole.Manager };
            var emp = new UserEntity { Id = System.Guid.NewGuid().ToString(), UserName = "employee", PasswordHash = ComputeHash("employee123"), Role = UserRole.Employee };
            _db.Users.Add(manager);
            _db.Users.Add(emp);
            await _db.SaveChangesAsync();
        }

        public async Task<List<UserEntity>> GetAllUsersAsync()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task<UserEntity?> GetAsync(string userId)
        {
            return await GetUserByIdAsync(userId);
        }

        public async Task<UserEntity?> GetUserByIdAsync(string userId)
        {
            return await _db.Users.FindAsync(userId);
        }

        public async Task<UserEntity> CreateAsync(string username, string password, UserRole role)
        {
            // Check if username already exists
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);
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

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(UserEntity user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdatePasswordAsync(string userId, string newPassword)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found.");
            }

            user.PasswordHash = ComputeHash(newPassword);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found.");
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return System.Convert.ToBase64String(bytes);
        }
    }
}