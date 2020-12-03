namespace Umbraco.Core.Models.Identity
{
    /// <summary>
    /// EntityType that represents a user belonging to a role
    /// </summary>
    /// <typeparam name="TKey"/>
    public class IdentityUserRole
    {
        /// <summary>
        /// Gets or sets userId for the user that is in the role
        /// </summary>
        public virtual string UserId { get; set; }

        /// <summary>
        /// Gets or sets roleId for the role
        /// </summary>
        public virtual string RoleId { get; set; }
    }
}
