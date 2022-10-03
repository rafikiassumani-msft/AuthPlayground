/*using Microsoft.AspNetCore.Http.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IdentityMinimalAPIs.Filters
{
    public class ProblemDetailsServiceEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync( context, EndpointFilterDelegate next)
         => await next(context) switch
      {
          ProblemHttpResult problemHttpResult => new ProblemDetailsServiceAwareResult(problemHttpResult.StatusCode, problemHttpResult.ProblemDetails),
          ProblemDetails problemDetails => new ProblemDetailsServiceAwareResult(null, problemDetails),
          { } result => result,
          null => null
      };
        private class ProblemDetailsServiceAwareResult : IResult
        {
            private readonly int? _statusCode;
            private readonly ProblemDetails _problemDetails;
            public ProblemDetailsServiceAwareResult(int? statusCode, ProblemDetails problemDetails)
            {
                _statusCode = statusCode ?? problemDetails.Status;
                _problemDetails = problemDetails;
            }
            public async Task ExecuteAsync(HttpContext httpContext)
            {
                if (httpContext.RequestServices.GetService<IProblemDetailsService>() is IProblemDetailsService problemDetailsService)
                {
                    if (_statusCode is { } statusCode)
                    {
                        httpContext.Response.StatusCode = statusCode;
                    }
                    await problemDetailsService.WriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = httpContext,
                        ProblemDetails = _problemDetails
                    });
                }
            }
        }
    }
}
*/