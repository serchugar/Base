using Microsoft.EntityFrameworkCore;
using Serchugar.Base.Backend.Tests.Manual.Data;
using Serchugar.Base.Shared;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Teams;

public class TeamRepository(AppDbContext context, string singular = "Team", string plural = "Teams")
    : BaseRepository<TeamModel>(context, singular, plural)
{
    public async Task<Response<IEnumerable<TeamDTO>>> GetAllAsync() =>
        (await base.GetAllAsync()).Map();
    
    public async Task<Response<TeamDTO>> GetByIdAsync(int id) =>
        (await base.GetByIdAsync(id)).Map();

    public async Task<Response<TeamDTO>> CreateAsync(TeamDTO team)
    {
        Response<bool> result = await base.CheckIfEntityExistsAsync(t => EF.Functions.ILike(t.Name, team.Name) && t.CountryId == team.CountryId);
        if (result.Code.IsError()) return result.MapErrorResponse<TeamDTO>();
        
        if (result.Data) return Response<TeamDTO>.FromError(ResponseCodes.Conflict, $"{singular} already exists");

        return (await base.CreateAsync(team.Map())).Map();
    }

    public async Task<Response<TeamDTO>> UpdateAsync(int id, TeamDTO team)
    {
        Response<TeamModel> result = await base.GetByIdAsync(id);
        if (result.Code.IsError()) return result.MapErrorResponse<TeamDTO>();
        
        TeamModel updatedTeam = result.Data!;
        updatedTeam.Name = team.Name;
        
        return (await base.UpdateAsync(updatedTeam)).Map();
    }
    
    public async Task<Response<TeamDTO>> DeleteAsync(int id) =>
        (await base.DeleteAsync(id)).Map();
}