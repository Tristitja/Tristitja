using Microsoft.AspNetCore.Mvc;

namespace Tristitja.Common;

public class HttpProblemException : Exception
{
    public ProblemDetails Problem { get; }

    public override string Message => Problem.Detail ?? "Unknown problem";

    public HttpProblemException(ProblemDetails problem) : base(problem.Detail)
        => Problem = problem;
    
    public HttpProblemException(ProblemDetails problem, Exception? innerException)
        : base(problem.Detail, innerException) =>
        Problem = problem;
}
