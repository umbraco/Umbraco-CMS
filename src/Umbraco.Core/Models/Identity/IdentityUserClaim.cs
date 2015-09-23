namespace Umbraco.Core.Models.Identity
{
    /// <summary>
    /// EntityType that represents one specific user claim
    /// 
    /// </summary>
    /// <typeparam name="TKey"/>
    /// <remarks>
    /// This class normally exists inside of the EntityFramework library, not sure why MS chose to explicitly put it there but we don't want
    /// references to that so we will create our own here
    /// </remarks>
    public class IdentityUserClaim<TKey>
    {
        /// <summary>
        /// Primary key
        /// 
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// User Id for the user who owns this login
        /// 
        /// </summary>
        public virtual TKey UserId { get; set; }

        /// <summary>
        /// Claim type
        /// 
        /// </summary>
        public virtual string ClaimType { get; set; }

        /// <summary>
        /// Claim value
        /// 
        /// </summary>
        public virtual string ClaimValue { get; set; }
    }
}