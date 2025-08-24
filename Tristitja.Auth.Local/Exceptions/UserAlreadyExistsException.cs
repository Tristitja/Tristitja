using Microsoft.AspNetCore.Mvc;
using Tristitja.Common;

namespace Tristitja.Auth.Local.Exceptions;

public class UserAlreadyExistsException : HttpProblemException
{
    public string Username { get; }
    
    public UserAlreadyExistsException(string username) : base(CreateProblemDetails(username))
    {
        Username = username;
    }

    private static ProblemDetails CreateProblemDetails(string username)
    {
        return new ProblemDetails
        {
            Title = typeof(UserAlreadyExistsException).FullName,
            Detail = $"User '{username}' already exists.",
            Extensions = new Dictionary<string, object?> { ["username"] = username },
            Status = 409
        };
    }
}
