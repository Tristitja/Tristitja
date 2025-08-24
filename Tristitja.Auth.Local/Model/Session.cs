using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tristitja.Auth.Local.Model;

[EntityTypeConfiguration(typeof(SessionConfiguration))]
public sealed class Session
{
    public Guid Id { get; set; }
    
    public User? User { get; set; }
    public Guid UserId { get; set; }
    public bool IsValid { get; set; }
    
    public string? LoggedInIpAddress { get; set; }
    public string? LastIpAddress { get; set; }
    
    public string? UserAgent { get; set; }
    
    public string? LastUserAgent { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccess { get; set; }
}

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .IsRequired();
    }    
}
