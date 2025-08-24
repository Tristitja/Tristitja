using FluentValidation;
using JetBrains.Annotations;

namespace Tristitja.Auth.Local.Dto;

public sealed class CreateInitialUserRequest
{
    public required string Username { get; set; }
    
    public required string Password { get; set; }

    [UsedImplicitly]
    public sealed class CreateInitialUserRequestValidator : AbstractValidator<CreateInitialUserRequest>
    {
        public CreateInitialUserRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .Length(3, 16)
                .Matches("^[a-zA-Z0-9]+[a-zA-Z0-9_]*[a-zA-Z0-9]$")
                .WithMessage(
                    "Username can consist only of letters, numbers and underscores. Username cannot start or end with underscore.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .Length(8, 72); // I think 72 is sane password length limit.
        }
    }
}
