using Microsoft.AspNetCore.Mvc;
using Serchugar.Base.Shared;

namespace Serchugar.Base.Backend;

/// <summary>
/// Abstract base API controller that provides standard response handling logic for CRUD operations via <see cref="SetResponse{T}"/>
/// </summary>
public abstract class BaseController : ControllerBase
{
    // TODO: Add and modify XML Comments
    protected ActionResult<T> SetResponse<T>(Response<T> response, bool includeLocationHeader = true) => 
        SetResponse(response, includeLocationHeader, null);
    
    /// <summary>
    /// Constructs an <see cref="ActionResult{T}"/> based on the provided <see cref="Response{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the response payload.</typeparam>
    /// <param name="response">The response object containing the status code, data, and optional error message.</param>
    /// <returns>An <see cref="ActionResult{T}"/> corresponding to the response code.</returns>
    /// <remarks>
    /// <para>Interpretation logic grouped by HTTP outcome:</para>
    /// <list type="bullet">
    ///   <item><description><b>HTTP 200 OK</b>:<br/>
    ///     • <see cref="ResponseCodes.Success"/> for non-empty results (Get All, Get Single).<br/>
    ///     • <see cref="ResponseCodes.Empty"/> for empty collections (Get All Empty).<br/>
    ///     • Bulk Create and Bulk Update/Delete when <typeparamref name="T"/> is <c>string</c>.</description></item>
    ///   <item><description><b>HTTP 201 Created</b> via <see cref="CreateWithLocation{T}"/>:<br/>
    ///     • <see cref="ResponseCodes.Created"/> for single-entity creation with Location header.</description></item>
    ///   <item><description><b>HTTP 204 NoContent</b> via <see cref="SetUpdateOrDeleteActionResult{T}"/>:<br/>
    ///     • <see cref="ResponseCodes.Updated"/> for single-entity updates.<br/>
    ///     • <see cref="ResponseCodes.Deleted"/> for single-entity deletes.</description></item>
    ///   <item><description><b>Client and Server Errors</b>:<br/>
    ///     • <see cref="ResponseCodes.NotFound"/> → 404 Not Found.<br/>
    ///     • <see cref="ResponseCodes.Unauthorized"/> → 401 Unauthorized.<br/>
    ///     • <see cref="ResponseCodes.Forbidden"/> → 403 Forbidden.<br/>
    ///     • <see cref="ResponseCodes.BadRequest"/> → 400 Bad Request.<br/>
    ///     • <see cref="ResponseCodes.Conflict"/> → 409 Conflict.<br/>
    ///     • <see cref="ResponseCodes.Error"/> (default) → 500 Internal Server Error via <see cref="ProblemDetails"/>.</description></item>
    /// </list>
    /// </remarks>
    protected ActionResult<T> SetResponse<T>(Response<T> response, bool includeLocationHeader, Type? controllerTypeOfCreatedEntity = null) => response.Code switch
    {
        // Success codes
        ResponseCodes.Success => Ok(response.Data), //Get entity, Get all, Get list. GET
        ResponseCodes.Created => CreateWithLocation(response, includeLocationHeader, controllerTypeOfCreatedEntity), // Create. POST
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

    /// <summary>
    /// Generates the appropriate <see cref="ActionResult{T}"/> for create operations.
    /// </summary>
    /// <typeparam name="T">The type of the created entity or bulk create result.</typeparam>
    /// <param name="response">The response containing the created entity or bulk result.</param>
    /// <returns>
    /// 201 <see cref="CreatedAtActionResult"/> including Location header for single entity.<br/>
    /// 200 <see cref="OkResult"/> for bulk create. <see cref="Response{T}.Data"/> must be a string in this case.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <typeparamref name="T"/> does not implement <see cref="IPrimaryKey"/>, preventing retrieval of the entity's identifier.
    /// </exception>
    private ActionResult<T> CreateWithLocation<T>(Response<T> response, bool includeLocationHeader = true, Type? controllerType = null)
    {
        // Bulk create. POST
        if (response.Data is string)
            return Ok(response.Data);
        
        // Create entity. POST
        if (!includeLocationHeader) return Created("", response.Data);
        
        Type type = controllerType ?? GetType();
        string prefix = StartupScanner.GetControllerRoute(type);
        object id = StartupScanner.GetEntityKey(response.Data!)!;
        return Created($"{prefix}/{id}", response.Data);
    }

    /// <summary>
    /// Generates the appropriate <see cref="ActionResult{T}"/> for update or delete operations.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="response">The response containing the status code and operation result.</param>
    /// <returns>
    /// 204 <see cref="NoContentResult"/> for single entity updates or deletes.<br/>
    /// 200 <see cref="OkResult"/> for bulk update or delete. <see cref="Response{T}.Data"/> must be a string in this case.
    /// </returns>
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

/// <summary>
/// This should be used in every controller once. This is necessary to construct the CreatedAt URL when POST method
/// for when <see cref="ResponseCodes"/> is 'Created' in a <see cref="Response{T}"/>.
/// </summary>
/// <example>
/// In a Controller, in the GetById or GetByGuid method, set: <code>[HttpGet("{id}", Name = RouteNames.GetById)]</code>
/// </example>
public static class RouteNames
{
    /// <summary>
    /// This should be used in every controller once. This is necessary to construct the CreatedAt URL when POST method
    /// for when <see cref="ResponseCodes"/> is 'Created' in a <see cref="Response{T}"/>.
    /// </summary>
    /// <example>
    /// In a Controller, in the GetById or GetByGuid method, set: <code>[HttpGet("{id}", Name = RouteNames.GetById)]</code>
    /// </example>
    public const string GetById = nameof(GetById);
}