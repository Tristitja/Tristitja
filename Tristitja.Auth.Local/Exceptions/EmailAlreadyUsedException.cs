using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Tristitja.Common;

namespace Tristitja.Auth.Local.Exceptions;

public class EmailAlreadyUsedException : HttpProblemException
{
    public string Email { get; }

    public EmailAlreadyUsedException(string email) : base(CreateProblemDetails(email))
    {
        Email = email;
    }
    
    private static ProblemDetails CreateProblemDetails(string email)
    {
        return new ProblemDetails
        {
            Title = typeof(EmailAlreadyUsedException).FullName,
            Detail = $"Email {email} is already in use.",
            Extensions = { ["email"] = email },
            Status = 409
        };
    }
}
