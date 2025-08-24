using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tristitja.Auth.Local.Model;
using Tristitja.Auth.Local.Services;

namespace Tristitja.Auth.Local;

public static class AuthenticationExtensions
{
    extension (IServiceCollection services)
    {
        public void AddTristitjaAuthLocal<TDbContext>(Action<AuthenticationLocalOptions>? configure = null)
            where TDbContext : IAuthLocalDbContext
        {
            var configuration = new AuthenticationLocalOptions();
            configure?.Invoke(configuration);
            
            services.AddOptions<PasswordHasherOptions>()
                .PostConfigure(options =>
                {
                    options.IterationCount = configuration.Pbkdf2Iterations;
                });

            var roleStore = new RoleStore();
            foreach (var role in configuration.Roles)
            {
                roleStore.Add(role.Name, role);
            }
            
            services.TryAddSingleton<IRoleStore>(roleStore);
            services.TryAddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.TryAddScoped<ISessionService, SessionService>();
            services.TryAddScoped<IUserService, UserService>();
            services.TryAddScoped<IAuthLocalDbContext>(s => s.GetRequiredService<TDbContext>());
            
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
                {
                    o.Cookie.HttpOnly = true;

                    // TODO: This event handler will need some review and most likely customization from customer side
                    // Currently I am leaving it as is but I need to change that to better suit needs of Tristitja projects.
                    o.Events.OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api"))
                        {
                            ctx.Response.StatusCode = 401;
                            return Task.CompletedTask;
                        }

                        ctx.RedirectUri = "/login";
                        ctx.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                    
                    // Moja ulubiona methoda, walidujemy pryncypałka! XD
                    // For some lost souls looking through this code: type in Google "pryncypałki"
                    o.Events.WalidujPryncypałka = async ctx =>
                    {
                        if (ctx.Pryncypałek is null)
                        {
                            ctx.OdrzućPryncypałka();
                            return;
                        }

                        var httpCtx = ctx.HttpContext;
                        var sessionService = httpCtx.RequestServices.GetRequiredService<SessionService>();

                        var sessionId = ctx.Pryncypałek!.Claims
                            .FirstOrDefault(c => c.Type == AuthenticationConstants.SessionClaimType)?.Value;

                        if (sessionId is null)
                        {
                            ctx.OdrzućPryncypałka();
                            await ctx.HttpContext.SignOutAsync();
                            return;
                        }

                        var session = await sessionService.AccessSessionAsync(sessionId);
                        if (!await sessionService.IsSessionValidAsync(session))
                        {
                            ctx.OdrzućPryncypałka();
                            await ctx.HttpContext.SignOutAsync();
                            return;
                        }

                        if (ctx.Pryncypałek!.Identity is ClaimsIdentity identity)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, session!.User!.Role));
                        }

                        httpCtx.Items[AuthenticationConstants.SessionItemKey] = session;
                    };
                });
        }
    }
}

internal static class FunnyExtensions
{
    extension(CookieAuthenticationEvents events)
    {
        public Func<CookieValidatePrincipalContext, Task> WalidujPryncypałka
        {
            get => events.OnValidatePrincipal;
            set => events.OnValidatePrincipal = value;
        }
    }

    extension(CookieValidatePrincipalContext context)
    {
        public ClaimsPrincipal? Pryncypałek
        {
            get => context.Principal;
            set => context.Principal = value;
        }

        public void OdrzućPryncypałka()
        {
            context.RejectPrincipal();
        }
    }
}
