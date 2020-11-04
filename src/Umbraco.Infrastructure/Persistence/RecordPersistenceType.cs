using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Persistence
{
    [UmbracoVolatile]
    public enum RecordPersistenceType
    {
        Insert,
        Update,
        Delete
    }
}
