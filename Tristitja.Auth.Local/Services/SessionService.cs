using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Tristitja.Auth.Local.Model;
using Tristitja.Common;

namespace Tristitja.Auth.Local.Services;

internal sealed class SessionService : ISessionService
{
    private readonly IAuthLocalDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionService(IHttpContextAccessor httpContextAccessor, IAuthLocalDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public async Task<Session?> GetSessionAsync(string sessionId)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid))
        {
            return null;
        }

        return await _dbContext.Sessions
            .Include(s => s.User)
            .Where(s => s.Id == sessionGuid)
            .FirstOrDefaultAsync();
    }
    
    public async Task<Session?> AccessSessionAsync(string sessionId)
    {
        var context = _httpContextAccessor.HttpContext!;
        var session = await GetSessionAsync(sessionId);

        if (session is null)
        {
            return null;
        }

        session.LastAccess = DateTime.UtcNow;
        session.LastIpAddress = HttpHelpers.GetRealRemoteIpAddress(context);
        session.LastUserAgent = context.Request.Headers.UserAgent;

        await _dbContext.SaveChangesAsync();
        return session;
    }

    public async Task<Session> CreateSessionAsync(User user)
    {
        var context = _httpContextAccessor.HttpContext!;
        var ip = HttpHelpers.GetRealRemoteIpAddress(context);

        var session = new Session
        {
            User = user,
            UserAgent = context.Request.Headers.UserAgent,
            LastUserAgent = context.Request.Headers.UserAgent,
            CreatedAt = DateTime.UtcNow,
            LoggedInIpAddress = ip,
            LastIpAddress = ip,
            IsValid = true,
            LastAccess = DateTime.UtcNow
        };

        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();
        return session;
    }

    public ClaimsPrincipal SessionToClaimsPrincipal(Session session)
    {
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(AuthenticationConstants.SessionClaimType, session.Id.ToString()));

        if (session.User is not null)
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, session.User.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, session.User.Username));
        }

        return new ClaimsPrincipal(identity);
    }

    public Session? GetCurrentSessionFromHttpContext()
    {
        var context = _httpContextAccessor.HttpContext!;
        var session = (Session?)context.Items[AuthenticationConstants.SessionItemKey];
        return session;
    }

    public async Task<Session?> InvalidateCurrentSessionAsync()
    {
        var context = _httpContextAccessor.HttpContext!;
        var sessionId = context.User.Claims
            .FirstOrDefault(c => c.Type == AuthenticationConstants.SessionClaimType)?.Value;

        if (sessionId is null)
        {
            return null;
        }

        var session = await GetSessionAsync(sessionId);
        if (session is null)
        {
            return null;
        }
        session.IsValid = false;
        await _dbContext.SaveChangesAsync();
        return session;
    }
    
    public Task<bool> IsSessionValidAsync([NotNullWhen(true)] Session? session)
    {
        if (session?.User is null)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(session.IsValid && !session.User.Disabled);
    }
}
