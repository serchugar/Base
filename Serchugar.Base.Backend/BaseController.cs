using Microsoft.AspNetCore.Mvc;
using Serchugar.Base.Shared;

namespace Serchugar.Base.Backend;

public abstract class BaseController : ControllerBase
{
    protected ActionResult<T> SetResponse<T>(Response<T> response) => response.Code switch
    {
        // Success codes
        ResponseCodes.Success => Ok(response.Data),
        ResponseCodes.Created => CreateWithLocation(response),
        ResponseCodes.Updated => NoContent(),
        ResponseCodes.Deleted => NoContent(),
        ResponseCodes.Empty => Ok(response.Data),
        
        // Error codes
        ResponseCodes.NotFound => NotFound(response.ErrorMessage),
        ResponseCodes.Unauthorized => Unauthorized(response.ErrorMessage),
        ResponseCodes.Forbidden => Forbid(),
        ResponseCodes.BadRequest => BadRequest(response.ErrorMessage),
        ResponseCodes.Conflict => Conflict(response.ErrorMessage),
        _ => Problem(response.ErrorMessage) // ResponseCodes.Error
    };

    private ActionResult<T> CreateWithLocation<T>(Response<T> response)
    {
        // When creating a single entity
        if (response.Data is IPrimarykey singleEntity)
            return CreatedAtAction(
                actionName: RouteNames.GetById,
                controllerName: null,
                routeValues: new { id = singleEntity.Id },
                value: response.Data
            );
        // When bulk create
        return Ok(response.Data);
    }
}

public static class RouteNames
{
    public const string GetById = nameof(GetById);
}