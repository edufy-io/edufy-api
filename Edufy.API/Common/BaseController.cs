using Edufy.API.Common.Extensions;
using Edufy.Shared.Common;
using Microsoft.AspNetCore.Mvc;

namespace Edufy.API.Common;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    #region Claims

    protected Guid UserId      => User.GetUserId();
    protected string UserEmail => User.GetEmail();
    protected string UserRole  => User.GetRole();
    protected string SessionId => User.GetSessionId();

    #endregion

    #region HandleResult — Result không có Value (Command)

    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return BuildErrorResponse(result);
    }

    #endregion

    #region HandleResult — Result có Value (Query)

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return BuildErrorResponse(result);
    }

    #endregion

    #region HandleCreatedResult — POST trả 201

    protected ActionResult HandleCreatedResult<T>(Result<T> result, string routeName, object routeValues)
    {
        if (result.IsSuccess)
            return CreatedAtRoute(routeName, routeValues, result.Value);

        return BuildErrorResponse(result);
    }

    #endregion

    #region Private

    private ActionResult BuildErrorResponse(Result result)
    {
        var problem = result.ToProblemDetails(HttpContext);

        return result.Error.Code switch
        {
            ErrorCode.NotFound     => NotFound(problem),
            ErrorCode.Conflict     => Conflict(problem),
            ErrorCode.Unauthorized => Unauthorized(problem),
            ErrorCode.Forbidden    => StatusCode(StatusCodes.Status403Forbidden, problem),
            ErrorCode.Validation   => BadRequest(problem),
            _                      => StatusCode(StatusCodes.Status500InternalServerError, problem)
        };
    }

    #endregion
}