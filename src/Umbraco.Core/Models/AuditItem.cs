using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public sealed class AuditItem : EntityBase, IAuditItem
    {
        /// <summary>
        /// Constructor for creating an item to be created
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="comment"></param>
        /// <param name="type"></param>
        /// <param name="userId"></param>
        public AuditItem(int objectId, AuditType type, int userId, string entityType, string comment = null, string parameters = null)
        {
            DisableChangeTracking();

            Id = objectId;
            Comment = comment;
            AuditType = type;
            UserId = userId;
            EntityType = entityType;
            Parameters = parameters;

            EnableChangeTracking();
        }

        public string Comment { get; }

        /// <inheritdoc/>
        public string EntityType { get; }
        /// <inheritdoc/>
        public string Parameters { get; }

        public AuditType AuditType { get; }
        public int UserId { get; }     
    }
}
