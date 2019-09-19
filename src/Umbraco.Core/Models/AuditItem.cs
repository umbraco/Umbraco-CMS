using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public sealed class AuditItem : Entity, IAuditItem
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

        public string Comment { get; private set; }
        public AuditType AuditType { get; private set; }
        public int UserId { get; private set; }
     
    }
}