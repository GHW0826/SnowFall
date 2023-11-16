
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SnowFall.Http;

internal class LoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public LoggingHandler(ILogger<LoggingHandler> logger) 
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) 
    {
        string requestBody = string.Empty;
        try 
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (request.Content != null)
            {
                requestBody = await request.Content!.ReadAsStringAsync();
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            string text = string.Empty;
            if (response.Content != null) 
            {
                text = await response.Content.ReadAsStringAsync();
            }

            _logger.LogDebug("[HTTP] {elapsed}ms {method} {uri} {request} {status} {response}", sw.ElapsedMilliseconds, request.Method, request.RequestUri, requestBody, (int)response.StatusCode, text);
            return response;
        }
        catch (Exception exception) {
            _logger.LogError(exception, "[HTTP] {method} {uri} {request}", request.Method, request.RequestUri, requestBody);
            throw;
        }
    }
}
