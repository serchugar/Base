namespace Serchugar.Base.Backend;

public interface IResponse
{
    ResponseCodes Code { get; }
    string? ErrorMessage { get; }
}

public class Response<T> : IResponse
{
    public ResponseCodes Code { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }

    // If private, mapperly won't work properly even if [Mapper(IncludedConstructors = MemberVisibility.All)]. As of version 4.3.0-next.2
    public Response(ResponseCodes code, T? data, string? errorMessage)
    {
        Code = code;
        Data = data;
        ErrorMessage = errorMessage;
    }

    public static Response<T> FromSuccess(ResponseCodes code, T data)
    {
        if (!code.IsSuccess()) throw new ArgumentException($"Expected {code} to be success", nameof(code));
        return new(code, data, null);   
    }

    public static Response<T> FromError(ResponseCodes code, string errorMessage)
    {
        if (!code.IsError()) throw new ArgumentException($"Expected {code} to be error", nameof(code));
        return new(code, default, errorMessage);   
    }
}

// If new ResponseCodes need to be added, must be added here, in ResponseExtension's IsSuccess and IsError methods, and in BaseController's SetResponse
/// <summary>Response Codes to propagate the state of the results in each layer of an application</summary>
/// <SuccessCodes>
/// Success <br />
/// Created <br />
/// Updated <br />
/// Deleted <br />
/// Empty   <br />
/// </SuccessCodes>
/// <br/>
/// <ErrorCodes>
/// NotFound     <br/>
/// Unauthorized <br/>
/// BadRequest   <br/>
/// Conflict     <br/>
/// Error        <br/>
/// </ErrorCodes>
public enum ResponseCodes
{
    // Success codes
    Success,
    Created,
    Updated,
    Deleted,
    Empty,
    
    // Error codes
    NotFound,
    Unauthorized,
    BadRequest,
    Conflict,
    Error
}

public static class ResponseExtensions
{
    public static bool IsSuccess(this ResponseCodes code) => code is 
        ResponseCodes.Success or 
        ResponseCodes.Created or 
        ResponseCodes.Updated or 
        ResponseCodes.Deleted or 
        ResponseCodes.Empty;

    public static bool IsError(this ResponseCodes code) => code is
        ResponseCodes.NotFound or
        ResponseCodes.Unauthorized or
        ResponseCodes.BadRequest or
        ResponseCodes.Conflict or
        ResponseCodes.Error;

    public static Response<T> MapErrorResponse<T>(this IResponse response, string? errorMessage = null) =>
        Response<T>.FromError(
            response.Code,
            errorMessage ?? response.ErrorMessage ?? string.Empty);
}
