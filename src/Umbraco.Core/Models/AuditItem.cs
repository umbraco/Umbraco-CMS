using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public sealed class AuditItem : Entity, IAggregateRoot
    {
        public AuditItem(int objectId, string comment, AuditType type, int userId)
        {
            Id = objectId;
            Comment = comment;
            AuditType = type;
            UserId = userId;
        }

        public string Comment { get; private set; }
        public AuditType AuditType { get; private set; }
        public int UserId { get; private set; }
    }
}