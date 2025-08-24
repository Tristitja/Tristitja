using Microsoft.AspNetCore.Mvc;
using Tristitja.Common;

namespace Tristitja.Auth.Local.Exceptions;

public class InitialUserAlreadyCreatedException : HttpProblemException
{
    public InitialUserAlreadyCreatedException() : base(CreateProblemDetails()) {}
    
    private static ProblemDetails CreateProblemDetails()
    {
        return new ProblemDetails
        {
            Title = typeof(InitialUserAlreadyCreatedException).FullName,
            Detail = $"Initial user was already created.",
            Status = 409
        };
    }
}
