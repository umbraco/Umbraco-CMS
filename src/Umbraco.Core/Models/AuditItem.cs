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
        public AuditItem(int objectId, string comment, AuditType type, int userId)
        {
            DisableChangeTracking();

            Id = objectId;
            Comment = comment;
            AuditType = type;
            UserId = userId;

            EnableChangeTracking();
        }

        public string Comment { get; }
        public AuditType AuditType { get; }
        public int UserId { get; }     
    }
}
