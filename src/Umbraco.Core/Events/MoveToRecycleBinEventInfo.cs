namespace Umbraco.Cms.Core.Events;

public class MoveToRecycleBinEventInfo<TEntity> : MoveEventInfoBase<TEntity>
{
    public MoveToRecycleBinEventInfo(TEntity entity, string originalPath) : base(entity, originalPath)
    {
    }
}
