using Microsoft.EntityFrameworkCore;
using Tristitja.Auth.Local.Model;

namespace Tristitja.Auth.Local;

public interface IAuthLocalDbContext
{
    public DbSet<User> Users { get; }
    public DbSet<Session> Sessions { get; }
    
    public Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }
    
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
