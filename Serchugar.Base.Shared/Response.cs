using System.Collections;
using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;

namespace Serchugar.Base.Shared;

/// <summary>
/// Not intended for direct consumption by library users.
/// <br/><br/>
/// Interface used by <see cref="Response{T}"/> to enable 
/// <c>Response&lt;T&gt;.MapErrorResponse&lt;T&gt;()</c> syntax 
/// instead of <c>Response&lt;T&gt;.MapErrorResponse&lt;TSource,T&gt;()</c>.
/// </summary>
public interface IResponse
{
    /// <summary>
    /// Gets the response code.
    /// </summary>
    ResponseCodes Code { get; }

    /// <summary>
    /// Gets the error message, if any.
    /// </summary>
    string? ErrorMessage { get; }
}

/// <summary>
/// Represents the outcome of an operation, encapsulating a status <see cref="Code"/>,
/// optional <see cref="Data"/> payload, and an <see cref="ErrorMessage"/> if the operation failed.
/// <br/><br/>
/// Data should always and only have value if the operation is successful.
/// ErrorMessage should only have value if the operation fails.
/// </summary>
/// <typeparam name="T">
/// The type of the data returned when the operation succeeds.
/// </typeparam>
public class Response<T> : IResponse
{
    /// <summary>
    /// Gets the <see cref="ResponseCodes"/> that indicates whether the operation was successful
    /// or the specific type of error that occurred.
    /// </summary>
    public ResponseCodes Code { get; }
    /// <summary>
    /// Gets the result data when the operation succeeds; otherwise, null.
    /// </summary>
    public T? Data { get; }
    /// <summary>
    /// Gets the error message describing why the operation failed;
    /// only set when <see cref="Code"/> represents a failure.
    /// </summary>
    public string? ErrorMessage { get; }

    // If private, mapperly won't work properly even if [Mapper(IncludedConstructors = MemberVisibility.All)]. As of version 4.3.0-next.2
    /// <summary>
    /// Not intended for direct instantiation; please use the provided factory methods instead.
    /// <br/><br/>
    /// Public constructor for <see cref="Response{T}"/> used by code generators.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Response(ResponseCodes code, T? data, string? errorMessage)
    {
        Code = code;
        Data = data;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful <see cref="Response{T}"/> with the specified success <paramref name="responseCode"/> and <paramref name="data"/>.
    /// </summary>
    /// <param name="responseCode">A <see cref="ResponseCodes"/> value indicating a successful response.</param>
    /// <param name="data">The data payload of the successful response.</param>
    /// <returns>A <see cref="Response{T}"/> instance representing the success.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="responseCode"/> is not a success code.</exception>
    public static Response<T> FromSuccess(ResponseCodes responseCode, T data)
    {
        if (!responseCode.IsSuccess()) throw new ArgumentException($"Expected {responseCode} to be success", nameof(responseCode));
        return new(responseCode, data, null);   
    }

    /// <summary>
    /// Creates an error <see cref="Response{T}"/> with the specified error <paramref name="responseCode"/> and <paramref name="errorMessage"/>.
    /// </summary>
    /// <param name="responseCode">A <see cref="ResponseCodes"/> value indicating an error response.</param>
    /// <param name="errorMessage">The error message for the response.</param>
    /// <returns>
    /// A <see cref="Response{T}"/> instance representing the error. 
    /// For <see cref="ResponseCodes.Forbidden"/>, the <paramref name="errorMessage"/> is ignored. 
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="responseCode"/> is not an error code.</exception>
    public static Response<T> FromError(ResponseCodes responseCode, string errorMessage)
    {
        if (!responseCode.IsError()) throw new ArgumentException($"Expected {responseCode} to be error", nameof(responseCode));
        if (responseCode == ResponseCodes.Forbidden) return new(responseCode, default, null);
        return new(responseCode, default, errorMessage);   
    }

    /// <summary>
/// Creates a <see cref="Response{T}"/> from the given <see cref="HttpResponseMessage"/>,
/// mapping HTTP status codes and JSON payloads into <see cref="ResponseCodes"/> values,
/// data instances or error messages. Intended for frontend clients to consume
/// backend endpoints that also use this <c>Response&lt;T&gt;</c> pattern.
/// </summary>
/// <typeparam name="T">The expected CLR type of the response content.</typeparam>
/// <param name="httpResponse">The HTTP response message returned by the backend.</param>
/// <returns>
/// A <see cref="Response{T}"/> representing either:
/// <list type="bullet">
///   <item><description>A successful result with deserialized <typeparamref name="T"/> data and a success <see cref="ResponseCodes"/>, or</description></item>
///   <item><description>An error result with a corresponding error <see cref="ResponseCodes"/> and message.</description></item>
/// </list>
/// </returns>
/// <remarks>
/// <para>Interpretation logic:</para>
/// <list type="bullet">
///   <item><description><b>Get All</b>: HTTP 200 with collection or enumerable of <typeparamref name="T"/>; returns <see cref="ResponseCodes.Success"/> if non-empty.</description></item>
///   <item><description><b>Get All (Empty)</b>: HTTP 200 with empty collection or enumerable; returns <see cref="ResponseCodes.Empty"/>.</description></item>
///   <item><description><b>Get Single</b>: HTTP 200 with a single entity; returns <see cref="ResponseCodes.Success"/>.</description></item>
///   <item><description><b>Create</b>: HTTP 201 Created; returns <see cref="ResponseCodes.Created"/> with deserialized data.</description></item>
///   <item><description><b>Bulk Create</b>: HTTP 200 for POST when <typeparamref name="T"/> is <c>string</c>; returns <see cref="ResponseCodes.Created"/> with message body.</description></item>
///   <item><description><b>Update</b>: HTTP 204 NoContent for PUT; returns <see cref="ResponseCodes.Updated"/>.</description></item>
///   <item><description><b>Bulk Update</b>: HTTP 200 for PUT when <typeparamref name="T"/> is <c>string</c>; returns <see cref="ResponseCodes.Updated"/> with message body.</description></item>
///   <item><description><b>Delete</b>: HTTP 204 NoContent for DELETE; returns <see cref="ResponseCodes.Deleted"/>.</description></item>
///   <item><description><b>Bulk Delete</b>: HTTP 200 for DELETE when <typeparamref name="T"/> is <c>string</c>; returns <see cref="ResponseCodes.Deleted"/> with message body.</description></item>
///   <item><description><b>Error</b>: non-success status codes; attempts to deserialize a <c>string</c> error message and maps to 
///   <see cref="ResponseCodes.NotFound"/>, <see cref="ResponseCodes.Unauthorized"/>, <see cref="ResponseCodes.BadRequest"/>, 
///   <see cref="ResponseCodes.Conflict"/> or <see cref="ResponseCodes.Error"/>; falls back to <see cref="ResponseCodes.Error"/> with a generic message if needed.</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// HttpResponseMessage httpResponse = await httpClient.SendAsync(request);
///
/// 
/// Response&lt;UserDto&gt; result = await Response&lt;UserDto&gt;.FromHttpResponseAsync(httpResponse);
/// if (result.Code.IsError()) // propagate with result.MapErrorResponse() or handle error
/// 
/// // Handle success, e.g. display result.Data
/// </code>
/// </example>
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
                        try { message = await httpResponse.Content.ReadAsStringAsync(); }
                        catch (Exception) { return FromError(ResponseCodes.Error, "Content of response has wrong format or is empty json"); }
                        data = (T)(object)message;
                        return FromSuccess(successCode, data);
                    }
                    // Bulk Update
                    if (method == HttpMethod.Put && typeof(T) == typeof(string))
                    {
                        successCode = ResponseCodes.Updated;
                        string message;
                        try { message = await httpResponse.Content.ReadAsStringAsync(); }
                        catch (Exception) { return FromError(ResponseCodes.Error, "Content of response has wrong format or is empty json"); }
                        data = (T)(object)message;
                        return FromSuccess(successCode, data);
                    }
                    // Bulk Delete
                    if (method == HttpMethod.Delete && typeof(T) == typeof(string))
                    {
                        successCode = ResponseCodes.Deleted;
                        string message;
                        try { message = await httpResponse.Content.ReadAsStringAsync(); }
                        catch (Exception) { return FromError(ResponseCodes.Error, "Content of response has wrong format or is empty json"); }
                        data = (T)(object)message;
                        return FromSuccess(successCode, data);
                    }
                    
                    try { data = (await httpResponse.Content.ReadFromJsonAsync<T>())!; }
                    catch (Exception) { return FromError(ResponseCodes.Error, "Content of response has wrong format or is empty json"); }

                    // Get All
                    if (data is ICollection collection)
                    {
                        successCode = collection.Count == 0 
                            ? ResponseCodes.Empty
                            : ResponseCodes.Success;
                        return FromSuccess(successCode, data);
                    }
                    if (data is IEnumerable enumerable && typeof(T) != typeof(string))
                    {
                        IEnumerator enumerator = enumerable.GetEnumerator();
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
                    catch (Exception) { return FromError(ResponseCodes.Error, "Content of response has wrong format or is empty json"); }
                    return FromSuccess(successCode, data);
                
                case HttpStatusCode.NoContent:
                    // Delete
                    if (method == HttpMethod.Delete)
                    {
                        successCode = ResponseCodes.Deleted;
                        return FromSuccess(successCode, default!);
                    }
                    
                    // Update
                    if (method == HttpMethod.Put)
                    {
                        successCode = ResponseCodes.Updated;
                        return FromSuccess(successCode, default!);
                    }
                    break;
                
                default:
                    return FromError(ResponseCodes.Error, $"Status code not supported: ({(int)httpResponse.StatusCode}) {httpResponse.StatusCode}");
            }
        }

        // Only one that has no content body. ReadFromJsonAsync would throw exception if content body empty
        if (httpResponse.StatusCode == HttpStatusCode.Forbidden) return FromError(ResponseCodes.Forbidden, string.Empty);
        
        string errorMessage;
        try { errorMessage = await httpResponse.Content.ReadAsStringAsync(); }
        catch (Exception) { return FromError(ResponseCodes.Error, "Content of response has wrong format or is empty json"); }
        
        ResponseCodes errorCode;
        switch (httpResponse.StatusCode)
        {
            case HttpStatusCode.NotFound:
                errorCode = ResponseCodes.NotFound;
                break;
            case HttpStatusCode.Unauthorized:
                errorCode = ResponseCodes.Unauthorized;
                break;
            case HttpStatusCode.BadRequest:
                errorCode = ResponseCodes.BadRequest;
                break;
            case HttpStatusCode.InternalServerError:
                errorCode = ResponseCodes.Error;
                break;
            case HttpStatusCode.Conflict:
                errorCode = ResponseCodes.Conflict;
                break;
            default:
                errorCode = ResponseCodes.Error;
                errorMessage = $"Status code not supported: ({(int)httpResponse.StatusCode}) {httpResponse.StatusCode}";
                break;
        }
        
        return FromError(errorCode, errorMessage);
    }
}

// If new ResponseCodes need to be added, must be added here, in ResponseExtension's IsSuccess and IsError methods, and in BaseController's SetResponse
/// <summary>
/// Response codes used by <see cref="Response{T}"/> to propagate the state of results through each layer of the application.
/// </summary>
/// <remarks>
/// <para>Success codes:</para>
/// <list type="bullet">
///   <item><description>Success</description></item>
///   <item><description>Created</description></item>
///   <item><description>Updated</description></item>
///   <item><description>Deleted</description></item>
///   <item><description>Empty</description></item>
/// </list>
/// <para>Error codes:</para>
/// <list type="bullet">
///   <item><description>NotFound</description></item>
///   <item><description>Unauthorized</description></item>
///   <item><description>Forbidden</description></item>
///   <item><description>BadRequest</description></item>
///   <item><description>Conflict</description></item>
///   <item><description>Error</description></item>
/// </list>
/// </remarks>
public enum ResponseCodes
{
    // Success codes
    // --------------------------------------------------------

    /// <summary>The operation completed successfully.</summary>
    /// <remarks>Response code type: Success.</remarks>
    Success,

    /// <summary>A new resource was created.</summary>
    /// <remarks>Response code type: Success.</remarks>
    Created,

    /// <summary>An existing resource was updated.</summary>
    /// <remarks>Response code type: Success.</remarks>
    Updated,

    /// <summary>A resource was deleted.</summary>
    /// <remarks>Response code type: Success.</remarks>
    Deleted,

    /// <summary>No content to return.</summary>
    /// <remarks>Response code type: Success.</remarks>
    Empty,

    // Error codes
    // --------------------------------------------------------

    /// <summary>The requested resource was not found.</summary>
    /// <remarks>Response code type: Error.</remarks>
    NotFound,

    /// <summary>The request requires user authentication.</summary>
    /// <remarks>Response code type: Error.</remarks>
    Unauthorized,

    /// <summary>The server understood the request but refuses to authorize it.</summary>
    /// <remarks>Response code type: Error.</remarks>
    Forbidden,

    /// <summary>The server cannot process the request due to client error.</summary>
    /// <remarks>Response code type: Error.</remarks>
    BadRequest,

    /// <summary>The request could not be completed due to a conflict with the current state of the target resource.</summary>
    /// <remarks>Response code type: Error.</remarks>
    Conflict,

    /// <summary>An internal error occurred while processing the request.</summary>
    /// <remarks>Response code type: Error.</remarks>
    Error
}

/// <summary>
/// Provides extension methods for <see cref="ResponseCodes"/> and <see cref="Response{T}"/>.
/// </summary>
public static class ResponseExtensions
{
    /// <summary>
    /// Determines whether the specified response code indicates a successful result.
    /// </summary>
    /// <param name="responseCode">The <see cref="ResponseCodes"/> value to check.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="responseCode"/> is <see cref="ResponseCodes.Success"/>, 
    /// <see cref="ResponseCodes.Created"/>, <see cref="ResponseCodes.Updated"/>, 
    /// <see cref="ResponseCodes.Deleted"/> or <see cref="ResponseCodes.Empty"/>; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool IsSuccess(this ResponseCodes responseCode) => responseCode is 
        ResponseCodes.Success or 
        ResponseCodes.Created or 
        ResponseCodes.Updated or 
        ResponseCodes.Deleted or 
        ResponseCodes.Empty;

    /// <summary>
    /// Determines whether the specified response code represents an error.
    /// </summary>
    /// <param name="responseCode">The <see cref="ResponseCodes"/> value to check.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="responseCode"/> is one of the error codes:
    /// <see cref="ResponseCodes.NotFound"/>, <see cref="ResponseCodes.Unauthorized"/>, 
    /// <see cref="ResponseCodes.Forbidden"/>, <see cref="ResponseCodes.BadRequest"/>, 
    /// <see cref="ResponseCodes.Conflict"/> or <see cref="ResponseCodes.Error"/>; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool IsError(this ResponseCodes responseCode) => responseCode is
        ResponseCodes.NotFound or
        ResponseCodes.Unauthorized or
        ResponseCodes.Forbidden or
        ResponseCodes.BadRequest or
        ResponseCodes.Conflict or
        ResponseCodes.Error;

    /// <summary>
    /// Maps a source <see cref="Response{T}"/> instance into a target <see cref="Response{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The target data type of the response.</typeparam>
    /// <param name="response">The source <see cref="Response{T}"/> instance whose error to map.</param>
    /// <param name="errorMessage">
    /// Optional override for the error message; if <c>null</c>, uses 
    /// <see cref="IResponse.ErrorMessage"/> or an empty string.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> carrying over the same error code and message.
    /// </returns>
    /// <remarks>
    /// It uses <see cref="IResponse"/> so that you can write 
    /// <c>Response&lt;T&gt;.MapErrorResponse&lt;T&gt;()</c> instead of the more verbose 
    /// <c>Response&lt;T&gt;.MapErrorResponse&lt;TSource,T&gt;()</c>. It enables easy propagation 
    /// of error responses between operations with different source and target types, 
    /// while remaining compatible with code generators.
    /// </remarks>
    /// <example>
    /// <code>
    /// public async Task&lt;Response&lt;string&gt;&gt; LoginAsync(UserDTO request)
    /// {
    ///     Response&lt;UserModel&gt; result = await userRepo.GetByNameExactAsync(request.Username);
    /// 
    ///
    ///     if (result.Code == ResponseCodes.NotFound)
    ///         return Response&lt;string&gt;.FromError(
    ///             ResponseCodes.BadRequest,
    ///             "Invalid username or password");
    ///
    /// 
    ///     if (result.Code.IsError()) return result.MapErrorResponse&lt;string&gt;();
    ///
    /// 
    ///     UserModel user = result.Data!;
    ///
    /// 
    ///     // ...
    /// }
    /// </code>
    /// </example>
    public static Response<T> MapErrorResponse<T>(this IResponse response, string? errorMessage = null) =>
        Response<T>.FromError(
            response.Code,
            errorMessage ?? response.ErrorMessage ?? string.Empty);
}