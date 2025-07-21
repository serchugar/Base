using Microsoft.AspNetCore.Mvc;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Countries;

[ApiController]
[Route("api/[controller]")]
public class CountriesController(CountryRepository repo) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CountryDTO>>> GetAllAsync() =>
        SetResponse(await repo.GetAllAsync());
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CountryDTO>> GetByIdAsync(int id) =>
        SetResponse(await repo.GetByIdAsync(id));
    
    [HttpPost]
    public async Task<ActionResult<CountryDTO>> CreateAsync(CountryDTO country) => 
        SetResponse(await repo.CreateAsync(country));
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CountryDTO>> UpdateAsync(int id, CountryDTO country) =>
        SetResponse(await repo.UpdateAsync(id, country));
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<CountryDTO>> DeleteAsync(int id) =>
        SetResponse(await repo.DeleteAsync(id));
    
    [HttpDelete]
    public async Task<ActionResult<CountryDTO>> DeleteAsync(CountryDTO dto) =>
        SetResponse(await repo.DeleteAsync(dto));
}