using Microsoft.AspNetCore.Mvc;
using Serchugar.Base.Shared;

namespace Serchugar.Base.Backend;

public abstract class BaseController : ControllerBase
{
    protected ActionResult<T> SetResponse<T>(Response<T> response) => response.Code switch
    {
        // Success codes
        ResponseCodes.Success => Ok(response.Data), //Get entity, Get all, Get list. GET
        ResponseCodes.Created => CreateWithLocation(response), // Create. POST
        ResponseCodes.Updated => SetUpdateOrDeleteActionResult(response), // Update. PUT
        ResponseCodes.Deleted => SetUpdateOrDeleteActionResult(response), // Delete. DELETE
        ResponseCodes.Empty => Ok(response.Data), //Get all, Get list. GET
        
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
        // Create entity. POST
        if (response.Data is IPrimarykey singleEntity)
            return CreatedAtAction(
                actionName: RouteNames.GetById,
                controllerName: null,
                routeValues: new { id = singleEntity.Id },
                value: response.Data
            );
        
        // Bulk create. POST
        if (response.Data is string)
            return Ok(response.Data);
        
        // This uses reflection but in theory it should never happen if things done right, as all classes should inherit
        // from IPrimaryKey to be able to get their id for the CreateWithLocation
        throw new InvalidOperationException($"Make sure class {typeof(T).Name} inherits from {nameof(IPrimarykey)} and set the property 'public {nameof(IPrimarykey)}.{nameof(IPrimarykey.Id)} => <class id property>'");
    }

    private ActionResult<T> SetUpdateOrDeleteActionResult<T>(Response<T> response)
    {
        // Bulk Update. PUT
        // Bulk Delete. DELETE
        if (response.Data is string)
            return Ok(response.Data);
        
        // Update Entity. PUT
        // Delete Entity. DELETE
        return NoContent();
    }
}

public static class RouteNames
{
    public const string GetById = nameof(GetById);
}