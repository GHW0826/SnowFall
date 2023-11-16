
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace SnowFall.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string commandName = request.ToString() ?? "";

        try
        {
            TResponse response = await next();

            _logger.LogDebug("Response {commandName} {@request} {@response}", commandName, request, response);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResponseException {exceptionType} {commandName} {@request}", TypeDescriptor.GetClassName(ex), commandName, request);
            throw;
        }
    }
}