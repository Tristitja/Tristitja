using Tristitja.Auth.Local.Model;

namespace Tristitja.Auth.Local.Services;

public interface IUserService
{
    Task<User> CreateInitialUser(string username, string password);

    Task<User> CreateUser(UserInit userInit);

    Task<User?> AuthenticateUser(string username, string password);

    Task<User?> FindById(Guid id);

    Task<User?> FindByName(string username);

    Task<User?> GetInitialUser();
}
