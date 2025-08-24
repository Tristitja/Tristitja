using FluentValidation;

namespace Tristitja.Auth.Local.Dto;
public class LoginRequest
{
    public required string Username { get; set; }
    
    public required string Password { get; set; }

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
