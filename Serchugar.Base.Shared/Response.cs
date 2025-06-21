using System.Collections;
using System.Net;
using System.Net.Http.Json;

namespace Serchugar.Base.Shared;

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
        if (code == ResponseCodes.Forbidden) return new(code, default, null);
        return new(code, default, errorMessage);   
    }

    public static async Task<Response<T>> FromHttpResponseAsync(HttpResponseMessage httpResponse)
    {
        HttpMethod method = httpResponse.RequestMessage?.Method ?? HttpMethod.Get;

        if (httpResponse.IsSuccessStatusCode)
        {
            ResponseCodes successCode;
            T data;
            switch (httpResponse.StatusCode)
            {
                case HttpStatusCode.OK:
                    // Bulk Create
                    if (method == HttpMethod.Post && typeof(T) == typeof(string))
                    {
                        successCode = ResponseCodes.Created;
                        string message;
                        try { message = (await httpResponse.Content.ReadFromJsonAsync<string>())!; }
                        catch (Exception) { throw new InvalidOperationException("Content of response has wrong format or is empty json"); }
                        data = (T)(object)message;
                        return FromSuccess(successCode, data);
                    }
                    // Bulk Update
                    if (method == HttpMethod.Put && typeof(T) == typeof(string))
                    {
                        successCode = ResponseCodes.Updated;
                        string message;
                        try { message = (await httpResponse.Content.ReadFromJsonAsync<string>())!; }
                        catch (Exception) { throw new InvalidOperationException("Content of response has wrong format or is empty json"); }
                        data = (T)(object)message;
                        return FromSuccess(successCode, data);
                    }
                    // Bulk Delete
                    if (method == HttpMethod.Delete && typeof(T) == typeof(string))
                    {
                        successCode = ResponseCodes.Deleted;
                        string message;
                        try { message = (await httpResponse.Content.ReadFromJsonAsync<string>())!; }
                        catch (Exception) { throw new InvalidOperationException("Content of response has wrong format or is empty json"); }
                        data = (T)(object)message;
                        return FromSuccess(successCode, data);
                    }
                    
                    try { data = (await httpResponse.Content.ReadFromJsonAsync<T>())!; }
                    catch (Exception) { throw new InvalidOperationException("Content of response has wrong format or is empty json"); }

                    // Get All
                    if (data is ICollection collection)
                    {
                        successCode = collection.Count == 0 
                            ? ResponseCodes.Empty
                            : ResponseCodes.Success;
                        return FromSuccess(successCode, data);
                    }
                    if (data is IEnumerable && typeof(T) != typeof(string))
                    {
                        IEnumerator enumerator = ((IEnumerable)data).GetEnumerator();
                        using (enumerator as IDisposable)
                            successCode = enumerator.MoveNext()
                                ? ResponseCodes.Success
                                : ResponseCodes.Empty;
                        return FromSuccess(successCode, data);
                    }
                    
                    // Get Entity
                    successCode = ResponseCodes.Success;
                    return FromSuccess(successCode, data);
                
                // Create
                case HttpStatusCode.Created:
                    successCode = ResponseCodes.Created;
                    try { data = (await httpResponse.Content.ReadFromJsonAsync<T>())!; }
                    catch (Exception) { throw new InvalidOperationException("Content of response has wrong format or is empty json"); }
                    return FromSuccess(successCode, data);
                
                case HttpStatusCode.NoContent:
                    // Delete
                    if (method == HttpMethod.Delete)
                    {
                        successCode = ResponseCodes.Deleted;
                        return FromSuccess(successCode, default!);
                    }
                    
                    // Update
                    successCode = ResponseCodes.Updated;
                    return FromSuccess(successCode, default!);
                
                default:
                    throw new InvalidOperationException($"Status code not supported: ({(int)httpResponse.StatusCode}) {httpResponse.StatusCode}");
            }
        }

        // Only one that has no content body. ReadFromJsonAsync would throw exception if content body empty
        if (httpResponse.StatusCode == HttpStatusCode.Forbidden) return FromError(ResponseCodes.Forbidden, string.Empty);
        
        // TODO: Think for better solutions for this. An invalid content format 
        string errorMessage;
        try { errorMessage = (await httpResponse.Content.ReadFromJsonAsync<string>())!; }
        catch (Exception) { throw new InvalidOperationException("Content of response has wrong format or is empty json"); }
        
        ResponseCodes errorCode = httpResponse.StatusCode switch
        {
            HttpStatusCode.NotFound => ResponseCodes.NotFound,
            HttpStatusCode.Unauthorized => ResponseCodes.Unauthorized,
            HttpStatusCode.BadRequest => ResponseCodes.BadRequest,
            HttpStatusCode.Conflict => ResponseCodes.Conflict,
            HttpStatusCode.InternalServerError => ResponseCodes.Error,
            _ => throw new InvalidOperationException($"Status code not supported: ({(int)httpResponse.StatusCode}) {httpResponse.StatusCode}")
        };
        return FromError(errorCode, errorMessage);
    }
}

// If new ResponseCodes need to be added, must be added here, in ResponseExtension's IsSuccess and IsError methods, and in BaseController's SetResponse
/// <summary>Response Codes to propagate the state of the results in each layer of an application</summary>
/// <SuccessCodes>
/// Success <br /> Created <br /> Updated <br /> Deleted <br /> Empty   <br /> </SuccessCodes> <br/>
/// <ErrorCodes>
/// NotFound <br/> Unauthorized <br/> Forbidden <br/> BadRequest <br/> Conflict <br/> Error <br/>
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
    Forbidden,
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
        ResponseCodes.Forbidden or
        ResponseCodes.BadRequest or
        ResponseCodes.Conflict or
        ResponseCodes.Error;

    public static Response<T> MapErrorResponse<T>(this IResponse response, string? errorMessage = null) =>
        Response<T>.FromError(
            response.Code,
            errorMessage ?? response.ErrorMessage ?? string.Empty);
}
