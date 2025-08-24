using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tristitja.Auth.Local.Model;

[EntityTypeConfiguration(typeof(UserConfiguration))]
public sealed class User
{
    public Guid Id { get; set; }

    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string DisplayName { get; set; }

    [Required]
    [JsonIgnore]
    public string HashedPassword { get; set; } = null!;

    public bool Disabled { get; set; }
    
    public string? Email { get; set; }

    public string Role { get; set; } = AuthenticationConstants.UserRole.Name;

    public bool IsInitial { get; set; } = false;
}

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
