
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SnowFall.Model;

namespace SnowFall.AspNetCore;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    private ISender _mediator = null!;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();



    [NonAction]
    public ResponseModel ResponseData(int code, string msg, object model)
    {
        return new ResponseModel
        {
            code = code,
            msg = msg,
            data = model
        };
    }

    [NonAction]
    public OkObjectResult success(Object response)
    {
        return Ok(ResponseData(0, "success", response));
    }

    /// <summary>
    /// Creates a <see cref="CreatedResult"/> object that produces a <see cref="StatusCodes.Status201Created"/> response.
    /// </summary>
    /// <param name="uri">The URI at which the content has been created.</param>
    /// <param name="value">The content value to format in the entity body.</param>
    /// <returns>The created <see cref="CreatedResult"/> for the response.</returns>
    [NonAction]
    public CreatedResult created(string uri, [ActionResultObjectValue] object value)
    {
        return Created(uri, ResponseData(0, "created", value));
    }

    /// <summary>
    /// default response
    /// {
    ///     "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
    ///     "title": "Unauthorized",
    ///     "status": 401,
    ///     "traceId": "00-ed644f3ca9b11f253d0123d77b85731d-0d722d75257e6fe6-00"
    /// }
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public UnauthorizedResult DefaultUnauthorized()
    {
        return Unauthorized();
    }

    [NonAction]
    public UnauthorizedObjectResult Unauthorized(int code, string msg, Object response)
    {
        return Unauthorized(ResponseData(code, msg, response));
    }

    /// <summary>
    /// UnauthorizedObjectResult is empty body
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    [NonAction]
    public new UnauthorizedObjectResult Unauthorized(Object response)
    {
        return Unauthorized(401, "Unauthorized Request", response);
    }


    /*
    /// <summary>
    /// Creates an <see cref="NotFoundResult"/> that produces a <see cref="StatusCodes.Status404NotFound"/> response.
    /// </summary>
    /// <returns>The created <see cref="NotFoundResult"/> for the response.</returns>
    [NonAction]
    public virtual NotFoundResult NotFound()
        => new NotFoundResult();

    /// <summary>
    /// Creates an <see cref="NotFoundObjectResult"/> that produces a <see cref="StatusCodes.Status404NotFound"/> response.
    /// </summary>
    /// <returns>The created <see cref="NotFoundObjectResult"/> for the response.</returns>
    [NonAction]
    public virtual NotFoundObjectResult NotFound([ActionResultObjectValue] object? value)
        => new NotFoundObjectResult(value);

    /// <summary>
    /// Creates an <see cref="BadRequestResult"/> that produces a <see cref="StatusCodes.Status400BadRequest"/> response.
    /// </summary>
    /// <returns>The created <see cref="BadRequestResult"/> for the response.</returns>
    [NonAction]
    public virtual BadRequestResult BadRequest()
        => new BadRequestResult();

    /// <summary>
    /// Creates an <see cref="BadRequestObjectResult"/> that produces a <see cref="StatusCodes.Status400BadRequest"/> response.
    /// </summary>
    /// <param name="error">An error object to be returned to the client.</param>
    /// <returns>The created <see cref="BadRequestObjectResult"/> for the response.</returns>
    [NonAction]
    public virtual BadRequestObjectResult BadRequest([ActionResultObjectValue] object? error)
        => new BadRequestObjectResult(error);

    /// <summary>
    /// Creates an <see cref="ConflictResult"/> that produces a <see cref="StatusCodes.Status409Conflict"/> response.
    /// </summary>
    /// <returns>The created <see cref="ConflictResult"/> for the response.</returns>
    [NonAction]
    public virtual ConflictResult Conflict()
        => new ConflictResult();

    /// <summary>
    /// Creates an <see cref="ConflictObjectResult"/> that produces a <see cref="StatusCodes.Status409Conflict"/> response.
    /// </summary>
    /// <param name="error">Contains errors to be returned to the client.</param>
    /// <returns>The created <see cref="ConflictObjectResult"/> for the response.</returns>
    [NonAction]
    public virtual ConflictObjectResult Conflict([ActionResultObjectValue] object? error)
        => new ConflictObjectResult(error);

    /// <summary>
    /// Creates a <see cref="CreatedResult"/> object that produces a <see cref="StatusCodes.Status201Created"/> response.
    /// </summary>
    /// <param name="uri">The URI at which the content has been created.</param>
    /// <param name="value">The content value to format in the entity body.</param>
    /// <returns>The created <see cref="CreatedResult"/> for the response.</returns>
    [NonAction]
    public virtual CreatedResult Created(string uri, [ActionResultObjectValue] object? value)
    {
        if (uri == null)
        {
            throw new ArgumentNullException(nameof(uri));
        }

        return new CreatedResult(uri, value);
    }
    */
}
