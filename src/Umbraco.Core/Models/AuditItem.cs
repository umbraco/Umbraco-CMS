using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public sealed class AuditItem : EntityBase
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
