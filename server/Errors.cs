using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace App;

public class CustomExceptionHandler : IExceptionHandler {
  private readonly ILogger<CustomExceptionHandler> logger;

  public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) {
    this.logger = logger;
  }

  public async ValueTask<bool> TryHandleAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken) {

    // TODO

    logger.LogError(
        exception, "Exception occurred: {Message}", exception.Message);

    var problemDetails = new ProblemDetails {
      Status = StatusCodes.Status500InternalServerError,
      Title = "Server error"
    };

    httpContext.Response.StatusCode = problemDetails.Status.Value;

    await httpContext.Response
        .WriteAsJsonAsync(problemDetails, cancellationToken);

    return true;
  }
}
