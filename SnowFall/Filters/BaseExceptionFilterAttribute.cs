
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SnowFall.Exceptions;
using ValidationException = SnowFall.Exceptions.ValidationException;

namespace SnowFall.Filters;


/// <summary>
/// Application Layer에서 발생한 예외를 처리
/// </summary>
public class BaseExceptionFilterAttribute : ExceptionFilterAttribute
{
    protected IDictionary<Type, Action<ExceptionContext>> ExceptionHandlers { get; }

    public BaseExceptionFilterAttribute()
    {
        ExceptionHandlers = new Dictionary<Type, Action<ExceptionContext>> {
            { typeof(HandlerException), HandleHandlerException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(ValidationException), HandleValidationException },
            { typeof(AuthorizationException), HandleAuthorizationException }
        };
    }


    public override void OnException(ExceptionContext context)
    {
        Type type = context.Exception.GetType();
        if (ExceptionHandlers.ContainsKey(context.Exception.GetType()))
            ExceptionHandlers[type].Invoke(context);
        else
            HandleException(context);

        base.OnException(context);
    }

    protected virtual void HandleException(ExceptionContext context)
    {
        Exception exception = context.Exception;

        string code = "9999";
        string message = "The request failed due to an unknown error.";
        dynamic details = exception.Data == null || exception.Data.Count == 0
            ? new { code, message }
            : new { code, message, exception.Data };

        // https://docs.microsoft.com/ko-kr/dotnet/api/microsoft.aspnetcore.http.statuscodes
        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.ExceptionHandled = true;
    }

    protected virtual void HandleHandlerException(ExceptionContext context)
    {
        HandlerException exception = (HandlerException)context.Exception;

        string code = exception.Code;
        string message = "The request has failed.";
        dynamic details = exception.Data == null || exception.Data.Count == 0
            ? new { code, message }
            : new { code, message, exception.Data };

        context.Result = new ConflictObjectResult(details);
        context.ExceptionHandled = true;
    }

    protected virtual void HandleValidationException(ExceptionContext context)
    {
        ValidationException exception = (ValidationException)context.Exception;

        string code = exception.Code;
        string message = "There are one or more invalid input values.";
        dynamic details = exception.Data == null || exception.Data.Count == 0
            ? new { code, message, Errors = exception.Errors.Keys }
            : new { code, message, Errors = exception.Errors.Keys, exception.Data };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    protected virtual void HandleNotFoundException(ExceptionContext context)
    {
        NotFoundException exception = (NotFoundException)context.Exception;

        string code = exception.Code;
        string message = "A resource associated with the request could not be found.";
        dynamic details = exception.Data == null || exception.Data.Count == 0
            ? new { code, message }
            : new { code, message, exception.Data };

        context.Result = new NotFoundObjectResult(details);
        context.ExceptionHandled = true;
    }

    protected virtual void HandleAuthorizationException(ExceptionContext context)
    {
        AuthorizationException exception = (AuthorizationException)context.Exception;

        string code = exception.Code;
        string message = "The authorization credentials are invalid.";
        dynamic details = exception.Data == null || exception.Data.Count == 0
            ? new { code, message }
            : new { code, message, exception.Data };

        context.Result = new UnauthorizedObjectResult(details);
        context.ExceptionHandled = true;
    }
}
