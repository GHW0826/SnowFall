
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace SnowFall.Logging;

public class LoggingPublisher : INotificationPublisher
{
    private readonly ILogger<LoggingPublisher> _logger;

    public LoggingPublisher(ILogger<LoggingPublisher> logger)
    {
        _logger = logger;
    }

    public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
    {
        foreach (var handler in handlerExecutors)
        {
            try
            {
                await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Notification {handlerName} {@notification}", TypeDescriptor.GetClassName(handler.HandlerInstance), notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NotificationException {exceptionType} {handlerName} {@notification}", TypeDescriptor.GetClassName(ex), TypeDescriptor.GetClassName(handler.HandlerInstance), notification);
            }
        }
    }
}
