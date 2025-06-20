using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serchugar.Base.Shared;

namespace Serchugar.Base.Backend;

public abstract class BaseRepository<T> (DbContext context, string singular = "Entity", string plural = "Entities") where T : class
{
    protected virtual async Task<Response<IEnumerable<T>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            IEnumerable<T> result = await context.Set<T>().ToListAsync(ct);
            if (!result.Any()) return Response<IEnumerable<T>>.FromSuccess(ResponseCodes.Empty, result);
            
            return Response<IEnumerable<T>>.FromSuccess(ResponseCodes.Success, result);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<T>>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
    }
    
    protected virtual async Task<Response<T>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return Response<T>.FromError(ResponseCodes.BadRequest, "Id must be greater than zero");
        
        try
        {
            T? entity = await context.FindAsync<T>([id], ct);
            if (entity is null) return Response<T>.FromError(ResponseCodes.NotFound, $"{singular} not found");
            
            return Response<T>.FromSuccess(ResponseCodes.Success, entity);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
    }
    
    protected virtual async Task<Response<IEnumerable<T>>> GetByFilterAsync(Expression<Func<T, bool>> expression, CancellationToken ct = default)
    {
        try
        {
            IEnumerable<T> result = await context.Set<T>().Where(expression).ToListAsync(ct);
            if (!result.Any()) return Response<IEnumerable<T>>.FromSuccess(ResponseCodes.Empty, result);
        
            return Response<IEnumerable<T>>.FromSuccess(ResponseCodes.Success, result);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<T>>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
    }

    protected virtual async Task<Response<T>> GetFirstByFilterAsync(Expression<Func<T, bool>> expression, CancellationToken ct = default)
    {
        try
        {
            T? entity = await context.Set<T>().Where(expression).FirstOrDefaultAsync(expression, ct);
            if (entity is null) return Response<T>.FromError(ResponseCodes.NotFound, $"{singular} not found");
            
            return Response<T>.FromSuccess(ResponseCodes.Success, entity);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
    }

    protected virtual async Task<Response<T>> CreateAsync(T entity, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await context.Database.BeginTransactionAsync(ct);
            context.Set<T>().Add(entity);
            
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            
            return Response<T>.FromSuccess(ResponseCodes.Created, entity);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                context.Entry(entity).State = EntityState.Detached;
            }
            
            return Response<T>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    protected virtual async Task<Response<T>> UpdateAsync(T entity, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await context.Database.BeginTransactionAsync(ct);
            context.Set<T>().Update(entity);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<T>.FromSuccess(ResponseCodes.Updated, entity);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                context.Entry(entity).State = EntityState.Detached;
            }

            return Response<T>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    protected virtual async Task<Response<T>> DeleteAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return Response<T>.FromError(ResponseCodes.BadRequest, "Id must be greater than zero");

        T? entity = null;
        IDbContextTransaction? transaction = null;
        try
        {
            entity = await context.FindAsync<T>([id], ct);
            if (entity is null) return Response<T>.FromError(ResponseCodes.NotFound, $"{singular} not found");
            
            transaction = await context.Database.BeginTransactionAsync(ct);
            context.Set<T>().Remove(entity);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<T>.FromSuccess(ResponseCodes.Deleted, entity);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                context.Entry(entity!).State = EntityState.Detached;
            }

            return Response<T>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }
    
    protected virtual async Task<Response<T>> DeleteAsync(T entity, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await context.Database.BeginTransactionAsync(ct);
            context.Set<T>().Remove(entity);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<T>.FromSuccess(ResponseCodes.Deleted, entity);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                context.Entry(entity).State = EntityState.Detached;
            }

            return Response<T>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    protected virtual async Task<Response<T>> UpsertAsync(T entity, Expression<Func<T, bool>> expression, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            bool exists = await context.Set<T>().IgnoreAutoIncludes().AnyAsync(expression, ct);

            transaction = await context.Database.BeginTransactionAsync(ct);

            if (exists)
                context.Set<T>().Update(entity);
            else
                context.Set<T>().Add(entity);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<T>.FromSuccess(exists ? ResponseCodes.Updated : ResponseCodes.Created, entity);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                context.Entry(entity).State = EntityState.Detached;
            }

            return Response<T>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // TODO: Add a stopOnFailure param to prevent the bulk creation unless no entity has conflicts. Otherwise return a dictionary of Object:Status and create only
    // those that have no conflicts
    protected virtual async Task<Response<string>> BulkCreateAsync(IEnumerable<T> entities, int batchSize = 500, CancellationToken ct = default)
    {
        List<T> buffer = new(batchSize);
        int totalInserted = 0;
        bool any = false;

        foreach (T entity in entities)
        {
            any = true;
            buffer.Add(entity);

            if (buffer.Count < batchSize) continue;

            Response<int> batchResult = await ExecuteBatchCreateAsync(buffer, ct);
            if (batchResult.Code.IsError()) return batchResult.MapErrorResponse<string>();
            
            totalInserted += batchResult.Data;
            buffer.Clear();
        }
        
        if (!any) return Response<string>.FromError(ResponseCodes.BadRequest, $"No {plural} provided");

        if (buffer.Count > 0)
        {
            Response<int> batchResult = await ExecuteBatchCreateAsync(buffer, ct);
            if (batchResult.Code.IsError()) return batchResult.MapErrorResponse<string>();
            
            totalInserted += batchResult.Data;
        }
        
        return Response<string>.FromSuccess(ResponseCodes.Created, $"{totalInserted} {plural} created");
    }

    protected virtual async Task<Response<string>> BulkUpdateAsync(IEnumerable<T> entities, int batchSize = 500, CancellationToken ct = default)
    {
        List<T> buffer = new(batchSize);
        int totalInserted = 0;
        bool any = false;

        foreach (T entity in entities)
        {
            any = true;
            buffer.Add(entity);

            if (buffer.Count < batchSize) continue;

            Response<int> batchResult = await ExecuteBatchUpdateAsync(buffer, ct);
            if (batchResult.Code.IsError()) return batchResult.MapErrorResponse<string>();
            
            totalInserted += batchResult.Data;
            buffer.Clear();
        }
        
        if (!any) return Response<string>.FromError(ResponseCodes.BadRequest, $"No {plural} provided");

        if (buffer.Count > 0)
        {
            Response<int> batchResult = await ExecuteBatchUpdateAsync(buffer, ct);
            if (batchResult.Code.IsError()) return batchResult.MapErrorResponse<string>();
            
            totalInserted += batchResult.Data;
        }
        
        return Response<string>.FromSuccess(ResponseCodes.Updated, $"{totalInserted} {plural} updated");
    }
    
    protected virtual async Task<Response<string>> BulkDeleteAsync(IEnumerable<T> entities, int batchSize = 500, CancellationToken ct = default)
    {
        List<T> buffer = new(batchSize);
        int totalInserted = 0;
        bool any = false;

        foreach (T entity in entities)
        {
            any = true;
            buffer.Add(entity);

            if (buffer.Count < batchSize) continue;

            Response<int> batchResult = await ExecuteBatchDeleteAsync(buffer, ct);
            if (batchResult.Code.IsError()) return batchResult.MapErrorResponse<string>();
            
            totalInserted += batchResult.Data;
            buffer.Clear();
        }
        
        if (!any) return Response<string>.FromError(ResponseCodes.BadRequest, $"No {plural} provided");

        if (buffer.Count > 0)
        {
            Response<int> batchResult = await ExecuteBatchDeleteAsync(buffer, ct);
            if (batchResult.Code.IsError()) return batchResult.MapErrorResponse<string>();
            
            totalInserted += batchResult.Data;
        }
        
        return Response<string>.FromSuccess(ResponseCodes.Deleted, $"{totalInserted} {plural} deleted");
    }
    
    protected virtual async Task<Response<bool>> CheckIfEntityExistsAsync(Expression<Func<T, bool>> expression, CancellationToken ct = default)
    {
        try
        {
            bool exists = await context.Set<T>().IgnoreAutoIncludes().AnyAsync(expression, ct);
            
            return Response<bool>.FromSuccess(exists ? ResponseCodes.Success : ResponseCodes.Empty, exists);
        }
        catch (Exception ex)
        {
            return Response<bool>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
    }
    
    private async Task<Response<int>> ExecuteBatchCreateAsync(List<T> batch, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await context.Database.BeginTransactionAsync(ct);
            context.Set<T>().AddRange(batch);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<int>.FromSuccess(ResponseCodes.Created, batch.Count);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                foreach (T e in batch) context.Entry(e).State = EntityState.Detached;
            }

            return Response<int>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    private async Task<Response<int>> ExecuteBatchUpdateAsync(List<T> batch, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await context.Database.BeginTransactionAsync(ct);
            context.Set<T>().UpdateRange(batch);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<int>.FromSuccess(ResponseCodes.Updated, batch.Count);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                foreach (T e in batch) context.Entry(e).State = EntityState.Detached;
            }

            return Response<int>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }
    
    private async Task<Response<int>> ExecuteBatchDeleteAsync(List<T> batch, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await context.Database.BeginTransactionAsync(ct);
            context.Set<T>().RemoveRange(batch);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<int>.FromSuccess(ResponseCodes.Deleted, batch.Count);
            
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                foreach (T e in batch) context.Entry(e).State = EntityState.Detached;
            }

            return Response<int>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }
}

public static class BaseRepositoryExtensions
{
    // This should be a specific explicit implementation per Service to avoid using reflection
    public static bool CheckAllPropertiesNull<T>(this T obj) => typeof(T).GetProperties().All(prop => prop.GetValue(obj) is null);
}
