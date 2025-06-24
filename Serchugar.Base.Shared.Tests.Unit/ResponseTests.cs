using System.Net;
using System.Net.Http.Json;
using Serchugar.Base.Shared.Tests.Unit.Entities;

namespace Serchugar.Base.Shared.Tests.Unit;

public class ResponseTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test, Category(nameof(ResponseCodes.Success))]
    public async Task Get_Returns_Success()
    {
        CountryDTO country = new() {Id = 1, Name = "Spain"};
        HttpResponseMessage httpResponse = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(country),
            RequestMessage = new(HttpMethod.Get, "foo/api/countries/1")
        };

        var response = await Response<CountryDTO>.FromHttpResponseAsync(httpResponse);

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Success));
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Id, Is.EqualTo(1));
            Assert.That(response.Data.Name, Is.EqualTo("Spain"));
        });
    }

    [Test, Category(nameof(ResponseCodes.Success))]
    public async Task GetMultiple_Returns_Success()
    {
        IEnumerable<CountryDTO> countries = 
        [
            new() {Id = 1, Name = "Spain"},
            new() {Id = 2, Name = "Italy"}
        ];
        HttpResponseMessage httpResponse = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(countries),
            RequestMessage = new(HttpMethod.Get, "foo/api/countries")
        };
        var response = await Response<IEnumerable<CountryDTO>>.FromHttpResponseAsync(httpResponse);

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Success));
            Assert.That(response.Data, Is.Not.Null);

            var list = response.Data!.ToList();
            Assert.That(list, Has.Count.EqualTo(2));

            // Primer elemento
            Assert.That(list[0].Id,   Is.EqualTo(1));
            Assert.That(list[0].Name, Is.EqualTo("Spain"));

            // Segundo elemento
            Assert.That(list[1].Id,   Is.EqualTo(2));
            Assert.That(list[1].Name, Is.EqualTo("Italy"));
        });
    }

    [Test, Category(nameof(ResponseCodes.Empty))]
    public async Task GetMultiple_Returns_Empty()
    {

        IEnumerable<CountryDTO> countries = [];
        HttpResponseMessage httpResponse = new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(countries),
            RequestMessage = new(HttpMethod.Get, "foo/api/countries")
        };
        var response = await Response<IEnumerable<CountryDTO>>.FromHttpResponseAsync(httpResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Empty));
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Count(), Is.Zero);
        });
    }

    [Test, Category(nameof(ResponseCodes.Created))]
    public async Task Create_Returns_Created()
    {
        CountryDTO country = new() {Id = 1, Name = "Spain"};
        HttpResponseMessage httpResponse = new(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(country),
            RequestMessage = new(HttpMethod.Post, "foo/api/countries")
        };
        var response = await Response<CountryDTO>.FromHttpResponseAsync(httpResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Created));    
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Id, Is.EqualTo(1));
            Assert.That(response.Data.Name, Is.EqualTo("Spain"));
        });
    }

    [Test, Category(nameof(ResponseCodes.Created))]
    public async Task BulkCreate_Returns_Created()
    {
        string message = "500 countries created";
        var content = new StringContent(message);
        content.Headers.ContentType!.MediaType = "text/plain";
        HttpResponseMessage httpResponse = new(HttpStatusCode.OK)
        {
            Content = content,
            RequestMessage = new(HttpMethod.Post, "foo/api/countries/import")
        };
        var response = await Response<string>.FromHttpResponseAsync(httpResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Created));    
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!, Is.TypeOf(typeof(string)));
            Assert.That(response.Data, Is.EqualTo(message));
        });
    }

    [Test, Category(nameof(ResponseCodes.Updated))]
    public async Task Update_Returns_Updated()
    {
        HttpResponseMessage httpResponse = new(HttpStatusCode.NoContent)
        {
            RequestMessage = new(HttpMethod.Put, "foo/api/countries/1")
        };
        var response = await Response<CountryDTO>.FromHttpResponseAsync(httpResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Updated));    
            Assert.That(response.Data, Is.Null);
        });
    }
    
    [Test, Category(nameof(ResponseCodes.Updated))]
    public async Task BulkUpdate_Returns_Updated()
    {
        string message = "500 countries updated";
        var content = new StringContent(message);
        content.Headers.ContentType!.MediaType = "text/plain";
        HttpResponseMessage httpResponse = new(HttpStatusCode.OK)
        {
            Content = content,
            RequestMessage = new(HttpMethod.Put, "foo/api/countries/bulk")
        };
        var response = await Response<string>.FromHttpResponseAsync(httpResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Updated));    
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!, Is.TypeOf(typeof(string)));
            Assert.That(response.Data, Is.EqualTo(message));
        });
    }
    
    [Test, Category(nameof(ResponseCodes.Deleted))]
    public async Task Delete_Returns_Deleted()
    {
        HttpResponseMessage httpResponse = new(HttpStatusCode.NoContent)
        {
            RequestMessage = new(HttpMethod.Delete, "foo/api/countries/1")
        };
        var response = await Response<CountryDTO>.FromHttpResponseAsync(httpResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Deleted));    
            Assert.That(response.Data, Is.Null);
        });
    }
    
    [Test, Category(nameof(ResponseCodes.Deleted))]
    public async Task BulkDelete_Returns_Deleted()
    {
        string message = "500 countries deleted";
        var content = new StringContent(message);
        content.Headers.ContentType!.MediaType = "text/plain";
        HttpResponseMessage httpResponse = new(HttpStatusCode.OK)
        {
            Content = content,
            RequestMessage = new(HttpMethod.Delete, "foo/api/countries/bulk")
        };
        var response = await Response<string>.FromHttpResponseAsync(httpResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Deleted));    
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!, Is.TypeOf(typeof(string)));
            Assert.That(response.Data, Is.EqualTo(message));
        });
    }

    [Test, Category(nameof(ResponseCodes.Error))]
    // For when exceptions happen
    public async Task Problem_ActionResult_Returns_Error()
    {
        string errorMessage = "Database exception";
        var content = new StringContent(errorMessage);
        content.Headers.ContentType!.MediaType = "text/plain";
        HttpResponseMessage httpResponse = new(HttpStatusCode.InternalServerError)
        {
            Content = content,
            RequestMessage = new(HttpMethod.Get, "foo/api/countries/1") // Http Verb does not matter here
        };
        var response = await Response<CountryDTO>.FromHttpResponseAsync(httpResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Error));    
            Assert.That(response.Data, Is.Null);
            Assert.That(response.ErrorMessage, Is.Not.Null);
            Assert.That(response.ErrorMessage, Is.EqualTo(errorMessage));
        });
    }
    
    [Test, Category(nameof(ResponseCodes.Forbidden))]
    // Only ActionResult (implemented as of now) that has no content body
    public async Task Forbid_ActionResult_Returns_Forbidden()
    {
        HttpResponseMessage httpResponse = new(HttpStatusCode.Forbidden)
        {
            RequestMessage = new(HttpMethod.Get, "foo/api/countries/1") // Http Verb does not matter here
        };
        var response = await Response<CountryDTO>.FromHttpResponseAsync(httpResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Forbidden));    
            Assert.That(response.Data, Is.Null);
            Assert.That(response.ErrorMessage, Is.Null);
        });
    }

    [Test, Category(nameof(ResponseCodes.Conflict))]
    public async Task Conflict_ActionResult_Returns_Conflict()
    {
        string errorMessage = "Country already exists";
        var content = new StringContent(errorMessage);
        content.Headers.ContentType!.MediaType = "text/plain";
        HttpResponseMessage httpResponse = new(HttpStatusCode.Conflict)
        {
            Content = content,
            RequestMessage = new(HttpMethod.Post, "foo/api/countries/1") // Http Verb does not matter here
        };
        var response = await Response<CountryDTO>.FromHttpResponseAsync(httpResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Conflict));    
            Assert.That(response.Data, Is.Null);
            Assert.That(response.ErrorMessage, Is.Not.Null);
            Assert.That(response.ErrorMessage, Is.EqualTo(errorMessage));
        });
        Console.WriteLine(response.ErrorMessage);
    }

    [Test, Category(nameof(ResponseCodes.Error))]
    public async Task NotSupportedHttpStatusCode_Returns_Error()
    {
        HttpResponseMessage httpResponse = new(HttpStatusCode.MisdirectedRequest) // Random http status code that is not implemented
        {
            Content = JsonContent.Create("whatever"),
            RequestMessage = new(HttpMethod.Get, "foo/api/countries/1") // Http Verb does not matter here
        };
        var response = await Response<CountryDTO>.FromHttpResponseAsync(httpResponse);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(ResponseCodes.Error));    
            Assert.That(response.Data, Is.Null);
            Assert.That(response.ErrorMessage, Is.Not.Null);
        });
        Console.WriteLine(response.ErrorMessage);
    }
}