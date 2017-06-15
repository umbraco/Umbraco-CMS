namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// The state of a user
    /// </summary>
    public enum UserState
    {
        All = 0,
        Active = 1,
        Disabled = 2,
        LockedOut = 3,
        Invited = 4
    }
}