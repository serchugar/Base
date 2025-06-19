using Microsoft.AspNetCore.Mvc;

namespace Serchugar.Base.Backend;

public abstract class BaseController : ControllerBase
{
    protected ActionResult<T> SetResponse<T>(Response<T> response) => response.Code switch
    {
        // Success codes
        ResponseCodes.Success => Ok(response.Data),
        ResponseCodes.Created => Created("", response.Data),
        ResponseCodes.Updated => Ok(response.Data),
        ResponseCodes.Deleted => NoContent(),
        ResponseCodes.Empty => NoContent(),
        
        // Error codes
        ResponseCodes.NotFound => NotFound(response.ErrorMessage),
        ResponseCodes.Unauthorized => Unauthorized(response.ErrorMessage),
        ResponseCodes.BadRequest => BadRequest(response.ErrorMessage),
        ResponseCodes.Conflict => Conflict(response.ErrorMessage),
        _ => Problem(response.ErrorMessage) // ResponseCodes.Error
    };
}