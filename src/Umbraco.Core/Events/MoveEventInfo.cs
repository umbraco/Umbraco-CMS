namespace Umbraco.Core.Events
{
    public class MoveEventInfo<TEntity>
    {
        public MoveEventInfo(TEntity entity, string originalPath, int newParentId)
        {
            Entity = entity;
            OriginalPath = originalPath;
            NewParentId = newParentId;
        }

        public TEntity Entity { get; set; }
        public string OriginalPath { get; set; }
        public int NewParentId { get; set; }
    }
}