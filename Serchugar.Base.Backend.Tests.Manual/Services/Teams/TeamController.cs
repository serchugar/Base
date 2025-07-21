using Microsoft.AspNetCore.Mvc;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Teams;

[ApiController]
[Route("api/[controller]")]
public class TeamsController(TeamRepository repo) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDTO>>> GetAllAsync() =>
        SetResponse(await repo.GetAllAsync());
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeamDTO>> GetByIdAsync(int id) =>
        SetResponse(await repo.GetByIdAsync(id));
    
    [HttpPost]
    public async Task<ActionResult<TeamDTO>> CreateAsync(TeamDTO country) => 
        SetResponse(await repo.CreateAsync(country));
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TeamDTO>> UpdateAsync(int id, TeamDTO country) =>
        SetResponse(await repo.UpdateAsync(id, country));
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<TeamDTO>> DeleteAsync(int id) =>
        SetResponse(await repo.DeleteAsync(id));
}