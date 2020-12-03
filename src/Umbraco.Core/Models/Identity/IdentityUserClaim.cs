namespace Umbraco.Core.Models.Identity
{
    /// <summary>
    /// EntityType that represents one specific user claim
    /// </summary>
    public class IdentityUserClaim
    {
        /// <summary>
        /// Gets or sets primary key
        /// </summary>
        public virtual string Id { get; set; } // TODO: Not used

        /// <summary>
        /// Gets or sets user Id for the user who owns this login
        /// </summary>
        public virtual string UserId { get; set; }

        /// <summary>
        /// Gets or sets claim type
        /// </summary>
        public virtual string ClaimType { get; set; }

        /// <summary>
        /// Gets or sets claim value
        /// </summary>
        public virtual string ClaimValue { get; set; }
    }
}
