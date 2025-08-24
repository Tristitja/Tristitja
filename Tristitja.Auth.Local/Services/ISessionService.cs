using System.Security.Claims;
using Tristitja.Auth.Local.Model;

namespace Tristitja.Auth.Local.Services;

public interface ISessionService
{
    Task<Session?> GetSessionAsync(string sessionId);
    Task<Session?> AccessSessionAsync(string sessionId);
    Task<Session> CreateSessionAsync(User user);
    ClaimsPrincipal SessionToClaimsPrincipal(Session session);
    Session? GetCurrentSessionFromHttpContext();
    Task<Session?> InvalidateCurrentSessionAsync();
    Task<bool> IsSessionValidAsync(Session? session);
}
