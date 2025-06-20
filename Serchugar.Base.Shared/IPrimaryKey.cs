namespace Serchugar.Base.Shared;

public interface IPrimarykey
{
    /// <summary>
    /// For Ids of type Int, BigInt, Guid
    /// </summary>
    public object Id { get; }
}
