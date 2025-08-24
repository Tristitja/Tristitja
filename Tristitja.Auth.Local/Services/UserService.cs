using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tristitja.Auth.Local.Exceptions;
using Tristitja.Auth.Local.Model;

namespace Tristitja.Auth.Local.Services;

internal sealed class UserService : IUserService
{
    private readonly IAuthLocalDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IAuthLocalDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<User> CreateInitialUser(string username, string password)
    {
        _logger.LogInformation("Creating initial user {Username}", username);
        
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        
        var existing = await _dbContext.Users.FirstOrDefaultAsync();
        if (existing is not null)
        {
            _logger.LogWarning("Initial user already exists, aborting...");
            throw new InitialUserAlreadyCreatedException();
        }

        var user = new User()
        {
            Username = username.ToLowerInvariant(),
            DisplayName = username,
            IsInitial = true,
            Role = AuthenticationConstants.AdminRole.Name
        };

        user.HashedPassword = _passwordHasher.HashPassword(user, password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        
        _logger.LogInformation("Initial admin user {Username} has been created successfully! (ID: {Id})", user.Username, user.Id);
        
        return user;
    }

    public async Task<User> CreateUser(UserInit userInit)
    {
        var username = userInit.Username.ToLowerInvariant();
        var email = userInit.Email?.ToLowerInvariant();
        
        // We don't need transaction here as username has unique constraint
        var existingByUsername = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (existingByUsername is not null)
        {
            throw new UserAlreadyExistsException(username);
        }

        if (email is not null)
        {
            var existingByEmail = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingByEmail is not null)
            { 
                throw new EmailAlreadyUsedException(email);
            }
        }

        var user = new User()
        {
            Username = username,
            DisplayName = userInit.Username,
            Email = email,
            Role = userInit.Role,
        };
        
        user.HashedPassword = _passwordHasher.HashPassword(user, userInit.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created user {Username} (ID: {Id}, Role: {Role})", user.Username, user.Id, user.Role);
        
        return user;
    }

    /// <summary>
    /// Method is very simple, if authentication succeed it returns the User, if not it returns the null
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns>User if authentication succeed, null otherwise</returns>
    public async Task<User?> AuthenticateUser(string username, string password)
    {
        username = username.ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user is null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, password);

        switch (result)
        {
            case PasswordVerificationResult.Failed:
                return null;
            case PasswordVerificationResult.Success:
                return user;
            case PasswordVerificationResult.SuccessRehashNeeded:
                _logger.LogWarning("Password rehash is needed for user {Username} (ID: {Id})", user.Username, user.Id);
                return user;
            
            default:
                throw new UnreachableException();
        }
    }

    public async Task<User?> FindById(Guid id)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> FindByName(string username)
    {
        username = username.ToLowerInvariant();
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetInitialUser()
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.IsInitial);
    }
}
