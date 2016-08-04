namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents an entity -> user/group & permission key value pair collection
    /// </summary>
    public abstract class EntityPermissionSet
    {
        /// <summary>
        /// The entity id with permissions assigned
        /// </summary>
        public int EntityId { get; private set; }

        protected EntityPermissionSet(int entityId)
        {
            EntityId = entityId;
        }
    }
}