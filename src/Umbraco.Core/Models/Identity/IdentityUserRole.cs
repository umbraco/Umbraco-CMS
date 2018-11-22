namespace Umbraco.Core.Models.Identity
{
    /// <summary>
    /// EntityType that represents a user belonging to a role
    /// 
    /// </summary>
    /// <typeparam name="TKey"/>
    /// <remarks>
    /// This class normally exists inside of the EntityFramework library, not sure why MS chose to explicitly put it there but we don't want
    /// references to that so we will create our own here
    /// </remarks>
    public class IdentityUserRole<TKey>
    {
        /// <summary>
        /// UserId for the user that is in the role
        /// 
        /// </summary>
        public virtual TKey UserId { get; set; }

        /// <summary>
        /// RoleId for the role
        /// 
        /// </summary>
        public virtual TKey RoleId { get; set; }
    }
}