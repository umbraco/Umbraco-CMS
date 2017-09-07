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

        /// <summary>
        /// Constructor for creating an item that is returned from the database
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="comment"></param>
        /// <param name="type"></param>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="userAvatar"></param>
        public AuditItem(int objectId, string comment, AuditType type, int userId, string userName, string userAvatar)
        {
            DisableChangeTracking();

            Id = objectId;
            Comment = comment;
            AuditType = type;
            UserId = userId;
            UserName = userName;
            UserAvatar = userAvatar;

            EnableChangeTracking();
        }

        public string Comment { get; private set; }
        public AuditType AuditType { get; private set; }
        public int UserId { get; private set; }
        public string UserName { get; private set; }
        public string UserAvatar { get; private set; }
    }
}