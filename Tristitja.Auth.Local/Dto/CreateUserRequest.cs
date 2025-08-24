using FluentValidation;

namespace Tristitja.Auth.Local.Dto;

public sealed class CreateUserRequest
{
    public required string Username { get; set; }
    
    public string? Email { get; set; }
    
    public required string Password { get; set; }
    
    public required string Role { get; set; }

    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator(IRoleStore roleStore)
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .Length(3, 16)
                .Matches("^[a-zA-Z0-9]+[a-zA-Z0-9_]*[a-zA-Z0-9]$")
                .WithMessage(
                    "Username can consist only of letters, numbers and underscores. Username cannot start or end with underscore.");

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Password)
                .NotEmpty()
                .Length(8, 72);

            RuleFor(x => x.Role)
                .NotEmpty()
                .Must(roleStore.ContainsKey)
                .WithMessage("Role is not registered to role store.");
        }
    }
}
