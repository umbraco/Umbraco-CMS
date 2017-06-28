namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// The state of a user
    /// </summary>
    public enum UserState
    {
        All = -1,
        Active = 0,
        Disabled = 1,
        LockedOut = 2,
        Invited = 3,

        /// <summary>
        /// Occurs when the user has been created (not invited) and has no credentials assigned
        /// </summary>
        /// <remarks>
        /// This state shouldn't really exist or occur
        /// </remarks>
        NoCredentials = 100
    }
}