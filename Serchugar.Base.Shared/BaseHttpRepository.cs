using System.Net.Http.Json;

namespace Serchugar.Base.Shared;

public abstract class BaseHttpRepository<T>(HttpClient httpClient) where T : class
{
    protected async Task<Response<T>> GetAsync(string url)
    {
        try
        {
            HttpResponseMessage responseHttp = await httpClient.GetAsync(url);
            return await Response<T>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(ResponseCodes.Error, ex.Message);
        }
    }

    protected async Task<Response<IEnumerable<T>>> GetManyAsync(string url)
    {
        try
        {
            HttpResponseMessage responseHttp = await httpClient.GetAsync(url);
            return await Response<IEnumerable<T>>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<T>>.FromError(ResponseCodes.Error, ex.Message);
        }
    }

    protected async Task<Response<T>> PostAsync(string url, T data)
    {
        try
        {
            HttpContent content = JsonContent.Create(data);
            HttpResponseMessage responseHttp = await httpClient.PostAsync(url, content);
            return await Response<T>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(ResponseCodes.Error, ex.Message);
        }
    }
    
    protected async Task<Response<T>> PostAsync<TSource>(string url, TSource data) where TSource : class
    {
        try
        {
            HttpContent content = JsonContent.Create(data);
            HttpResponseMessage responseHttp = await httpClient.PostAsync(url, content);
            return await Response<T>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(ResponseCodes.Error, ex.Message);
        }
    }

    protected async Task<Response<T>> PutAsync(string url, T data)
    {
        try
        {
            HttpContent content = JsonContent.Create(data);
            HttpResponseMessage responseHttp = await httpClient.PutAsync(url, content);
            return await Response<T>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(ResponseCodes.Error, ex.Message);
        }
    }
    
    protected async Task<Response<T>> PutAsync<TSource>(string url, TSource data) where TSource : class
    {
        try
        {
            HttpContent content = JsonContent.Create(data);
            HttpResponseMessage responseHttp = await httpClient.PutAsync(url, content);
            return await Response<T>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(ResponseCodes.Error, ex.Message);
        }
    }

    protected async Task<Response<T>> DeleteAsync(string url)
    {
        try
        {
            HttpResponseMessage responseHttp = await httpClient.DeleteAsync(url);
            return await Response<T>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(ResponseCodes.Error, ex.Message);
        }
    }
    
    protected async Task<Response<string>> PostManyAsync(string url, IEnumerable<T> data)
    {
        try
        {
            HttpContent content = JsonContent.Create(data);
            HttpResponseMessage responseHttp = await httpClient.PostAsync(url, content);
            return await Response<string>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<string>.FromError(ResponseCodes.Error, ex.Message);
        }
    }
    
    protected async Task<Response<string>> PostManyAsync<TSource>(string url, IEnumerable<TSource> data) where TSource : class
    {
        try
        {
            HttpContent content = JsonContent.Create(data);
            HttpResponseMessage responseHttp = await httpClient.PostAsync(url, content);
            return await Response<string>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<string>.FromError(ResponseCodes.Error, ex.Message);
        }
    }

    protected async Task<Response<string>> PutManyAsync(string url, IEnumerable<T> data)
    {
        try
        {
            HttpContent content = JsonContent.Create(data);
            HttpResponseMessage responseHttp = await httpClient.PutAsync(url, content);
            return await Response<string>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<string>.FromError(ResponseCodes.Error, ex.Message);
        }
    }
    
    protected async Task<Response<string>> PutManyAsync<TSource>(string url, IEnumerable<TSource> data) where TSource : class
    {
        try
        {
            HttpContent content = JsonContent.Create(data);
            HttpResponseMessage responseHttp = await httpClient.PutAsync(url, content);
            return await Response<string>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<string>.FromError(ResponseCodes.Error, ex.Message);
        }
    }

    protected async Task<Response<string>> DeleteManyAsync(string url, IEnumerable<T> data)
    {
        try
        {
            HttpContent content = JsonContent.Create(data);
            HttpResponseMessage responseHttp = await httpClient.PostAsync(url, content);
            return await Response<string>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<string>.FromError(ResponseCodes.Error, ex.Message);
        }
    }
    
    protected async Task<Response<string>> DeleteManyAsync<TSource>(string url, IEnumerable<TSource> data) where TSource : class
    {
        try
        {
            HttpContent content = JsonContent.Create(data);
            HttpResponseMessage responseHttp = await httpClient.PostAsync(url, content);
            return await Response<string>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<string>.FromError(ResponseCodes.Error, ex.Message);
        }
    }

    protected async Task<Response<T>> GenericRequestAsync(HttpMethod method, string url, T? data = null)
    {
        try
        {
            HttpContent? content = data is null ? null : JsonContent.Create(data);
            HttpResponseMessage responseHttp = method.Method switch
            {
                "GET" => await httpClient.GetAsync(url),
                "POST" => await httpClient.PostAsync(url, content),
                "PUT" => await httpClient.PutAsync(url, content),
                "DELETE" => await httpClient.DeleteAsync(url),
                _ => throw new NotImplementedException($"Method {method.Method} not implemented")
            };
            return await Response<T>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(ResponseCodes.Error, ex.Message);
        }
    }
    
    protected async Task<Response<T>> GenericRequestAsync<TSource>(HttpMethod method, string url, TSource? data = null) where TSource : class
    {
        try
        {
            HttpContent? content = data is null ? null : JsonContent.Create(data);
            HttpResponseMessage responseHttp = method.Method switch
            {
                "GET" => await httpClient.GetAsync(url),
                "POST" => await httpClient.PostAsync(url, content),
                "PUT" => await httpClient.PutAsync(url, content),
                "DELETE" => await httpClient.DeleteAsync(url),
                _ => throw new NotImplementedException($"Method {method.Method} not implemented")
            };
            return await Response<T>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(ResponseCodes.Error, ex.Message);
        }
    }
    
    protected async Task<Response<TTarget>> GenericRequestAsync<TSource, TTarget>(HttpMethod method, string url, TSource? data = null) where TSource : class
    {
        try
        {
            HttpContent? content = data is null ? null : JsonContent.Create(data);
            HttpResponseMessage responseHttp = method.Method switch
            {
                "GET" => await httpClient.GetAsync(url),
                "POST" => await httpClient.PostAsync(url, content),
                "PUT" => await httpClient.PutAsync(url, content),
                "DELETE" => await httpClient.DeleteAsync(url),
                _ => throw new NotImplementedException($"Method {method.Method} not implemented")
            };
            return await Response<TTarget>.FromHttpResponseAsync(responseHttp);
        }
        catch (Exception ex)
        {
            return Response<TTarget>.FromError(ResponseCodes.Error, ex.Message);
        }
    }
}