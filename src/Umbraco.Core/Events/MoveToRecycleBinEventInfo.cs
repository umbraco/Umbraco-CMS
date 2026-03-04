namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents information about a move to recycle bin operation.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being moved to the recycle bin.</typeparam>
public class MoveToRecycleBinEventInfo<TEntity> : MoveEventInfoBase<TEntity>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MoveToRecycleBinEventInfo{TEntity}" /> class.
    /// </summary>
    /// <param name="entity">The entity being moved to the recycle bin.</param>
    /// <param name="originalPath">The original path of the entity.</param>
    public MoveToRecycleBinEventInfo(TEntity entity, string originalPath) : base(entity, originalPath)
    {
    }
}
