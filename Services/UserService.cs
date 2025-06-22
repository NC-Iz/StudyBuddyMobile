using Microsoft.EntityFrameworkCore;
using StudyBuddyMobile.Data;
using StudyBuddyMobile.Models;
using System.Security.Cryptography;
using System.Text;

namespace StudyBuddyMobile.Services
{
    public class UserService
    {
        private readonly StudyBuddyDbContext _context;

        public UserService()
        {
            _context = new StudyBuddyDbContext();
        }

        // Register a new user (FIXED VERSION)
        public async Task<(bool Success, string Message)> RegisterAsync(string email, string password, string name, string? studyInterests = null)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (existingUser != null)
                {
                    // If user exists but is deactivated, reactivate the account
                    if (!existingUser.IsActive)
                    {
                        // Reactivate and update the account
                        existingUser.IsActive = true;
                        existingUser.Password = HashPassword(password);
                        existingUser.Name = name;
                        existingUser.StudyInterests = studyInterests;
                        existingUser.CreatedDate = DateTime.Now; // Reset creation date
                        existingUser.LastLoginDate = null; // Reset last login

                        await _context.SaveChangesAsync();

                        return (true, "Account reactivated successfully! You can now log in.");
                    }
                    else
                    {
                        // User exists and is active
                        return (false, "Email already exists with an active account");
                    }
                }

                // Create new user (email doesn't exist at all)
                var user = new User
                {
                    Email = email,
                    Password = HashPassword(password),
                    Name = name,
                    StudyInterests = studyInterests,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return (true, "Registration successful!");
            }
            catch (Exception ex)
            {
                return (false, $"Registration failed: {ex.Message}");
            }
        }

        // Login user
        public async Task<(bool Success, User? User, string Message)> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.IsActive == true);

                if (user != null && VerifyPassword(password, user.Password))
                {
                    // Update last login date
                    user.LastLoginDate = DateTime.Now;
                    await _context.SaveChangesAsync();

                    return (true, user, "Login successful!");
                }

                return (false, null, "Invalid email or password");
            }
            catch (Exception ex)
            {
                return (false, null, $"Login failed: {ex.Message}");
            }
        }

        // Get user by ID
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        // Update user profile
        public async Task<(bool Success, string Message)> UpdateUserAsync(int userId, string name, string? studyInterests)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return (false, "User not found");
                }

                user.Name = name;
                user.StudyInterests = studyInterests;

                await _context.SaveChangesAsync();
                return (true, "Profile updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Update failed: {ex.Message}");
            }
        }

        // Deactivate user account
        public async Task<(bool Success, string Message)> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return (false, "User not found");
                }

                user.IsActive = false;
                await _context.SaveChangesAsync();

                return (true, "Account deactivated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Deactivation failed: {ex.Message}");
            }
        }

        // Hash password
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Verify password
        private bool VerifyPassword(string password, string hash)
        {
            string hashOfInput = HashPassword(password);
            return string.Equals(hashOfInput, hash, StringComparison.OrdinalIgnoreCase);
        }

        // Dispose context
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}