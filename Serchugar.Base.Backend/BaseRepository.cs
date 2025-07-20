using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serchugar.Base.Shared;

namespace Serchugar.Base.Backend;

/// <summary>
/// Provides base repository functionality for handling CRUD operations on entities of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the entity managed by this repository.</typeparam>
public abstract class BaseRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly string _singular;
    private readonly string _plural;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository{T}"/> class.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> used to interact with the database.</param>
    /// <param name="singular">
    /// The singular name of the entity type, used in response messages.
    /// Default is <c>"Entity"</c>.
    /// </param>
    /// <param name="plural">
    /// The plural name of the entity type, used in response messages.
    /// Default is <c>"Entities"</c>.
    /// </param>
    protected BaseRepository(DbContext context, string singular = "Entity", string plural = "Entities")
    {
        _context = context;    
        _singular = singular;
        _plural = plural;
    }
    
    /// <summary>
    /// Asynchronously retrieves all instances of <typeparamref name="T"/> from the database.
    /// </summary>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> of <see cref="IEnumerable{T}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The list of entities on success (with <see cref="ResponseCodes.Success"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An empty collection if no entities were found (<see cref="ResponseCodes.Empty"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and message if an exception occurred (<see cref="ResponseCodes.Error"/>).</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<IEnumerable<T>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            IEnumerable<T> result = await _context.Set<T>().IgnoreAutoIncludes().ToListAsync(ct);
            if (!result.Any()) return Response<IEnumerable<T>>.FromSuccess(ResponseCodes.Empty, []);
            
            return Response<IEnumerable<T>>.FromSuccess(ResponseCodes.Success, result);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<T>>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
    }
    
    /// <summary>
    /// Asynchronously retrieves an entity of type <typeparamref name="T"/> by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the entity. Must be greater than zero.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The requested entity on success (<see cref="ResponseCodes.Success"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and message if the <paramref name="id"/> is invalid (<see cref="ResponseCodes.BadRequest"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and message if the entity is not found (<see cref="ResponseCodes.NotFound"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs (<see cref="ResponseCodes.Error"/>).</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<T>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return Response<T>.FromError(ResponseCodes.BadRequest, "Id must be greater than zero");
        
        try
        {
            T? entity = await _context.FindAsync<T>([id], ct);
            if (entity is null) return Response<T>.FromError(ResponseCodes.NotFound, $"{_singular} not found");
            
            return Response<T>.FromSuccess(ResponseCodes.Success, entity);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
    }
    
    /// <summary>
    /// Asynchronously retrieves an entity of type <typeparamref name="T"/> by its GUID identifier.
    /// </summary>
    /// <param name="id">
    /// The GUID identifier of the entity. Must be a non-empty GUID.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The requested entity on success (<see cref="ResponseCodes.Success"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and message if <paramref name="id"/> is an empty GUID (<see cref="ResponseCodes.BadRequest"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and message if the entity is not found (<see cref="ResponseCodes.NotFound"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs (<see cref="ResponseCodes.Error"/>).</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<T>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty) return Response<T>.FromError(ResponseCodes.BadRequest, "Id must be a non-empty GUID");
        
        try
        {
            T? entity = await _context.FindAsync<T>([id], ct);
            if (entity is null) return Response<T>.FromError(ResponseCodes.NotFound, $"{_singular} not found");
            
            return Response<T>.FromSuccess(ResponseCodes.Success, entity);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
    }
    
    /// <summary>
    /// Asynchronously retrieves all entities of type <typeparamref name="T"/> that satisfy the given filter expression.
    /// </summary>
    /// <param name="expression">
    /// A LINQ expression used to filter entities of type <typeparamref name="T"/>.
    /// </param>
    /// <param name="withAutoIncludes">
    /// If <c>true</c>, includes related navigation properties configured on the context; otherwise ignores auto-includes.
    /// Defaults to <c>false</c>.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> of <see cref="IEnumerable{T}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The list of matching entities on success (<see cref="ResponseCodes.Success"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An empty collection if no matching entities were found (<see cref="ResponseCodes.Empty"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs (<see cref="ResponseCodes.Error"/>).</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<IEnumerable<T>>> GetByFilterAsync(Expression<Func<T, bool>> expression, bool withAutoIncludes = false, CancellationToken ct = default)
    {
        try
        {
            IQueryable<T> query = withAutoIncludes ? _context.Set<T>() : _context.Set<T>().IgnoreAutoIncludes();
            
            IEnumerable<T> result = await query.Where(expression).ToListAsync(ct);
            if (!result.Any()) return Response<IEnumerable<T>>.FromSuccess(ResponseCodes.Empty, []);
        
            return Response<IEnumerable<T>>.FromSuccess(ResponseCodes.Success, result);
        }
        catch (Exception ex)
        {
            return Response<IEnumerable<T>>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
    }

    /// <summary>
    /// Asynchronously retrieves the first entity of type <typeparamref name="T"/> that satisfies the given filter expression.
    /// </summary>
    /// <param name="expression">
    /// A LINQ expression used to filter entities of type <typeparamref name="T"/>.
    /// </param>
    /// <param name="withAutoIncludes">
    /// If <c>true</c>, includes related navigation properties configured on the context; otherwise ignores auto-includes.
    /// Defaults to <c>false</c>.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The first matching entity on success (<see cref="ResponseCodes.Success"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and message if no matching entity is found (<see cref="ResponseCodes.NotFound"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs (<see cref="ResponseCodes.Error"/>).</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<T>> GetFirstByFilterAsync(Expression<Func<T, bool>> expression, bool withAutoIncludes = false, CancellationToken ct = default)
    {
        try
        {
            IQueryable<T> query = withAutoIncludes ? _context.Set<T>() : _context.Set<T>().IgnoreAutoIncludes();
            
            T? entity = await query.Where(expression).FirstOrDefaultAsync(expression, ct);
            if (entity is null) return Response<T>.FromError(ResponseCodes.NotFound, $"{_singular} not found");
            
            return Response<T>.FromSuccess(ResponseCodes.Success, entity);
        }
        catch (Exception ex)
        {
            return Response<T>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
    }

    /// <summary>
    /// Asynchronously creates a new entity of type <typeparamref name="T"/> in the database within a transaction.<br/><br/>
    /// This method does not verify that the entity does not exist before creating. That check must be performed
    /// beforehand — consider using <see cref="CheckIfEntityExistsAsync"/> to ensure the entity is not present.
    /// </summary>
    /// <param name="entity">
    /// The entity to add to the database.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The created entity on success (<see cref="ResponseCodes.Created"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs during creation (<see cref="ResponseCodes.Error"/>), with the transaction rolled back and the entity detached.</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<T>> CreateAsync(T entity, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await _context.Database.BeginTransactionAsync(ct);
            _context.Set<T>().Add(entity);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<T>.FromSuccess(ResponseCodes.Created, entity);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                _context.Entry(entity).State = EntityState.Detached;
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

    /// <summary>
    /// Asynchronously updates an existing entity of type <typeparamref name="T"/> in the database within a transaction.<br/><br/>
    /// This method does not verify that the entity already exists before updating. That check must be performed
    /// beforehand — consider using <see cref="CheckIfEntityExistsAsync"/> to ensure the entity is present.
    /// </summary>
    /// <param name="entity">
    /// The entity with updated values to save to the database.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The updated entity on success (<see cref="ResponseCodes.Updated"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs during update (<see cref="ResponseCodes.Error"/>), with the transaction rolled back and the entity detached.</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<T>> UpdateAsync(T entity, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await _context.Database.BeginTransactionAsync(ct);
            _context.Set<T>().Update(entity);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<T>.FromSuccess(ResponseCodes.Updated, entity);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                _context.Entry(entity).State = EntityState.Detached;
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

    /// <summary>
    /// Asynchronously deletes an entity of type <typeparamref name="T"/> identified by its integer ID within a transaction.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the entity to delete. Must be greater than zero.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The deleted entity on success (<see cref="ResponseCodes.Deleted"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and message if <paramref name="id"/> is invalid (<see cref="ResponseCodes.BadRequest"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and message if the entity is not found (<see cref="ResponseCodes.NotFound"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs during deletion (<see cref="ResponseCodes.Error"/>), with the transaction rolled back and the entity detached.</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<T>> DeleteAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return Response<T>.FromError(ResponseCodes.BadRequest, "Id must be greater than zero");

        T? entity = null;
        IDbContextTransaction? transaction = null;
        try
        {
            entity = await _context.FindAsync<T>([id], ct);
            if (entity is null) return Response<T>.FromError(ResponseCodes.NotFound, $"{_singular} not found");
            
            transaction = await _context.Database.BeginTransactionAsync(ct);
            _context.Set<T>().Remove(entity);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<T>.FromSuccess(ResponseCodes.Deleted, entity);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                _context.Entry(entity!).State = EntityState.Detached;
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
    
    /// <summary>
    /// Asynchronously deletes an entity of type <typeparamref name="T"/> identified by its GUID within a transaction.
    /// </summary>
    /// <param name="id">
    /// The GUID identifier of the entity to delete. Must be a non-empty GUID.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The deleted entity on success (<see cref="ResponseCodes.Deleted"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and message if <paramref name="id"/> is an empty GUID (<see cref="ResponseCodes.BadRequest"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and message if the entity is not found (<see cref="ResponseCodes.NotFound"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs during deletion (<see cref="ResponseCodes.Error"/>), with the transaction rolled back and the entity detached.</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<T>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty) return Response<T>.FromError(ResponseCodes.BadRequest, "Id must be a non-empty GUID");

        T? entity = null;
        IDbContextTransaction? transaction = null;
        try
        {
            entity = await _context.FindAsync<T>([id], ct);
            if (entity is null) return Response<T>.FromError(ResponseCodes.NotFound, $"{_singular} not found");
            
            transaction = await _context.Database.BeginTransactionAsync(ct);
            _context.Set<T>().Remove(entity);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<T>.FromSuccess(ResponseCodes.Deleted, entity);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                _context.Entry(entity!).State = EntityState.Detached;
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
    
    /// <summary>
    /// Asynchronously deletes the specified entity of type <typeparamref name="T"/> within a transaction.<br/><br/>
    /// This method assumes the entity exists and does not perform an existence check.
    /// Ensure existence beforehand — consider using <see cref="CheckIfEntityExistsAsync"/> before calling this method.
    /// </summary>
    /// <param name="entity">
    /// The entity to remove from the database.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The deleted entity on success (<see cref="ResponseCodes.Deleted"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs during deletion (<see cref="ResponseCodes.Error"/>), with the transaction rolled back and the entity detached.</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<T>> DeleteAsync(T entity, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await _context.Database.BeginTransactionAsync(ct);
            _context.Set<T>().Remove(entity);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<T>.FromSuccess(ResponseCodes.Deleted, entity);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                _context.Entry(entity).State = EntityState.Detached;
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

    /// <summary>
    /// Asynchronously inserts or updates an entity of type <typeparamref name="T"/> within a transaction, based on the provided existence filter expression.<br/><br/>
    /// This method checks automatically if an entity exists using the <paramref name="expression"/>; no prior existence check is required.
    /// </summary>
    /// <param name="entity">
    /// The entity to upsert in the database.
    /// </param>
    /// <param name="expression">
    /// A LINQ expression used to determine whether the entity already exists.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{T}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The upserted entity on success, with <see cref="ResponseCodes.Created"/> if newly added, or <see cref="ResponseCodes.Updated"/> if updated.</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs during the operation (<see cref="ResponseCodes.Error"/>), with the transaction rolled back and the entity detached.</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<T>> UpsertAsync(T entity, Expression<Func<T, bool>> expression, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            bool exists = await _context.Set<T>().IgnoreAutoIncludes().AnyAsync(expression, ct);

            transaction = await _context.Database.BeginTransactionAsync(ct);

            if (exists)
                _context.Set<T>().Update(entity);
            else
                _context.Set<T>().Add(entity);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<T>.FromSuccess(exists ? ResponseCodes.Updated : ResponseCodes.Created, entity);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                _context.Entry(entity).State = EntityState.Detached;
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

    // TODO: Add a cancelOnFailure param to prevent the bulk creation unless no entity has conflicts. Otherwise return a dictionary of Object:Status and create only
    // those that have no conflicts. Same for Update and Delete.
    /// <summary>
    /// Asynchronously creates multiple entities of type <typeparamref name="T"/> in the database in batches.
    /// <br/><br/>
    /// Internally, each batch is processed via <see cref="ExecuteBatchCreateAsync"/>, which handles its own transaction.
    /// This method does not verify whether each entity already exists; perform such checks beforehand if required.
    /// </summary>
    /// <param name="entities">
    /// The collection of entities to add to the database.
    /// </param>
    /// <param name="batchSize">
    /// The maximum number of entities to insert per batch. Defaults to <c>500</c>.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> to cancel the operation. Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{String}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       An error code and message (<see cref="ResponseCodes.BadRequest"/>) if <paramref name="entities"/> is empty.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       An error code and mapped error response if any batch fails (<see cref="ExecuteBatchCreateAsync"/> returns an error).</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       A success code (<see cref="ResponseCodes.Created"/>) and a message indicating the total number of entities created if all batches succeed.
    ///     </description>
    ///   </item>
    /// </list>
    /// </returns>
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
        
        if (!any) return Response<string>.FromError(ResponseCodes.BadRequest, $"No {_plural} provided");

        if (buffer.Count > 0)
        {
            Response<int> batchResult = await ExecuteBatchCreateAsync(buffer, ct);
            if (batchResult.Code.IsError()) return batchResult.MapErrorResponse<string>();
            
            totalInserted += batchResult.Data;
        }
        
        return Response<string>.FromSuccess(ResponseCodes.Created, $"{totalInserted} {_plural} created");
    }

    /// <summary>
    /// Asynchronously updates multiple entities of type <typeparamref name="T"/> in the database in batches.<br/><br/>
    /// Internally, each batch is processed via <see cref="ExecuteBatchUpdateAsync"/>, which handles its own transaction.
    /// This method does not verify whether each entity exists; perform such checks beforehand if required.
    /// </summary>
    /// <param name="entities">
    /// The collection of entities to update in the database.
    /// </param>
    /// <param name="batchSize">
    /// The maximum number of entities to update per batch. Defaults to <c>500</c>.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> to cancel the operation. Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{String}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>An error code and message (<see cref="ResponseCodes.BadRequest"/>) if <paramref name="entities"/> is empty.</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and mapped error response if any batch fails (<see cref="ExecuteBatchUpdateAsync"/> returns an error).</description>
    ///   </item>
    ///   <item>
    ///     <description>A success code (<see cref="ResponseCodes.Updated"/>) and a message indicating the total number of entities updated if all batches succeed.</description>
    ///   </item>
    /// </list>
    /// </returns>
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
        
        if (!any) return Response<string>.FromError(ResponseCodes.BadRequest, $"No {_plural} provided");

        if (buffer.Count > 0)
        {
            Response<int> batchResult = await ExecuteBatchUpdateAsync(buffer, ct);
            if (batchResult.Code.IsError()) return batchResult.MapErrorResponse<string>();
            
            totalInserted += batchResult.Data;
        }
        
        return Response<string>.FromSuccess(ResponseCodes.Updated, $"{totalInserted} {_plural} updated");
    }
    
    /// <summary>
    /// Asynchronously deletes multiple entities of type <typeparamref name="T"/> in the database in batches.<br/><br/>
    /// Internally, each batch is processed via <see cref="ExecuteBatchDeleteAsync"/>, which handles its own transaction.
    /// This method does not verify whether each entity exists; perform such checks beforehand if required.
    /// </summary>
    /// <param name="entities">
    /// The collection of entities to remove from the database.
    /// </param>
    /// <param name="batchSize">
    /// The maximum number of entities to delete per batch. Defaults to <c>500</c>.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> to cancel the operation. Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{String}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>An error code and message (<see cref="ResponseCodes.BadRequest"/>) if <paramref name="entities"/> is empty.</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and mapped error response if any batch fails (<see cref="ExecuteBatchDeleteAsync"/> returns an error).</description>
    ///   </item>
    ///   <item>
    ///     <description>A success code (<see cref="ResponseCodes.Deleted"/>) and a message indicating the total number of entities deleted if all batches succeed.</description>
    ///   </item>
    /// </list>
    /// </returns>
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
        
        if (!any) return Response<string>.FromError(ResponseCodes.BadRequest, $"No {_plural} provided");

        if (buffer.Count > 0)
        {
            Response<int> batchResult = await ExecuteBatchDeleteAsync(buffer, ct);
            if (batchResult.Code.IsError()) return batchResult.MapErrorResponse<string>();
            
            totalInserted += batchResult.Data;
        }
        
        return Response<string>.FromSuccess(ResponseCodes.Deleted, $"{totalInserted} {_plural} deleted");
    }
    
    /// <summary>
    /// Asynchronously checks whether any entities of type <typeparamref name="T"/> satisfy the given filter expression.
    /// </summary>
    /// <param name="expression">
    /// A LINQ expression used to test for the existence of entities of type <typeparamref name="T"/>.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{Bool}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>true</c> and <see cref="ResponseCodes.Success"/> if at least one matching entity exists.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>false</c> and <see cref="ResponseCodes.Empty"/> if no matching entities are found.</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs (<see cref="ResponseCodes.Error"/>).</description>
    ///   </item>
    /// </list>
    /// </returns>
    protected virtual async Task<Response<bool>> CheckIfEntityExistsAsync(Expression<Func<T, bool>> expression, CancellationToken ct = default)
    {
        try
        {
            bool exists = await _context.Set<T>().IgnoreAutoIncludes().AnyAsync(expression, ct);
            
            return Response<bool>.FromSuccess(exists ? ResponseCodes.Success : ResponseCodes.Empty, exists);
        }
        catch (Exception ex)
        {
            return Response<bool>.FromError(
                ResponseCodes.Error,
                ex.InnerException?.Data["MessageText"]?.ToString() ?? ex.Message);
        }
    }
    
    /// <summary>
    /// Asynchronously executes the creation of a batch of entities of type <typeparamref name="T"/> within a transaction.
    /// </summary>
    /// <param name="batch">
    /// The list of entities to add to the database in this batch.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{Int32}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The number of entities successfully created on success (<see cref="ResponseCodes.Created"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs (<see cref="ResponseCodes.Error"/>), with the transaction rolled back and all entities in the batch detached.</description>
    ///   </item>
    /// </list>
    /// </returns>
    private async Task<Response<int>> ExecuteBatchCreateAsync(List<T> batch, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await _context.Database.BeginTransactionAsync(ct);
            _context.Set<T>().AddRange(batch);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<int>.FromSuccess(ResponseCodes.Created, batch.Count);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                foreach (T e in batch) _context.Entry(e).State = EntityState.Detached;
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

    /// <summary>
    /// Asynchronously executes the update of a batch of entities of type <typeparamref name="T"/> within a transaction.
    /// </summary>
    /// <param name="batch">
    /// The list of entities to update in the database in this batch.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{Int32}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The number of entities successfully updated on success (<see cref="ResponseCodes.Updated"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs (<see cref="ResponseCodes.Error"/>), with the transaction rolled back and all entities in the batch detached.</description>
    ///   </item>
    /// </list>
    /// </returns>
    private async Task<Response<int>> ExecuteBatchUpdateAsync(List<T> batch, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await _context.Database.BeginTransactionAsync(ct);
            _context.Set<T>().UpdateRange(batch);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<int>.FromSuccess(ResponseCodes.Updated, batch.Count);
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                foreach (T e in batch) _context.Entry(e).State = EntityState.Detached;
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
    
    /// <summary>
    /// Asynchronously executes the deletion of a batch of entities of type <typeparamref name="T"/> within a transaction.<br/><br/>
    /// This method does not verify that each entity exists before deletion. That validation should be performed
    /// prior to calling this method—consider using <see cref="CheckIfEntityExistsAsync"/> if needed.
    /// </summary>
    /// <param name="batch">
    /// The list of entities to remove from the database in this batch.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Response{Int32}"/> containing:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The number of entities successfully deleted on success (<see cref="ResponseCodes.Deleted"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>An error code and exception message if an exception occurs (<see cref="ResponseCodes.Error"/>), with the transaction rolled back and all entities in the batch detached.</description>
    ///   </item>
    /// </list>
    /// </returns>
    private async Task<Response<int>> ExecuteBatchDeleteAsync(List<T> batch, CancellationToken ct = default)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await _context.Database.BeginTransactionAsync(ct);
            _context.Set<T>().RemoveRange(batch);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Response<int>.FromSuccess(ResponseCodes.Deleted, batch.Count);
            
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(ct);
                foreach (T e in batch) _context.Entry(e).State = EntityState.Detached;
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

/// <summary>
/// Provides extension methods for repository-related operations.
/// </summary>
public static class BaseRepositoryExtensions
{
    /// <summary>
    /// Determines whether all public properties of the specified object are <c>null</c>.
    /// </summary>
    /// <typeparam name="T">
    /// The reference type of the object whose properties will be checked. Must be a class.
    /// </typeparam>
    /// <param name="obj">
    /// The object whose public properties are inspected.
    /// </param>
    /// <returns>
    /// <c>true</c> if every public property of <paramref name="obj"/> has a <c>null</c> value; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This implementation uses reflection. For better performance and type safety,
    /// provide explicit implementations per service instead of relying on reflection at runtime.
    /// </remarks>
    public static bool CheckAllPropertiesNull<T>(this T obj) where T : class => typeof(T).GetProperties().All(prop => prop.GetValue(obj) is null);
}