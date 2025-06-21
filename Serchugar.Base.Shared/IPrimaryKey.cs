namespace Serchugar.Base.Shared;

/// <summary>
/// This should be implemented by entities to standardize primary key access. Necessary for constructing URLs when creating resources with CreatedAtAction.
/// </summary>
/// <example>
/// Usage in an entity:
/// <code>
/// public class User : IPrimaryKey
/// {
///     public int UserId { get; set; } // Can be a Guid as well
///     public object IPrimaryKey.Id => UserId;
///
/// 
///     public string Name { get; set; }
/// }
/// </code>
/// </example>
public interface IPrimaryKey
{
    /// <summary>
    /// This should be implemented by entities to standardize primary key access. Necessary for constructing URLs when creating resources with CreatedAtAction.
    /// </summary>
    /// <example>
    /// Usage in an entity:
    /// <code>
    /// public class User : IPrimaryKey
    /// {
    ///     public int UserId { get; set; } // Can be a Guid as well
    ///     public object IPrimaryKey.Id => UserId;
    ///
    /// 
    ///     public string Name { get; set; }
    /// }
    /// </code>
    /// </example>
    public object Id { get; }
}